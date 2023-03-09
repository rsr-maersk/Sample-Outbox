namespace Sample.Components;

public interface IRegistrationService
{
    Task<RegistrationTest1> SubmitRegistration(string eventId, string memberId, decimal payment);
}