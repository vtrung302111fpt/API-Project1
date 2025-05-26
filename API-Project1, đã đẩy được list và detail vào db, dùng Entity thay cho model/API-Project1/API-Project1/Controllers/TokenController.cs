using System.Threading.Tasks;
using API_Project1.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_Project1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
            //gán interface của ITokenService
        }
        [HttpPost("get-token")]
        public async Task<IActionResult> GetToken()
        {
            try
            {
                var responseJson = await _tokenService.GetFullLoginResponseAsync();
                //gán kết quả của hàm GetFullLoginResponseAsync vào responseJson

                if (string.IsNullOrEmpty(responseJson))
                    //nếu kết quả đó null
                {
                    return BadRequest("Login failed or resposne was empty.");
                }
                return Content(responseJson, "application/json");
            }
            catch (Exception ex) 
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");     
            }
        }

    }
}
