using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Billing.Client;
using CargoWise.eServices.TestHelpers.Database.Common;
using Confluent.Kafka;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using WTG.DevTools.TestFramework;

namespace CargoWise.eServices.Billing.WcfService.IntegrationTests
{
	public class WithBillingWcfServiceAttribute : WithWebApplicationAttribute
	{
		const string applicationName = "BillingWcfService";

		readonly bool configureClient;

		public WithBillingWcfServiceAttribute(bool configureClient = true)
		{
			this.configureClient = configureClient;
			connectionString =
				new SqlConnectionStringBuilder(
					SqlServerHelper.GetAdminConnectionString(Tests.Common.Deployments.IntegrationTestingDbName))
				{
					ApplicationName = "TestBillingWebService"
				}.ConnectionString;
			var sqlStorage = new SqlServerStorage(connectionString);
			hangfireMonitoringApi = sqlStorage.GetMonitoringApi();
			backgroundJobClient = new BackgroundJobClient(sqlStorage);
		}

		public static WithBillingWcfServiceAttribute Current => (WithBillingWcfServiceAttribute)TestContext.CurrentContext.Test.Properties.Get(applicationName);

		protected override string RelativeBuildPath => "WcfService";

		protected override string ApplicationName => applicationName;

		protected override void InitializeWebConfig(XDocument doc)
		{
			var bindings = doc.XPathSelectElement("//system.serviceModel/bindings");
			bindings.Add(
				new XElement("basicHttpBinding",
					new XElement("binding",
						new XAttribute("name", "for_test"),
						new XAttribute("maxBufferPoolSize", int.MaxValue),
						new XAttribute("maxBufferSize", int.MaxValue),
						new XAttribute("maxReceivedMessageSize", int.MaxValue),
						new XElement("security",
							new XAttribute("mode", "None")))));

			var endpoint = doc.XPathSelectElement("//system.serviceModel//endpoint");
			endpoint.SetAttributeValue("binding", "basicHttpBinding");
			endpoint.SetAttributeValue("bindingConfiguration", "for_test");

			var log4net = doc.XPathSelectElement("/configuration/log4net");
			log4net.RemoveAll();
			log4net.SetAttributeValue("threshold", "OFF");

			var connectionStringElement = 
				doc.XPathSelectElement("//connectionStrings/add[@name='BillingContext']");
			connectionStringElement.SetAttributeValue("connectionString", this.connectionString);

			var circuitBreakerRetryTimeout = doc.XPathSelectElement("//appSettings/add[@key='RetryTimeoutInMilliseconds']");
			circuitBreakerRetryTimeout?.SetAttributeValue("value", "10");
			var ELKFailedTransactionThreshold = doc.XPathSelectElement("//appSettings/add[@key='ELKFailedTransactionThreshold']");
			ELKFailedTransactionThreshold.SetAttributeValue("value", "10");
			var notSendToELKBillingCodes = doc.XPathSelectElement("//appSettings/add[@key='NotSendToELKBillingCodes']");
			notSendToELKBillingCodes.SetAttributeValue("value", "TST");

			var metadata = MockKafkaCluster.GetMetadata("billing-topic", TimeSpan.FromMinutes(1));
			var bootstrapServers = string.Join(",", metadata.Brokers.Select(b => $"{b.Host}:{b.Port}"));
			KafkaBrokers = bootstrapServers;

			var kafkaBootstrapServer = doc.XPathSelectElement("//billingKafkaClientSettings/add[@key='bootstrap.servers']");
			kafkaBootstrapServer.SetAttributeValue("value", bootstrapServers);
			doc.Descendants("add").Where(x => x.Attribute("key")?.Value.ToString() == "security.protocol").Remove();
		}
		
		public override void BeforeTest(ITest test)
		{
			base.BeforeTest(test);

			if (configureClient)
			{
				var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

				var serviceConfig = ServiceModelSectionGroup.GetSectionGroup(configuration);
				var securityConfig =
					serviceConfig
						.Bindings
						.BasicHttpBinding
						.Bindings
						.OfType<BasicHttpBindingElement>()
						.First()
						.Security;
				securityConfig.Mode = BasicHttpSecurityMode.None;

				var endpointConfig =
					serviceConfig
						.Client
						.Endpoints
						.OfType<ChannelEndpointElement>()
						.First();

				endpointConfig.Address = EndpointUri;
				endpointConfig.Name = ConfigName;
				endpointConfig.Binding = "basicHttpBinding";
				endpointConfig.Contract = "BillingServiceReference.IBillingService";

				configuration.Save();

				ConfigurationManager.RefreshSection("system.serviceModel/client");
			}
		}

		public override void AfterTest(ITest test)
		{
			TestHelper.DoWithRetry(() =>
			{
				backgroundJobClient.DeleteProcessingJobs(hangfireMonitoringApi);
				if (hangfireMonitoringApi.ProcessingCount() != 0)
				{
					throw new InvalidOperationException($"There are {hangfireMonitoringApi.ProcessingCount()} backgound job(s) still running");
				}
			}, "AwaitProcessingJobStopped", TimeSpan.FromSeconds(10), maxAttemptCount:10);
			base.AfterTest(test);
		}

		public Uri EndpointUri => GetHttpUri("BillingService.svc");

		public string ConfigName => $"port_{EndpointUri.Port}";

		public BillingServiceClient CreateClient() => new BillingServiceClient(ConfigName);

		IAdminClient mockKafkaCluster;
		IAdminClient MockKafkaCluster
		{
			get
			{
				return mockKafkaCluster ?? (mockKafkaCluster = new AdminClientBuilder(new ClientConfig(new Dictionary<string, string>()
				{
					{"bootstrap.servers", "localhost:9300"},
					{"test.mock.num.brokers", "3"}
				})).Build());
			}
		}

		public string KafkaBrokers { get; private set; }
		readonly string connectionString;
		readonly IMonitoringApi hangfireMonitoringApi;
		readonly IBackgroundJobClient backgroundJobClient;
	}
}
