using Lab06.Models;
using Lab06.Repositories;

namespace Lab06.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {

        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        return categories.ToList();
    }

}