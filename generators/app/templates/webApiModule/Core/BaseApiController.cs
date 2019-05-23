using System.Net.Http;
using System.Web.Http;

using Castle.Core.Logging;

using Metrics;

using NHibernate;

using <%= options.moduleName %>.Core.NHibernate;
using Quark.Services.Metrics;

namespace <%= options.moduleName %>.Core {

    public class BaseApiController : ApiController {

        //private            Lazy<PoolUser> _user;
        protected readonly IQuarkMetric   Metric;
        protected readonly MetricsContext MetricsContext;


        public BaseApiController(IQuarkMetric metric) {
            Metric = metric;
            //_user = new Lazy<PoolUser>(Init);
            //MetricsContext = Metric.Context("datapool");
            MetricsContext = metric;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        protected virtual ISession Session => GetSession();

        protected virtual ISession GetSession() {
            var env = Request.GetOwinEnvironment();
            if (env.ContainsKey(NHibernateOwinHandler.NHibernateSessionKey))
                return (ISession) env[NHibernateOwinHandler.NHibernateSessionKey];

            return null;
        }

        // protected PoolUser PoolUser => _user.Value;


//        private PoolUser Init() {
//            PoolUser user = null;
//            try {
//                var userClaim = ((ClaimsPrincipal) base.User).Claims.SingleOrDefault(x => x.Type == ClaimTypes.UserData);
//                if (userClaim != null) {
//                    var userid = int.Parse(userClaim.Value);
//                    user = Session.Get<PoolUser>(userid);
//                }
//            }
//            catch (Exception e) {
//                Logger.Error("Error while retreiving current user", e);
//            }
//
//            return user;
//        }

    }

}