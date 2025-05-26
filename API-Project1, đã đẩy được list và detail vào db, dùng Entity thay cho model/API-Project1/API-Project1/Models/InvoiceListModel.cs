using System.Text.Json;
using System.Text.Json.Serialization;

namespace API_Project1.Models
{
    public class InvoiceListResponse
    {
        public List<InvoiceListDataModel> data { get; set; }
    }

    public class InvoiceListDataModel
    {
        public int id { get; set; }
        public string maHoaDon { get; set; }
        public string maLichSuFile { get; set; }
        public string soHoaDon { get; set; }
        public int loaiHoaDon { get; set; }
        public string tenNCC { get; set; }
        public string mstNCC { get; set; }
        public int trangThaiTct { get; set; }
        public string tongTien { get; set; }
        public string tienTruocThue { get; set; }
        public string tienThue { get; set; }
        public string? nhanHoaDon { get; set; }
        public int trangThaiPheDuyet { get; set; }
        public int trangThaiHoaDon { get; set; }
        public string? soDonHang { get; set; }
        public int kiHieuMauSoHoaDon { get; set; }
        public string kiHieuHoaDon { get; set; }
        public int tinhChatHoaDon { get; set; }
        public string? ngayLap { get; set; }
        public string? ngayNhan { get; set; }

        public void OnDeserialize(JsonElement element)
        {
            nhanHoaDon = element.GetProperty("nhanHoaDon").GetString();
            soDonHang = element.GetProperty("soDonHang").GetString();
        }
        public int phuongThucNhap { get; set; }

        // Đây là bảng phụ
        public GhiChuModel ghiChu { get; set; }
    }

    public class GhiChuModel
    {
        public int ghiChu_id { get; set; }

        public int checkTrangThaiXuLy { get; set; }
        public int? checkTrangThaiHoaDon { get; set; }
        public int? checkTenNguoiMua { get; set; }
        public int? checkDiaChiNguoiMua { get; set; }
        public int? checkMstNguoiMua { get; set; }
        public int? checkHoaDonKyDienTu { get; set; }
    }

}