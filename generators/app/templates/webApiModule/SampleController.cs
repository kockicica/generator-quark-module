using System.Web.Http;

using <%= options.moduleName %>.Core;
using Quark.Services.Metrics;


namespace <%= options.moduleName %> {

    [RoutePrefix("api/sample")]
    public class SampleController : BaseApiController {

        public SampleController(IQuarkMetric metric) : base(metric) { }

        [Route("value")]
        [HttpGet]
        [Core.Authorize]
        public IHttpActionResult Value() {
            return Ok(new {value = 42});
        }

    }

}