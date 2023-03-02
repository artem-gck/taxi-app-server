using Automatonymous;
using MassTransit;
using Contracts.Shared.StartTripTransaction;
using Microsoft.Extensions.Logging;
using OrchestratorService.Saga.State;

namespace OrchestratorService.Saga
{
    public class ProcessCarSaga : MassTransitStateMachine<ProcessCarSagaState>
    {
        //private readonly ILogger<ProcessCarSaga> _logger;

        public ProcessCarSaga(ILogger<ProcessCarSaga> logger)
        {
            

            InstanceState(x => x.CurrentState);

            Event(() => ProcessCar, x => x.CorrelateById(y => y.Message.Id));

            Request(() => SetProcessingStatusToOrder);
            Request(() => CancelSetProcessingStatusToOrder);
            Request(() => SetOnTheTripUser);
            Request(() => CancelSetOnTheTripUser);
            Request(() => SetOnTheTripDriver);

            Initially(
                When(ProcessCar).Then(x =>
                {
                    //logger.LogCritical("try get payload");

                    if (!x.TryGetPayload(out SagaConsumeContext<ProcessCarSagaState, ProcessCarRequest> payload))
                        throw new Exception("Unable to retrieve required payload for callback data.");

                    x.Instance.RequestId = payload.RequestId;
                    x.Instance.ResponseAddress = payload.ResponseAddress;
                })
                .Request(SetProcessingStatusToOrder, x =>
                {
                    x.Instance.Request = x.Data;
                    return x.Init<SetProcessingStatusToOrderRequest>(new SetProcessingStatusToOrderRequest() { OrderId = x.Data.OrderId });
                })
                .TransitionTo(SetProcessingStatusToOrder.Pending)
            );

            During(SetProcessingStatusToOrder.Pending,

                When(SetProcessingStatusToOrder.Completed)
                    .Then(x => x.Instance.Order = x.Data)
                    .Then(x => logger.LogCritical(x.Instance.Order.DriverId.ToString()))
                    .Request(SetOnTheTripUser, x => x.Init<SetOnTheTripUserStatusRequest>(new SetOnTheTripUserStatusRequest { UserId = x.Data.UserId }))
                    .TransitionTo(SetOnTheTripUser.Pending),

                When(SetProcessingStatusToOrder.Faulted)
                   .ThenAsync(async context => { await RespondFromSaga(context, "Faulted On Set Order Status " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message))); })
                   .TransitionTo(FailedSetOrder),

                When(SetProcessingStatusToOrder.TimeoutExpired)
                   .ThenAsync(async context => { await RespondFromSaga(context, "Timeout Expired On Set Order Status"); })
                   .TransitionTo(FailedSetOrder)
            );

            During(FailedSetOrder,

                When(SetProcessingStatusToOrder.Faulted)
                    .Finalize(),

                 When(SetProcessingStatusToOrder.TimeoutExpired)
                    .Finalize()
            );

            During(SetOnTheTripUser.Pending,

                When(SetOnTheTripUser.Completed)
                    .Then(x => logger.LogCritical(x.Instance.Order.DriverId.ToString()))
                    .Request(SetOnTheTripDriver, x => x.Init<SetOnTheTripStatusRequest>(new SetOnTheTripStatusRequest { DriverId = x.Instance.Order.DriverId }))
                    .Then(x => logger.LogCritical(x.Instance.Order.DriverId.ToString()))
                    .TransitionTo(SetOnTheTripDriver.Pending),

                When(SetOnTheTripUser.Faulted)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Faulted On Set User Status " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message))); })
                    .TransitionTo(FailedSetUser),

                When(SetOnTheTripUser.TimeoutExpired)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Timeout Expired On Set User Status"); })
                    .TransitionTo(FailedSetUser)
            );

