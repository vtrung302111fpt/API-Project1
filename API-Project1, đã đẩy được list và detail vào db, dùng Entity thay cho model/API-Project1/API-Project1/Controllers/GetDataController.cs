using API_Project1.Interfaces;
using API_Project1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_Project1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetDataController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IGetDataService _getDataService;

        public GetDataController(AppDbContext dbContext, IGetDataService getDataService)
        {
            _dbContext = dbContext;
            _getDataService = getDataService;
        }

        [HttpGet(Name = "get-data")]
        public async Task<IActionResult> GetDataAsync(int currentPage = 0)
        {
            try
            {
                var result = await _getDataService.GetDataAsync(currentPage);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }

    }
}
