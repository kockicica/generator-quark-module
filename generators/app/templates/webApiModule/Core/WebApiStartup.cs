using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Cors;

using Castle.Core.Logging;

using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;

using Newtonsoft.Json;

using Owin;

using Quark.Security.ApiKey;
using Quark.Security.ApiKey.Extensions;

using <%= options.moduleName %>.Core.Config;
using <%= options.moduleName %>.Core.NHibernate;

namespace <%= options.moduleName %>.Core {

    public class WebApiStartup : IWebApiStartup {

        private readonly HttpConfiguration     _httpConfiguration;
        private readonly NHibernateOwinHandler _hibernateOwinHandler;
        public readonly  MetricsMiddleware     _metricsMiddleware;
        private readonly IWebApiConfig         _webApiConfig;

        public WebApiStartup(HttpConfiguration httpConfiguration, NHibernateOwinHandler hibernateOwinHandler,
                             MetricsMiddleware metricsMiddleware, IWebApiConfig webApiConfig) {
            _httpConfiguration = httpConfiguration;
            _hibernateOwinHandler = hibernateOwinHandler;
            _metricsMiddleware = metricsMiddleware;
            _webApiConfig = webApiConfig;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;

        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public virtual void Configuration(IAppBuilder app) {

            _httpConfiguration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            _httpConfiguration.MapHttpAttributeRoutes();

            ConfigureCORS(app);

            ConfigureAuthentication(app);

            ConfigureNHibernateHandler(app);

            ConfigureSPAApplication(app);

            ConfigureMetricsMiddleware(app);

            ConfigureStaticFileServe(app);

            ConfigureWebAPI(app);
        }

        protected virtual void ConfigureAuthentication(IAppBuilder app) {
            if (_webApiConfig.ApiKeyValidation) {
                app.UseApiKeyAuthentication(new ApiKeyAuthenticationOptions {
                    Provider = new ApiKeyAuthenticationProvider {
                        OnValidateIdentity = _webApiConfig.ValidateIdentity,
                        OnGenerateClaims = _webApiConfig.GenerateClaims
                    },
                    Header = _webApiConfig.ValidationHeader,
                    HeaderKey = _webApiConfig.ValidationKey,
                });

            }

        }

        protected virtual void ConfigureCORS(IAppBuilder app) {

            var cors = new EnableCorsAttribute("*", "*", "*");
            _httpConfiguration.EnableCors(cors);

        }

        protected virtual void ConfigureStaticFileServe(IAppBuilder app) {
            if (!string.IsNullOrEmpty(_webApiConfig.StaticFileRoot)) {
                // there is static file root configured
                var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var servePath = Path.Combine(rootPath, _webApiConfig.StaticFileRoot);

                Logger.Debug($"serving static files from:{servePath}");

                if (!File.Exists(servePath)) {
                    Logger.Warn($"Static files root {servePath} does not exist!");
                    return;
                }

                var filesystem = new PhysicalFileSystem(servePath);
                var fileServerOptions = new FileServerOptions {
                    RequestPath = PathString.Empty,
                    EnableDefaultFiles = true,
                    FileSystem = filesystem,
                    EnableDirectoryBrowsing = false,
                };
                fileServerOptions.StaticFileOptions.ServeUnknownFileTypes = true;
                app.UseFileServer(fileServerOptions);
            }

        }

        protected virtual void ConfigureSPAApplication(IAppBuilder app) {

            if (!_webApiConfig.HandleSPA) {
                // do not handle SPA problems :)
                return;
            }

            // simple request url rewrite to proper serve angular spa application using static middleware
            app.Use(async (context, next) => {
                var req = context.Request.Path;

                if (_webApiConfig.SPAPrefixes.All(spaPrefix => !req.StartsWithSegments(new PathString(spaPrefix)))) {
                    // request path does not start with any of the provided pass-through segments
                    var name = req.Value;

                    // does this path have an extension?
                    var ext = Path.GetExtension(name);

                    // and if it does have an extension, it is probably file that static files middleware will serve
                    // if it does not - its probably angular route leaked to server as full page refresh on some route. 
                    if (string.IsNullOrEmpty(ext)) {
                        context.Request.Path = new PathString("/");
                    }

                }

                await next();
            });
        }

        protected virtual void ConfigureMetricsMiddleware(IAppBuilder app) {
            app.Use(_metricsMiddleware.Handle);

        }

        protected virtual void ConfigureNHibernateHandler(IAppBuilder app) {
            app.UseNHibernateHandler(_hibernateOwinHandler);
        }

        protected virtual void ConfigureWebAPI(IAppBuilder app) {
            app.UseWebApi(_httpConfiguration);
        }

//        protected virtual async Task ValidateIdentity(ApiKeyValidateIdentityContext ctx) {
//            ctx.Validate();
//        }
//
//        protected virtual async Task<IEnumerable<Claim>> GenerateClaims(ApiKeyGenerateClaimsContext ctx) {
//
//            throw new NotImplementedException();
//        }

//        private async Task ValidateIdentity(ApiKeyValidateIdentityContext ctx) {
//            if (ctx.ApiKey.Length == 0) {
//                return;
//            }
//
//            try {
//                var usr = _poolUsers.GetByAPIKey(ctx.ApiKey);
//                if (usr != null && usr.Active) {
//                    ctx.Validate();
//                }
//            }
//            catch (Exception e) {
//                Logger.Error("ValidateIdentity error", e);
//            }
//
//        }
//
//        private async Task<IEnumerable<Claim>> GenerateClaims(ApiKeyGenerateClaimsContext ctx) {
//            //return new[]{new Claim(ClaimTypes.Name, "1")}.AsEnumerable();
//            var usr = _poolUsers.GetByAPIKey(ctx.ApiKey);
//            var claims = new List<Claim>();
//            if (usr != null) {
//                claims.Add(new Claim(ClaimTypes.Name, usr.Id.ToString()));
//                claims.Add(new Claim(ClaimTypes.UserData, usr.Id.ToString()));
//                var permissions = _poolUsers.GetPermissions(usr.Id);
//                foreach (var permission in permissions) {
//                    claims.Add(new Claim(ClaimTypes.Role, permission));
//                }
//            }
//
//            return claims;
//        }

    }

}