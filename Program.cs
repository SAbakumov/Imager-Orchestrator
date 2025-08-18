using DagOrchestrator.Exceptions;
using DagOrchestrator.Models;
using DagOrchestrator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft;
using System.Net;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);
System.Net.ServicePointManager.Expect100Continue = false;

// Add services to the container.
builder.WebHost.UseKestrel();
builder.Services.AddControllers().AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options  =>

{

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Imager API",
        Description = "Imager API for DAG control",
       
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});


builder.Services.AddHttpClient<PythonComService>(client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:8400");
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls13 | System.Security.Authentication.SslProtocols.Tls12;
    return handler;
}); ;


builder.Services.AddTransient<NodeProcessor>();
builder.Services.AddTransient<DagProcessingService>();       

builder.Services.AddHostedService<DagProcessingService>();
builder.Services.AddSingleton<IDagScheduler,DagScheduler>();
builder.Services.AddExceptionHandler<DeserializationExceptionHandler>();
builder.Services.AddProblemDetails();



var app = builder.Build();

// MAKE SURE IT IS SET TO DEVELOPMENT.
//if (app.Environment.IsDevelopment())
//{
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
