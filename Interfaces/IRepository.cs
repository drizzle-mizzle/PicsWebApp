namespace PicsWebApp.Interfaces
{
    public interface IRepository<T>
    {
        List<T> All();
        T? GetById(ulong id);
        Task<bool> AddAsync(T obj);
        Task<bool> DeleteAsync(T obj);
    }
}
