using System.Reflection;
using CargoWise.Billing.Client;
using CargoWise.Billing.Kafka.API;
using CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common;
using Confluent.Kafka;
using Serilog;
using WTG.ErrorReporting;

Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
	options.ServiceName = "ElasticSearch Billing Collector Service";
});
builder.Services.AddSerilog((_, loggerConfig) => loggerConfig.ReadFrom.Configuration(builder.Configuration));
builder.Services.AddTransient<IBillingServiceClient>((_) => new BillingServiceClient(builder.Configuration["BillingWcfServiceEndpoint"]));
builder.Services.AddTransient<IBillingKafkaClient>((_) =>
{
	var clientConfigDic = builder.Configuration.GetSection("BillingKafkaClientConfig").AsEnumerable().Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
	var producerConfigDic = builder.Configuration.GetSection("BillingKafkaProducerConfig").AsEnumerable().Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
	var producerConfig = BillingKafkaClient.GetKafkaConfig<ProducerConfig>(clientConfigDic, producerConfigDic);
	return new BillingKafkaClient(producerConfig);
});
builder.Services.AddSingleton<IErrorReportingClient>((_) => new ErrorReportingClient(builder.Configuration["IssueManagerUri"] != null ? new Uri(builder.Configuration["IssueManagerUri"]!) : new Uri("", UriKind.Relative)));
builder.Services.AddHostedService(provider => new BillingService(provider, typeof(Program).Assembly, "plugins_settings.xml", "plugins_state.xml"));


var host = builder.Build();
host.Run();
