using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Prometheus;

namespace WebApiMetrics
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "WebApiMetrics", Version = "v1"}); });
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			var requestCounter = Metrics.CreateCounter("webApi_request_counter", "API'ye yapılan istek sayacı", new CounterConfiguration
			{
				LabelNames = new[] { "scheme", "method", "path" }
			});

			app.Use((context, next) =>
			{
				requestCounter.WithLabels(context.Request.Scheme,context.Request.Method, context.Request.Path).Inc();
				return next();
			});

			app.UseMetricServer();
			app.UseHttpMetrics();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApiMetrics v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}