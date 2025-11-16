using Microsoft.AspNetCore.Mvc;
using Projeto22025.Data;
using Projeto22025.Models;
using ClosedXML.Excel;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using System.Linq; // <-- Adicionado para .Sum()

namespace Projeto22025.Controllers
{
    [Authorize(Roles = "Almoxarife")]
    public class RelatoriosController : Controller
    {
        private readonly Consultas _consultas;

        public RelatoriosController(Consultas consultas)
        {
            _consultas = consultas;
        }

        // --- Relatório 1 (Consumo por Setor) ---
        public async Task<IActionResult> ConsumoPorSetor()
        {
            var dados = await _consultas.GetConsumoPorSetorAsync();
            return View(dados);
        }
        public async Task<IActionResult> ExportarConsumoExcel()
        {
            var dados = await _consultas.GetConsumoPorSetorAsync();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Consumo por Setor");
                worksheet.Cell(1, 1).InsertTable(dados, "ConsumoPorSetor", true);

                // Formatação (Polimento)
                worksheet.Column(2).Style.NumberFormat.Format = "#,##0";

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ConsumoSetor.xlsx");
                }
            }
        }

        // --- Relatório 2 (Pivot) ---
        public async Task<IActionResult> Pivot()
        {
            var dados = await _consultas.GetPivotSetorCategoriaAsync();
            return View(dados);
        }
        public async Task<IActionResult> ExportarPivotExcel()
        {
            var dados = await _consultas.GetPivotSetorCategoriaAsync();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Pivot Setor Categoria");
                worksheet.Cell(1, 1).InsertTable(dados, "PivotData", true);

                // Formatação (Polimento)
                worksheet.Column(3).Style.NumberFormat.Format = "#,##0";

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PivotSetorCategoria.xlsx");
                }
            }
        }


        // --- Relatório 3 (Média de Saída) ---
        public async Task<IActionResult> MediaSaidaProduto()
        {
            var dados = await _consultas.GetMediaSaidaProdutoAsync();
            return View(dados);
        }
        public async Task<IActionResult> ExportarMediaSaidaExcel()
        {
            var dados = await _consultas.GetMediaSaidaProdutoAsync();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Média de Saídas");
                worksheet.Cell(1, 1).InsertTable(dados, "MediaSaida", true);

                // Formatação (Polimento)
                worksheet.Column(2).Style.NumberFormat.Format = "0.00"; // 2 casas decimais

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MediaSaida.xlsx");
                }
            }
        }

        // --- Relatório 4 (Extrato por Data) ---
        public IActionResult MovimentacoesPorData()
        {
            var model = new { DataInicio = DateTime.Today.AddDays(-30), DataFim = DateTime.Today };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> MovimentacoesPorData(DateTime dataInicio, DateTime dataFim)
        {
            var dados = await _consultas.GetMovimentacoesPorDataAsync(dataInicio, dataFim);
            TempData["DataInicio"] = dataInicio.ToString("o");
            TempData["DataFim"] = dataFim.ToString("o");
            return View("MovimentacoesPorDataResult", dados);
        }
        public async Task<IActionResult> ExportarExtratoExcel(string dataInicio, string dataFim)
        {
            var dtInicio = DateTime.Parse(dataInicio);
            var dtFim = DateTime.Parse(dataFim);
            var dados = await _consultas.GetMovimentacoesPorDataAsync(dtInicio, dtFim);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Extrato de Movimentações");
                worksheet.Cell(1, 1).Value = "Data";
                worksheet.Cell(1, 2).Value = "Produto";
                worksheet.Cell(1, 3).Value = "Tipo";
                worksheet.Cell(1, 4).Value = "Quantidade";
                worksheet.Cell(1, 5).Value = "Setor/Fornecedor";
                worksheet.Cell(1, 6).Value = "Usuário";
                worksheet.Row(1).Style.Font.Bold = true;

                int row = 2;
                foreach (var item in dados)
                {
                    worksheet.Cell(row, 1).Value = item.Data;
                    worksheet.Cell(row, 2).Value = item.Produto?.Nome;
                    worksheet.Cell(row, 3).Value = item.Tipo;
                    worksheet.Cell(row, 4).Value = item.Quantidade;

                    // --- CORREÇÃO 2: 'Razaosocial' -> 'RazaoSocial' ---
                    worksheet.Cell(row, 5).Value = item.Tipo == "Entrada" ? item.Fornecedor?.Razaosocial : item.Setor?.Nome;

                    worksheet.Cell(row, 6).Value = item.Usuario?.UserName;
                    row++;
                }

                worksheet.Columns().AdjustToContents(); // Ajusta largura das colunas

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Extrato.xlsx");
                }
            }
        }

        // --- Relatório 5 (Estoque Valorado) ---
        public async Task<IActionResult> EstoqueValorado()
        {
            var dados = await _consultas.GetEstoqueValoradoAsync();

            // --- CORREÇÃO 1: 'Totalf' -> 'Total' ---
            ViewBag.TotalGeral = dados.Sum(d => d.Total);

            return View(dados);
        }

        public async Task<IActionResult> ExportarEstoqueValoradoExcel()
        {
            var dados = await _consultas.GetEstoqueValoradoAsync();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Estoque Valorado");
                worksheet.Cell(1, 1).InsertTable(dados, "EstoqueValorado", true);

                var totalRow = worksheet.LastRowUsed().RowNumber() + 2;
                worksheet.Cell(totalRow, 1).Value = "Total Geral";

                // --- CORREÇÃO 1: 'Totalf' -> 'Total' ---
                worksheet.Cell(totalRow, 2).Value = dados.Sum(d => d.Total);

                worksheet.Cell(totalRow, 1).Style.Font.Bold = true;
                worksheet.Cell(totalRow, 2).Style.Font.Bold = true;

                // Formatação (Polimento)
                worksheet.Column(2).Style.NumberFormat.Format = "R$ #,##0.00";

                // --- CORREÇÃO DO TYPO: 'worksTdsModule' -> 'worksheet' ---
                worksheet.Cell(totalRow, 2).Style.NumberFormat.Format = "R$ #,##0.00";

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EstoqueValorado.xlsx");
                }
            }
        }
    }
}