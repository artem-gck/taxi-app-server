﻿namespace Contracts.Shared.OrderCarTransaction
{
    public class SetWaitingUserStatusResponse
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}
