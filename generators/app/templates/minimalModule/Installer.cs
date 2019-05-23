using Castle.Core.Logging;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

using <%= options.moduleName %>.Config;

namespace <%= options.moduleName %> {

    public class Installer : IWindsorInstaller {

        protected string ModuleConfigName      = "<%= options.moduleName %>.Windsor";

        public void Install(IWindsorContainer container, IConfigurationStore store) {
            var loggerFactory = container.Resolve<ILoggerFactory>();
            var logger = loggerFactory.Create(GetType());

            logger.Debug("Installing");

            container.Register();


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
                    .Named($"{ModuleConfigName}.configurationCfg")
            );

        }

    }

}