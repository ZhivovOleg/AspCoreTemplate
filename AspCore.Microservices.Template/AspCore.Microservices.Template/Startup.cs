using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AspCore.Microservices.Template.Data;
using AspCore.Microservices.Template.Dto.AppSettings;
using AspCore.Microservices.Template.Extensions;
using Serilog;

namespace AspCore.Microservices.Template;

/// <summary>
/// Startup
/// </summary>
public class Startup
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Base ctor
    /// </summary>
    public Startup(IConfiguration configuration) => _configuration = configuration;

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        AppSettings appSettings = new();
        _configuration.Bind(appSettings);

        _ = services
            .Configure<AppSettings>(_configuration)
            .AddCors()
            .AddDbContext<SharedDbContext>(
                o => o.UseNpgsql(_configuration.GetSection("Connections").GetValue<string>("SharedDb")))
            .AddSwagger()
            .AddLogicServices()
            .AddHealthChecks(appSettings.HealthChecks)
            .AddDbContext(appSettings.Connections)
            .AddSupportApiVersioning()
            .AddControllers()
            .AddJsonOptions(jsonOptions =>
            {
                jsonOptions.JsonSerializerOptions.AllowTrailingCommas = true;
                jsonOptions.JsonSerializerOptions.WriteIndented = true;
                jsonOptions.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    /// </summary>
    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env,
        IApiVersionDescriptionProvider provider)
    {
        try
        {
            _ = app
                .AddBaseFunctions(env, _configuration)
                .UseSwaggerWithUi(provider);
        }
        catch (Exception exception)
        {
            Log.Error(exception, exception.Message);
        }
    }
}