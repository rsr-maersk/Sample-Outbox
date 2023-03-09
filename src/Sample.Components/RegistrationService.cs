namespace Sample.Components;

using Contracts;
using MassTransit;
using MassTransit.Middleware.Outbox;
using Microsoft.EntityFrameworkCore;
using Npgsql;


public class RegistrationService :
    IRegistrationService
{
    readonly RegistrationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public RegistrationService(RegistrationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<RegistrationTest1> SubmitRegistration(string eventId, string memberId, decimal payment)
    {
        var registration = new RegistrationTest1
        {
            RegistrationId = NewId.NextGuid(),
            RegistrationDate = DateTime.UtcNow,
            MemberId = memberId,
            EventId = eventId,
            Payment = payment
        };

        await _dbContext.Set<RegistrationTest1>().AddAsync(registration);

        await _publishEndpoint.Publish(new RegistrationSubmitted
        {
            RegistrationId = registration.RegistrationId,
            RegistrationDate = registration.RegistrationDate.AddMinutes(1),
            MemberId = registration.MemberId,
            EventId = registration.EventId,
            Payment = payment,
        });

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DuplicateRegistrationException("Duplicate registration", exception);
        }

        return registration;
    }
}