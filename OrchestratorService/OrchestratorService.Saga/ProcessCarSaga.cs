using Automatonymous;
using MassTransit;
using Contracts.Shared.StartTripTransaction;
using Microsoft.Extensions.Logging;
using OrchestratorService.Saga.State;

namespace OrchestratorService.Saga
{
    public class ProcessCarSaga : MassTransitStateMachine<ProcessCarSagaState>
    {
        private readonly ILogger<ProcessCarSaga> _logger;

        public ProcessCarSaga(ILogger<ProcessCarSaga> logger)
        {
            _logger = logger;
            ProcessCarRequest request = new ProcessCarRequest();

            InstanceState(x => x.CurrentState);

            Event(() => ProcessCar, x => x.CorrelateById(y => y.Message.Id));

            Request(() => SetOnTheTripUser);
            Request(() => CancelSetOnTheTripUser);
            Request(() => SetOnTheTripDriver);
            Request(() => CancelSetOnTheTripDriver);
            Request(() => SetProcessingStatusToOrder);

            Initially(
                When(ProcessCar).Then(x =>
                {
                    logger.LogCritical("try get payload");

                    if (!x.TryGetPayload(out SagaConsumeContext<ProcessCarSagaState, ProcessCarRequest> payload))
                        throw new Exception("Unable to retrieve required payload for callback data.");

                    //logger.LogCritical(x.Data.ResponseAddress.ToString());

                    logger.LogCritical(payload.RequestId.ToString());
                    logger.LogCritical(payload.ResponseAddress.ToString());

                    x.Instance.RequestId = payload.RequestId;
                    x.Instance.ResponseAddress = payload.ResponseAddress;
                })
                .Request(SetOnTheTripUser, x =>
                {
                    request = x.Data;
                    logger.LogCritical(request.OrderId.ToString());
                    return x.Init<SetOnTheTripUserStatusRequest>(new SetOnTheTripUserStatusRequest() { UserId = x.Data.UserId });
                })
                .TransitionTo(SetOnTheTripUser.Pending)
            );

            During(SetOnTheTripUser.Pending,

                When(SetOnTheTripUser.Completed)
                    .Then(context => { logger.LogCritical("SetOnTheTripUser.Completed"); })
                    .Request(SetOnTheTripDriver, x => x.Init<SetOnTheTripStatusRequest>(new SetOnTheTripStatusRequest { DriverId = request.DriverId }))
                    .TransitionTo(SetOnTheTripDriver.Pending),

                When(SetOnTheTripUser.Faulted)
                   .ThenAsync(async context => {  await RespondFromSaga(context, "Faulted On Set User Status " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message))); })
                   .TransitionTo(FailedSetUser),

                When(SetOnTheTripUser.TimeoutExpired)
                   .ThenAsync(async context => { await RespondFromSaga(context, "Timeout Expired On Set User Status"); })
                   .TransitionTo(FailedSetUser)
            );

            During(FailedSetUser,

                When(SetOnTheTripUser.Faulted)
                    .Finalize(),

                 When(SetOnTheTripUser.TimeoutExpired)
                    .Finalize()
            );

            During(SetOnTheTripDriver.Pending,

                When(SetOnTheTripDriver.Completed)
                    .Then(context => { logger.LogCritical("SetOnTheTripDriver.Completed"); })
                    .Request(SetProcessingStatusToOrder, x => x.Init<SetProcessingStatusToOrderRequest>(new SetProcessingStatusToOrderRequest { OrderId = request.OrderId }))
                    .TransitionTo(SetProcessingStatusToOrder.Pending),

                When(SetOnTheTripDriver.Faulted)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Faulted On Set Driver Status " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message))); })
                    .TransitionTo(FailedSetDriver),

                When(SetOnTheTripDriver.TimeoutExpired)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Timeout Expired On Set Driver Status"); })
                    .TransitionTo(FailedSetDriver)
            );

