using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace <%= options.moduleName %>.Core.Auth {

    public class AddChalengeResult : IHttpActionResult {

        public IHttpActionResult         InnerResult { get; }
        public AuthenticationHeaderValue Challenge   { get; }

        public AddChalengeResult(IHttpActionResult innerResult, AuthenticationHeaderValue challenge) {
            InnerResult = innerResult;
            Challenge = challenge;
        }


        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken) {
            var responseMessage = await InnerResult.ExecuteAsync(cancellationToken);
            if (responseMessage.StatusCode == HttpStatusCode.Unauthorized) {
                if (!responseMessage.Headers.WwwAuthenticate.Any(x => x.Scheme == Challenge.Scheme)) {
                    responseMessage.Headers.WwwAuthenticate.Add(Challenge);
                }
            } else {
                responseMessage.Headers.Add("Token", Challenge.Parameter);
            }

            return responseMessage;
        }

    }

}