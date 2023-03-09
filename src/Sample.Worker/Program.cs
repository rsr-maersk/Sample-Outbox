using System.Diagnostics;
using MassTransit;
using MassTransit.Metadata;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sample.Components;
using Sample.Components.Consumers;
using Sample.Components.Contracts;
using Sample.Components.Services;
using Sample.Components.StateMachines;
using Serilog;
using Serilog.Events;
using static MassTransit.Transports.ReceiveEndpoint;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<RegistrationDbContext>(x =>
        {
            var connectionString = hostContext.Configuration.GetConnectionString("Default");

            x.UseNpgsql(connectionString, options =>
            {
                options.MinBatchSize(1);
            });
        });
        
        services.AddScoped<IRegistrationValidationService, RegistrationValidationService>();
        services.AddMassTransit(x =>
        {
            x.AddDelayedMessageScheduler();

            x.AddEntityFrameworkOutbox<RegistrationDbContext>(o =>
            {
                o.UsePostgres();
                o.DuplicateDetectionWindow = TimeSpan.FromSeconds(30);
                o.DisableInboxCleanupService();
            });

            x.SetKebabCaseEndpointNameFormatter();


            x.AddConsumer<NotifyRegistrationConsumer>();
            x.AddConsumer<SendRegistrationEmailConsumer>();
            x.AddConsumer<AddEventAttendeeConsumer>();
            x.AddConsumer<ValidateRegistrationConsumer, ValidateRegistrationConsumerDefinition>();
            x.AddSagaStateMachine<RegistrationStateMachine, RegistrationState, RegistrationStateDefinition>()
                .EntityFrameworkRepository(r =>
                {
                    r.ExistingDbContext<RegistrationDbContext>();
                    r.UsePostgres();
                });
            x.UsingInMemory((context, cfg) =>
            {
                cfg.UseDelayedMessageScheduler();
                cfg.UseScheduledRedelivery(y => y.Interval(10, TimeSpan.FromSeconds(10)));
                cfg.ConfigureEndpoints(context);
            });

            //x.UsingRabbitMq((context, cfg) =>
            //{
            //    cfg.UseDelayedMessageScheduler();
            //    cfg.UseScheduledRedelivery(y => y.Interval(10, TimeSpan.FromSeconds(10)));
            //    cfg.ConfigureEndpoints(context);
            //});

        });

        services.AddHostedService<TestSchedules>();
    })
    .UseSerilog()
    .Build();

    

await host.RunAsync();

public class GetSchedules : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    
    public GetSchedules(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //await using var scope = _serviceProvider.CreateAsyncScope();
        //await using var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
        //var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        //foreach (var state in db.RegistrationState.Where(x => x.CurrentState != "Complete"))
        //{
        //    await publishEndpoint.Publish(new RegistrationRestart
        //    {
        //        RegistrationDate = state.RegistrationDate,
        //        RegistrationId = Guid.NewGuid(),
        //        EventId = state.EventId,
        //        Payment = state.Payment,
        //        MemberId = state.MemberId,
        //    }, stoppingToken);
        //    ;
        //}
        ;
    }
}

public class TestSchedules : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    
    public TestSchedules(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int eventId;
        int memberId = eventId = 1;
        decimal payment = 1m;


        while (true)
        {
            Thread.Sleep(10000);
            await using var scope = _serviceProvider.CreateAsyncScope();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
            await publishEndpoint.Publish(new RegistrationSubmitted()
            {
                RegistrationDate = DateTime.UtcNow,
                RegistrationId = Guid.NewGuid(),
                EventId = eventId.ToString(),
                Payment = payment,
                MemberId = memberId.ToString(),
            }, stoppingToken);
        }


    }
}