using MassTransit.Azure.ServiceBus.Core;
using MassTransit;
using MassTransit.Definition;
using OrchestratorService.Saga.State;

namespace OrchestratorService.Saga.Definition
{
    public class FinishCarSagaDefinition : SagaDefinition<FinishCarSagaState>
    {
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FinishCarSagaState> sagaConfigurator)
        {
            if (endpointConfigurator is IServiceBusReceiveEndpointConfigurator sb)
                sb.RequiresSession = true;
        }
    }
}
