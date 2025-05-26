using System.Text.Json;
using API_Project1.Entities;
using API_Project1.Interfaces;
using API_Project1.Models;
using API_Project1.Services;
using Microsoft.EntityFrameworkCore;

namespace API_Project1.Services
{
    public class GetDataService : IGetDataService
    {

        private readonly ITokenService _tokenService;
        private readonly IUserInfoService _userInfoService;
        private readonly IInvoiceListService _invoiceListService;
        private readonly IInvoiceDetailService _invoiceDetailService;
        private readonly AppDbContext _dbContext;


        public GetDataService(
            ITokenService tokenService, 
            IUserInfoService userInfoService, 
            IInvoiceListService invoiceListService, 
            IInvoiceDetailService invoiceDetailService,
            AppDbContext dbContext)
        {
            _tokenService = tokenService;
            _userInfoService = userInfoService;
            _invoiceListService = invoiceListService;
            _invoiceDetailService = invoiceDetailService;
            _dbContext = dbContext;
        }


        public async Task<List<InvoiceListEntity>> GetDataAsync(int currentPage = 0)
        {
            var listJson = await _invoiceListService.GetDataListAsync(currentPage);
            var detailJson = await _invoiceDetailService.GetDataDetailAsync(currentPage);
            //lấy kết quả trả về từ hai Service gọi invoice list và detail (JSON)


            using var listDoc = JsonDocument.Parse(listJson);
            using var detailDoc = JsonDocument.Parse(detailJson);
            //parse sang JsonDocument
            //using: tự động dispose sau khi dùng xóng


            var listRoot = listDoc.RootElement;
            var detailRoot = detailDoc.RootElement;
            //lấy phần gốc của JSON

            var listModels = _invoiceListService.ConvertJsonToInvoiceList(listRoot);
            var detailModels = _invoiceDetailService.ConvertJsonToInvoiceDetail(detailRoot.EnumerateArray().ToList());
            //chuyển listRoot thành danh sách các đối tượng InvoiceListEntity
            //detailRoot.EnumerateArray().ToList() là cách duyệt qua từng phần tử nếu detailRoot là một mảng JSON


            await _invoiceListService.SaveListToDatabaseAsync(listModels);
            await _invoiceDetailService.SaveDetailToDatabaseAsync(detailModels);
            //gọi các hàm đẩy dữ liệu vào db

            return listModels;
        }

        public async Task SaveListAndDetailWithTransactionAsync(List<InvoiceListEntity> lists, List<InvoiceDetailEntity> details)
            //nhập vào hai danh sách list và detail, từ hai Entity
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            //tạo transaction mới từ DbContext
            //nếu qtrinh xử lý có lỗi, transaction được rollback để không lưu bất kỳ thay đổi nào vào db
            try
            {
                await _invoiceListService.SaveListToDatabaseAsync(lists);
                await _invoiceDetailService.SaveDetailToDatabaseAsync(details);
                //gọi hai hàm lưu dữ liệu

                await transaction.CommitAsync();
                //nếu thành công mới xác nhận giao  dịch bằng CommitAsync(), thực sự lưu vào db, 
            }
            catch
            {
                await transaction.RollbackAsync();
                //nếu có lỗi thì sẽ rollback
                throw;
            }
        }
    }
}