            During(FailedSetUser,

                When(SetOnTheTripUser.Faulted)
                    .Request(CancelSetProcessingStatusToOrder, x => x.Init<CancelSetProcessingStatusToOrderRequest>(new CancelSetProcessingStatusToOrderRequest { OrderId = x.Instance.Request.OrderId }))
                    .Finalize(),

                When(SetOnTheTripUser.TimeoutExpired)
                    .Request(CancelSetProcessingStatusToOrder, x => x.Init<CancelSetProcessingStatusToOrderRequest>(new CancelSetProcessingStatusToOrderRequest { OrderId = x.Instance.Request.OrderId }))
                    .Finalize()
            );

            During(SetOnTheTripDriver.Pending,

                When(SetOnTheTripDriver.Completed)
                    .ThenAsync(async context => { await RespondFromSaga(context, null); })
                    .Finalize(),

                When(SetOnTheTripDriver.Faulted)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Faulted On Set Driver Status " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message))); })
                    .TransitionTo(FailedSetDriver),

                When(SetOnTheTripDriver.TimeoutExpired)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Timeout Expired On Set Driver Status"); })
                    .TransitionTo(FailedSetDriver)
            );

            During(FailedSetDriver,

                When(SetProcessingStatusToOrder.Faulted)
                    .Request(CancelSetProcessingStatusToOrder, x => x.Init<CancelSetProcessingStatusToOrderRequest>(new CancelSetProcessingStatusToOrderRequest { OrderId = x.Instance.Request.OrderId }))
                    .Request(CancelSetOnTheTripUser, x => x.Init<CancelSetOnTheTripUserStatusRequest>(new CancelSetOnTheTripUserStatusRequest { UserId = x.Instance.Order.UserId }))
                    .Finalize(),

                When(SetProcessingStatusToOrder.TimeoutExpired)
                    .Request(CancelSetProcessingStatusToOrder, x => x.Init<CancelSetProcessingStatusToOrderRequest>(new CancelSetProcessingStatusToOrderRequest { OrderId = x.Instance.Request.OrderId }))
                    .Request(CancelSetOnTheTripUser, x => x.Init<CancelSetOnTheTripUserStatusRequest>(new CancelSetOnTheTripUserStatusRequest { UserId = x.Instance.Order.UserId }))
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }

        public Request<ProcessCarSagaState, SetProcessingStatusToOrderRequest, SetProcessingStatusToOrderResponse> SetProcessingStatusToOrder { get; set; }
        public Request<ProcessCarSagaState, CancelSetProcessingStatusToOrderRequest, CancelSetProcessingStatusToOrderResponse> CancelSetProcessingStatusToOrder { get; set; }
        public Request<ProcessCarSagaState, SetOnTheTripUserStatusRequest, SetOnTheTripUserStatusResponse> SetOnTheTripUser { get; set; }
        public Request<ProcessCarSagaState, CancelSetOnTheTripUserStatusRequest, CancelSetOnTheTripUserStatusResponse> CancelSetOnTheTripUser { get; set; }
        public Request<ProcessCarSagaState, SetOnTheTripStatusRequest, SetOnTheTripStatusResponse> SetOnTheTripDriver { get; set; }

        public Automatonymous.State FailedSetUser { get; set; }
        public Automatonymous.State FailedSetDriver { get; set; }
        public Automatonymous.State FailedSetOrder { get; set; }

        public Event<ProcessCarRequest> ProcessCar { get; set; }

        private async Task RespondFromSaga<T>(BehaviorContext<ProcessCarSagaState, T> context, string error) where T : class
        {
            //_logger.LogCritical("RespondFromSaga");
            //_logger.LogCritical(context.Instance.ResponseAddress.ToString());

            var endpoint = await context.GetSendEndpoint(context.Instance.ResponseAddress);
            if (string.IsNullOrWhiteSpace(error))
            {
                await endpoint.Send(new ProcessCarResponse
                {
                    CorrelationId = context.Instance.CorrelationId,
                    OrderId = context.Instance.Order.OrderId,
                    ErrorMessage = error
                },
                r => r.RequestId = context.Instance.RequestId);
            }
            else
            {
                await endpoint.Send(new ProcessCarResponse
                {
                    CorrelationId = context.Instance.CorrelationId,
                    ErrorMessage = error
                },
                r => r.RequestId = context.Instance.RequestId);
            }
        }
    }
}
