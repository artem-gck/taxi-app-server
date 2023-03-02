using Automatonymous;
using Contracts.Shared.StartTripTransaction;
using MassTransit.Saga;

namespace OrchestratorService.Saga.State
{
    public sealed class ProcessCarSagaState : SagaStateMachineInstance//, ISagaVersion
    {
        public Guid CorrelationId { get; set; }
        public string? CurrentState { get; set; }
        public Guid? RequestId { get; set; }
        public Uri? ResponseAddress { get; set; }
        //public int Version { get; set; }
        public ProcessCarRequest? Request { get; set; }
        public SetProcessingStatusToOrderResponse? Order { get; set; }
    }
}
