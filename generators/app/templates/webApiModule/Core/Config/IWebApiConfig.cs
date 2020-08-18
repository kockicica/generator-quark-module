using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

using Quark.Security.ApiKey.Contexts;

namespace <%= options.moduleName %>.Core.Config {

    public interface IWebApiConfig {

        string   StaticFileRoot   { get; }
        bool     ApiKeyValidation { get; }
        string   ValidationHeader { get; }
        string   ValidationKey    { get; }
        bool     HandleSPA        { get; }
        string[] SPAPrefixes      { get; }

        Task ValidateIdentity(ApiKeyValidateIdentityContext arg);

        Task<IEnumerable<Claim>> GenerateClaims(ApiKeyGenerateClaimsContext arg);

    }

}