using ApiGateway.Models;
using Contracts.Shared.OrderCarTransaction;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : Controller
    {
        private readonly IBus _bus;

        public OrdersController(IBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        [HttpPost]
        public async Task<IActionResult> BuyAsync(OrderCarRequestModel model)
        {
            model.Id = Guid.NewGuid();

            var response = await _bus.Request<OrderCarRequest, OrderCarResponse>(model);

            if (string.IsNullOrWhiteSpace(response.Message.ErrorMessage))
                return Ok(response.Message);
            else
                return BadRequest(response.Message);
        }
    }
}
