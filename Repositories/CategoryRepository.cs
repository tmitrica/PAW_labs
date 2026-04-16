using Lab06.Models;
using Lab06.Data;
using Microsoft.EntityFrameworkCore;

namespace Lab06.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _context.Set<Category>()
                                 .FirstOrDefaultAsync(c => c.Name == name);
        }
    }
}