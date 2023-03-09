namespace Sample.Components.Contracts;

public interface IOutboxRegistration
{
    Guid RegistrationId { get; init; }
    DateTime RegistrationDate { get; init; }
    string MemberId { get; init; }
    string EventId { get; init; }
    decimal Payment { get; init; }
}

public record RegistrationSubmitted : IOutboxRegistration
{
    public Guid RegistrationId { get; init; }
    public DateTime RegistrationDate { get; init; }
    public string MemberId { get; init; }
    public string EventId { get; init; }
    public decimal Payment { get; init; }
}

public record RegistrationRestart : IOutboxRegistration
{
    public Guid RegistrationId { get; init; }
    public DateTime RegistrationDate { get; init; }
    public string MemberId { get; init; }
    public string EventId { get; init; }
    public decimal Payment { get; init; }
}
