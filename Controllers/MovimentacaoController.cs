using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto22025.Data;
using Projeto22025.Models;
using System.Security.Claims; // Para pegar o ID do usuário logado
using Microsoft.AspNetCore.Authorization; // Para [Authorize]

namespace Projeto22025.Controllers
{
    [Authorize(Roles = "Almoxarife")]
    public class MovimentacaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovimentacaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- LÓGICA DE ENTRADA ---

        // GET: /Movimentacao/RegistrarEntrada
        public IActionResult RegistrarEntrada()
        {
            // Pré-popula a data de hoje no formulário
            var model = new Movimentacao { Data = DateTime.Today };
            ViewData["FornecedorId"] = new SelectList(_context.Fornecedores, "Id", "RazaoSocial");
            ViewData["ProdutoId"] = new SelectList(_context.Produtos, "Id", "Nome");
            return View(model);
        }

        // POST: /Movimentacao/RegistrarEntrada
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarEntrada(Movimentacao movimentacao)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "Erro ao identificar usuário.");
                return View(movimentacao);
            }

            movimentacao.Tipo = "Entrada";
            // A 'Data' vem do formulário
            movimentacao.UsuarioId = userId;
            movimentacao.SetorId = null;

            ModelState.Remove("Setor");
            ModelState.Remove("Produto");
            ModelState.Remove("Usuario");

            if (ModelState.IsValid)
            {
                var produto = await _context.Produtos.FindAsync(movimentacao.ProdutoId);
                if (produto != null)
                {
                    produto.EstoqueAtual += movimentacao.Quantidade;
                    _context.Update(produto);
                }

                _context.Add(movimentacao);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Produtos");
            }

            ViewData["FornecedorId"] = new SelectList(_context.Fornecedores, "Id", "RazaoSocial", movimentacao.FornecedorId);
            ViewData["ProdutoId"] = new SelectList(_context.Produtos, "Id", "Nome", movimentacao.ProdutoId);
            return View(movimentacao);
        }

        // --- LÓGICA DE SAÍDA ---

        // GET: /Movimentacao/RegistrarSaida
        public IActionResult RegistrarSaida()
        {
            // Pré-popula a data de hoje no formulário
            var model = new Movimentacao { Data = DateTime.Today };
            ViewData["SetorId"] = new SelectList(_context.Setores, "Id", "Nome");
            // Mostra apenas produtos que têm estoque
            ViewData["ProdutoId"] = new SelectList(_context.Produtos.Where(p => p.EstoqueAtual > 0), "Id", "Nome");
            return View(model);
        }

        // POST: /Movimentacao/RegistrarSaida
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarSaida(Movimentacao movimentacao)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "Erro ao identificar usuário.");
                return View(movimentacao);
            }

            movimentacao.Tipo = "Saída";
            // A 'Data' vem do formulário
            movimentacao.UsuarioId = userId;
            movimentacao.FornecedorId = null;

            ModelState.Remove("Fornecedor");
            ModelState.Remove("Produto");
            ModelState.Remove("Usuario");

            // Validação de estoque
            if (ModelState.IsValid)
            {
                var produto = await _context.Produtos.FindAsync(movimentacao.ProdutoId);
                if (produto == null)
                {
                    ModelState.AddModelError("ProdutoId", "Produto não encontrado.");
                }
                else if (produto.EstoqueAtual < movimentacao.Quantidade)
                {
                    // Erro se tentar tirar mais do que tem
                    ModelState.AddModelError("Quantidade", $"Estoque insuficiente. Disponível: {produto.EstoqueAtual}");
                }
                else
                {
                    // Se tudo estiver OK, subtrai o estoque e salva
                    produto.EstoqueAtual -= movimentacao.Quantidade;
                    _context.Update(produto);
                    _context.Add(movimentacao);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Produtos");
                }
            }

            // Se o ModelState for inválido (ou falhar na validação de estoque), recarrega os dropdowns e retorna
            ViewData["SetorId"] = new SelectList(_context.Setores, "Id", "Nome", movimentacao.SetorId);
            ViewData["ProdutoId"] = new SelectList(_context.Produtos.Where(p => p.EstoqueAtual > 0), "Id", "Nome", movimentacao.ProdutoId);
            return View(movimentacao);
        }
    }
}