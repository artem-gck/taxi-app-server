using Contracts.Shared.GetAllDrivers;
using HealthChecks.UI.Core;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/drivers")]
    public class DriversController : Controller
    {
        private readonly IBus _bus;
        private readonly HttpClient _httpClient;
        private readonly string _driversServiceHealthUri;

        public DriversController(IBus bus, HttpClient httpClient, IConfiguration configuration)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            _driversServiceHealthUri = Environment.GetEnvironmentVariable("DriversServiceHealth") ?? configuration.GetConnectionString("DriversServiceHealth");
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(string status)
        {
            if (!(await IsApisHealth()))
                return BadRequest("Services unhealthy");

            var model = new GetAllDriversRequest()
            {
                Status = status
            };

            var response = await _bus.Request<GetAllDriversRequest, GetAllDriversResponse>(model);

            return Ok(response.Message);
        }

        private async Task<bool> IsApisHealth()
        {
            try
            {
                var driversServiceResponse = await _httpClient.GetAsync(_driversServiceHealthUri);
                var driversServiceResponseString = await driversServiceResponse.Content.ReadAsStringAsync();
                var driversServiceHealthReport = JsonConvert.DeserializeObject<UIHealthReport>(driversServiceResponseString);
                var driversServiceStatus = driversServiceHealthReport.Status;

                if (driversServiceStatus != UIHealthStatus.Healthy)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
