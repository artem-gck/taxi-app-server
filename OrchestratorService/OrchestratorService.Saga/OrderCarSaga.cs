using Automatonymous;
using Contracts.Shared.OrderCarTransaction;
using DnsClient.Internal;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrchestratorService.Saga.State;

namespace OrchestratorService.Saga
{
    public class OrderCarSaga : MassTransitStateMachine<OrderCarSagaState>
    {
        private static ILogger<OrderCarSaga> _logger;

        public OrderCarSaga(ILogger<OrderCarSaga> logger)
        {
            _logger= logger;

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
                        x.Instance.Request = x.Data;

                        return x.Init<SetWaitingUserStatusRequest>(new SetWaitingUserStatusRequest() { UserId = x.Data.UserId });
                    })
                    .TransitionTo(SetWaitingUser.Pending)
            );

            During(SetWaitingUser.Pending,

                When(SetWaitingUser.Completed)
                    .Then(x => x.Instance.UserResponse = x.Data)
                    .Request(SetGoesToUserDriver, x => x.Init<SetGoesToUserDriverStatusRequest>(new SetGoesToUserDriverStatusRequest { DriverId = x.Instance.Request.DriverId }))
                    .TransitionTo(SetGoesToUserDriver.Pending),

                When(SetWaitingUser.Faulted)
                   .ThenAsync(async context => { await RespondFromSaga(context, "Faulted On Set User Status " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message))); })
                   .TransitionTo(FailedSetUser),

                When(SetWaitingUser.TimeoutExpired)
                   .ThenAsync(async context => { await RespondFromSaga(context, "Timeout Expired On Set User Status"); })
                   .TransitionTo(FailedSetUser)
            );

            During(FailedSetUser,

                When(SetWaitingUser.Faulted)
                    .Finalize(),

                 When(SetWaitingUser.TimeoutExpired)
                    .Finalize()
            );

            During(SetGoesToUserDriver.Pending,

                When(SetGoesToUserDriver.Completed)
                    .Request(AddOrder, x => x.Init<AddOrderRequest>(new AddOrderRequest
                    {
                        UserId = x.Instance.Request.UserId,
                        UserName = x.Instance.UserResponse.Name,
                        UserSurname = x.Instance.UserResponse.Surname,
                        DriverId = x.Instance.Request.DriverId,
                        DriverName = x.Data.Name,
                        DriverSurname = x.Data.Surname,
                        StartLatitude = x.Instance.Request.StartLatitude,
                        StartLongitude = x.Instance.Request.StartLongitude,
                        FinishLatitude = x.Instance.Request.FinishLatitude,
                        FinishLongitude = x.Instance.Request.FinishLongitude
                    }))
                    .TransitionTo(AddOrder.Pending),

                When(SetGoesToUserDriver.Faulted)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Faulted On Set Driver Status " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message))); })
                    .TransitionTo(FailedSetDriver),

                When(SetGoesToUserDriver.TimeoutExpired)     
                    .ThenAsync(async context => { await RespondFromSaga(context, "Timeout Expired On Set Driver Status"); })
                    .TransitionTo(FailedSetDriver)
            );

            During(FailedSetDriver,

                When(SetGoesToUserDriver.Faulted)
                    .Request(CancelSetWaitingUser, x => x.Init<CancelSetWaitingUserStatusRequest>(new CancelSetWaitingUserStatusRequest { UserId = x.Instance.Request.UserId }))
                    .Finalize(),

                When(SetGoesToUserDriver.TimeoutExpired)
                    .Request(CancelSetWaitingUser, x => x.Init<CancelSetWaitingUserStatusRequest>(new CancelSetWaitingUserStatusRequest { UserId = x.Instance.Request.UserId }))
                    .Finalize()
            );

            During(AddOrder.Pending,

                When(AddOrder.Completed)
                    .ThenAsync(async context => {  await RespondFromSaga(context, null); })
                    .Finalize(),

                When(AddOrder.Faulted)
                    .ThenAsync(async context => {  await RespondFromSaga(context, "Faulted On Add Order " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message))); })
                    .TransitionTo(FailedAddOrder),

                When(AddOrder.TimeoutExpired)
                    .Request(CancelSetWaitingUser, x => x.Init<CancelSetWaitingUserStatusRequest>(new CancelSetWaitingUserStatusRequest { UserId = x.Instance.Request.UserId }))
                    .Request(CancelSetGoesToUserDriver, x => x.Init<CancelSetGoesToUserDriverStatusRequest>(new CancelSetGoesToUserDriverStatusRequest { DriverId = x.Instance.Request.DriverId }))
                    .ThenAsync(async context => { await RespondFromSaga(context, "Timeout Expired On Add Order"); })
                    .TransitionTo(FailedAddOrder)
            );

            During(FailedAddOrder,

                When(AddOrder.Faulted)
                    .Request(CancelSetWaitingUser, x => x.Init<CancelSetWaitingUserStatusRequest>(new CancelSetWaitingUserStatusRequest { UserId = x.Instance.Request.UserId }))
                    .Request(CancelSetGoesToUserDriver, x => x.Init<CancelSetGoesToUserDriverStatusRequest>(new CancelSetGoesToUserDriverStatusRequest { DriverId = x.Instance.Request.DriverId }))
                    .Finalize(),

                When(AddOrder.TimeoutExpired)
                    .Request(CancelSetWaitingUser, x => x.Init<CancelSetWaitingUserStatusRequest>(new CancelSetWaitingUserStatusRequest { UserId = x.Instance.Request.UserId }))
                    .Request(CancelSetGoesToUserDriver, x => x.Init<CancelSetGoesToUserDriverStatusRequest>(new CancelSetGoesToUserDriverStatusRequest { DriverId = x.Instance.Request.DriverId }))
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }

        public Request<OrderCarSagaState, SetWaitingUserStatusRequest, SetWaitingUserStatusResponse> SetWaitingUser { get; set; }
        public Request<OrderCarSagaState, CancelSetWaitingUserStatusRequest, CancelSetWaitingUserStatusResponse> CancelSetWaitingUser { get; set; }
        public Request<OrderCarSagaState, SetGoesToUserDriverStatusRequest, SetGoesToUserDriverStatusResponse> SetGoesToUserDriver { get; set; }
        public Request<OrderCarSagaState, CancelSetGoesToUserDriverStatusRequest, CancelSetGoesToUserDriverStatusResponse> CancelSetGoesToUserDriver { get; set; }
        public Request<OrderCarSagaState, AddOrderRequest, AddOrderResponse> AddOrder { get; set; }

        public Automatonymous.State FailedSetUser { get; set; }
        public Automatonymous.State FailedSetDriver { get; set; }
        public Automatonymous.State FailedAddOrder { get; set; }

        public Event<OrderCarRequest> OrderCar { get; set; }

        private static async Task RespondFromSaga<T>(BehaviorContext<OrderCarSagaState, T> context, string error) where T : class
        {
            var endpoint = await context.GetSendEndpoint(context.Instance.ResponseAddress);
            
            if (string.IsNullOrWhiteSpace(error))
            {
                _logger.LogWarning($"{context.Instance.ResponseAddress}");
                
                await endpoint.Send(new OrderCarResponse
                {
                    CorrelationId = context.Instance.CorrelationId,
                    OrderId = (context as BehaviorContext<OrderCarSagaState, AddOrderResponse>).Data.OrderId,
                    ErrorMessage = error
                },
                r => r.RequestId = context.Instance.RequestId);
            }
            else
            {
                await endpoint.Send(new OrderCarResponse
                {
                    CorrelationId = context.Instance.CorrelationId,
                    ErrorMessage = error
                },
                r => r.RequestId = context.Instance.RequestId);
            }
        }
    }
}