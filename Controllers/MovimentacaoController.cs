using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto22025.Data;
using Projeto22025.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
        // Este método é chamado quando você clica no link do menu.
        public IActionResult RegistrarEntrada()
        {
            // Correção 2 (Boa Prática): Define o Título
            ViewData["Title"] = "Registrar Entrada";

            var model = new Movimentacao { Data = DateTime.Today };

            // Correção 3 (A Causa do Erro): Busca os dados e envia para a View
            var fornecedores = _context.Fornecedores.OrderBy(f => f.Razaosocial).ToList();
            var produtos = _context.Produtos.OrderBy(p => p.Nome).ToList();

            ViewData["FornecedorId"] = new SelectList(fornecedores, "Id", "Razaosocial");
            ViewData["ProdutoId"] = new SelectList(produtos, "Id", "Nome");

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

            // Se a validação falhar, re-popula os dropdowns e o título
            ViewData["Title"] = "Registrar Entrada";
            ViewData["FornecedorId"] = new SelectList(_context.Fornecedores, "Id", "Razaosocial", movimentacao.FornecedorId);
            ViewData["ProdutoId"] = new SelectList(_context.Produtos, "Id", "Nome", movimentacao.ProdutoId);
            return View(movimentacao);
        }

        // --- LÓGICA DE SAÍDA ---

        // GET: /Movimentacao/RegistrarSaida
        public IActionResult RegistrarSaida()
        {
            ViewData["Title"] = "Registrar Saída";
            var model = new Movimentacao { Data = DateTime.Today };
            ViewData["SetorId"] = new SelectList(_context.Setores, "Id", "Nome");
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
            movimentacao.UsuarioId = userId;
            movimentacao.FornecedorId = null;

            ModelState.Remove("Fornecedor");
            ModelState.Remove("Produto");
            ModelState.Remove("Usuario");

            if (ModelState.IsValid)
            {
                var produto = await _context.Produtos.FindAsync(movimentacao.ProdutoId);
                if (produto == null)
                {
                    ModelState.AddModelError("ProdutoId", "Produto não encontrado.");
                }
                else if (produto.EstoqueAtual < movimentacao.Quantidade)
                {
                    ModelState.AddModelError("Quantidade", $"Estoque insuficiente. Disponível: {produto.EstoqueAtual}");
                }
                else
                {
                    produto.EstoqueAtual -= movimentacao.Quantidade;
                    _context.Update(produto);
                    _context.Add(movimentacao);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Produtos");
                }
            }

            ViewData["Title"] = "Registrar Saída";
            ViewData["SetorId"] = new SelectList(_context.Setores, "Id", "Nome", movimentacao.SetorId);
            ViewData["ProdutoId"] = new SelectList(_context.Produtos.Where(p => p.EstoqueAtual > 0), "Id", "Nome", movimentacao.ProdutoId);
            return View(movimentacao);
        }
    }
}