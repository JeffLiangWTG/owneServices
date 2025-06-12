using System;
using CargoWise.eServices.Billing.WcfService.Hangfire;
using Castle.MicroKernel.Registration;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(CargoWise.eServices.Billing.WcfService.Startup))]
namespace CargoWise.eServices.Billing.WcfService
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			var connectionString = Global.WindsorContainer.Resolve<IConfigurationProvider>().BillingConnectionString;
			GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);
			Global.WindsorContainer.Register(Component.For<IMonitoringApi>().Instance(JobStorage.Current.GetMonitoringApi()));
			app.UseHangfireDashboard("/hangfire", new DashboardOptions
			{
				Authorization = new[] { new CustomAuthorizationFilter() }
			});

			app.UseHangfireServer(new BackgroundJobServerOptions
			{
				ServerName = $"{Environment.MachineName}:Default"
			});

			app.UseHangfireServer(new BackgroundJobServerOptions()
			{
				ServerName = $"{Environment.MachineName}:BillingProcessing",
				Queues = new[] { "billing-processing-alpha", "billing-processing-hourly", "billing-processing-daily" },
				WorkerCount = 1,
				
			}, new SqlServerStorage(connectionString, new SqlServerStorageOptions()
			{
				QueuePollInterval = TimeSpan.FromSeconds(5)
			}));
			Global.WindsorContainer.Resolve<IBillingJobManager>().ResetJobs();
		}
	}
}
