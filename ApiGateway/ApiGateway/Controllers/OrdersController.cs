using Contracts.Shared.FinishTheTransaction;
using Contracts.Shared.OrderCarTransaction;
using Contracts.Shared.StartTripTransaction;
using HealthChecks.UI.Core;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : Controller
    {
        private readonly IBus _bus;
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrdersController> logger;
        private readonly string _usersServiceHealthUri;
        private readonly string _driversServiceHealthUri;
        private readonly string _ordersServiceHealthUri;
        private readonly string _orchestratorServiceHealthUri;

        public OrdersController(IBus bus, HttpClient httpClient, IConfiguration configuration, ILogger<OrdersController> logger)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.logger = logger;

            //Насколько правильно передавать IConfiguration через di, или лучше создать модельки и в Programm ее заполнить и прокинуть???

            _usersServiceHealthUri = Environment.GetEnvironmentVariable("UserServiceHealth") ?? configuration.GetConnectionString("UserServiceHealth");
            _driversServiceHealthUri = Environment.GetEnvironmentVariable("DriversServiceHealth") ?? configuration.GetConnectionString("DriversServiceHealth");
            _ordersServiceHealthUri = Environment.GetEnvironmentVariable("OrdersServiceHealth") ?? configuration.GetConnectionString("OrdersServiceHealth");
            _orchestratorServiceHealthUri = Environment.GetEnvironmentVariable("OrchestratorServiceHealth") ?? configuration.GetConnectionString("OrchestratorServiceHealth");
        }

        [HttpPost]
        public async Task<IActionResult> BuyAsync(OrderCarRequest model)
        {
            if (!(await IsApisHealth()))
                return BadRequest("Services unhealthy");

            model.Id = Guid.NewGuid();

            logger.LogWarning("Send");
            var response = await _bus.Request<OrderCarRequest, OrderCarResponse>(model);
            logger.LogWarning("Answer");

            if (string.IsNullOrWhiteSpace(response.Message.ErrorMessage))
                return Ok(response.Message);
            else
                return BadRequest(response.Message);
        }

        [HttpPut("process/{id}")]
        public async Task<IActionResult> ProcessAsync(Guid id)
        {
            if (!(await IsApisHealth()))
                return BadRequest("Services unhealthy");

            var model = new ProcessCarRequest();

            model.Id = Guid.NewGuid();
            model.OrderId = id;

            var response = await _bus.Request<ProcessCarRequest, ProcessCarResponse>(model);

            if (string.IsNullOrWhiteSpace(response.Message.ErrorMessage))
                return Ok(response.Message);
            else
                return BadRequest(response.Message);
        }

        [HttpPut("delete/{id}")]
        public async Task<IActionResult> FinishAsync(FinishCarRequest model, Guid id)
        {
            if (!(await IsApisHealth()))
                return BadRequest("Services unhealthy");

            model.Id = Guid.NewGuid();
            model.OrderId = id;

            var response = await _bus.Request<FinishCarRequest, FinishCarResponse>(model);

            if (string.IsNullOrWhiteSpace(response.Message.ErrorMessage))
                return Ok(response.Message);
            else
                return BadRequest(response.Message);
        }

        private async Task<bool> IsApisHealth()
        {
            try
            {
                var usersServiceResponse = await _httpClient.GetAsync(_usersServiceHealthUri);
                var usersServiceResponseString = await usersServiceResponse.Content.ReadAsStringAsync();
                var usersServiceHealthReport = JsonConvert.DeserializeObject<UIHealthReport>(usersServiceResponseString);
                var usersServiceStatus = usersServiceHealthReport.Status;

                var driversServiceResponse = await _httpClient.GetAsync(_driversServiceHealthUri);
                var driversServiceResponseString = await driversServiceResponse.Content.ReadAsStringAsync();
                var driversServiceHealthReport = JsonConvert.DeserializeObject<UIHealthReport>(driversServiceResponseString);
                var driversServiceStatus = driversServiceHealthReport.Status;

                var ordersServiceResponse = await _httpClient.GetAsync(_ordersServiceHealthUri);
                var ordersServiceResponseString = await ordersServiceResponse.Content.ReadAsStringAsync();
                var ordersServiceHealthReport = JsonConvert.DeserializeObject<UIHealthReport>(ordersServiceResponseString);
                var ordersServiceStatus = ordersServiceHealthReport.Status;

                var orchestratorServiceResponse = await _httpClient.GetAsync(_orchestratorServiceHealthUri);
                var orchestratorServiceResponseString = await orchestratorServiceResponse.Content.ReadAsStringAsync();
                var orchestratorServiceHealthReport = JsonConvert.DeserializeObject<UIHealthReport>(orchestratorServiceResponseString);
                var orchestratorServiceStatus = orchestratorServiceHealthReport.Status;

                if (usersServiceStatus != UIHealthStatus.Healthy ||
                    driversServiceStatus != UIHealthStatus.Healthy ||
                    ordersServiceStatus != UIHealthStatus.Healthy ||
                    orchestratorServiceStatus != UIHealthStatus.Healthy)
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
