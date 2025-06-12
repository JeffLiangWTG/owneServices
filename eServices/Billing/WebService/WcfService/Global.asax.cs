using System;
using CargoWise.Billing.Kafka.API;
using Castle.Facilities.WcfIntegration;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Confluent.Kafka;
using CargoWise.eServices.Billing.WcfService.Hangfire;

namespace CargoWise.eServices.Billing.WcfService
{
	public class Global : System.Web.HttpApplication
	{
		protected void Application_Start(object sender, EventArgs e)
		{
			var hangfireJobWrapper = new HangfireJobWrapper();
			var billingJobManager = new BillingJobManager();
			var configurationProvider = new ConfigurationProvider();
			var config =  KafkaConfigurationProvider.GetKafkaConfig<ProducerConfig>();
			var dnsResolver = new DnsResolver();
			IAdminClient adminClient;
			try
			{
				adminClient = new AdminClientBuilder(KafkaConfigurationProvider.GetKafkaConfig<AdminClientConfig>()).Build();
			}
			catch
			{
				adminClient = null;
			}

			WindsorContainer.AddFacility<WcfFacility>()
				.Register
				(
					Component.For<IBillingKafkaClient>().ImplementedBy<BillingKafkaClient>()
						.DependsOn(Dependency.OnValue("producerConfig", config)).LifestyleSingleton(),
					Component.For<IConfigurationProvider>().Instance(configurationProvider),
					Component.For<IDnsResolver>().Instance(dnsResolver),
					Component.For<IAdminClient>().Instance(adminClient),
					Component.For<IHangfireJobWrapper>().Instance(hangfireJobWrapper),
					Component.For<IBillingJobManager>().Instance(billingJobManager)
				);
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		protected void Application_Error(object sender, EventArgs e)
		{

		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{
			WindsorContainer?.Resolve<IBillingKafkaClient>()?.Dispose();
			WindsorContainer?.Dispose();
		}

		public static IWindsorContainer WindsorContainer { get; internal set; } = new WindsorContainer();
	}
}
