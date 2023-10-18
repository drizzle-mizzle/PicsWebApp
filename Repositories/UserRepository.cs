using Microsoft.EntityFrameworkCore;
using PicsWebApp.Data;
using PicsWebApp.Interfaces;
using PicsWebApp.Models.Database;

namespace PicsWebApp.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public List<User> All()
            => _db.Users.Include(u => u.Images).Include(u => u.FriendshipsInc).ToList();

        public User? GetById(ulong id)
            => _db.Users.Include(u => u.Images).Include(u => u.FriendshipsInc).FirstOrDefault(u => u.Id == id);

        public async Task<bool> AddAsync(User user)
        {
            await _db.Users.AddAsync(user);
            int result = await _db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(User user)
        {
            _db.Users.Remove(user);
            int result = await _db.SaveChangesAsync();

            return result > 0;
        }
    }
}
