using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.Owin.Security.ApiKey.Contexts;

namespace <%= options.moduleName %>.Core.Config {

    public class WebApiConfig : IWebApiConfig {

        public virtual string   StaticFileRoot   => null;
        public virtual bool     ApiKeyValidation => true;
        public virtual bool     HandleSPA        => true;
        public virtual string[] SPAPrefixes      => new[] {"/api"};
        public virtual string   ValidationHeader => "Authorization";
        public virtual string   ValidationKey    => "ApiKey";

        public async virtual Task ValidateIdentity(ApiKeyValidateIdentityContext ctx) {
            if (ctx.ApiKey == "123456") {
                ctx.Validate();
            }
        }

        public async virtual Task<IEnumerable<Claim>> GenerateClaims(ApiKeyGenerateClaimsContext arg) {
            var claims = new List<Claim>();
            return claims;
        }


    }

}