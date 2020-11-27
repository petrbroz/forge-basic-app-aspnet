using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BasicForgeApp.Services;
using iText.Kernel.Pdf;
using iText.Svg.Converter;
using System.IO;
using System;
using Microsoft.AspNetCore.Hosting;
using System.Text.RegularExpressions;

namespace BasicForgeApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarkupsController : ControllerBase
    {
        private IHostingEnvironment env;

        public MarkupsController(IForgeService forge, IHostingEnvironment _env)
        {
            env = _env;
        }

        private string TransformSvg(string svg, float modelWidth, float modelHeight, float pageWidth, float pageHeight)
        {
            // Assuming <svg> is on the first line
            var lines = svg.Split('>');
            lines[0] = Regex.Replace(lines[0], " viewBox=\"[^\"]*\"", string.Format(" viewBox=\"0 0 {0} {1}\"", modelWidth, modelHeight));
            lines[0] = Regex.Replace(lines[0], " style=\"[^\"]*\"", string.Format(" style=\"transform: scale(1, -1) translate(0, -{0})\"", pageHeight));
            lines[0] = Regex.Replace(lines[0], " width=\"[^\"]*\"", string.Format(" width=\"{0}\"", pageWidth));
            lines[0] = Regex.Replace(lines[0], " height=\"[^\"]*\"", string.Format(" height=\"{0}\"", pageHeight));
            return string.Join('>', lines);
        }

        // POST api/markups
        [HttpPost]
        public ActionResult Post()
        {
            var page = int.Parse(Request.Form["page_number"]);
            var width = float.Parse(Request.Form["page_width"]);
            var height = float.Parse(Request.Form["page_height"]);
            var units = Request.Form["page_units"].ToString();
            var markups = Request.Form["markups"].ToString();
            var srcPdfPath = Path.Combine(env.WebRootPath, "sample.pdf");
            var dstPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tmp.pdf");
            using (var writer = new PdfWriter(dstPath))
            {
                var doc = new PdfDocument(new PdfReader(srcPdfPath), writer);
                var size = doc.GetFirstPage().GetPageSize();
                // For some reason, if we change the SVG dimensions to 800x600 (corresponding to 11.11x8.33"),
                // the SVG element does not cover the entire page...???
                // Multiplying the dimensions by 4/3 seems to resolve that issue, but I have no idea why.
                markups = TransformSvg(markups, width, height, size.GetWidth() * 4f / 3f, size.GetHeight() * 4f / 3f);
                SvgConverter.DrawOnDocument(markups, doc, page, 0f, 0f);
                doc.Close();
            }
            return File(System.IO.File.ReadAllBytes(dstPath), "application/pdf");
        }
    }
}
