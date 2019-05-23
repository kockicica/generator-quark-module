namespace <%= options.moduleName %>.Core.Auth {

    public interface ITokenIssuer {

        string Create(object user);
        string Refresh(string token);

    }

}