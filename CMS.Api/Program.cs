using CMS.Api.Application;
using CMS.Api.Application.Repositories;
using CMS.Api.Application.Services;
using Minio;
using MongoDB.Driver;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
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

builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = "redis:6379";
    options.InstanceName = "OutputCache";
});

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(10)));
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

app.UseOutputCache();


app.RegisterEndpoints();
app.Run();
