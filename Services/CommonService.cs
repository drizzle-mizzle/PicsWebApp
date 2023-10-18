using Microsoft.AspNetCore.Mvc;
using PicsWebApp.Controllers;
using PicsWebApp.Controllers.Api;
using PicsWebApp.Models.Database;
using PicsWebApp.Repositories;
using System.Text;

namespace PicsWebApp.Services
{
    public static class CommonService
    {
        public static KeyValuePair<string,string> GetUserLoginAndPassword(string tokenCookie)
        {
            try
            {
                var loginPass = Encoding.UTF8.GetString(Convert.FromBase64String(tokenCookie)).Split(":");
                return new(loginPass[0], loginPass[1]);
            }
            catch
            {
                return new(string.Empty, string.Empty);
            }
        }

        public static User? TryToAuthorizeUser(this UserController controller)
            => AuthorizeUser(controller, controller.usersRepo);

        public static User? TryToAuthorizeUser(this PagesController controller)
            => AuthorizeUser(controller, controller.usersRepo);

        private static User? AuthorizeUser(Controller controller, UserRepository usersRepo)
        {
            var tokenCookie = controller.Request.Cookies["token"];
            if (tokenCookie is null)
            {
                var bearer = controller.Request.Headers.Authorization.SingleOrDefault();
                if (bearer is not null && bearer.StartsWith("Bearer") && bearer.Split(" ").LastOrDefault() is string token)
                    tokenCookie = token;
            }
            if (tokenCookie is null)
            {
                return null;
            }

            var loginPass = GetUserLoginAndPassword(tokenCookie);

            string login = loginPass.Key;
            string password = loginPass.Value;

            var user = usersRepo.All().FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user is null && controller.HttpContext.Request.Cookies["userId"] is not null)
                controller.HttpContext.Response.Cookies.Delete("userId");

            return user;
        }
    }
}
