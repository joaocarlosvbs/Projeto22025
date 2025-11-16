using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto22025.Data;
using Projeto22025.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace Projeto22025.Controllers
{
    [Authorize]
    public class ProdutosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProdutosController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString)
        {
            // 1. Começa a consulta
            var produtos = from p in _context.Produtos.Include(p => p.Categoria)
                           select p;

            // 2. Se a 'searchString' não for nula, aplica o filtro 'Where'
            if (!String.IsNullOrEmpty(searchString))
            {
                produtos = produtos.Where(s => s.Nome.Contains(searchString)
                                            || s.Descricao.Contains(searchString));
            }

            // 3. Guarda o termo de busca para mostrar na View
            ViewData["FiltroAtual"] = searchString;

            // 4. Executa a consulta
            return View(await produtos.OrderBy(p => p.Nome).ToListAsync());
        }

        [Authorize(Roles = "Almoxarife, Servidor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var produto = await _context.Produtos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (produto == null) return NotFound();

            return View(produto);
        }

        [Authorize(Roles = "Almoxarife")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Criar Produto";
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Almoxarife")]
        public async Task<IActionResult> Create(Produto produto)
        {
            ModelState.Remove("Categoria");

            if (ModelState.IsValid)
            {
                if (produto.ImagemUpload != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(produto.ImagemUpload.FileName).ToLowerInvariant();

                    if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImagemUpload", "Por favor, envie apenas arquivos de imagem (jpg, png, gif).");
                        ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", produto.CategoriaId);
                        return View(produto);
                    }

                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string pastaImagens = Path.Combine(wwwRootPath, "imagens", "produtos");

                    if (!Directory.Exists(pastaImagens))
                        Directory.CreateDirectory(pastaImagens);

                    string fileName = Guid.NewGuid().ToString() + extension;
                    string filePath = Path.Combine(pastaImagens, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await produto.ImagemUpload.CopyToAsync(fileStream);
                    }
                    produto.ImagemUrl = "/imagens/produtos/" + fileName;
                }

                _context.Add(produto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", produto.CategoriaId);
            return View(produto);
        }

        [Authorize(Roles = "Almoxarife")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();

            ViewData["Title"] = "Editar Produto";
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", produto.CategoriaId);
            return View(produto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Almoxarife")]
        public async Task<IActionResult> Edit(int id, Produto produto)
        {
            if (id != produto.Id) return NotFound();

            ModelState.Remove("Categoria");

            if (produto.ImagemUpload != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(produto.ImagemUpload.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImagemUpload", "Arquivo de imagem inválido.");
                    ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", produto.CategoriaId);
                    return View(produto);
                }

                string wwwRootPath = _hostEnvironment.WebRootPath;
                string pastaImagens = Path.Combine(wwwRootPath, "imagens", "produtos");
                string fileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(pastaImagens, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await produto.ImagemUpload.CopyToAsync(fileStream);
                }

                if (!string.IsNullOrEmpty(produto.ImagemUrl))
                {
                    string oldFilePath = Path.Combine(wwwRootPath, produto.ImagemUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                produto.ImagemUrl = "/imagens/produtos/" + fileName;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Produtos.Any(e => e.Id == produto.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", produto.CategoriaId);
            return View(produto);
        }

        [Authorize(Roles = "Almoxarife")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var produto = await _context.Produtos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (produto == null) return NotFound();

            ViewData["Title"] = "Deletar Produto";
            return View(produto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Almoxarife")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null)
            {
                if (!string.IsNullOrEmpty(produto.ImagemUrl))
                {
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, produto.ImagemUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}