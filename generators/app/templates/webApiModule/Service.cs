using System;

using Castle.Core.Logging;

using Microsoft.Owin.Hosting;

using Newtonsoft.Json.Linq;

using Quark.Common;
using <%= options.moduleName %>.Config;
using <%= options.moduleName %>.Core;

namespace <%= options.moduleName %> {

    public class Service : IQuarkService {

        private          IWebApiStartup        _webApiStartup;
        private readonly IConfiguration        _configuration;
        private          IDisposable           _webApp;
        private          QuarkServiceState     _state;
        private readonly IWebAppStartupFactory _appStartupFactory;

        public Service(IConfiguration configuration, IWebAppStartupFactory appStartupFactory) {
            _configuration = configuration;
            _appStartupFactory = appStartupFactory;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public void Start() {
            try {
                Logger.Debug($"Starting {Const.ServiceName}");
                _webApiStartup = _appStartupFactory.Create();
                _webApp = WebApp.Start(_configuration.HostUrl, builder => _webApiStartup.Configuration(builder));
                _state = QuarkServiceState.Running;
                Logger.Debug($"Service {Const.ServiceName} started, listening at:{_configuration.HostUrl}");
            }
            catch (Exception e) {
                Logger.Error($"Error starting service:", e);
                _webApp?.Dispose();
            }
        }

        public void Stop() {
            try {
                Logger.Debug($"Stopping {Const.ServiceName} service");
                _webApp?.Dispose();
                _appStartupFactory.Release(_webApiStartup);
                Logger.Debug($"Service {Const.ServiceName} stopped");
            }
            catch (Exception e) {
                Logger.Error("Error stopping service:", e);
            }
            finally {
                _state = QuarkServiceState.Stopped;
            }
        }

        public QuarkServiceState State              => _state;
        public string            Name               => Const.ServiceName;
        public JObject           QuarkConfig        => SerializationHelper.SerializeObject(_configuration);
        public JObject           QuarkDefaultConfig => SerializationHelper.SerializeObject(ConfigurationValue.Default);

    }

}