using System.Text;
using System.Text.Json;
using API_Project1.Interfaces;

namespace API_Project1.Services
{
    public class TokenService : ITokenService
    {
        private readonly HttpClient _httpClient;
        private readonly LoginConfig _loginConfig;

        private string _accessToken;                                                    //tạo biến _accessToken 
        private DateTime _expiryTime;                                                   //tạo biến _expiryTime
        public TokenService(HttpClient httpClient, LoginConfig loginConfig)
        {
            _httpClient = httpClient;       
            _loginConfig = loginConfig;     
        }
        public async Task<string> GetFullLoginResponseAsync()
        {
            var json = await CallLoginApiAsync();
            return json.RootElement.ToString();
        }
        private async Task LoginAsync()                                                 //hàm lấy access token và expires_in
        {
            var json = await CallLoginApiAsync();                                       //gọi CallLoginApiAsync, trả response vào biến json
            var root = json.RootElement;                                                //RootElement lấy các phần tử trong object của response

            _accessToken = root.GetProperty("access_token").GetString();                //lấy access_token từ respsonse
            var expiresIn = root.GetProperty("expires_in").GetInt32();                  //lấy expires_in từ respsonse

            _expiryTime = DateTime.UtcNow.AddSeconds(expiresIn - 30);                   // _expiryTime: thời điểm thực token hết hạn
        }
        public async Task<string> GetAccessTokenAsync()                                 //hàm kiểm tra, nếu chưa có thì gọi LoginAsync() để lấy token
        {
            if(string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow > _expiryTime)     //kiểm tra token được có chưa, hoặc token còn hạn không
            {
                await LoginAsync();
            }
            return _accessToken;                                                        //đợi LoginAsync rồi lấy access token
        }
        private async Task<JsonDocument> CallLoginApiAsync()
        {
            var loginUrl = "https://stg-accounts-api.xcyber.vn/management/cyberid/login";

            var body = new
            {
                username = _loginConfig.Username,
                password = _loginConfig.Password,
            };

            var json = JsonSerializer.Serialize(body);                                  //body được đổi thành json
            var content = new StringContent(json, Encoding.UTF8, "application/json");   //json ở trên được đóng gói vào StringContent tên là content

            var response = await _httpClient.PostAsync(loginUrl, content);              //thực hiện POST với loginUrl cùng với nội dung content để gửi đi, hàm trả về kết quả và gán cho biến response
            var responseBody = await response.Content.ReadAsStringAsync();              //trả lại repsonse ở dạng string, gán vào biến responseBody

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Failed to login: {response.StatusCode}\nResponse: {responseBody}" //trả về nội dung phản hồi từ server
                );                
            }

            return JsonDocument.Parse(responseBody);                                    //trả về responseBody quay lại về JSON
        }
    }
}