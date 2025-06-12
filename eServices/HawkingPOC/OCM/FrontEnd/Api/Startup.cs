using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OcmPoc.FrontEnd.Bootstrapper;

namespace OcmPoc.FrontEnd.Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;

			AppConfig = new Configuration();
			configuration.Bind(AppConfig);
		}

		public IConfiguration Configuration { get; }

		private Configuration AppConfig { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();

			services.AddAutoMapper();

			if (!Configuration.GetValue<bool>("ConfiguredForTest"))
			{
				services.AddApplicationServices(AppConfig);
			}
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMvc();
		}
	}
}