            During(FailedSetDriver,

                When(SetOnTheTripDriver.Faulted)
                    .Request(CancelSetOnTheTripUser, x => x.Init<CancelSetOnTheTripUserStatusRequest>(new CancelSetOnTheTripUserStatusRequest { UserId = request.UserId }))
                    .Finalize(),

                When(SetOnTheTripDriver.TimeoutExpired)
                    .Request(CancelSetOnTheTripUser, x => x.Init<CancelSetOnTheTripUserStatusRequest>(new CancelSetOnTheTripUserStatusRequest { UserId = request.UserId }))
                    .Finalize()
            );

            During(SetProcessingStatusToOrder.Pending,

                When(SetProcessingStatusToOrder.Completed)
                    .Then(context => { logger.LogCritical("SetProcessingStatusToOrder.Completed"); })
                    .ThenAsync(async context => { await RespondFromSaga(context, null); })
                    .Finalize(),

                When(SetProcessingStatusToOrder.Faulted)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Faulted On Set Order Status " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message))); })
                    .TransitionTo(FailedSetOrder),

                When(SetProcessingStatusToOrder.TimeoutExpired)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Timeout Expired On Set Order Status"); })
                    .TransitionTo(FailedSetOrder)
            );

            During(FailedSetOrder,

                When(SetProcessingStatusToOrder.Faulted)
                    .Request(CancelSetOnTheTripUser, x => x.Init<CancelSetOnTheTripUserStatusRequest>(new CancelSetOnTheTripUserStatusRequest { UserId = request.UserId }))
                    .Request(CancelSetOnTheTripDriver, x => x.Init<CancelSetOnTheTripStatusRequest>(new CancelSetOnTheTripStatusRequest { DriverId = request.DriverId }))
                    .Finalize(),

                When(SetProcessingStatusToOrder.TimeoutExpired)
                    .Request(CancelSetOnTheTripUser, x => x.Init<CancelSetOnTheTripUserStatusRequest>(new CancelSetOnTheTripUserStatusRequest { UserId = request.UserId }))
                    .Request(CancelSetOnTheTripDriver, x => x.Init<CancelSetOnTheTripStatusRequest>(new CancelSetOnTheTripStatusRequest { DriverId = request.DriverId }))
                    .Finalize()
            );
        }

        public Request<ProcessCarSagaState, SetOnTheTripUserStatusRequest, SetOnTheTripUserStatusResponse> SetOnTheTripUser { get; set; }
        public Request<ProcessCarSagaState, CancelSetOnTheTripUserStatusRequest, CancelSetOnTheTripUserStatusResponse> CancelSetOnTheTripUser { get; set; }
        public Request<ProcessCarSagaState, SetOnTheTripStatusRequest, SetOnTheTripStatusResponse> SetOnTheTripDriver { get; set; }
        public Request<ProcessCarSagaState, CancelSetOnTheTripStatusRequest, CancelSetOnTheTripStatusResponse> CancelSetOnTheTripDriver { get; set; }
        public Request<ProcessCarSagaState, SetProcessingStatusToOrderRequest, SetProcessingStatusToOrderResponse> SetProcessingStatusToOrder { get; set; }

        public Automatonymous.State FailedSetUser { get; set; }
        public Automatonymous.State FailedSetDriver { get; set; }
        public Automatonymous.State FailedSetOrder { get; set; }

        public Event<ProcessCarRequest> ProcessCar { get; set; }

        private async Task RespondFromSaga<T>(BehaviorContext<ProcessCarSagaState, T> context, string error) where T : class
        {
            _logger.LogCritical("RespondFromSaga");
            _logger.LogCritical(context.Instance.ResponseAddress.ToString());

            var endpoint = await context.GetSendEndpoint(context.Instance.ResponseAddress);
            await endpoint.Send(new ProcessCarResponse
            {
                OrderId = context.Instance.CorrelationId,
                ErrorMessage = error
            },
            r => r.RequestId = context.Instance.RequestId);
        }
    }
}
