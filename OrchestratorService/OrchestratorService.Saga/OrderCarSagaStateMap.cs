using MassTransit;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OrchestratorService.Saga.State;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;

namespace OrchestratorService.Saga
{
    public class OrderCarSagaStateMap : SagaClassMap<OrderCarSagaState>
    {
        protected override void Configure(EntityTypeBuilder<OrderCarSagaState> entity, ModelBuilder model)
        {
            base.Configure(entity, model);
            entity.Property(x => x.CurrentState).HasMaxLength(255);
        }

    }
}
