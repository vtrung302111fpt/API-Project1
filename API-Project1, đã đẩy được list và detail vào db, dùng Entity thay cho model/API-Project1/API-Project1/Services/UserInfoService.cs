using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using API_Project1.Interfaces;

namespace API_Project1.Services
{
    public class UserInfoService : IUserInfoService
    {
        private readonly ITokenService _tokenService;
        private readonly HttpClient _httpClient;

        private string _doanhNghiepMa;
        private string _userMa;

        public UserInfoService(ITokenService tokenService, HttpClient httpClient, IHttpClientFactory httpClientFactory)
        {
            _tokenService = tokenService;
            //_httpClient = httpClientFactory.CreateClient();
            _httpClient = httpClient;
        }



        private async Task FetchUserAndCompanyInfoAsync()                               //hàm lấy mã người dùng và mã doanh nghiệp
        {
            var json = await GetUserInfoAsync();
            var root = JsonDocument.Parse(json).RootElement;

            _userMa = root.GetProperty("data").GetProperty("hddvNguoiDungReponse").GetProperty("maNguoiDung").GetString();

            _doanhNghiepMa = root.GetProperty("data").GetProperty("hddvDoanhNghiepReponse").GetProperty("maDoanhNghiep").GetString();
            Console.WriteLine($"Fetched: UserMa = {_userMa}, DoanhNghiepMa = {_doanhNghiepMa}");
        }


        public async Task<(string MaNguoiDung, string MaDoanhNghiep)> GetUserAndCompanyCodeAsync()
        {
            if (string.IsNullOrEmpty(_doanhNghiepMa) || string.IsNullOrEmpty(_userMa))  //kiểm tra xem có mã chưa, chưa thì gọi FetchUserAndCompanyInfoAsync
            {
                await FetchUserAndCompanyInfoAsync();
            }
            return (_userMa, _doanhNghiepMa);
        }




        public async Task<string> GetUserInfoAsync()
        {
            var accessToken = await _tokenService.GetAccessTokenAsync();                //lấy access token từ hàm GetAccessTokenAsync

            var request = new HttpRequestMessage(HttpMethod.Get, "https://dev-billstore.xcyber.vn/api/hddv/nguoidung/get-user-info");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            //tạo request gọi đến API trên


            var response = await _httpClient.SendAsync(request);                        //gửi request kia đi, gán kết quả vào response
            var content = await response.Content.ReadAsStringAsync();                   //chuyển response ở trên thành string

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed: {response.StatusCode}\n{content}");
            }

            return content;
        }
    }
}
