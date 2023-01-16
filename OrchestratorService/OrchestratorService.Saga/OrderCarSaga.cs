using Automatonymous;
using Contracts.Shared.OrderCarTransaction;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrchestratorService.Saga.State;

namespace OrchestratorService.Saga
{
    public class OrderCarSaga : MassTransitStateMachine<OrderCarSagaState>
    {
        public OrderCarSaga(ILogger<OrderCarSaga> logger)
        {
            OrderCarRequest request = new OrderCarRequest();

            InstanceState(x => x.CurrentState);

            Event(() => OrderCar, x => x.CorrelateById(y => y.Message.Id));

            Request(() => SetWaitingUser);
            Request(() => CancelSetWaitingUser);
            Request(() => SetGoesToUserDriver);
            Request(() => CancelSetGoesToUserDriver);
            Request(() => AddOrder);

            Initially(
                When(OrderCar).Then(x => 
                    {
                        if (!x.TryGetPayload(out SagaConsumeContext<OrderCarSagaState, OrderCarRequest> payload))
                            throw new Exception("Unable to retrieve required payload for callback data.");
                        x.Instance.RequestId = payload.RequestId;
                        x.Instance.ResponseAddress = payload.ResponseAddress;
                    })
                    .Request(SetWaitingUser, x =>
                    {
                        request = x.Data;

                        return x.Init<SetWaitingUserStatusRequest>(new SetWaitingUserStatusRequest() { UserId = x.Data.UserId });
                    })
                    .TransitionTo(SetWaitingUser.Pending)
            );

            During(SetWaitingUser.Pending,

                When(SetWaitingUser.Completed)
                    .Request(SetGoesToUserDriver, x => x.Init<SetGoesToUserDriverStatusRequest>(new SetGoesToUserDriverStatusRequest { DriverId = request.DriverId }))
                    .TransitionTo(SetGoesToUserDriver.Pending),

                When(SetWaitingUser.Faulted)
                   .ThenAsync(async context =>
                   {
                       await RespondFromSaga(context, "Faulted On Get Money " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message)));
                   })
                   .Finalize(),

                When(SetWaitingUser.TimeoutExpired)
                   .ThenAsync(async context =>
                   {
                       await RespondFromSaga(context, "Timeout Expired On Get Money");
                   })
                   .Finalize()
            );

            During(SetGoesToUserDriver.Pending,

                When(SetGoesToUserDriver.Completed)
                    .Request(AddOrder, x => x.Init<AddOrderRequest>(new AddOrderRequest
                    {
                        UserId = request.UserId,
                        UserName = request.UserName,
                        UserSurname = request.UserSurname,
                        DriverId = request.DriverId,
                        DriverName = request.DriverName,
                        DriverSurname = request.DriverSurname,
                        StartLatitude = request.StartLatitude,
                        StartLongitude = request.StartLongitude,
                        FinishLatitude = request.FinishLatitude,
                        FinishLongitude = request.FinishLongitude
                    }))
                    .TransitionTo(AddOrder.Pending),

                When(SetGoesToUserDriver.Faulted)
                    .Request(CancelSetWaitingUser, x => x.Init<CancelSetWaitingUserStatusRequest>(new CancelSetWaitingUserStatusRequest { UserId = request.UserId }))
                    .ThenAsync(async context =>
                    {
                        await RespondFromSaga(context, "Faulted On Get Money " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message)));
                    })
                    .Finalize(),

                When(SetGoesToUserDriver.TimeoutExpired)
                    .Request(CancelSetWaitingUser, x => x.Init<CancelSetWaitingUserStatusRequest>(new CancelSetWaitingUserStatusRequest { UserId = request.UserId }))
                    .ThenAsync(async context =>
                    {
                        await RespondFromSaga(context, "Timeout Expired On Get Money");
                    })
                    .Finalize()
            );

            During(AddOrder.Pending,

                When(AddOrder.Completed)
                    .ThenAsync(async context =>
                    {
                        await RespondFromSaga(context, null);
                    })
                    .Finalize(),

                When(AddOrder.Faulted)
                    .Request(CancelSetWaitingUser, x => x.Init<CancelSetWaitingUserStatusRequest>(new CancelSetWaitingUserStatusRequest { UserId = request.UserId }))
                    .Request(CancelSetGoesToUserDriver, x => x.Init<CancelSetGoesToUserDriverStatusRequest>(new CancelSetGoesToUserDriverStatusRequest { DriverId = request.DriverId }))
                    .ThenAsync(async context =>
                    {
                        await RespondFromSaga(context, "Faulted On Get Items " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message)));
                    })
                    .Finalize(),

                When(AddOrder.TimeoutExpired)
                    .Request(CancelSetWaitingUser, x => x.Init<CancelSetWaitingUserStatusRequest>(new CancelSetWaitingUserStatusRequest { UserId = request.UserId }))
                    .Request(CancelSetGoesToUserDriver, x => x.Init<CancelSetGoesToUserDriverStatusRequest>(new CancelSetGoesToUserDriverStatusRequest { DriverId = request.DriverId }))
                    .ThenAsync(async context =>
                    {
                        await RespondFromSaga(context, "Timeout Expired On Get Items");
                    })
                    .Finalize()
            );
        }

        public Request<OrderCarSagaState, SetWaitingUserStatusRequest, SetWaitingUserStatusResponse> SetWaitingUser { get; set; }
        public Request<OrderCarSagaState, CancelSetWaitingUserStatusRequest, CancelSetWaitingUserStatusResponse> CancelSetWaitingUser { get; set; }
        public Request<OrderCarSagaState, SetGoesToUserDriverStatusRequest, SetGoesToUserDriverStatusResponse> SetGoesToUserDriver { get; set; }
        public Request<OrderCarSagaState, CancelSetGoesToUserDriverStatusRequest, CancelSetGoesToUserDriverStatusResponse> CancelSetGoesToUserDriver { get; set; }
        public Request<OrderCarSagaState, AddOrderRequest, AddOrderResponse> AddOrder { get; set; }

        public Event<OrderCarRequest> OrderCar { get; set; }

        private static async Task RespondFromSaga<T>(BehaviorContext<OrderCarSagaState, T> context, string error) where T : class
        {
            var endpoint = await context.GetSendEndpoint(context.Instance.ResponseAddress);
            await endpoint.Send(new OrderCarResponse
            {
                OrderId = context.Instance.CorrelationId,
                ErrorMessage = error
            }, 
            r => r.RequestId = context.Instance.RequestId);
        }
    }
}