using Contracts.Shared.GetUserState;
using HealthChecks.UI.Core;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private readonly IBus _bus;
        private readonly HttpClient _httpClient;
        private readonly ILogger<UsersController> _logger;
        private readonly string _usersServiceHealthUri;

        public UsersController(IBus bus, HttpClient httpClient, IConfiguration configuration, ILogger<UsersController> logger)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;
            _usersServiceHealthUri = Environment.GetEnvironmentVariable("UserServiceHealth") ?? configuration.GetConnectionString("UserServiceHealth");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            if (!(await IsApisHealth()))
                return BadRequest("Services unhealthy");

            _logger.LogCritical(id.ToString());

            var model = new GetUserStateRequest()
            {
                UserId = id
            };

            var response = await _bus.Request<GetUserStateRequest, GetUserStateResponse>(model);

            return Ok(response.Message);
        }

        private async Task<bool> IsApisHealth()
        {
            try
            {
                var usersServiceResponse = await _httpClient.GetAsync(_usersServiceHealthUri);
                var usersServiceResponseString = await usersServiceResponse.Content.ReadAsStringAsync();
                var usersServiceHealthReport = JsonConvert.DeserializeObject<UIHealthReport>(usersServiceResponseString);
                var usersServiceStatus = usersServiceHealthReport.Status;

                if (usersServiceStatus != UIHealthStatus.Healthy)
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
