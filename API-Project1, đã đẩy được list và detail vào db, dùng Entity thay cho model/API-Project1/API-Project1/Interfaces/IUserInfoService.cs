using System.Threading.Tasks;

namespace API_Project1.Interfaces
{
    public interface IUserInfoService
    {
        Task<(string MaNguoiDung, string MaDoanhNghiep)> GetUserAndCompanyCodeAsync();
        Task<string> GetUserInfoAsync();
    }
}
