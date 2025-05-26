using System.Text.Json;
using API_Project1.Interfaces;
using API_Project1.Models;
using API_Project1.Services;

namespace API_Project1.Services
{
    public class GetDataService : IGetDataService
    {

        private readonly ITokenService _tokenService;
        private readonly IUserInfoService _userInfoService;
        private readonly IInvoiceListService _invoiceListService;
        private readonly IInvoiceDetailService _invoiceDetailService;


        public GetDataService(ITokenService tokenService, IUserInfoService userInfoService, IInvoiceListService invoiceListService, IInvoiceDetailService invoiceDetailService)
        {
            _tokenService = tokenService;
            _userInfoService = userInfoService;
            _invoiceListService = invoiceListService;
            _invoiceDetailService = invoiceDetailService;
        }


        //public async Task GetDataAsync(int currentPage = 0)
        //{
        //    var listJson = await _invoiceListService.GetDataListAsync(currentPage);
        //    var detailJson = await _invoiceDetailService.GetDataDetailAsync(currentPage);


        //    using var listDoc = JsonDocument.Parse(listJson);
        //    using var detailDoc = JsonDocument.Parse(detailJson);

        //    var listRoot = listDoc.RootElement;
        //    var detailRoot = detailDoc.RootElement;

        //    var listModels = _invoiceListService.ConvertJsonToInvoiceList(listRoot);
        //    var detailModels = _invoiceDetailService.ConvertJsonToInvoiceDetail(detailRoot.EnumerateArray().ToList());

        //    await _invoiceListService.SaveListToDatabaseAsync(listModels);
        //    await _invoiceDetailService.SaveDetailToDatabaseAsync(detailModels);


        //}

        //public async Task SaveListAndDetailWithTransactionAsync(List<InvoiceListDataModel> lists, List<InvoiceDetailDataModel> details)
        //{
        //    using var transaction = await _dbContext.Database.BeginTransactionAsync();

        //    try
        //    {
        //        await _invoiceListService.SaveListToDatabaseAsync(lists);
        //        await _invoiceDetailService.SaveDetailToDatabaseAsync(details);

        //        await transaction.CommitAsync();
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}
    }
}

