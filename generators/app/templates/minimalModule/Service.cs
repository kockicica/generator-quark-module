using System;

using Castle.Core.Logging;

using Newtonsoft.Json.Linq;

using Quark.Common;
using <%= options.moduleName %>.Config;

namespace <%= options.moduleName %> {

    public class Service : IQuarkService {

        private readonly IConfiguration        _configuration;
        private          QuarkServiceState     _state;

        public Service(IConfiguration configuration) {
            _configuration = configuration;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public void Start() {
            try {
                Logger.Debug($"Starting {Const.ServiceName}");
                _state = QuarkServiceState.Running;
                Logger.Debug($"Service {Const.ServiceName} started, listening at:{_configuration.HostUrl}");
            }
            catch (Exception e) {
                Logger.Error($"Error starting service:", e);
            }
        }

        public void Stop() {
            try {
                Logger.Debug($"Stopping {Const.ServiceName} service");
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