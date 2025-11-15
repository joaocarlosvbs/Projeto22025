using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto22025.Data;
using Projeto22025.Models;
using Microsoft.AspNetCore.Hosting; // Para IWebHostEnvironment
using Microsoft.AspNetCore.Authorization; // Para [Authorize]
using System.IO; // Para Path e File

namespace Projeto22025.Controllers
{
    // Controller com permissões granulares (Servidor só pode ver)
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

        // GET: Produtos (Permitido para todos os logados)
        [AllowAnonymous] // Vamos permitir que mesmo não logados vejam (ou use [Authorize(Roles="Almoxarife, Servidor")])
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Produtos.Include(p => p.Categoria);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Produtos/Details/5 (Permitido para todos os logados)
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

        // GET: Produtos/Create (Apenas Almoxarife)
        [Authorize(Roles = "Almoxarife")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Criar Produto";
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome");
            return View();
        }

        // POST: Produtos/Create (Apenas Almoxarife)
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
                    // Validação de tipo de arquivo
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(produto.ImagemUpload.FileName).ToLowerInvariant();

                    if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImagemUpload", "Por favor, envie apenas arquivos de imagem (jpg, png, gif).");
                        ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", produto.CategoriaId);
                        return View(produto);
                    }

                    // Salvar o arquivo
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

        // GET: Produtos/Edit/5 (Apenas Almoxarife)
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

        // POST: Produtos/Edit/5 (Apenas Almoxarife)
        // --- ESTE É O MÉTODO CORRIGIDO ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Almoxarife")]
        public async Task<IActionResult> Edit(int id, Produto produto)
        {
            if (id != produto.Id) return NotFound();

            ModelState.Remove("Categoria");

            // --- INÍCIO DA MUDANÇA (Lógica de Upload no Edit) ---
            if (produto.ImagemUpload != null)
            {
                // 1. Validar a nova imagem
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(produto.ImagemUpload.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImagemUpload", "Arquivo de imagem inválido.");
                    ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", produto.CategoriaId);
                    return View(produto);
                }

                // 2. Salvar a nova imagem
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string pastaImagens = Path.Combine(wwwRootPath, "imagens", "produtos");
                string fileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(pastaImagens, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await produto.ImagemUpload.CopyToAsync(fileStream);
                }

                // 3. Deletar a imagem antiga (se existir)
                if (!string.IsNullOrEmpty(produto.ImagemUrl))
                {
                    // TrimStart('/') é crucial para o Path.Combine funcionar
                    string oldFilePath = Path.Combine(wwwRootPath, produto.ImagemUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // 4. Atualizar o Model com o caminho da nova imagem
                produto.ImagemUrl = "/imagens/produtos/" + fileName;
            }
            // --- FIM DA MUDANÇA ---

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

        // GET: Produtos/Delete/5 (Apenas Almoxarife)
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

        // POST: Produtos/Delete/5 (Apenas Almoxarife)
        // --- ESTE É O SEGUNDO MÉTODO CORRIGIDO ---
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Almoxarife")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null)
            {
                // --- INÍCIO DA MUDANÇA (Deletar arquivo de imagem) ---
                if (!string.IsNullOrEmpty(produto.ImagemUrl))
                {
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, produto.ImagemUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                // --- FIM DA MUDANÇA ---

                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}