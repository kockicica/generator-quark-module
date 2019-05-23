using System;
using System.Collections.Generic;

using JWT;

namespace <%= options.moduleName %>.Core.Auth {

    public class TokenIssuer : ITokenIssuer {

        private readonly string _key;
        private readonly int    _validForHours;

        public TokenIssuer(int validForHours, string key) {
            _validForHours = validForHours;
            _key = key;
        }

        public virtual string Create(object user) {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var expiry = Math.Round((DateTime.Now.AddHours(_validForHours) - unixEpoch).TotalSeconds);
            var issuedAt = Math.Round((DateTime.UtcNow - unixEpoch).TotalSeconds);
            var notBefore = Math.Round((DateTime.UtcNow.AddMonths(6) - unixEpoch).TotalSeconds);

            var payload = new Dictionary<string, object> {
                {"userId", GetUserId(user)},
                {"sub", GetUserId(user)},
                {"nbf", notBefore},
                {"iat", issuedAt},
                {"exp", expiry}
            };
            var token = JsonWebToken.Encode(payload, _key, JwtHashAlgorithm.HS256);
            return token;
        }

        public virtual string Refresh(string token) {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var expiry = Math.Round((DateTime.Now.AddHours(_validForHours) - unixEpoch).TotalSeconds);
            var issuedAt = Math.Round((DateTime.UtcNow - unixEpoch).TotalSeconds);
            var notBefore = Math.Round((DateTime.UtcNow.AddMonths(6) - unixEpoch).TotalSeconds);
            var payload = JsonWebToken.DecodeToObject<Dictionary<string, object>>(token, _key);
            payload["nbf"] = notBefore;
            payload["iat"] = issuedAt;
            payload["exp"] = expiry;

            return JsonWebToken.Encode(payload, _key, JwtHashAlgorithm.HS256);
        }

        protected virtual object GetUserId(object user) {
            return user;
        }


    }

}