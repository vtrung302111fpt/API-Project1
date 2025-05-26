using System.Net.Http.Headers;
using System.Threading.Tasks;
using API_Project1.Interfaces;
using API_Project1.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_Project1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserInfoController : ControllerBase
    {
        private readonly IUserInfoService _userInfoService;

        public UserInfoController(IUserInfoService userInfoService)
        {
            _userInfoService = userInfoService;
        }

        [HttpGet("get-user-info")]
        public async Task<IActionResult> GetUserInfoAsync()
        {
            try
            {
                var content = await _userInfoService.GetUserInfoAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }

}
