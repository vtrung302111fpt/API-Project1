using System.Net.Http.Headers;
using System.Text.Json;
using API_Project1.Entities;
using API_Project1.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace API_Project1.Services
{
    public class InvoiceListService: IInvoiceListService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly IUserInfoService _userInfoService;
        private readonly AppDbContext _dbContext;

        public InvoiceListService(
            IHttpClientFactory httpClientFactory,
            ITokenService tokenService,
            IUserInfoService userInfoService,
            AppDbContext dbContext)
        {
            _httpClient = httpClientFactory.CreateClient();
            _tokenService = tokenService;
            _userInfoService = userInfoService;
            _dbContext = dbContext;
        }

        public async Task GetAllDataAsync()
        {
            int currentPage = 0;

            var firstResponse = await GetInvoiceListAsync(currentPage);                              // Lấy lần đầu tiên để biết totalPage
            using var firstDoc = JsonDocument.Parse(firstResponse);
            var root = firstDoc.RootElement;

            int totalPage = root.GetProperty("totalPage").GetInt32();

            var items = root.GetProperty("items");                                                  // Xử lý items trang đầu tiên
            Console.WriteLine($"Trang {currentPage + 1}/{totalPage}");
            foreach (var item in items.EnumerateArray())
            {
                // xử lý từng item
            }
            currentPage++;

            while (currentPage < totalPage)                                                         // Vòng lặp lấy các trang còn lại
            {
                var response = await GetInvoiceListAsync(currentPage);
                using var jsonDoc = JsonDocument.Parse(response);
                var pageRoot = jsonDoc.RootElement;

                var moreItems = pageRoot.GetProperty("items");
                Console.WriteLine($"Trang {currentPage + 1}/{totalPage}");
                foreach (var item in moreItems.EnumerateArray())
                {
                    // xử lý từng item
                }
                currentPage++;
            }
        }

        public async Task<List<string>> GetMaHoaDonListAsync(int currentPage = 0)           //hàm async (bất đồng bộ) trả về list string chứa các mã hóa đơn
        {
            var maHoaDonList = new List<string>();                                          //list rỗng để chứa các mã hóa đơn
            var json = await GetInvoiceListAsync(currentPage);                              //đợi các mã ở trang thứ currentPage, lưu response dạng chuỗi JSON vào biến 'json'
            using var doc = JsonDocument.Parse(json);                                       //phân tích json thành JsonDocument rồi truy cập nội dung chính của JSON
            var root = doc.RootElement;

            var dataArray = root.GetProperty("data");                                       //truy cập vào mảng dữ liệu chính trong JSON, property "data"
            foreach (var item in dataArray.EnumerateArray())                                //duyệt qua từng item trong data
            {
                if (item.TryGetProperty("maHoaDon", out var maHoaDonElement))               //kiểm tra trường maHoaDon, nếu có thì gán vào biến 'maHoaDonElement'
                {
                    string maHoaDon = maHoaDonElement.GetString();                          //lấy giá trị string của maHoaDon, 
                    if (!string.IsNullOrEmpty(maHoaDon))
                    {
                        maHoaDonList.Add(maHoaDon);                                         //giá trị không rỗng thì thêm vào danh sách maHoaDonList
                    }
                }
            }
            return maHoaDonList;
        }

        public async Task<string> GetInvoiceListAsync(int currentPage = 0)
        {

            var accessToken = await _tokenService.GetAccessTokenAsync();
            var (maNguoiDung, maDoanhNghiep) = await _userInfoService.GetUserAndCompanyCodeAsync();
            //lấy response từ hàm GetUserAndCompanyCodeAsync(), lưu vào userMa và doanhNghiepMa


            var request = new HttpRequestMessage(HttpMethod.Get, $"https://dev-billstore.xcyber.vn/api/hddv-hoa-don/get-list?current=1&page={currentPage}&pageSize=10&size=10&trangThaiPheDuyet");
           
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("doanhnghiepma", maDoanhNghiep);
            request.Headers.Add("userma", maNguoiDung);
            request.Headers.AcceptCharset.Add(
                new StringWithQualityHeaderValue("utf-8")
            );

            var content = await _httpClient.SendAsync(request);
            var response = await content.Content.ReadAsStringAsync();



            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var entityList = JsonSerializer.Deserialize<Entities.InvoiceListResponse>(response, options);

            var invoices = entityList?.data;
            return response;

        }
        

        //hàm lọc data riêng để controller gọi
        public async Task<string> GetDataListAsync(int currentPage = 0)
        {
            var json = await GetInvoiceListAsync(currentPage);                              //đợi các mã ở trang thứ currentPage, lưu response dạng chuỗi JSON vào biến 'json'
            using var doc = JsonDocument.Parse(json);                                       //phân tích json thành JsonDocument rồi truy cập nội dung chính của JSON
            var root = doc.RootElement;

            var dataArray = root.GetProperty("data");
            return dataArray.GetRawText();
        }



        public async Task SaveListToDatabaseAsync(List<InvoiceListEntity> invoices)
        {
            foreach (var invoice in invoices)
            // lặp qua tất cả các hóa đơn trong danh sách
            {
                // Tìm entity theo id
                var entity = await _dbContext.INVOICE_LIST
                // truy vấn table INVOICE_LIST
                    .Include(i => i.ghiChu)
                    // include ghiChu để đẩy thông tin ghiChu
                    .FirstOrDefaultAsync(i => i.id == invoice.id);
                    // nếu không thấy, entity là null

                if (entity == null)
                // nếu chưa có thì tạo bản ghi mới
                {
                    entity = new InvoiceListEntity();
                    _dbContext.INVOICE_LIST.Add(entity);
                }

                // Map dữ liệu từ input entity sang entity database
                entity.id = invoice.id;
                entity.maHoaDon = invoice.maHoaDon;
                entity.maLichSuFile = invoice.maLichSuFile;
                entity.soHoaDon = invoice.soHoaDon;
                entity.loaiHoaDon = invoice.loaiHoaDon;
                entity.tenNCC = invoice.tenNCC;
                entity.mstNCC = invoice.mstNCC;
                entity.tongTien = invoice.tongTien;
                entity.tienTruocThue = invoice.tienTruocThue;
                entity.tienThue = invoice.tienThue;
                entity.nhanHoaDon = invoice.nhanHoaDon;
                entity.trangThaiPheDuyet = invoice.trangThaiPheDuyet;
                entity.trangThaiHoaDon = invoice.trangThaiHoaDon;
                entity.soDonHang = invoice.soDonHang;
                entity.kiHieuMauSoHoaDon = invoice.kiHieuMauSoHoaDon;
                entity.kiHieuHoaDon = invoice.kiHieuHoaDon;
                entity.tinhChatHoaDon = invoice.tinhChatHoaDon;
                entity.ngayLap = invoice.ngayLap;
                entity.ngayNhan = invoice.ngayNhan;
                entity.phuongThucNhap = invoice.phuongThucNhap;
                //dữ liệu từ invoice được map vào entity

                // Xử lý ghi chú
                if (invoice.ghiChu != null)
                {
                    if (entity.ghiChu == null)
                    {
                        entity.ghiChu = new GhiChuEntity();
                    }

                    entity.ghiChu.ghiChu_id = invoice.id;  // Foreign Key cũng là id hóa đơn
                    entity.ghiChu.checkTrangThaiXuLy = invoice.ghiChu.checkTrangThaiXuLy;
                    entity.ghiChu.checkTrangThaiHoaDon = invoice.ghiChu.checkTrangThaiHoaDon;
                    entity.ghiChu.checkTenNguoiMua = invoice.ghiChu.checkTenNguoiMua;
                    entity.ghiChu.checkDiaChiNguoiMua = invoice.ghiChu.checkDiaChiNguoiMua;
                    entity.ghiChu.checkMstNguoiMua = invoice.ghiChu.checkMstNguoiMua;
                    entity.ghiChu.checkHoaDonKyDienTu = invoice.ghiChu.checkHoaDonKyDienTu;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public List<InvoiceListEntity> ConvertJsonToInvoiceList(JsonElement data)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            string jsonString = data.GetRawText();
            return JsonSerializer.Deserialize<List<InvoiceListEntity>>(jsonString, options);
        }

    }
}
