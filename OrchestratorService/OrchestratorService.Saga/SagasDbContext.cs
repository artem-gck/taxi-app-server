using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;

namespace OrchestratorService.Saga
{
    public class SagasDbContext : SagaDbContext
    {
        public SagasDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations => new ISagaClassMap[]
        {
            new OrderCarSagaStateMap()
        };

    }
}
