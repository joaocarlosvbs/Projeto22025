using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto22025.Data;
using Projeto22025.Models;
using Microsoft.AspNetCore.Hosting; // Para IWebHostEnvironment
using Microsoft.AspNetCore.Authorization; // Para [Authorize]

namespace Projeto22025.Controllers
{
    [Authorize(Roles = "Almoxarife")] // Protege o controller inteiro
    public class ProdutosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // Para salvar imagens

        // Construtor atualizado para receber os dois serviços
        public ProdutosController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Produtos
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Produtos.Include(p => p.Categoria);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Produtos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var produto = await _context.Produtos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (produto == null) return NotFound();

            return View(produto);
        }

        // GET: Produtos/Create
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome");
            return View();
        }

        // POST: Produtos/Create (Versão corrigida com Upload)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Produto produto)
        {
            // Removemos a validação do objeto Categoria, pois só precisamos do CategoriaId
            ModelState.Remove("Categoria");

            if (ModelState.IsValid)
            {
                // --- Lógica de Upload de Imagem ---
                if (produto.ImagemUpload != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string pastaImagens = Path.Combine(wwwRootPath, "imagens", "produtos");

                    if (!Directory.Exists(pastaImagens))
                        Directory.CreateDirectory(pastaImagens);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(produto.ImagemUpload.FileName);
                    string filePath = Path.Combine(pastaImagens, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await produto.ImagemUpload.CopyToAsync(fileStream);
                    }
                    produto.ImagemUrl = "/imagens/produtos/" + fileName;
                }
                // --- Fim da Lógica de Upload ---

                _context.Add(produto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", produto.CategoriaId);
            return View(produto);
        }

        // GET: Produtos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", produto.CategoriaId);
            return View(produto);
        }

        // POST: Produtos/Edit/5 (Precisamos adicionar a lógica de imagem aqui também)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Produto produto)
        {
            if (id != produto.Id) return NotFound();

            ModelState.Remove("Categoria");

            if (ModelState.IsValid)
            {
                try
                {
                    // (Aqui você também precisaria adicionar a lógica de upload
                    // para atualizar a imagem, se uma nova foi enviada)

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

        // GET: Produtos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var produto = await _context.Produtos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (produto == null) return NotFound();

            return View(produto);
        }

        // POST: Produtos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null)
            {
                // (Aqui você também precisaria deletar o arquivo de imagem da pasta wwwroot)
                _context.Produtos.Remove(produto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    } // <-- Verifique se esta chave ( } ) está fechando a CLASSE
} // <-- Verifique se esta chave ( } ) está fechando o NAMESPACE