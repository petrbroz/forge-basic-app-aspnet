using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BasicForgeApp.Services;

namespace ForgeMarkupPDF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IForgeService _forge;

        public AuthController(IForgeService forge)
        {
            _forge = forge;
        }

        // GET api/auth/token
        [HttpGet("token")]
        public async Task<ActionResult<Auth>> Get()
        {
            var auth = await _forge.GetPublicToken();
            return auth;
        }
    }
}
