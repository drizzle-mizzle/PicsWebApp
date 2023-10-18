using Microsoft.AspNetCore.Mvc;
using PicsWebApp.Data;
using PicsWebApp.Models.Database;
using PicsWebApp.Repositories;
using System.Text;
using static PicsWebApp.Services.CommonService;

namespace PicsWebApp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly AppDbContext _db;
        protected internal readonly UserRepository usersRepo;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly IConfiguration _configuration;

        public UserController(AppDbContext db, IConfiguration configuration, IWebHostEnvironment appEnvironment)
        {
            _db = db;
            usersRepo = new(_db);
            _configuration = configuration;
            _appEnvironment = appEnvironment;
        }

        /// <summary>
        /// Выйти из аккаунта
        /// </summary>
        /// <returns>200 и редирект на главную</returns>
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("token");
            HttpContext.Response.Cookies.Delete("userId");

            return Redirect("/");
        }

        /// <summary>
        /// Зарегистрировать пользователя
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <response code="200">OK</response>
        /// <response code="500">Не вышло сохранить пользователя</response>
        [HttpPost("create")]
        public async Task<IActionResult> CreateUserAsync([FromForm] string login, [FromForm] string password)
        {
            var user = new User()
            {
                Login = login,
                Password = password
            };
            bool success = await usersRepo.AddAsync(user);
            if (success)
                return SignIn(login, password);
            else
                return StatusCode(500);
        }

        /// <summary>
        /// Авторизовать пользователя
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <response code="200">OK</response>
        /// <response code="404">Не вышло найти пользователя</response>
        [HttpPost("auth")]
        public IActionResult SignIn([FromForm] string login, [FromForm] string password)
        {
            var authUser = usersRepo.All().FirstOrDefault(u => u.Login == login && u.Password == password);
            if (authUser is null)
                return StatusCode(404);

            string token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{password}"));
            Response.Cookies.Append("token", token);
            Response.Cookies.Append("userId", $"{authUser.Id}");

            return RedirectToAction("UserProfile", "Pages", new { id = authUser.Id });
        }

        /// <summary>
        /// Загрузить файл
        /// </summary>
        /// <param name="file">Изображение</param>
        /// <response code="200">OK</response>
        /// <response code="401">Пользователь не авторизован</response>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFileAsync([FromForm] IFormFile file)
        {
            var currentUser = this.TryToAuthorizeUser();
            if (currentUser is null)
                return StatusCode(401);

            string? message = null;
            if (file is not null && file.Length > 0)
            {
                try
                {
                    var fileName = file.FileName;
                    if (fileName.Contains(".."))
                        throw new();

                    var imgPath = $"{_appEnvironment.WebRootPath}{Path.DirectorySeparatorChar}{_configuration["ImagesRelPath"]}";
                    if (!Directory.Exists(imgPath))
                        Directory.CreateDirectory(imgPath);

                    string savePath = Path.Combine(imgPath, file.FileName);
                    using var fs = new FileStream(savePath, FileMode.Create);
                    await file.CopyToAsync(fs);

                    var imagesRepo = new ImagesRepository(_db);
                    await imagesRepo.AddAsync(new() { FileName = file.FileName, UserId = currentUser.Id });

                    message = "Изображение загружено успешно";
                }
                catch (Exception e)
                {
                    message = "Не удалось загрузить изображение" + e.ToString();
                }
            }

            return RedirectToAction("UserProfile", "Pages", new { id = currentUser.Id, message });
        }
    }
}
