﻿namespace Contracts.Shared.StartTripTransaction
{
    public class SetProcessingStatusToOrderResponse
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public Guid DriverId { get; set; }
    }
}
