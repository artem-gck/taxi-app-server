using Automatonymous;
using Contracts.Shared.OrderCarTransaction;
using MassTransit.Saga;

namespace OrchestratorService.Saga.State
{
    public sealed class OrderCarSagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string? CurrentState { get; set; }
        public Guid? RequestId { get; set; }
        public Uri? ResponseAddress { get; set; }
        public OrderCarRequest? Request { get; set; }
        public SetWaitingUserStatusResponse? UserResponse { get; set; }
    }
}
