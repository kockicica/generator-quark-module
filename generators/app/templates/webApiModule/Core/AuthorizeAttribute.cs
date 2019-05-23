using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace <%= options.moduleName %>.Core {

    public class AuthorizeAttribute : System.Web.Http.AuthorizeAttribute {

        private HashSet<string> _roles;


        public AuthorizeAttribute(params string[] roles) {
            Roles = string.Join(",", roles.Select(x => x.ToString()));
            _roles = new HashSet<string>(roles.Select(x => x.ToString()));
        }

        protected override bool IsAuthorized(HttpActionContext actionContext) {

            if (actionContext.ControllerContext.RequestContext.Principal is ClaimsPrincipal p) {
                var userRoles = p.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
                if (_roles.Overlaps(userRoles) || _roles.Count == 0) {
                    return true;
                }

                return false;
            }

            return base.IsAuthorized(actionContext);
        }

        public override void OnAuthorization(HttpActionContext actionContext) {
            base.OnAuthorization(actionContext);
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext) {
            base.HandleUnauthorizedRequest(actionContext);
        }

        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken) {
            return base.OnAuthorizationAsync(actionContext, cancellationToken);
        }

    }

}