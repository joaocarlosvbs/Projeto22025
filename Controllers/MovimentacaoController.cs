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

        // GET: /Movimentacao/RegistrarEntrada
        public IActionResult RegistrarEntrada()
        {
            ViewData["FornecedorId"] = new SelectList(_context.Fornecedores, "Id", "RazaoSocial");
            ViewData["ProdutoId"] = new SelectList(_context.Produtos.Where(p => p.EstoqueAtual >= 0), "Id", "Nome"); // Apenas exemplo
            return View();
        }

        // POST: /Movimentacao/RegistrarEntrada
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarEntrada(Movimentacao movimentacao)
        {
            movimentacao.Tipo = "Entrada";
            movimentacao.Data = DateTime.Now;
            movimentacao.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!; // O '!' assume que não será nulo
            movimentacao.SetorId = null;

            ModelState.Remove("Setor");
            ModelState.Remove("Produto");
            ModelState.Remove("Usuario");
            ModelState.Remove("UsuarioId"); // O UsuarioId pegamos do login

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

        // (Aqui viria a lógica de RegistrarSaida, similar à de Entrada)

    } // <-- Verifique se esta chave ( } ) está fechando a CLASSE
} // <-- Verifique se esta chave ( } ) está fechando o NAMESPACE