using Owin;

namespace <%= options.moduleName %>.Core {

    public interface IWebApiStartup {

        void Configuration(IAppBuilder app);

    }

}