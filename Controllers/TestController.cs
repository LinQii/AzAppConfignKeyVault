using AzAppConfignKeyVault.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AzAppConfignKeyVault.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ColorSettings _settings;


        public TestController(
            IOptionsSnapshot<ColorSettings> settings
        )
        {
            _settings = settings.Value;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var color = _settings.Color;
            return Ok(color);
        }
    }
}
