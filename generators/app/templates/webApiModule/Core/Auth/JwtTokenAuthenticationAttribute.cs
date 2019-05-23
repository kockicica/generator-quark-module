using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

using JWT;

using Newtonsoft.Json;

namespace <%= options.moduleName %>.Core.Auth {

    public class JwtTokenAuthenticationAttribute : Attribute, IAuthenticationFilter {

        private readonly ITokenIssuer _tokenIssuer;

        public JwtTokenAuthenticationAttribute(ITokenIssuer tokenIssuer) {
            _tokenIssuer = tokenIssuer;
        }

        public bool AllowMultiple => false;

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken) {
            var request = context.Request;

            try {
                var auth = request.Headers.Authorization;

                if (auth == null) {
                    return;
                }

                if (auth.Scheme != "Bearer") {
                    return;
                }

                var bearerToken = auth.Parameter;
                if (string.IsNullOrEmpty(bearerToken)) {
                    context.ErrorResult = new AuthenticationFailureResult(request, "No auth informations");
                    return;
                }

                var token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;
                var secret = "someSecretKey";

                var principal = await Task.FromResult(ValidateToken(token, secret, true));
                if (principal == null) {
                    context.ErrorResult = new AuthenticationFailureResult(request, "No auth informations");
                } else {
                    context.Principal = principal;
                }
            }
            catch (SignatureVerificationException e) {
                context.ErrorResult = new AuthenticationFailureResult(request, e.Message);
                return;
            }
            catch (Exception e) {
                context.ErrorResult = new AuthenticationFailureResult(request, e.Message);
                return;
            }

        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken) {
            var request = context.Request;
            var auth = request.Headers.Authorization;
            AuthenticationHeaderValue challenge;
            if (auth != null)
                challenge = new AuthenticationHeaderValue("Bearer", _tokenIssuer.Refresh(auth.Parameter));
            else
                challenge = new AuthenticationHeaderValue("Bearer");
            context.Result = new AddChalengeResult(context.Result, challenge);
            return Task.FromResult(0);
        }

        private static DateTime FromUnixTime(long unixTime) {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        private static ClaimsPrincipal ValidateToken(string token, string secret, bool checkExpiration) {
            //var jsonSerializer = new JavaScriptSerializer();
            //jsonSerializer.RetainCasing = true;
            var payloadJson = JsonWebToken.Decode(token, secret);
            var payloadData = JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadJson);
            object exp;
            if (payloadData != null && (checkExpiration && payloadData.TryGetValue("exp", out exp))) {
                var validTo = FromUnixTime(long.Parse(exp.ToString()));
                if (DateTime.Compare(validTo, DateTime.UtcNow) <= 0) {
                    throw new Exception(string.Format("Token has expired. Exp:'{0}', Current:'{1}'", validTo, DateTime.UtcNow));
                }
            }

            var subject = new ClaimsIdentity("Federation", ClaimTypes.Actor, ClaimTypes.Role);
            var claims = new List<Claim>();
            if (payloadData != null) {
                foreach (var pair in payloadData) {
                    var claimType = pair.Key;
                    var source = pair.Value as ArrayList;
                    if (source != null) {
                        claims.AddRange(from object item in source select new Claim(claimType, item.ToString(), ClaimValueTypes.String));
                        continue;
                    }

                    switch (pair.Key) {
                        case "name":
                            claims.Add(new Claim(ClaimTypes.Name, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                        case "surname":
                            claims.Add(new Claim(ClaimTypes.Surname, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                        case "email":
                            claims.Add(new Claim(ClaimTypes.Email, pair.Value.ToString(), ClaimValueTypes.Email));
                            break;
                        case "role":
                            claims.Add(new Claim(ClaimTypes.Role, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                        case "userId":
                            claims.Add(new Claim(ClaimTypes.UserData, pair.Value.ToString(), ClaimValueTypes.Integer));
                            break;
                        default:
                            claims.Add(new Claim(claimType, pair.Value.ToString(), ClaimValueTypes.String));
                            break;
                    }
                }
            }

            subject.AddClaims(claims);
            return new ClaimsPrincipal(subject);
        }

    }

}