using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BasicForgeApp.Services;

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
    }
}