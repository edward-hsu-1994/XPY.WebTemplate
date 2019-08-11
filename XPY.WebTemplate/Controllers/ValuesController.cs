using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviceDetectorNET;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPY.WebTemplate.Core.Authorization;

namespace XPY.WebTemplate.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get([FromServices]DeviceDetector detector) {
            var os = detector.GetOs();
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id) {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public string Post([FromBody] string value, [FromServices] JwtFactory jwt) {
            return jwt.BuildToken("aaa");
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value) {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id) {
        }
    }
}
