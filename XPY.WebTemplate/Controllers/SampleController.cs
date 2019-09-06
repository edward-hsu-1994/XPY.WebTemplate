using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviceDetectorNET;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XPY.WebTemplate.Core.Authorization;
using XPY.WebTemplate.Core.Mvc;
using XPY.WebTemplate.Models;
using XPY.WebTemplate.Services;

namespace XPY.WebTemplate.Controllers {
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class SampleController : Controller {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get(
            [FromServices]DeviceDetector detector,
            [FromServices]ILogger<SampleController> logger) {
            logger.LogError("TEST");
            var os = detector.GetOs();
            return new string[] { os.Match.Name };
        }

        [HttpPost]
        public string Post([FromBody]SampleModel loginData, [FromServices] SampleService jwt) {
            return jwt.JwtHelper.BuildToken("userId");
        }

        [HttpPost("jsonAndFile")]
        public string Post2([FromFormJson]SampleModel loginData, IFormFile photo) {
            return null;
        }

    }
}
