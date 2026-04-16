using Lab06.Services;
using Lab06.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Lab06.Controllers;

public class HomeController : Controller
{
    private readonly IArticleService _articleService;
    private readonly ICategoryService _categoryService;

    public HomeController(IArticleService articleService, ICategoryService categoryService)
    {
        _articleService = articleService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var recentArticles = await _articleService.GetPagedAsync(1, 3, cancellationToken);

        var articleViewModels = recentArticles.Select(a => new ArticleViewModel
        {
            Id = a.Id,
            Title = a.Title,
            Content = a.Content,
            PublishedAt = a.PublishedAt,
            CategoryName = a.Category?.Name ?? "N/A",
            AuthorName = a.Author?.FullName ?? "N/A",
            ImagePath = a.ImagePath
        }).ToList();

        var totalArticles = await _articleService.CountAsync(cancellationToken);

        var categories = await _categoryService.GetAllAsync(cancellationToken);
        var totalCategories = categories.Count;

        var viewModel = new HomeViewModel
        {
            RecentArticles = articleViewModels,
            TotalArticles = totalArticles,
            TotalCategories = totalCategories
        };

        return View(viewModel);
    }
}