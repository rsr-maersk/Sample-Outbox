using Sample.Components.StateMachines;

namespace Sample.Components.Consumers;

using Contracts;
using MassTransit;
using MassTransit.Configuration;
using MassTransit.Transports;
using Microsoft.Extensions.Logging;


public class SendRegistrationEmailConsumer :
    IConsumer<SendRegistrationEmail>
{
    readonly ILogger<SendRegistrationEmailConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendRegistrationEmailConsumer(ILogger<SendRegistrationEmailConsumer> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<SendRegistrationEmail> context)
    {
        _logger.LogInformation("Notifying Member {MemberId} that they registered for event {EventId} on {RegistrationDate}", context.Message.MemberId,
        context.Message.EventId, context.Message.RegistrationDate);
        Thread.Sleep(10000);
        await _publishEndpoint.Publish(new RegistrationSubmitted
        {
            RegistrationId = context.Message.RegistrationId,
        });
        
    }
}