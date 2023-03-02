using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.Definition;
using OrchestratorService.Saga.State;

namespace OrchestratorService.Saga.Definition
{
    public class OrderCarSagaDefinition : SagaDefinition<OrderCarSagaState>
    {
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OrderCarSagaState> sagaConfigurator)
        {
            if (endpointConfigurator is IServiceBusReceiveEndpointConfigurator sb)
                sb.RequiresSession = true;
        }
    }
}
