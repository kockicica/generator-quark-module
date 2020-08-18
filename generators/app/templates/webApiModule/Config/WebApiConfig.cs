using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Quark.Security.ApiKey.Contexts;

namespace <%= options.moduleName %>.Config {

    public class WebApiConfig : Core.Config.WebApiConfig {
        public override string   StaticFileRoot   => <% if (answers.serveStatic) {%>"<%= answers.staticRoot %>";<% } else { %>null<% } %>
        public override bool     ApiKeyValidation => <%= answers.apiKeyValidation %>;
        public override string   ValidationHeader => "ApiKey";
        public override string   ValidationKey    => string.Empty;
        public override bool     HandleSPA        => <%= answers.handleSPA %>;
        <% if (answers.handleSPA) {%>public override string[] SPAPrefixes      => new[] {<%- answers.spaExemptPrefixes.split(',').map(s=>s.trim()).map(pr=>`"${pr}"`).join(',')%>};<%}%>

        public override async Task ValidateIdentity(ApiKeyValidateIdentityContext ctx) {
            <% if (answers.apiKeyValidation) {%>if (ctx.ApiKey == "123456") {
                ctx.Validate();
            }<%} else {%>throw new NotImplementedException();<%}%>
        }

        public override async Task<IEnumerable<Claim>> GenerateClaims(ApiKeyGenerateClaimsContext arg) {
            <% if (answers.apiKeyValidation) {%>var claims = new List<Claim>();
            return claims;<% } else { -%>throw new NotImplementedException();<% } %>
        }
    }
}