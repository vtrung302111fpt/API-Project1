using System.Text.Json;
using API_Project1.Interfaces;
using API_Project1.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace API_Project1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceDetailController : ControllerBase
    {
        private readonly IInvoiceDetailService _invoiceDetailService;
        private readonly ILogger<InvoiceDetailController> _logger;
        
        public InvoiceDetailController(IInvoiceDetailService invoiceDetailService, ILogger<InvoiceDetailController> logger)
        {
            _invoiceDetailService = invoiceDetailService;
            _logger = logger;
        }

        [HttpGet("get-invoice-detail")]
        public async Task<IActionResult> GetInvoiceDetailAsync([FromQuery]int currentPage = 0)
        {
            try
            {
                var content = await _invoiceDetailService.GetDataDetailAsync(currentPage);

                // Parse để xử lý tiếp
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array)
                {
                    return BadRequest("Dữ liệu không hợp lệ, không phải mảng JSON.");
                }

                var dataList = root.EnumerateArray().Select(e => e.Clone()).ToList();

                var invoiceDetails = _invoiceDetailService.ConvertJsonToInvoiceDetail(dataList);
                //await _invoiceDetailService.SaveDetailToDatabaseAsync(List<InvoiceDetailEntity> invoices);
                await _invoiceDetailService.SaveDetailToDatabaseAsync(invoiceDetails);

                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
