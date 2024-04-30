using Asteroids.API.Services;

var builder = WebApplication.CreateBuilder(args);

string signalRHubUrl = builder.Configuration.GetSection("SIGNALRHUB").Value ?? "";

if (signalRHubUrl == null || signalRHubUrl == "${SIGNALRHUB}") signalRHubUrl = "http://localhost:5000/asteroidHub";

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SignalRService>(provider =>
{
    return new SignalRService(signalRHubUrl);
});
builder.Services.AddSingleton<IHostedService, ClusterAkkaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.UseCors(policy =>
    policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    );

app.Run();
