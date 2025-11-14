// Em RelatoriosController.cs
using Microsoft.AspNetCore.Mvc;
using Projeto22025.Data; // <-- Para a classe Consultas
//using Rotativa.AspNetCore; // <-- Para o PDF
using ClosedXML.Excel; // <-- Para o Excel
using System.IO; // <-- Para o Excel
using Microsoft.AspNetCore.Authorization;

namespace Projeto22025.Controllers
{
    [Authorize(Roles = "Almoxarife")]
    public class RelatoriosController : Controller
    {
        // Injeta sua classe de Consultas
        private readonly Consultas _consultas;

        public RelatoriosController(Consultas consultas)
        {
            _consultas = consultas;
        }

        // --- AÇÃO 1: Mostrar o Relatório na Tela ---
        public async Task<IActionResult> ConsumoPorSetor()
        {
            var dados = await _consultas.GetConsumoPorSetorAsync();
            return View(dados); // Crie esta View (passo 5.5)
        }

        
        // --- Exportar para Excel ---
        public async Task<IActionResult> ExportarConsumoExcel()
        {
            var dados = await _consultas.GetConsumoPorSetorAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Consumo por Setor");
                worksheet.Cell(1, 1).Value = "Setor";
                worksheet.Cell(1, 2).Value = "Total Consumido";
                worksheet.Row(1).Style.Font.Bold = true;

                int row = 2;
                foreach (var item in dados)
                {
                    worksheet.Cell(row, 1).Value = item.Grupo;
                    worksheet.Cell(row, 2).Value = item.Total;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ConsumoPorSetor.xlsx");
                }
            }
        }
    }
}