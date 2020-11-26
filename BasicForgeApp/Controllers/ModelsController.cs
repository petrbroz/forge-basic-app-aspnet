using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BasicForgeApp.Services;
using System.IO;

namespace BasicForgeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelsController : ControllerBase
    {
        private readonly IForgeService _forge;

        public ModelsController(IForgeService forge)
        {
            _forge = forge;
        }

        // GET api/models
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model>>> Get()
        {
            var models = await _forge.ListModels();
            return models;
        }

        // POST api/models
        [HttpPost]
        public async Task<ActionResult<Model>> Post()
        {
            if (!Request.Form.ContainsKey("model-name"))
            {
                throw new System.Exception("Missing name of uploaded model.");
            }
            if (Request.Form.Files.Count != 1)
            {
                throw new System.Exception("Missing file to upload.");
            }
            var objectKey = Request.Form["model-name"];
            var zipEntrypoint = Request.Form["model-zip-entrypoint"];
            var file = Request.Form.Files[0];
            var localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), file.FileName);
            Model model = null;
            using (var local = System.IO.File.Create(localPath))
            {
                await file.CopyToAsync(local);
            }
            using (var stream = System.IO.File.OpenRead(localPath))
            {
                model = await _forge.UploadModel(objectKey, stream, zipEntrypoint);
            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.IO.File.Delete(localPath);
            return model;
        }
    }
}