using System.Text.Json;
using API_Project1.Entities;
using API_Project1.Models;

namespace API_Project1.Interfaces
{
    public interface IInvoiceDetailService
    {
        Task<string> GetInvoiceDetailAsync(int currentPage);
        Task SaveDetailToDatabaseAsync(List<InvoiceDetailEntity> invoices);
        List<InvoiceDetailEntity> ConvertJsonToInvoiceDetail(List<JsonElement> dataList);
        Task<string> GetDataDetailAsync(int currentPage);
    }
}
