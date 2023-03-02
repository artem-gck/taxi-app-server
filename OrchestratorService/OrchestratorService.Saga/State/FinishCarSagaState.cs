using Automatonymous;
using Contracts.Shared.FinishTheTransaction;
using MassTransit.Saga;

namespace OrchestratorService.Saga.State
{
    public sealed class FinishCarSagaState : SagaStateMachineInstance//, ISagaVersion
    {
        public Guid CorrelationId { get; set; }
        public string? CurrentState { get; set; }
        public Guid? RequestId { get; set; }
        public Uri? ResponseAddress { get; set; }
        //public int Version { get; set; }
        public FinishCarRequest? Request { get; set; }
        public SetFinishStatusToOrderResponse? Order { get; set; }
    }
}
