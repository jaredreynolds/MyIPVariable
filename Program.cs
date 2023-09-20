using MyIPVariable;

IHost host = Host
    .CreateDefaultBuilder(args)
    .UseWindowsService(options => { options.ServiceName = "MyIPVariable"; })
    .ConfigureServices((builder, services) =>
    {
        var config = builder.Configuration.GetSection(Configuration.SectionName);

        services
            .Configure<Configuration>(config)
            .AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
