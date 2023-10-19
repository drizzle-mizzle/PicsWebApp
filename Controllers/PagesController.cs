using Microsoft.AspNetCore.Mvc;
using PicsWebApp.Data;
using PicsWebApp.Repositories;
using static PicsWebApp.Services.CommonService;

namespace PicsWebApp.Controllers
{
    [Controller]
    public class PagesController : Controller
    {
        private readonly AppDbContext _db;
        protected internal readonly UserRepository usersRepo;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly IConfiguration _configuration;

        public PagesController(AppDbContext db, IConfiguration configuration, IWebHostEnvironment appEnvironment)
        {
            _db = db;
            usersRepo = new(_db);
            _configuration = configuration;
            _appEnvironment = appEnvironment;
        }

        [HttpGet]
        public IActionResult Home()
        {
            var users = _db.Users.ToList();
            return View("Home", users);
        }

        /// <summary>
        /// Страничка пользователя
        /// </summary>
        [HttpGet]
        public IActionResult UserProfile(ulong id, string? message = null, string? token = null)
        {
            var currentUser = this.TryToAuthorizeUser();
            if (currentUser is null)
                return StatusCode(401);

            var pageOwnerUser = usersRepo.GetById(id);
            if (pageOwnerUser is null)
                return StatusCode(404);

            bool isMyPage = pageOwnerUser.Id == currentUser.Id;
            ViewBag.IsMyFriend = isMyPage || currentUser.HasUserInFriendlist(pageOwnerUser);
            ViewBag.ImHisFriend = isMyPage || pageOwnerUser.HasUserInFriendlist(currentUser);
            ViewBag.IsMyPage = isMyPage;

            ViewBag.Message = message;
            ViewBag.ImagesDir = $"/{_configuration["ImagesRelPath"]}";

            if (token is not null)
                Response.Headers.Add("Authorization", token);

            return View("UserProfile", pageOwnerUser);
        }

        [HttpGet]
        public IActionResult Register()
        {
            var currentUser = this.TryToAuthorizeUser();
            if (currentUser is null)
                return View();
            else
                return View("SignIn", currentUser);
        }

        [HttpGet]
        public IActionResult Login()
        {
            var currentUser = this.TryToAuthorizeUser();
            if (currentUser is null)
                return View();
            else
                return View("SignIn", currentUser);
        }
    }
}