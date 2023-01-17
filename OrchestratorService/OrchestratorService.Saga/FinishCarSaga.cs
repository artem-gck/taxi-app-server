﻿using Automatonymous;
using Contracts.Shared.FinishTheTransaction;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrchestratorService.Saga.State;

namespace OrchestratorService.Saga
{
    public class FinishCarSaga : MassTransitStateMachine<FinishCarSagaState>
    {
        private readonly ILogger<FinishCarSaga> _logger;

        public FinishCarSaga(ILogger<FinishCarSaga> logger)
        {
            _logger = logger;
            FinishCarRequest request = new FinishCarRequest();
            SetFinishStatusToOrderResponse order = new SetFinishStatusToOrderResponse();

            InstanceState(x => x.CurrentState);

            Event(() => FinishCar, x => x.CorrelateById(y => y.Message.Id));

            Request(() => SetFinishStatusToOrder);
            Request(() => CancelSetFinishStatusToOrder);
            Request(() => SetFreeUser);
            Request(() => CancelSetFreeUser);
            Request(() => SetFreeDriver);

            Initially(
                When(FinishCar).Then(x =>
                {
                    if (!x.TryGetPayload(out SagaConsumeContext<FinishCarSagaState, FinishCarRequest> payload))
                        throw new Exception("Unable to retrieve required payload for callback data.");

                    x.Instance.RequestId = payload.RequestId;
                    x.Instance.ResponseAddress = payload.ResponseAddress;
                })
                .Request(SetFinishStatusToOrder, x =>
                {
                    request = x.Data;
                    return x.Init<SetFinishStatusToOrderRequest>(new SetFinishStatusToOrderRequest() 
                    { 
                        OrderId = x.Data.OrderId,
                        Price = x.Data.Price,
                        Distance = x.Data.Distance,
                        Duration = x.Data.Duration
                    });
                })
                .TransitionTo(SetFinishStatusToOrder.Pending)
            );

            During(SetFinishStatusToOrder.Pending,

                When(SetFinishStatusToOrder.Completed)
                    .Then(x => order = x.Data)
                    .Request(SetFreeUser, x => x.Init<SetFreeStatusToUserRequest>(new SetFreeStatusToUserRequest { UserId = x.Data.UserId }))
                    .TransitionTo(SetFreeUser.Pending),

                When(SetFinishStatusToOrder.Faulted)
                   .ThenAsync(async context => { await RespondFromSaga(context, "Faulted On Set Order Status " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message))); })
                   .TransitionTo(FailedSetOrder),

                When(SetFinishStatusToOrder.TimeoutExpired)
                   .ThenAsync(async context => { await RespondFromSaga(context, "Timeout Expired On Set Order Status"); })
                   .TransitionTo(FailedSetOrder)
            );

            During(FailedSetOrder,

                When(SetFinishStatusToOrder.Faulted)
                    .Finalize(),

                 When(SetFinishStatusToOrder.TimeoutExpired)
                    .Finalize()
            );

            During(SetFreeUser.Pending,

                When(SetFreeUser.Completed)
                    .Request(SetFreeDriver, x => x.Init<SetFreeStatusToDriverRequest>(new SetFreeStatusToDriverRequest { DriverId = order.DriverId }))
                    .TransitionTo(SetFreeDriver.Pending),

                When(SetFreeUser.Faulted)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Faulted On Set User Status " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message))); })
                    .TransitionTo(FailedSetUser),

                When(SetFreeUser.TimeoutExpired)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Timeout Expired On Set User Status"); })
                    .TransitionTo(FailedSetUser)
            );

            During(FailedSetUser,

                When(SetFreeUser.Faulted)
                    .Request(CancelSetFinishStatusToOrder, x => x.Init<CancelSetFinishStatusToOrderRequest>(new CancelSetFinishStatusToOrderRequest { OrderId = request.OrderId }))
                    .Finalize(),

                When(SetFreeUser.TimeoutExpired)
                    .Request(CancelSetFinishStatusToOrder, x => x.Init<CancelSetFinishStatusToOrderRequest>(new CancelSetFinishStatusToOrderRequest { OrderId = request.OrderId }))
                    .Finalize()
            );

            During(SetFreeDriver.Pending,

                When(SetFreeDriver.Completed)
                    .ThenAsync(async context => { await RespondFromSaga(context, null); })
                    .Finalize(),

                When(SetFreeDriver.Faulted)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Faulted On Set Driver Status " + string.Join("; ", context.Data.Exceptions.Select(x => x.Message))); })
                    .TransitionTo(FailedSetDriver),

                When(SetFreeDriver.TimeoutExpired)
                    .ThenAsync(async context => { await RespondFromSaga(context, "Timeout Expired On Set Driver Status"); })
                    .TransitionTo(FailedSetDriver)
            );

            During(FailedSetDriver,

                When(SetFreeDriver.Faulted)
                    .Request(CancelSetFinishStatusToOrder, x => x.Init<CancelSetFinishStatusToOrderRequest>(new CancelSetFinishStatusToOrderRequest { OrderId = request.OrderId }))
                    .Request(CancelSetFreeUser, x => x.Init<CancelSetFreeStatusToUserRequest>(new CancelSetFreeStatusToUserRequest { UserId = order.UserId }))
                    .Finalize(),

                When(SetFreeDriver.TimeoutExpired)
                    .Request(CancelSetFinishStatusToOrder, x => x.Init<CancelSetFinishStatusToOrderRequest>(new CancelSetFinishStatusToOrderRequest { OrderId = request.OrderId }))
                    .Request(CancelSetFreeUser, x => x.Init<CancelSetFreeStatusToUserRequest>(new CancelSetFreeStatusToUserRequest { UserId = order.UserId }))
                    .Finalize()
            );
        }

        public Request<FinishCarSagaState, SetFinishStatusToOrderRequest, SetFinishStatusToOrderResponse> SetFinishStatusToOrder { get; set; }
        public Request<FinishCarSagaState, CancelSetFinishStatusToOrderRequest, CancelSetFinishStatusToOrderResponse> CancelSetFinishStatusToOrder { get; set; }
        public Request<FinishCarSagaState, SetFreeStatusToUserRequest, SetFreeStatusToUserResponse> SetFreeUser { get; set; }
        public Request<FinishCarSagaState, CancelSetFreeStatusToUserRequest, CancelSetFreeStatusToUserResponse> CancelSetFreeUser { get; set; }
        public Request<FinishCarSagaState, SetFreeStatusToDriverRequest, SetFreeStatusToDriverResponse> SetFreeDriver { get; set; }

        public Automatonymous.State FailedSetUser { get; set; }
        public Automatonymous.State FailedSetDriver { get; set; }
        public Automatonymous.State FailedSetOrder { get; set; }

        public Event<FinishCarRequest> FinishCar { get; set; }

        private async Task RespondFromSaga<T>(BehaviorContext<FinishCarSagaState, T> context, string error) where T : class
        {
            var endpoint = await context.GetSendEndpoint(context.Instance.ResponseAddress);
            await endpoint.Send(new FinishCarResponse
            {
                OrderId = context.Instance.CorrelationId,
                ErrorMessage = error
            },
            r => r.RequestId = context.Instance.RequestId);
        }
    }
}