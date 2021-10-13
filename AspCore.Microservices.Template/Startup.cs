using System;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AspCore.Microservices.Template.Data;
using AspCore.Microservices.Template.Dto.AppSettings;
using AspCore.Microservices.Template.Extentions;
using Serilog;

namespace AspCore.Microservices.Template
{
	/// <summary>
	/// Startup
	/// </summary>
    public class Startup
    {
	    private readonly IConfiguration _configuration;
	    
		/// <summary>
		/// Base ctor
		/// </summary>
		public Startup()
		{
			string aspEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			if (string.IsNullOrEmpty(aspEnv))
				aspEnv = Debugger.IsAttached ? "Development" : "Production";
			IConfigurationBuilder builder = new ConfigurationBuilder()
				.AddJsonFile(aspEnv == "Development" ? $"appsettings.{aspEnv}.json" : "appsettings.json", false, true);
			_configuration = builder.Build();
		}

		/// <summary>
		/// This method gets called by the runtime. Use this method to add services to the container
		/// </summary>
        public void ConfigureServices(IServiceCollection services)
		{
			AppSettings appSettings = new();
			_configuration.Bind(appSettings);
			services
				.Configure<AppSettings>(_configuration)
				.AddCors()
				.AddDbContext<SharedDbContext>(o => o.UseNpgsql(_configuration.GetSection("Connections").GetValue<string>("SharedDb")))
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
					jsonOptions.JsonSerializerOptions.IgnoreNullValues = true;
				});
		}

		/// <summary>
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
		/// </summary>
        public void Configure(
			IApplicationBuilder app,
			IWebHostEnvironment env,
			ILoggerFactory loggerFactory,
			IApiVersionDescriptionProvider provider)
        {
	        try
	        {
		        app
			        .AddBaseFunctions(env, loggerFactory, _configuration)
					.UseSwaggerWithUi(provider);
	        }
	        catch (Exception exception)
	        {
		        Log.Logger = new LoggerConfiguration()
			        .MinimumLevel.Error()
			        .WriteTo.File($"Logs/{nameof(Startup)}.log")
			        .CreateLogger();
		        Log.Error(exception, exception.Message);            
	        }
        }
    }
}