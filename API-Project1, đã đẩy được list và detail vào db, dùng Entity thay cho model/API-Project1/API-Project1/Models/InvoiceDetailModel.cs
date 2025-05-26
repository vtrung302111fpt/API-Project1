using System.Text.Json;

namespace API_Project1.Models
{
    public class InvoiceDetailModel
    {
        public List<InvoiceDetailModel> data { get; set; }
    }

    public class InvoiceDetailDataModel
    {
        public string maHoaDon { get; set; }
        public string maLichSuFile { get; set; }
        public string tenHoaDon { get; set; }
        public int kiHieuMauSoHoaDon { get; set; }
        public string? kiHieuHoaDon { get; set; }
        public string soHoaDon { get; set; }
        public string ngayLap { get; set; }
        public string? ngayDuyet { get; set; }
        public string? ngayNhan { get; set; }
        public string? ngayThanhToan { get; set; }
        public string? ngayKy { get; set; }
        public string? hoaDonDanhChoKhuPhiThueQuan { get; set; }
        public string? donViTienTe { get; set; }
        public double? tiGia { get; set; }
        public string? hinhThucThanhToan { get; set; }
        public string? mstToChucGiaiPhap { get; set; }
        public string tenNguoiBan { get; set; }
        public string mstNguoiBan { get; set; }
        public string diaChiNguoiBan { get; set; }
        public string tenNguoiMua { get; set; }
        public string mstNguoiMua { get; set; }
        public string diaChiNguoiMua { get; set; }
        public string? nhanHoaDon { get; set; }
        public string? ghiChu { get; set; }
        public string tongTienChuaThue { get; set; }
        public string tongTienThue { get; set; }
        public string tongTienChietKhauThuongMai { get; set; }
        public string tongTienThanhToanBangSo { get; set; }
        public string tongTienThanhToanBangChu { get; set; }
        public int trangThaiHoaDon { get; set; }
        public int trangThaiPheDuyet { get; set; }
        public int loaiHoaDon { get; set; }
        public string? lyDoKhongDuyet { get; set; }
        public string? nguoiDuyet { get; set; }
        public int phuongThucNhap { get; set; }
        public int checkTrangThaiXuLy { get; set; }
        public int checkTrangThaiHoaDon { get; set; }
        public int? checkMstNguoiMua { get; set; }
        public int? checkDiaChiNguoiMua { get; set; }
        public int? checkTenNguoiMua { get; set; }
        public int? checkHDonKyDienTu { get; set; }
        public int kiemTraChungThu { get; set; }
        public int kiemTraTenNban { get; set; }
        public int kiemTraMstNban { get; set; }
        public int kiemTraHoatDongNmua { get; set; }
        public int? kiemTraHoatDongNban { get; set; }
        public string? dsFileDinhKem { get; set; }
        public List<DsHangHoaModel> dsHangHoa { get; set; }
        public string? fileExcel { get; set; }
        public List<DsThueSuat> dsThueSuat { get; set; }


        public void OnDeserialize(JsonElement element)
        {
            nhanHoaDon = element.GetProperty("nhanHoaDon").GetString();
            //soDonHang = element.GetProperty("soDonHang").GetString();

        }
        

    }

    public class DsHangHoaModel
    {
        public int id { get; set; }
        public string maHoaDon { get; set; }
        public string? khuyenMai { get; set; }
        public int stt {  get; set; }
        public string? tenHangHoa {  get; set; }
        public string donGia { get; set; }
        public string? loai {  get; set; }
        public string? donViTinh {  get; set; }
        public string soLuong { get; set; }
        public string thanhTien { get; set; }
        public string thueSuat { get; set; }
        public string tienThue { get; set; }
        public bool checkSua { get; set; }
    }


    public class DsThueSuat
    {
        public int id { get; set; }
        public string maHoaDon { get; set; }
        public string thueSuat { get; set; }
        public string tienThue { get; set; }
    }
}
