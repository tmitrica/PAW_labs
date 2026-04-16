using Lab06.Models;
using Lab06.Services;
using Lab06.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims; 

namespace Lab06.Controllers;

public class ArticlesController : Controller
{
    private readonly IArticleService _articleService;
    private readonly ICategoryService _categoryService;
    private readonly IWebHostEnvironment _env;

    public ArticlesController(
        IArticleService articleService,
        ICategoryService categoryService,
        IWebHostEnvironment env)
    {
        _articleService = articleService;
        _categoryService = categoryService;
        _env = env;
    }

    // GET: /Articles
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        int pageSize = 5;

        var totalArticles = await _articleService.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalArticles / pageSize);

        var articles = await _articleService.GetPagedAsync(page, pageSize, cancellationToken);

        var viewModels = articles.Select(a => new ArticleViewModel
        {
            Id = a.Id,
            Title = a.Title,
            Content = a.Content,
            PublishedAt = a.PublishedAt,
            CategoryName = a.Category?.Name ?? "N/A",
            AuthorName = a.Author?.FullName ?? "N/A",
            ImagePath = a.ImagePath
        }).ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(viewModels);
    }

    // GET: /Articles/Details/5
    public async Task<IActionResult> Details(int? id, CancellationToken cancellationToken)
    {
        if (id == null)
            return NotFound();

        var article = await _articleService.GetByIdAsync(id.Value, cancellationToken);
        if (article == null)
            return NotFound();

        var viewModel = new ArticleViewModel
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            PublishedAt = article.PublishedAt,
            CategoryName = article.Category?.Name ?? "N/A",
            AuthorName = article.Author?.FullName ?? "N/A",
            ImagePath = article.ImagePath
        };

        return View(viewModel);
    }

    // GET: /Articles/Create
    [Authorize] 
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var viewModel = new CreateArticleViewModel();
        await LoadDropdownsAsync(viewModel, cancellationToken);
        return View(viewModel);
    }

    // POST: /Articles/Create
    [HttpPost]
    [Authorize] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateArticleViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(viewModel, cancellationToken);
            return View(viewModel);
        }

        var article = new Article
        {
            Title = viewModel.Title,
            Content = viewModel.Content,
            CategoryId = viewModel.CategoryId,
            AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };

        if (viewModel.Upload != null)
        {
            var fileName = Path.GetFileName(viewModel.Upload.FileName);
            var savePath = Path.Combine(_env.WebRootPath, "images", fileName);
            using var stream = System.IO.File.Create(savePath);
            await viewModel.Upload.CopyToAsync(stream, cancellationToken);
            article.ImagePath = $"/images/{fileName}";
        }

        await _articleService.AddAsync(article, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Articles/Edit/5
    [Authorize] 
    public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
    {
        if (id == null)
            return NotFound();

        var article = await _articleService.GetByIdAsync(id.Value, cancellationToken);
        if (article == null)
            return NotFound();

        if (!IsOwnerOrAdmin(article))
            return Forbid();

        var viewModel = new EditArticleViewModel
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            CategoryId = article.CategoryId,
            ExistingImagePath = article.ImagePath
        };

        await LoadDropdownsAsync(viewModel, cancellationToken);
        return View(viewModel);
    }

    // POST: /Articles/Edit/5
    [HttpPost]
    [Authorize] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditArticleViewModel viewModel, CancellationToken cancellationToken)
    {
        if (id != viewModel.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(viewModel, cancellationToken);
            return View(viewModel);
        }

        var article = await _articleService.GetByIdAsync(id, cancellationToken);
        if (article == null)
            return NotFound();

        if (!IsOwnerOrAdmin(article))
            return Forbid();

        article.Title = viewModel.Title;
        article.Content = viewModel.Content;
        article.CategoryId = viewModel.CategoryId;

        if (viewModel.Upload != null)
        {
            var fileName = Path.GetFileName(viewModel.Upload.FileName);
            var savePath = Path.Combine(_env.WebRootPath, "images", fileName);
            using var stream = System.IO.File.Create(savePath);
            await viewModel.Upload.CopyToAsync(stream, cancellationToken);
            article.ImagePath = $"/images/{fileName}";
        }
        else if (viewModel.ExistingImagePath != null)
        {
            article.ImagePath = viewModel.ExistingImagePath;
        }

        await _articleService.UpdateAsync(article, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Articles/Delete/5
    [Authorize] 
    public async Task<IActionResult> Delete(int? id, CancellationToken cancellationToken)
    {
        if (id == null)
            return NotFound();

        var article = await _articleService.GetByIdAsync(id.Value, cancellationToken);
        if (article == null)
            return NotFound();

        if (!IsOwnerOrAdmin(article))
            return Forbid();

        var viewModel = new ArticleViewModel
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            PublishedAt = article.PublishedAt,
            CategoryName = article.Category?.Name ?? "N/A",
            AuthorName = article.Author?.FullName ?? "N/A",
        };

        return View(viewModel);
    }

    // POST: /Articles/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, string confirmTitle, CancellationToken cancellationToken)
    {
        var article = await _articleService.GetByIdAsync(id, cancellationToken);
        if (article == null)
            return NotFound();

        if (!IsOwnerOrAdmin(article))
            return Forbid();

        if (string.IsNullOrWhiteSpace(confirmTitle) || !article.Title.Equals(confirmTitle.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            var viewModel = new ArticleViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                PublishedAt = article.PublishedAt,
                CategoryName = article.Category?.Name ?? "N/A",
                AuthorName = article.Author?.FullName ?? "N/A",
            };

            ViewBag.ErrorMessage = "Titlul introdus nu corespunde! Încearcă din nou.";
            return View("Delete", viewModel);
        }

        await _articleService.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDropdownsAsync(CreateArticleViewModel viewModel, CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        viewModel.Categories = categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
    }

    private bool IsOwnerOrAdmin(Article article)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return article.AuthorId == userId || User.IsInRole("Admin");
    }
}