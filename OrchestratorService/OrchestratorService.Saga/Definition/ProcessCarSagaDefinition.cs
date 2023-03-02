using MassTransit.Azure.ServiceBus.Core;
using MassTransit;
using MassTransit.Definition;
using OrchestratorService.Saga.State;

namespace OrchestratorService.Saga.Definition
{
    public class ProcessCarSagaDefinition : SagaDefinition<ProcessCarSagaState>
    {
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<ProcessCarSagaState> sagaConfigurator)
        {
            if (endpointConfigurator is IServiceBusReceiveEndpointConfigurator sb)
                sb.RequiresSession = true;
        }
    }
}
