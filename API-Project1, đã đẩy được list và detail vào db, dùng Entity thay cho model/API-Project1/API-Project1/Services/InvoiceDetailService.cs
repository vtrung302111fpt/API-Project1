using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using API_Project1.Entities;
using API_Project1.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API_Project1.Services
{
    public class InvoiceDetailService: IInvoiceDetailService
    {
        private readonly ITokenService _tokenService;
        private readonly HttpClient _httpClient;
        private readonly IInvoiceListService _interfaceInvoiceList;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public InvoiceDetailService(
            ITokenService tokenService, 
            HttpClient httpClient, 
            IInvoiceListService invoiceList, 
            AppDbContext dbContext,
            IMapper mapper
            )
        {
            _tokenService = tokenService;
            _httpClient = httpClient;
            _interfaceInvoiceList = invoiceList;
            _dbContext = dbContext;
            _mapper = mapper;
        }


        public async Task<string> GetInvoiceDetailAsync(int currentPage = 0)
        {
            var accessToken = await _tokenService.GetAccessTokenAsync();

            var maHoaDonList = await _interfaceInvoiceList.GetMaHoaDonListAsync(currentPage);           //lây danh sách mã hóa đơn, truyền currentPage vào như tham số vào hàm, trả về List<string> gồm các maHoaDon, user có thể truyền vào số trang để điều khiền dữ liệu cần lấy

            if (maHoaDonList == null || !maHoaDonList.Any()) 
            {
                throw new Exception("Không tìm thấy mã hóa đơn!");                                      //kiểm tra nếu list rỗng                                           
            }


            var detailList = new List<JsonElement>();                                                   //tạo danh sách rỗng để chứa các hóa đơn chi tiết dạng JSON

            foreach (var maHoaDon in maHoaDonList)                                                      //duyệt qua từng mã hóa đơn
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://dev-billstore.xcyber.vn/api/hddv-hoa-don/detail/{maHoaDon}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);   //tạo request, gắn maHoaDon vào URL và access token vào header

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();                               //gửi request, đọc response dạng string

                if (response.IsSuccessStatusCode)
                {
                    using var jsonDoc = JsonDocument.Parse(content);                                    //Parse JSON thành JsonDocument, lấy RootElement, clone lại vì JsonDocument bị dispose sau đó??? 
                    detailList.Add(jsonDoc.RootElement.Clone());                                        //add jsonDoc vào detailList
                }
                else
                {
                    Console.WriteLine($"Lỗi khi lấy chi tiết mã hóa đơn {maHoaDon}: {response.StatusCode}");
                }    
            }

            var finalSon = JsonSerializer.Serialize(detailList, new JsonSerializerOptions { WriteIndented = true });
            return finalSon;
        }

        //hàm lấy data detail riêng
        public async Task<string> GetDataDetailAsync(int currentPage = 0)
        {
            var json = await GetInvoiceDetailAsync(currentPage);                              //đợi các mã ở trang thứ currentPage, lưu response dạng chuỗi JSON vào biến 'json'
            using var doc = JsonDocument.Parse(json);                                       //phân tích json thành JsonDocument rồi truy cập nội dung chính của JSON
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array)
            {
                throw new Exception("Dữ liệu không phải dạng mảng JSON.");
            }

            var dataOnlyList = new List<JsonElement>();

            foreach (var item in root.EnumerateArray())
            {
                if (item.TryGetProperty("data", out var dataElement))
                {
                    dataOnlyList.Add(dataElement.Clone());
                }
            }

            var resultJson = JsonSerializer.Serialize(dataOnlyList, new JsonSerializerOptions { WriteIndented = true });
            return resultJson;
        }

        
        public async Task SaveDetailToDatabaseAsync(List<InvoiceDetailEntity> invoices)
        {
            foreach (var invoice in invoices)
            {
                var entity = await _dbContext.INVOICE_DETAIL
                //truy vấn table INVOICE_DETAIL
                    .Include(i => i.dsHangHoa)
                    .Include(i => i.dsThueSuat)
                    //include dsHangHoa và dsThueSuat để nạp thông tin hai trường này vào
                    .FirstOrDefaultAsync(i => i.maHoaDon == invoice.maHoaDon);

                if (entity == null)
                {
                    entity = new InvoiceDetailEntity
                    {
                        maHoaDon = invoice.maHoaDon
                    };
                    _dbContext.INVOICE_DETAIL.Add(entity);
                }

                //map dữ liệu đầu vào invoice và entity thực tế
                _mapper.Map(invoice, entity);

                if (invoice.dsHangHoa != null)
                {
                    if (entity.dsHangHoa == null)
                        entity.dsHangHoa = new List<DsHangHoaEntity>();

                    var newIds = invoice.dsHangHoa.Select(h => h.id).ToList();

                    // Xóa những phần tử không còn tồn tại
                    entity.dsHangHoa.RemoveAll(h => !newIds.Contains(h.id));

                    foreach (var hangHoa in invoice.dsHangHoa)
                    {
                        var existing = entity.dsHangHoa.FirstOrDefault(h => h.id == hangHoa.id);
                        if (existing != null)
                        {
                            _mapper.Map(hangHoa, existing); // cập nhật
                        }
                        else
                        {
                            var newItem = _mapper.Map<DsHangHoaEntity>(hangHoa); // thêm mới
                            entity.dsHangHoa.Add(newItem);
                        }
                    }
                }




                if (invoice.dsThueSuat != null)
                {
                    if (entity.dsThueSuat == null)
                        entity.dsThueSuat = new List<DsThueSuatEntity>();

                    var newIds = invoice.dsThueSuat.Select(h => h.id).ToList();

                    // Xóa những phần tử không còn tồn tại
                    entity.dsThueSuat.RemoveAll(h => !newIds.Contains(h.id));

                    foreach (var thueSuat in invoice.dsThueSuat)
                    {
                        var existing = entity.dsThueSuat.FirstOrDefault(h => h.id == thueSuat.id);
                        if (existing != null)
                        {
                            _mapper.Map(thueSuat, existing); // cập nhật
                        }
                        else
                        {
                            var newItem = _mapper.Map<DsThueSuatEntity>(thueSuat); // thêm mới
                            entity.dsThueSuat.Add(newItem);
                        }
                    }
                }

            }
            await _dbContext.SaveChangesAsync();
        }
        public List<InvoiceDetailEntity> ConvertJsonToInvoiceDetail(List<JsonElement> dataList)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartArray();
                foreach (var element in dataList)
                {
                    element.WriteTo(writer);
                }
                writer.WriteEndArray();
            }

            string jsonString = Encoding.UTF8.GetString(stream.ToArray());
            return JsonSerializer.Deserialize<List<InvoiceDetailEntity>>(jsonString, options);
        }

    }
}



