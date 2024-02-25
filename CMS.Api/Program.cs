using Serilog.Events;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CMS.Api.Application;
using Minio;
using CMS.Api.Application.Repositories;
using MongoDB.Driver;
using CMS.Api.Application.Services;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
                 //.Enrich.FromLogContext()
                 .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                 .MinimumLevel.Override("System", LogEventLevel.Warning)
                 //.MinimumLevel.Information()
                 .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                 .WriteTo.Debug()
                 .WriteTo.Console()
                 .CreateLogger();

Serilog.Debugging.SelfLog.Enable(msg =>
{
    Debug.Print(msg);
    Debugger.Break();
});

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Настройки сериалайзера.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Чтобы enum конвертировались в текст
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    // Чтобы пропускались свойства, которые имеют нулевое значение.
    // Иначе будет "somePropery": null, которое не сможет распарсить сериалайзер по умолчанию
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

});
// Настройка для swagger, чтоб он тоже конвертировал enum to string
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<ICacheService, CacheService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis:6379";
});

builder.Services.AddScoped((sp) =>
    new MinioClient()
        .WithEndpoint("minio:9000")
        .WithCredentials("minioadmin", "minioadmin")
        .Build());

builder.Services.AddScoped<IMongoClient>((sp) =>
    new MongoClient("mongodb://mongodb:27017"));

builder.Services.AddScoped<IStorageRepository, StorageRepository>();
builder.Services.AddScoped<IInternalRepository, InternalRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();


builder.Services.Decorate<IInternalRepository, CachedInternalRepository>();


builder.Services.AddScoped<IInternalService,  InternalService>();
builder.Services.AddScoped<IMessageService, MessageService>();





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.RegisterEndpoints();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
