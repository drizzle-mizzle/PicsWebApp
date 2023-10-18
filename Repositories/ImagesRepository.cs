using Microsoft.EntityFrameworkCore;
using PicsWebApp.Data;
using PicsWebApp.Interfaces;
using PicsWebApp.Models.Database;

namespace PicsWebApp.Repositories
{
    public class ImagesRepository : IRepository<Image>
    {
        private readonly AppDbContext _db;
        public ImagesRepository(AppDbContext db)
        {
            _db = db;
        }

        public List<Image> All()
            => _db.Images.ToList();

        public Image? GetById(ulong id)
            => _db.Images.FirstOrDefault(i => i.Id == id);

        public async Task<bool> AddAsync(Image image)
        {
            await _db.Images.AddAsync(image);
            int result = await _db.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAsync(Image image)
        {
            _db.Images.Remove(image);
            int result = await _db.SaveChangesAsync();

            return result > 0;
        }
    }
}
