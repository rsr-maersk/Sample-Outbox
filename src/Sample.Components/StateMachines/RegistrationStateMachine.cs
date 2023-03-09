using MassTransit.SagaStateMachine;

namespace Sample.Components.StateMachines;

using Contracts;
using MassTransit;
using MassTransit.Configuration;


public class RegistrationStateMachine :
    MassTransitStateMachine<RegistrationState>
{
    public RegistrationStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => RegistrationSubmitted, x => x.CorrelateById(m => m.Message.RegistrationId));
        Event(() => RegistrationRestart, x => x.CorrelateById(m => m.Message.RegistrationId));
        
        Schedule(() => DelayStart, x => x.DelayStartTokenId, x => x.Received = e => e.CorrelateById(m => m.Message.CorrelationId));

        Initially(
            WhenRegistration(RegistrationSubmitted)
            //, WhenRegistration(RegistrationRestart!)
            ); 
        
        DuringAny(WhenRegistration(RegistrationRestart!));

        During(Delayed,
            When(DelayStart!.Received)
                .TransitionTo(Complete));


    }

   
    private EventActivityBinder<RegistrationState, T> WhenRegistration<T>(Event<T> registrationRestart)
        where T : class, IOutboxRegistration
    {
        return When(registrationRestart)
            .Then(context =>
            {
                context.Saga.RegistrationDate = context.Message.RegistrationDate;
                context.Saga.EventId = context.Message.EventId;
                context.Saga.MemberId = context.Message.MemberId;
                context.Saga.Payment = context.Message.Payment;
            })
            .TransitionTo(Registered)
            .Schedule(DelayStart, x => new DelayStart { CorrelationId = x.Saga.CorrelationId },
                _ => TimeSpan.FromSeconds(10))
            .TransitionTo(Delayed);
    }

    public Event<RegistrationRestart> RegistrationRestart { get; set; }
    
    public Schedule<RegistrationState, DelayStart> DelayStart { get; set; }
    public State Delayed { get; private set; }

    public Event OrderAccepted { get; set; }

    public State Submitted { get; set; }

    public Event SubmitOrder { get; set; }

    public State RetryState { get; set; }



    
    //
    // ReSharper disable MemberCanBePrivate.Global
    public State Registered { get; } = null!;
    public Event<RegistrationSubmitted> RegistrationSubmitted { get; } = null!;
    public State Complete { get; } = null!;
}

public class DelayStart :
    CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
}

