using Microsoft.AspNetCore.Mvc;
using PicsWebApp.Data;
using PicsWebApp.Repositories;
using static PicsWebApp.Services.CommonService;

namespace PicsWebApp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendshipController : Controller
    {
        private readonly AppDbContext _db;
        protected internal readonly UserRepository usersRepo;

        public FriendshipController(AppDbContext db)
        {
            _db = db;
            usersRepo = new(_db);
        }

        /// <summary>
        /// Создать запрос дружбы
        /// </summary>
        /// <param name="proposerId">ID отправителя запроса</param>
        /// <param name="receiverId">ID добавляемого пользователя</param>
        /// <response code="200">OK</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Пользователь не найден</response>
        [HttpPost("create")]
        public async Task<IActionResult> CreateFriendshipAsync([FromForm] ulong proposerId, [FromForm] ulong receiverId)
        {
            if (!UserIsAuthorized())
                return StatusCode(401);

            if (usersRepo.GetById(proposerId) is null || usersRepo.GetById(receiverId) is null)
                return StatusCode(404);

            await _db.Friendships.AddAsync(new()
            {
                ProposerId = proposerId,
                ReceiverId = receiverId
            });

            await _db.SaveChangesAsync();

            var url = Request.Headers.Referer;
            if (url.Any())
                return Redirect(url!);
            else
                return StatusCode(200);
        }

        /// <summary>
        /// Удалить из друзей
        /// </summary>
        /// <param name="proposerId">ID отправителя запроса</param>
        /// <param name="receiverId">ID удаляемого из списка друзей пользователя</param>
        /// <response code="200">OK</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Пользователь не найден</response>
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteFriendshipAsync([FromForm] ulong proposerId, [FromForm] ulong receiverId)
        {
            if (!UserIsAuthorized())
                return StatusCode(401);

            var fs = _db.Friendships.FirstOrDefault(fs => fs.ProposerId == proposerId && fs.ReceiverId == receiverId);
            if (fs is null)
                return StatusCode(404);

            _db.Remove(fs);
            await _db.SaveChangesAsync();

            var url = Request.Headers.Referer;
            if (url.Any())
                return Redirect(url!);
            else
                return StatusCode(200);
        }

        /// <summary>
        /// Принять входящий запрос дружбы
        /// </summary>
        /// <param name="id">ID запроса</param>
        /// <response code="200">OK</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Пользователь не найден</response>
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptFriendshipAsync([FromForm] ulong id)
        {
            if (!UserIsAuthorized())
                return StatusCode(401);

            var fs = await _db.Friendships.FindAsync(id);
            if (fs is null)
                return StatusCode(404);

            await _db.Friendships.AddAsync(new()
            {
                ProposerId = fs.ReceiverId,
                ReceiverId = fs.ProposerId,
                IsPending = false
            });
            fs.IsPending = false;

            await _db.SaveChangesAsync();

            var url = Request.Headers.Referer;
            if (url.Any())
                return Redirect(url!);
            else
                return StatusCode(200);
        }

        /// <summary>
        /// Отклонить входящий запрос дружбы
        /// </summary>
        /// <param name="id">ID запроса</param>
        /// <response code="200">OK</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Пользователь не найден</response>
        [HttpPost("decline")]
        public async Task<IActionResult> DeclineFriendshipAsync([FromForm] ulong id)
        {
            if (!UserIsAuthorized())
                return StatusCode(401);

            var fs = await _db.Friendships.FindAsync(id);
            if (fs is null)
                return StatusCode(404);

            fs.IsPending = false;

            await _db.SaveChangesAsync();

            var url = Request.Headers.Referer;
            if (url.Any())
                return Redirect(url!);
            else
                return StatusCode(200);
        }

        [NonAction]
        private bool UserIsAuthorized()
        {
            var tokenCookie = Request.Cookies["token"];
            if (tokenCookie is null)
            {
                var bearer = Request.Headers.Authorization.SingleOrDefault();
                if (bearer is not null && bearer.StartsWith("Bearer") && bearer.Split(" ").LastOrDefault() is string token)
                    tokenCookie = token;
            }
            if (tokenCookie is null) return false;

            var loginPass = GetUserLoginAndPassword(tokenCookie);
            var authUser = usersRepo.All().FirstOrDefault(u => u.Login == loginPass.Key && u.Password == loginPass.Value);

            return authUser is not null;
        }
    }
}
