using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;

using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;

namespace <%= options.moduleName %>.Core.Windsor
{
    public class WindsorDependencyScope : IDependencyScope
    {
        private readonly IWindsorContainer _container;
        private readonly IDisposable _scope;


        public WindsorDependencyScope(IWindsorContainer container)
        {
            _container = container;
            _scope = _container.BeginScope();
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        public object GetService(Type serviceType)
        {
            if (_container.Kernel.HasComponent(serviceType))
                return _container.Resolve(serviceType);

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.ResolveAll(serviceType).Cast<object>();
        }
    }
}