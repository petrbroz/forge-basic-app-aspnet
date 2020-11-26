using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BasicForgeApp.Services;

namespace BasicForgeApp.Controllers
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
            return await _forge.GetPublicToken();
        }
    }
}
