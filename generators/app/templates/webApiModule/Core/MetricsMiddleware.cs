using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Castle.Core.Logging;

using Metrics;

using Microsoft.Owin;

using Quark.Services.Metrics;

namespace <%= options.moduleName %>.Core {

    public class MetricsMiddleware {

        private readonly IQuarkMetric   _metric;
        private readonly Timer          _timer;
        private readonly Counter        _activeRequests;
        private readonly Meter          _errorMeter;
        private readonly Histogram      _payloadSizeHistogram;


        public MetricsMiddleware(IQuarkMetric metric) {
            _metric = metric;
            _timer = _metric.Timer("request:timer", Unit.Requests);
            _activeRequests = _metric.Counter("request:active", Unit.Custom("ActiveRequests"));
            _errorMeter = _metric.Meter("request:errors", Unit.Errors);
            _payloadSizeHistogram = _metric.Histogram("request:payload_size", Unit.Bytes);
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public async Task Handle(IOwinContext ctx, Func<Task> next) {
            _activeRequests.Increment();
            var httpMethod = ctx.Environment["owin.RequestMethod"].ToString().ToUpper();

            if (httpMethod == "POST" || httpMethod == "PUT") {
                var headers = (IDictionary<string, string[]>) ctx.Environment["owin.RequestHeaders"];
                if (headers != null && headers.ContainsKey("Content-Length")) {
                    _payloadSizeHistogram.Update(long.Parse(headers["Content-Length"].First()));
                }
            }

            using (_timer.NewContext()) {
                await next.Invoke();
            }

            var responseCode = int.Parse(ctx.Environment["owin.ResponseStatusCode"].ToString());
            if (responseCode == (int) HttpStatusCode.InternalServerError) {
                _errorMeter.Mark();
            }

            _activeRequests.Decrement();
        }

    }

}