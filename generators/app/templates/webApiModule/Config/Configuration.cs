using Quark.Services;

namespace <%= options.moduleName %>.Config {

    public class Configuration : IConfiguration {

        private readonly        IQuarkLocalStorage _quarkLocalStorage;
        private static readonly string             Key = Const.ServiceName;

        public Configuration(IQuarkLocalStorage quarkLocalStorage) {
            _quarkLocalStorage = quarkLocalStorage;
        }

        public string HostUrl => _quarkLocalStorage.Read<string>(Key, "hostUrl");

    }

    public class ConfigurationValue : IConfiguration {

        public string HostUrl { get; }

        public ConfigurationValue(string hostUrl) {
            HostUrl = hostUrl;
        }

        public static IConfiguration Default => new ConfigurationValue("http://+:20000");

    }

}