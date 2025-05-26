using System.Data;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using API_Project1.Entities;
using API_Project1.Interfaces;
using API_Project1.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace API_Project1.Services
{
    public class InvoiceDetailService: IInvoiceDetailService
    {
        private readonly ITokenService _tokenService;
        private readonly HttpClient _httpClient;
        private readonly IInvoiceListService _interfaceInvoiceList;
        private readonly AppDbContext _dbContext;

        public InvoiceDetailService(
            ITokenService tokenService, 
            HttpClient httpClient, 
            IInvoiceListService invoiceList, 
            AppDbContext dbContext)
        {
            _tokenService = tokenService;
            _httpClient = httpClient;
            _interfaceInvoiceList = invoiceList;
            _dbContext = dbContext;
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
                    .Include(i => i.dsHangHoa)
                    .Include(i => i.dsThueSuat)
                    .FirstOrDefaultAsync(i => i.maHoaDon == invoice.maHoaDon);

                if (entity == null)
                {
                    entity = new InvoiceDetailEntity
                    {
                        maHoaDon = invoice.maHoaDon
                    };
                    _dbContext.INVOICE_DETAIL.Add(entity);
                }

                entity.maHoaDon = invoice.maHoaDon;
                entity.maLichSuFile = invoice.maLichSuFile;
                entity.tenHoaDon = invoice.tenHoaDon;
                entity.kiHieuMauSoHoaDon = invoice.kiHieuMauSoHoaDon;
                entity.kiHieuHoaDon = invoice.kiHieuHoaDon;
                entity.soHoaDon = invoice.soHoaDon;
                entity.ngayLap = invoice.ngayLap;
                entity.ngayDuyet = invoice.ngayDuyet;
                entity.ngayNhan = invoice.ngayNhan;
                entity.ngayThanhToan = invoice.ngayThanhToan;
                entity.ngayKy = invoice.ngayKy;
                entity.hoaDonDanhChoKhuPhiThueQuan = invoice.hoaDonDanhChoKhuPhiThueQuan;
                entity.donViTienTe = invoice.donViTienTe;
                entity.tiGia = invoice.tiGia;
                entity.hinhThucThanhToan = invoice.hinhThucThanhToan;
                entity.mstToChucGiaiPhap = invoice.mstToChucGiaiPhap;
                entity.tenNguoiBan = invoice.tenNguoiBan;
                entity.mstNguoiBan = invoice.mstNguoiBan;
                entity.diaChiNguoiBan = invoice.diaChiNguoiBan;
                entity.tenNguoiMua = invoice.tenNguoiMua;
                entity.mstNguoiMua = invoice.mstNguoiMua;
                entity.diaChiNguoiMua = invoice.diaChiNguoiMua;
                entity.nhanHoaDon = invoice.nhanHoaDon;
                entity.ghiChu = invoice.ghiChu;
                entity.tongTienChuaThue = invoice.tongTienChuaThue;
                entity.tongTienThue = invoice.tongTienThue;
                entity.tongTienChietKhauThuongMai = invoice.tongTienChietKhauThuongMai;
                entity.tongTienThanhToanBangSo = invoice.tongTienThanhToanBangSo;
                entity.tongTienThanhToanBangChu = invoice.tongTienThanhToanBangChu;
                entity.trangThaiHoaDon = invoice.trangThaiHoaDon;
                entity.trangThaiPheDuyet = invoice.trangThaiPheDuyet;
                entity.loaiHoaDon = invoice.loaiHoaDon;
                entity.lyDoKhongDuyet = invoice.lyDoKhongDuyet;
                entity.nguoiDuyet = invoice.nguoiDuyet;
                entity.phuongThucNhap = invoice.phuongThucNhap;
                entity.checkTrangThaiXuLy = invoice.checkTrangThaiXuLy;
                entity.checkTrangThaiHoaDon = invoice.checkTrangThaiHoaDon;
                entity.checkMstNguoiMua = invoice.checkMstNguoiMua;
                entity.checkDiaChiNguoiMua = invoice.checkDiaChiNguoiMua;
                entity.checkTenNguoiMua = invoice.checkTenNguoiMua;
                entity.checkHDonKyDienTu = invoice.checkHDonKyDienTu;
                entity.kiemTraChungThu = invoice.kiemTraChungThu;
                entity.kiemTraTenNban = invoice.kiemTraTenNban;
                entity.kiemTraMstNban = invoice.kiemTraMstNban;
                entity.kiemTraHoatDongNmua = invoice.kiemTraHoatDongNmua;
                entity.kiemTraHoatDongNban = invoice.kiemTraHoatDongNban;
                entity.dsFileDinhKem = invoice.dsFileDinhKem;
                entity.fileExcel = invoice.fileExcel;

                if (invoice.dsHangHoa != null)
                {
                    if (entity.dsHangHoa == null)
                    {
                        entity.dsHangHoa = new List<DsHangHoaEntity>();
                    }

                    // Tạo một danh sách id của dsHangHoa mới để đối chiếu
                    var newIds = invoice.dsHangHoa.Select(h => h.id).ToList();

                    // Xóa những phần tử không còn tồn tại trong invoice.dsHangHoa
                    entity.dsHangHoa.RemoveAll(h => !newIds.Contains(h.id));

                    // Cập nhật hoặc thêm mới từng phần tử
                    foreach (var hangHoa in invoice.dsHangHoa)
                    {
                        var existing = entity.dsHangHoa.FirstOrDefault(h => h.id == hangHoa.id);
                        if (existing != null)
                        {
                            // Cập nhật các trường
                            existing.maHoaDon = hangHoa.maHoaDon;
                            existing.khuyenMai = hangHoa.khuyenMai;
                            existing.stt = hangHoa.stt;
                            existing.tenHangHoa = hangHoa.tenHangHoa;
                            existing.donGia = hangHoa.donGia;
                            existing.loai = hangHoa.loai;
                            existing.donViTinh = hangHoa.donViTinh;
                            existing.soLuong = hangHoa.soLuong;
                            existing.thanhTien = hangHoa.thanhTien;
                            existing.thueSuat = hangHoa.thueSuat;
                            existing.tienThue = hangHoa.tienThue;
                            existing.checkSua = hangHoa.checkSua;
                        }
                        else
                        {
                            // Thêm mới
                            entity.dsHangHoa.Add(new DsHangHoaEntity
                            {
                                id = hangHoa.id,
                                maHoaDon = hangHoa.maHoaDon,
                                khuyenMai = hangHoa.khuyenMai,
                                stt = hangHoa.stt,
                                tenHangHoa = hangHoa.tenHangHoa,
                                donGia = hangHoa.donGia,
                                loai = hangHoa.loai,
                                donViTinh = hangHoa.donViTinh,
                                soLuong = hangHoa.soLuong,
                                thanhTien = hangHoa.thanhTien,
                                thueSuat = hangHoa.thueSuat,
                                tienThue = hangHoa.tienThue,
                                checkSua = hangHoa.checkSua
                            });
                        }
                    }
                }

                if (invoice.dsThueSuat != null)
                {
                    if (entity.dsThueSuat == null)
                    {
                        entity.dsThueSuat = new List<DsThueSuatEntity>();
                    }

                    // Tạo một danh sách id của dsHangHoa mới để đối chiếu
                    var newIds = invoice.dsThueSuat.Select(h => h.id).ToList();

                    // Xóa những phần tử không còn tồn tại trong invoice.dsHangHoa
                    entity.dsThueSuat.RemoveAll(h => !newIds.Contains(h.id));

                    // Cập nhật hoặc thêm mới từng phần tử
                    foreach (var thueSuat in invoice.dsThueSuat)
                    {
                        var existing = entity.dsThueSuat.FirstOrDefault(h => h.id == thueSuat.id);
                        if (existing != null)
                        {
                            // Cập nhật các trường
                            existing.id = thueSuat.id;
                            existing.maHoaDon = thueSuat.maHoaDon;
                            existing.thueSuat = thueSuat.thueSuat;
                            existing.tienThue = thueSuat.tienThue;
                        }
                        else
                        {
                            // Thêm mới
                            entity.dsThueSuat.Add(new DsThueSuatEntity
                            {
                                id = thueSuat.id,
                                maHoaDon = thueSuat.maHoaDon,
                                thueSuat = thueSuat.thueSuat,
                                tienThue = thueSuat.tienThue,
                            });
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





//if (invoice.dsHangHoa != null)
//{
//    if (entity.dsHangHoa == null)
//    {
//        entity.dsHangHoa = new DsHangHoaEntity();
//    }

//    entity.dsHangHoa.id = invoice.dsHangHoa.id;  // FK cũng là id hóa đơn
//    entity.dsHangHoa.maHoaDon = invoice.dsHangHoa.maHoaDon;
//    entity.dsHangHoa.khuyenMai = invoice.dsHangHoa.khuyenMai;
//    entity.dsHangHoa.stt = invoice.dsHangHoa.stt;
//    entity.dsHangHoa.tenHangHoa = invoice.dsHangHoa.tenHangHoa;
//    entity.dsHangHoa.donGia = invoice.dsHangHoa.donGia;
//    entity.dsHangHoa.loai = invoice.dsHangHoa.loai;
//    entity.dsHangHoa.donViTinh = invoice.dsHangHoa.donViTinh;
//    entity.dsHangHoa.soLuong = invoice.dsHangHoa.soLuong;
//    entity.dsHangHoa.thanhTien = invoice.dsHangHoa.thanhTien;
//    entity.dsHangHoa.thueSuat = invoice.dsHangHoa.thueSuat;
//    entity.dsHangHoa.tienThue = invoice.dsHangHoa.tienThue;
//    entity.dsHangHoa.checkSua = invoice.dsHangHoa.checkSua;
//}

//if (invoice.dsThueSuat != null)
//{
//    if (entity.dsThueSuat == null) 
//    {
//        entity.dsThueSuat = new DsThueSuatEntity();
//    }

//    entity.dsThueSuat.id = invoice.dsThueSuat.id;
//    entity.dsThueSuat.maHoaDon = invoice.dsThueSuat.maHoaDon;
//    entity.dsThueSuat.thueSuat = invoice.dsThueSuat.thueSuat;
//    entity.dsThueSuat.tienThue = invoice.dsThueSuat.tienThue;
//}











//public async Task SaveDetailToDatabaseAsync(List<Invoice>)


//public async Task SaveDetailToDatabaseAsync(List<InvoiceDetailDataModel> invoiceDetails)
//{
//    var connectionString = "Server=localhost\\SQLEXPRESS; Database=BILL_STORE; Trusted_Connection=True;Encrypt=False; TrustServerCertificate=False;";

//    //Integrated Security = True; Encrypt = False; TrustServerCertificate = False;
//    using (var connection = new SqlConnection(connectionString))
//    {
//        await connection.OpenAsync();

//        foreach (var invoiceDetail in invoiceDetails)
//        {
//            var insertHoaDonDetail = new SqlCommand(@"

//            MERGE INTO INVOICE_DETAIL AS target
//            USING (SELECT 
//                @MaHoaDon as MaHoaDon, @MaLichSuFile as MaLichSuFile,
//                @TenHoaDon as TenHoaDon, @KiHieuMauSoHoaDon as KiHieuMauSoHoaDon, @SoHoaDon as SoHoaDOn,
//                @NgayLap as NgayLap, @NgayDuyet as NgayDuyet, @NgayNhan as NgayNhan, @NgayThanhToan as NgayThanhToan,
//                @NgayKy as NgayKy, @HoaDonDanhChoKhuPhiThueQuan as HoaDonDanhChoKhuPhiThueQuan, 
//                @DonViTienTe as DonViTienTe, @TiGia as TiGia, @HinhThucThanhToan as HinhThucThanhToan,
//                @MstToChucGiaiPhap as MstToChucGiaiPhap, @TenNguoiBan as TenNguoiBan, @MstNguoiBan as MstNguoiBan,
//                @DiaChiNguoiBan as DiaChiNguoiBan, @TenNguoiMua as TenNguoiMua, @MstNguoiMua as MstNguoiMua,
//                @DiaChiNguoiMua as DiaChiNguoiMua, @NhanHoaDon as NhanHoaDon, @GhiChu as GhiChu,
//                @TongTienChuaThue as TongTienChuaThue, @TongTienThue as TongTienThue,
//                @TongTienChietKhauThuongMai as TongTienChietKhauThuongMai, @TongTienThanhToanBangSo as TongTienThanhToanBangSo,
//                @TongTienThanhToanBangChu as TongTienThanhToanBangChu, @TrangThaiHoaDon as TrangThaiHoaDon,
//                @TrangThaiPheDuyet as TrangThaiPheDuyet, @LoaiHoaDon as LoaiHoaDon, @LyDoKhongDuyet as LyDoKhongDuyet,
//                @NguoiDuyet as NguoiDuyet, @PhuongThucNhap as PhuongThucNhap, @CheckTrangThaiXuLy as CheckTrangThaiXuLy,
//                @CheckTrangThaiHoaDon as CheckTrangThaiHoaDon, @CheckMstNguoiMua as CheckMstNguoiMua,
//                @CheckDiaChiNguoiMua as CheckDiaChiNguoiMua, @CheckTenNguoiMua as CheckTenNguoiMua,
//                @CheckHDonKyDienTu as CheckHDonKyDienTu, @KiemTraChungThu as KiemTraChungThu,
//                @KiemTraTenNban as KiemTraTenNban, @KiemTraMstNban as KiemTraMstNban, @KiemTraHoatDongNmua as KiemTraHoatDongNmua,
//                @KiemTraHoatDongNban as KiemTraHoatDongNban, @DsFileDinhKem as DsFileDinhKem, @FileExcel as FileExcel
//            ) AS source 
//            ON target.MaHoaDon = source.MaHoaDon

//            WHEN MATCHED THEN 
//                UPDATE SET 
//                    MaHoaDon = source.MaHoaDon,
//                    MaLichSuFile = source.MaLichSuFile,
//                    TenHoaDon = source.TenHoaDon,
//                    KiHieuMauSoHoaDon = source.KiHieuMauSoHoaDon,
//                    SoHoaDon = source.SoHoaDon,
//                    NgayLap = source.NgayLap,
//                    NgayDuyet = source.NgayDuyet,
//                    NgayNhan = source.NgayNhan,
//                    NgayThanhToan = source.NgayThanhToan,
//                    NgayKy = source.NgayKy,
//                    HoaDonDanhChoKhuPhiThueQuan = source.HoaDonDanhChoKhuPhiThueQuan,
//                    DonViTienTe = source.DonViTienTe,
//                    TiGia = source.TiGia,
//                    HinhThucThanhToan = source.HinhThucThanhToan,
//                    MstToChucGiaiPhap = source.MstToChucGiaiPhap,
//                    TenNguoiBan = source.TenNguoiBan,
//                    MstNguoiBan = source.MstNguoiBan,
//                    DiaChiNguoiBan = source.DiaChiNguoiBan,
//                    TenNguoiMua = source.TenNguoiMua,
//                    MstNguoiMua = source.MstNguoiMua,
//                    DiaChiNguoiMua = source.DiaChiNguoiMua,
//                    NhanHoaDon = source.NhanHoaDon,
//                    GhiChu = source.GhiChu,
//                    TongTienChuaThue = source.TongTienChuaThue,
//                    TongTienThue = source.TongTienThue,
//                    TongTienChietKhauThuongMai = source.TongTienChietKhauThuongMai,
//                    TongTienThanhToanBangSo = source.TongTienThanhToanBangSo,
//                    TongTienThanhToanBangChu = source.TongTienThanhToanBangChu,
//                    TrangThaiHoaDon = source.TrangThaiHoaDon,
//                    TrangThaiPheDuyet = source.TrangThaiPheDuyet,
//                    LoaiHoaDon = source.LoaiHoaDon,
//                    LyDoKhongDuyet = source.LyDoKhongDuyet,
//                    NguoiDuyet = source.NguoiDuyet,
//                    PhuongThucNhap = source.PhuongThucNhap,
//                    CheckTrangThaiXuLy = source.CheckTrangThaiXuLy,
//                    CheckTrangThaiHoaDon = source.CheckTrangThaiHoaDon,
//                    CheckMstNguoiMua = source.CheckMstNguoiMua,
//                    CheckDiaChiNguoiMua = source.CheckDiaChiNguoiMua,
//                    CheckTenNguoiMua = source.CheckTenNguoiMua,
//                    CheckHDonKyDienTu = source.CheckHDonKyDienTu,
//                    KiemTraChungThu = source.KiemTraChungThu,
//                    KiemTraTenNban = source.KiemTraTenNban,
//                    KiemTraMstNban = source.KiemTraMstNban,
//                    KiemTraHoatDongNmua = source.KiemTraHoatDongNmua,
//                    KiemTraHoatDongNban = source.KiemTraHoatDongNban,
//                    DsFileDinhKem = source.DsFileDinhKem,
//                    FileExcel = source.FileExcel

//            WHEN NOT MATCHED THEN 
//                INSERT (
//                    MaHoaDon, MaLichSuFile, TenHoaDon, KiHieuMauSoHoaDon, SoHoaDon, NgayLap, NgayDuyet, NgayNhan, NgayThanhToan, NgayKy,
//                    HoaDonDanhChoKhuPhiThueQuan, DonViTienTe, TiGia, HinhThucThanhToan, MstToChucGiaiPhap,
//                    TenNguoiBan, MstNguoiBan, DiaChiNguoiBan, TenNguoiMua, MstNguoiMua, DiaChiNguoiMua,
//                    NhanHoaDon, GhiChu, TongTienChuaThue, TongTienThue, TongTienChietKhauThuongMai,
//                    TongTienThanhToanBangSo, TongTienThanhToanBangChu, TrangThaiHoaDon, TrangThaiPheDuyet,
//                    LoaiHoaDon, LyDoKhongDuyet, NguoiDuyet, PhuongThucNhap, CheckTrangThaiXuLy,
//                    CheckTrangThaiHoaDon, CheckMstNguoiMua, CheckDiaChiNguoiMua, CheckTenNguoiMua,
//                    CheckHDonKyDienTu, KiemTraChungThu, KiemTraTenNban, KiemTraMstNban,
//                    KiemTraHoatDongNmua, KiemTraHoatDongNban, DsFileDinhKem, FileExcel
//                )
//                VALUES (
//                    source.MaHoaDon, source.MaLichSuFile, source.TenHoaDon, source.KiHieuMauSoHoaDon, source.SoHoaDon, source.NgayLap, source.NgayDuyet, source.NgayNhan, source.NgayThanhToan, source.NgayKy,
//                    source.HoaDonDanhChoKhuPhiThueQuan, source.DonViTienTe, source.TiGia, source.HinhThucThanhToan, source.MstToChucGiaiPhap,
//                    source.TenNguoiBan, source.MstNguoiBan, source.DiaChiNguoiBan, source.TenNguoiMua, source.MstNguoiMua, source.DiaChiNguoiMua,
//                    source.NhanHoaDon, source.GhiChu, source.TongTienChuaThue, source.TongTienThue, source.TongTienChietKhauThuongMai,
//                    source.TongTienThanhToanBangSo, source.TongTienThanhToanBangChu, source.TrangThaiHoaDon, source.TrangThaiPheDuyet,
//                    source.LoaiHoaDon, source.LyDoKhongDuyet, source.NguoiDuyet, source.PhuongThucNhap, source.CheckTrangThaiXuLy,
//                    source.CheckTrangThaiHoaDon, source.CheckMstNguoiMua, source.CheckDiaChiNguoiMua, source.CheckTenNguoiMua,
//                    source.CheckHDonKyDienTu, source.KiemTraChungThu, source.KiemTraTenNban, source.KiemTraMstNban,
//                    source.KiemTraHoatDongNmua, source.KiemTraHoatDongNban, source.DsFileDinhKem, source.FileExcel
//                );
//        ", connection);



//            insertHoaDonDetail.Parameters.AddWithValue("@MaHoaDon", invoiceDetail.maHoaDon);
//            insertHoaDonDetail.Parameters.AddWithValue("@MaLichSuFile", invoiceDetail.maLichSuFile);
//            insertHoaDonDetail.Parameters.AddWithValue("@TenHoaDon", invoiceDetail.tenHoaDon);
//            insertHoaDonDetail.Parameters.AddWithValue("@KiHieuMauSoHoaDon", invoiceDetail.kiHieuMauSoHoaDon);
//            insertHoaDonDetail.Parameters.AddWithValue("@SoHoaDon", invoiceDetail.soHoaDon);

//            insertHoaDonDetail.Parameters.AddWithValue("@HoaDonDanhChoKhuPhiThueQuan", (object?)invoiceDetail.hoaDonDanhChoKhuPhiThueQuan ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@DonViTienTe", (object?)invoiceDetail.donViTienTe ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@TiGia", (object?)invoiceDetail.tiGia ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@HinhThucThanhToan", (object?)invoiceDetail.hinhThucThanhToan ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@MstToChucGiaiPhap", (object?)invoiceDetail.mstToChucGiaiPhap ?? DBNull.Value);

//            insertHoaDonDetail.Parameters.AddWithValue("@TenNguoiBan", invoiceDetail.tenNguoiBan);
//            insertHoaDonDetail.Parameters.AddWithValue("@MstNguoiBan", invoiceDetail.mstNguoiBan);
//            insertHoaDonDetail.Parameters.AddWithValue("@DiaChiNguoiBan", invoiceDetail.diaChiNguoiBan);
//            insertHoaDonDetail.Parameters.AddWithValue("@TenNguoiMua", invoiceDetail.tenNguoiMua);
//            insertHoaDonDetail.Parameters.AddWithValue("@MstNguoiMua", invoiceDetail.mstNguoiMua);
//            insertHoaDonDetail.Parameters.AddWithValue("@DiaChiNguoiMua", invoiceDetail.diaChiNguoiMua);

//            insertHoaDonDetail.Parameters.AddWithValue("@NhanHoaDon", (object?)invoiceDetail.nhanHoaDon ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@GhiChu", (object?)invoiceDetail.ghiChu ?? DBNull.Value);

//            insertHoaDonDetail.Parameters.AddWithValue("@TongTienChuaThue", invoiceDetail.tongTienChuaThue);
//            insertHoaDonDetail.Parameters.AddWithValue("@TongTienThue", invoiceDetail.tongTienThue);
//            insertHoaDonDetail.Parameters.AddWithValue("@TongTienChietKhauThuongMai", (object?)invoiceDetail.tongTienChietKhauThuongMai ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@TongTienThanhToanBangSo", invoiceDetail.tongTienThanhToanBangSo);
//            insertHoaDonDetail.Parameters.AddWithValue("@TongTienThanhToanBangChu", invoiceDetail.tongTienThanhToanBangChu);
//            insertHoaDonDetail.Parameters.AddWithValue("@TrangThaiHoaDon", invoiceDetail.trangThaiHoaDon);
//            insertHoaDonDetail.Parameters.AddWithValue("@TrangThaiPheDuyet", invoiceDetail.trangThaiPheDuyet);
//            insertHoaDonDetail.Parameters.AddWithValue("@LoaiHoaDon", invoiceDetail.loaiHoaDon);

//            insertHoaDonDetail.Parameters.AddWithValue("@LyDoKhongDuyet", (object?)invoiceDetail.lyDoKhongDuyet ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@NguoiDuyet", (object?)invoiceDetail.nguoiDuyet ?? DBNull.Value);

//            insertHoaDonDetail.Parameters.AddWithValue("@PhuongThucNhap", invoiceDetail.phuongThucNhap);
//            insertHoaDonDetail.Parameters.AddWithValue("@CheckTrangThaiXuLy", invoiceDetail.checkTrangThaiXuLy);
//            insertHoaDonDetail.Parameters.AddWithValue("@CheckTrangThaiHoaDon", invoiceDetail.checkTrangThaiHoaDon);
//            insertHoaDonDetail.Parameters.AddWithValue("@CheckMstNguoiMua", (object?)invoiceDetail.checkMstNguoiMua ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@CheckDiaChiNguoiMua", (object?)invoiceDetail.checkDiaChiNguoiMua ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@CheckTenNguoiMua", (object?)invoiceDetail.checkTenNguoiMua ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@CheckHDonKyDienTu", (object?)invoiceDetail.checkHDonKyDienTu ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@KiemTraChungThu", invoiceDetail.kiemTraChungThu);
//            insertHoaDonDetail.Parameters.AddWithValue("@KiemTraTenNban", invoiceDetail.kiemTraTenNban);
//            insertHoaDonDetail.Parameters.AddWithValue("@KiemTraMstNban", invoiceDetail.kiemTraMstNban);
//            insertHoaDonDetail.Parameters.AddWithValue("@KiemTraHoatDongNmua", invoiceDetail.kiemTraHoatDongNmua);
//            insertHoaDonDetail.Parameters.AddWithValue("@KiemTraHoatDongNban", (object?)invoiceDetail.kiemTraHoatDongNban ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@DsFileDinhKem", (object?)invoiceDetail.dsFileDinhKem ?? DBNull.Value);
//            insertHoaDonDetail.Parameters.AddWithValue("@FileExcel", (object?)invoiceDetail.fileExcel ?? DBNull.Value);


//            var format = "dd/MM/yyyy HH:mm:ss";
//            var culture = CultureInfo.InvariantCulture;

//            DateTime parsedNgayLap = DateTime.ParseExact(invoiceDetail.ngayLap ?? throw new Exception("ngayLap is required"), format, culture);


//            //xử lý trong trường hợp các trường ngayDuyet/Nhan/.... trả về null
//            //nếu không null thì sẽ trả về invoiceDetail.ngay....., parse nó về theo dạng format, định dạng như culture (ngày tháng tiêu chuẩn quốc tế), data type là DateTime
//            DateTime? parsedngayLap = null;
//            if (!string.IsNullOrEmpty(invoiceDetail.ngayLap))
//            {
//                parsedngayLap = DateTime.ParseExact(invoiceDetail.ngayLap, format, culture);
//            }
//            DateTime? parsedNgayDuyet = null;
//            if (!string.IsNullOrEmpty(invoiceDetail.ngayDuyet))
//            {
//                parsedNgayDuyet = DateTime.ParseExact(invoiceDetail.ngayDuyet, format, culture);
//            }

//            DateTime? parsedNgayNhan = null;
//            if (!string.IsNullOrEmpty(invoiceDetail.ngayNhan))
//            {
//                parsedNgayNhan = DateTime.ParseExact(invoiceDetail.ngayNhan, format, culture);
//            }
//            DateTime? parsedNgayThanhToan = null;
//            if (!string.IsNullOrEmpty(invoiceDetail.ngayThanhToan))
//            {
//                parsedNgayThanhToan = DateTime.ParseExact(invoiceDetail.ngayThanhToan, format, culture);
//            }
//            DateTime? parsedNgayKy = null;
//            if (!string.IsNullOrEmpty(invoiceDetail.ngayKy))
//            {
//                //parsedNgayKy = DateTime.ParseExact(invoiceDetail.ngayKy, format, culture);
//                try
//                {
//                    parsedNgayKy = DateTime.ParseExact(invoiceDetail.ngayKy, format, culture);
//                }
//                catch (FormatException ex)
//                {
//                    Console.WriteLine($"ParseExact failed for ngayKy = {invoiceDetail.ngayKy}");
//                    throw;
//                }
//                //đoạn try catch dùng để check data type của response trả về
//            }
//            //Phải xử lý các trường hợp null trước mới gọi và truyền các giá trị này vào @NgayLap... sau, nếu không sẽ nhận phải lỗi null
//            insertHoaDonDetail.Parameters.Add("@NgayLap", SqlDbType.DateTime).Value = (object?)parsedngayLap ?? DBNull.Value;
//            insertHoaDonDetail.Parameters.Add("@NgayDuyet", SqlDbType.DateTime).Value = (object?)parsedNgayDuyet ?? DBNull.Value;
//            insertHoaDonDetail.Parameters.Add("@NgayNhan", SqlDbType.DateTime).Value = (object?)parsedNgayNhan ?? DBNull.Value;
//            insertHoaDonDetail.Parameters.Add("@NgayThanhToan", SqlDbType.DateTime).Value = (object?)parsedNgayThanhToan ?? DBNull.Value;
//            insertHoaDonDetail.Parameters.Add("@NgayKy", SqlDbType.DateTime).Value = (object?)parsedNgayKy ?? DBNull.Value;

//            //phải xử lý các trường liên quan đến ngày trước, đỏi từ string về datetime rồi mới AddWithValue vào được, nếu Add ngay từ đầu là (object?)invoiceDetail.ngayDuyet ?? DBNull.Value) thì nhận được sẽ bị lỗi null
//            foreach (SqlParameter param in insertHoaDonDetail.Parameters)
//            {
//                Console.WriteLine($"{param.ParameterName} = {param.Value} ({param.Value?.GetType()})");
//            }
//            await insertHoaDonDetail.ExecuteNonQueryAsync();



//            if (invoiceDetail.maHoaDon != null && invoiceDetail.dsHangHoa != null)
//            {
//                foreach (var item in invoiceDetail.dsHangHoa)
//                {
//                    var insertDsHangHoa = new SqlCommand(@"
//                MERGE INTO DANH_SACH_HANG_HOA AS target
//                    USING (SELECT
//                        @Id as Id, @MaHoaDon as MaHoaDon, @KhuyenMai as KhuyenMai, @Stt as Stt, 
//                        @TenHangHoa as TenHangHoa, @DonGia as DonGia, @Loai as Loai, @DonViTinh as DonViTinh, 
//                        @SoLuong as SoLuong, @ThanhTien as ThanhTien, @ThueSuat as ThueSuat,
//                        @TienThue as TienThue, @CheckSua as CheckSua) as source
//                    ON target.maHoaDon = source.maHoaDon


//                    WHEN MATCHED THEN
//                        UPDATE SET
//                            id = source.id,
//                            khuyenMai = source.khuyenMai,
//                            stt = source.stt,
//                            tenHangHoa = source.tenHangHoa,
//                            donGia = source.donGia,
//                            loai = source.loai,
//                            donViTinh = source.donViTinh,
//                            soLuong = source.soLuong,
//                            thanhTien = source.thanhTien,
//                            thueSuat = source.thueSuat,
//                            tienThue = source.tienThue,
//                            checkSua = source.checkSua

//                    WHEN NOT MATCHED THEN 
//                        INSERT ( 
//                            id, maHoaDon, khuyenMai, stt, tenHangHoa, donGia, loai, donViTinh, 
//                            soLuong, thanhTien, thueSuat, tienThue, checkSua
//                        )
//                        VALUES (
//                            source.id, source.maHoaDon, source.khuyenMai, source.stt, source.tenHangHoa, source.donGia, source.loai, source.donViTinh, 
//                            source.soLuong, source.thanhTien, source.thueSuat, source.tienThue, source.checkSua
//                        );
//                    ", connection);

//                    insertDsHangHoa.Parameters.AddWithValue("@Id", item.id);
//                    insertDsHangHoa.Parameters.AddWithValue("@MaHoaDon", invoiceDetail.maHoaDon);
//                    insertDsHangHoa.Parameters.AddWithValue("@KhuyenMai", (object?)item.khuyenMai ?? DBNull.Value);
//                    insertDsHangHoa.Parameters.AddWithValue("@Stt", item.stt);
//                    insertDsHangHoa.Parameters.AddWithValue("@TenHangHoa", (object?)item.tenHangHoa ?? DBNull.Value);
//                    insertDsHangHoa.Parameters.AddWithValue("@DonGia", item.donGia);
//                    insertDsHangHoa.Parameters.AddWithValue("@Loai", (object?)item.loai ?? DBNull.Value);
//                    insertDsHangHoa.Parameters.AddWithValue("@DonViTinh", (object?)item.donViTinh ?? DBNull.Value);
//                    insertDsHangHoa.Parameters.AddWithValue("@SoLuong", item.soLuong);
//                    insertDsHangHoa.Parameters.AddWithValue("@ThanhTien", item.thanhTien);
//                    insertDsHangHoa.Parameters.AddWithValue("@ThueSuat", item.thueSuat);
//                    insertDsHangHoa.Parameters.AddWithValue("@TienThue", item.tienThue);
//                    insertDsHangHoa.Parameters.AddWithValue("@CheckSua", item.checkSua);

//                    await insertDsHangHoa.ExecuteNonQueryAsync();
//                }
//            }

//            if (invoiceDetail.maHoaDon != null && invoiceDetail.dsThueSuat != null)
//            {
//                foreach (var item in invoiceDetail.dsThueSuat)
//                {
//                    var insertDsThueSuat = new SqlCommand(@"
//                MERGE INTO DANH_SACH_THUE_SUAT AS target
//                    USING (SELECT
//                        @Id as Id, @MaHoaDon as MaHoaDon, @ThueSuat as ThueSuat, @TienThue as TienThue) as source
//                    ON target.maHoaDon = source.maHoaDon


//                    WHEN MATCHED THEN
//                        UPDATE SET
//                            id = source.id,
//                            thueSuat = source.thueSuat,
//                            tienThue = source.tienThue
//                    WHEN NOT MATCHED THEN 
//                        INSERT ( 
//                            id, maHoaDon, thueSuat, tienThue
//                        )
//                        VALUES (
//                            source.id, source.maHoaDon, source.thueSuat, source.tienThue
//                        );
//                    ", connection);

//                    insertDsThueSuat.Parameters.AddWithValue("@Id", item.id);
//                    insertDsThueSuat.Parameters.AddWithValue("@MaHoaDon", item.maHoaDon);
//                    insertDsThueSuat.Parameters.AddWithValue("@ThueSuat", item.thueSuat);
//                    insertDsThueSuat.Parameters.AddWithValue("@TienThue", item.tienThue);

//                    await insertDsThueSuat.ExecuteNonQueryAsync();
//                }
//            }

//            try
//            {
//                await insertHoaDonDetail.ExecuteNonQueryAsync();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ SQL Insert Error: {ex.Message}");
//            }

//        }
//    }
//}
