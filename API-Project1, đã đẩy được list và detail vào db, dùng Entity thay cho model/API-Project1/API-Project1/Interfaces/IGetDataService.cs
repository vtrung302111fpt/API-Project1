using API_Project1.Entities;

namespace API_Project1.Interfaces
{
    public interface IGetDataService
    {
        Task<List<InvoiceListEntity>> GetDataAsync(int currentPage = 0);
        Task SaveListAndDetailWithTransactionAsync(List<InvoiceListEntity> lists, List<InvoiceDetailEntity> details);
    }
}
