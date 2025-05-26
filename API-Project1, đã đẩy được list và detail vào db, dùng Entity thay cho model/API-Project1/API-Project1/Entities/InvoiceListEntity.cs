using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_Project1.Entities
{
    public class InvoiceListResponse
    {
        public List<InvoiceListEntity> data { get; set; }
    }

    public class InvoiceListEntity
    {
        [Key]
        public int id { get; set; }
        public string maHoaDon { get; set; }
        public string maLichSuFile { get; set; }
        public string soHoaDon { get; set; }
        public int loaiHoaDon { get; set; }
        public string tenNCC { get; set; }
        public string mstNCC { get; set; }
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

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? ngayLap { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? ngayNhan { get; set; }
        public int phuongThucNhap { get; set; }

        // Navigation property
        public GhiChuEntity ghiChu { get; set; }
    }

    public class GhiChuEntity
    {
        [Key]
        public int ghiChu_id { get; set; }

        public int checkTrangThaiXuLy { get; set; }
        public int? checkTrangThaiHoaDon { get; set; }
        public int? checkTenNguoiMua { get; set; }
        public int? checkDiaChiNguoiMua { get; set; }
        public int? checkMstNguoiMua { get; set; }
        public int? checkHoaDonKyDienTu { get; set; }

        // Navigation back to Invoice
        public InvoiceListEntity InvoiceListEntity { get; set; }
    }
}
