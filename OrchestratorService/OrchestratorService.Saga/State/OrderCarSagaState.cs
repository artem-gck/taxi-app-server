using Automatonymous;
using MassTransit;
using MassTransit.Saga;

namespace OrchestratorService.Saga.State
{
    public sealed class OrderCarSagaState : SagaStateMachineInstance, ISagaVersion
    {
        public Guid CorrelationId { get; set; }
        public string? CurrentState { get; set; }
        public Guid? RequestId { get; set; }
        public Uri? ResponseAddress { get; set; }
        public int Version { get; set; }
    }
}
