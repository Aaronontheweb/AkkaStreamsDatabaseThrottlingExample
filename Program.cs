using Akka.Actor;
using Akka.Hosting;
using MyAkkaApp;
using MyAkkaApp.Actors;

var builder = WebApplication.CreateBuilder(args);

// register IProductService
builder.Services.AddScoped<ConcreteDatabaseImplementation>();
builder.Services.AddSingleton<IProductService, ThrottledImplementation>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAkka("MyActorSystem", (akkaBuilder, provider) =>
{
    // Configure your ActorSystem here
    akkaBuilder.WithActors((system, registry, di) =>
    {
        // start ThrottlerActor and add to registry
        var throttlerActor = system.ActorOf(di.Props<ThrottlerActor>(), "throttler");
        registry.Register<ThrottlerActor>(throttlerActor);
    }).AddStartup((system, registry) =>
    {
        // hit the ThrottlerActor with 100 messages
        var throttlerActor = registry.Get<ThrottlerActor>();
        for (var i = 0; i < 100; i++)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            throttlerActor.Ask(new ReadProductById($"product-{i}", cts.Token));
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName.Equals("Azure"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGet("/", () => "Hello from Akka.NET!");

app.Run();