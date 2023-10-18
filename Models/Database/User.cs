using Microsoft.EntityFrameworkCore;
using PicsWebApp.Data;
using PicsWebApp.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PicsWebApp.Models.Database
{
    public class User
    {
        [Key]
        public ulong Id { get; set; }
        public required string Login { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }

        // Картинки
        private readonly List<Image> _images = new();

        [BackingField(nameof(_images))]
        public IReadOnlyCollection<Image> Images
            => _images.AsReadOnly();

        // Друзья
        public virtual List<Friendship> FriendshipsInc { get; } = new();
        public virtual List<Friendship> FriendshipsOut { get; } = new();

        public List<Friendship> GetIncomingFriendshipRequests()
            => FriendshipsInc.Where(f => f.IsPending).ToList();

        public bool HasUserInFriendlist(User user, AppDbContext db)
            => db.Friendships.FirstOrDefault(request => request.ProposerId == Id && request.ReceiverId == user.Id) is not null;
    }
}
