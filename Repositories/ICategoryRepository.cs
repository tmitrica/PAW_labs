using Lab06.Models;

namespace Lab06.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
    }
}