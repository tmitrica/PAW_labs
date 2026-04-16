using Lab06.Data;
using Lab06.Models;

namespace Lab06.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IArticleRepository? _articleRepository;
    private IRepository<Category>? _categoryRepository;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IArticleRepository ArticleRepository
        => _articleRepository ??= new ArticleRepository(_context);

    public IRepository<Category> CategoryRepository
        => _categoryRepository ??= new Repository<Category>(_context);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
