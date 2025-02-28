﻿using Application.Interfaces;
using Application.Services;
using Data.Context;
using Data.Messaging;
using Data.Repository;
using Domain.Interfaces;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Configuração do MongoDB
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.AddSingleton<MongoDBContext>();


builder.Services.AddControllers(options =>
{
    options.InputFormatters.Insert(0, JsonPatchSample.MyJPIF.GetJsonPatchInputFormatter());
}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
    options.SerializerSettings.Formatting = Formatting.Indented;
    options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
});

builder.Services.AddScoped<IAgendamentoService, AgendamentoService>();
builder.Services.AddScoped<IAgendamentoRepository, AgendamentoRepository>();
builder.Services.AddScoped<IAgendamentoMessageQueue, AgendamentoMessageQueue>();
builder.Services.AddScoped<IAgendamentoMessageQueueError, AgendamentoMessageQueueError>();
builder.Services.AddScoped<IAgendamentoMessageSender, AgendamentoMessageSender>();
builder.Services.AddScoped<IAgendamentoMessageService, AgendamentoMessageService>();
builder.Services.AddHostedService<AgendamentoWorkerService>();
builder.Services.AddScoped<IAgendamentoScopedService, AgendamentoScopedService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hackaton - Grupo 13 | FIAP",
        Description = "Documentação dos endpoints da API.",
        Contact = new OpenApiContact { Name = "Hackaton - Grupo 13", Email = "grupo13@fiap.com" },
        License = new OpenApiLicense { Name = "MIT License", Url = new Uri("https://opensource.org/licenses/MIT") },
        Version = "1.0.11"
    });
    //c.SchemaFilter<AgendamentoInputSchemaFilter>();
});

var app = builder.Build();

app.Use((context, next) =>
{
    context.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
    context.Response.Headers["Content-Encoding"] = "utf-8";
    context.Response.Headers["Content-Language"] = CultureInfo.CurrentCulture.Name;
    return next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.DefaultModelRendering(ModelRendering.Example);
        c.DisplayOperationId();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
    });

    app.UseReDoc(c =>
    {
        c.DocumentTitle = "REDOC API Documentation";
        c.SpecUrl = "/swagger/v1/swagger.json";
    });
}

app.UseAuthorization();
app.MapControllers();
app.Run();
