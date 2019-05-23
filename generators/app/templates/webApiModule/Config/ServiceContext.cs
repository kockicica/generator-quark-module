using System;

using Castle.Core.Logging;

using Quark.Common;
using Quark.Services;

namespace <%= options.moduleName %>.Config {

    public class ServiceContext : IServiceContext {

        private readonly IQuarkServiceHostContext _hostContext;

        public ServiceContext(IQuarkServiceHostContext hostContext) {
            _hostContext = hostContext;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public QuarkServiceState State {
            get {
                (var hCtx, var error) = _hostContext.GetServiceContext(Const.ServiceName);
                if (error) {
                    Logger.Warn($"There is no host context for {Const.ServiceName}");
                    return QuarkServiceState.Stopped;
                }

                return hCtx.State;

            }
            set => throw new NotImplementedException();
        }

        public bool Active => State == QuarkServiceState.Running;

    }

}