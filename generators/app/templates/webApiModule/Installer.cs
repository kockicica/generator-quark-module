using System.Web.Http;
using System.Web.Http.Dispatcher;

using Castle.Core.Logging;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

using <%= options.moduleName %>.Config;
using <%= options.moduleName %>.Core;
using <%= options.moduleName %>.Core.Auth;
using <%= options.moduleName %>.Core.Config;
using <%= options.moduleName %>.Core.NHibernate;
using <%= options.moduleName %>.Core.Windsor;

namespace <%= options.moduleName %> {

    public class Installer : IWindsorInstaller {

        protected string ModuleConfigName      = "<%= options.moduleName %>.Windsor";
        protected string JwtSecretKey          = "someSecretKey";
        protected int    JwtTokenValidForHours = 6;

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            var loggerFactory = container.Resolve<ILoggerFactory>();
            var logger = loggerFactory.Create(GetType());

            logger.Debug("Installing");

            container.Register(
                Component
                    .For<HttpConfiguration>()
                    .UsingFactoryMethod(() => new HttpConfiguration {
                        DependencyResolver = new WindsorDependencyResolver(container),
                        IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always,
                        Initializer = configuration => {
                            configuration
                                .Services
                                .Replace(typeof(IHttpControllerTypeResolver), new CustomHttpControllerTypeResolver());
                        }
                    })
                    .LifestyleTransient()
                    .Named($"{ModuleConfigName}.httpCfg"),
                Component
                    .For<IWebApiStartup>()
                    .ImplementedBy<WebApiStartup>()
                    .DependsOn(
                        Dependency.OnComponent("httpConfiguration", $"{ModuleConfigName}.httpCfg"),
                        Dependency.OnComponent("webApiConfig", $"{ModuleConfigName}.webApiCfg"))
                    .LifestyleTransient(),
                Component
                    .For<ITokenIssuer>()
                    .ImplementedBy<TokenIssuer>()
                    .DependsOn(
                        Dependency.OnValue("validForHours", JwtTokenValidForHours),
                        Dependency.OnValue("key", JwtSecretKey)
                    )
                    .Named($"{ModuleConfigName}.tokenIssuerCfg"),
                Component
                    .For<IWebAppStartupFactory>()
                    .AsFactory(),
                Classes
                    .FromThisAssembly()
                    .BasedOn<ApiController>()
                    .WithServiceSelf()
                    .LifestyleScoped(),
                Component
                    .For<NHibernateOwinHandler>()
                    .Named($"{ModuleConfigName}.nhibernateOwinHandlerCfg"),
                Component
                    .For<MetricsMiddleware>()
                    .Named($"{ModuleConfigName}.metricsMiddlewareCfg")
            );


            Register(container, store);
            container.Release(loggerFactory);

        }

        public void Register(IWindsorContainer container, IConfigurationStore store) {
            container.Register(
                Component
                    .For<IServiceContext>()
                    .ImplementedBy<ServiceContext>()
                    .Named($"{ModuleConfigName}.serviceContextCfg"),
                Component
                    .For<IConfiguration>()
                    .ImplementedBy<Configuration>()
                    .Named($"{ModuleConfigName}.configurationCfg"),
                Component
                    .For<IWebApiConfig>()
                    .ImplementedBy<Config.WebApiConfig>()
                    .Named($"{ModuleConfigName}.webApiCfg")
            );

        }

    }

}