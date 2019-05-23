namespace <%= options.moduleName %>.Core
{
    public interface IWebAppStartupFactory
    {
        IWebApiStartup Create();
        IWebApiStartup Create(string name);
        void Release(IWebApiStartup instance);
        void Dispose();
    }
}
