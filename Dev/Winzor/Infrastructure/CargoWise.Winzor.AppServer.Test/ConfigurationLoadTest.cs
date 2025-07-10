using System;
using System.IO;
using System.Reflection;
using CargoWise.Blazor.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CargoWise.Winzor.AppServer.Test
{
	public class ConfigurationLoadTest
	{
		readonly string configurationRootPath = Path.GetFullPath("./TempTestFolder");
		string originalEnvVariable;

		[SetUp]
		public void Setup()
		{
			originalEnvVariable = Environment.GetEnvironmentVariable("CARGOWISEWEB_CONFIGROOT");
			Environment.SetEnvironmentVariable("CARGOWISEWEB_CONFIGROOT", configurationRootPath, EnvironmentVariableTarget.Process);
			if (!Directory.Exists(configurationRootPath))
			{
				Directory.CreateDirectory(configurationRootPath);
			}

			string appSettingJsonString = @"
{
	""CargoWiseOptions"": {
		""DatabaseName"": ""testDB""
	},
	""Serilog"": {
		""WriteTo"": [
			{
				""Name"": ""Async""
			},
			{
				""Name"": ""Kafka"",
				""Args"": {
					""brokers"": ""abc.test-1.kafka.wtg.ws:9093"",
					""topic"": ""test-topic-xyz"",
					""sslCertificatePath"": ""./kafkaCertificate.pem""
				}
			},
			{
				""Name"": ""Dummy-X""
			},
		],
	}
}
";
			var assemblyVersionAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>();
			var appVersion = assemblyVersionAttribute?.Version ?? "0.0.0.0";
			var testAppSettingFileName = $"appsettings.CargoWise.Winzor.AppServer.{appVersion}.json";
			var testAppSettingFilePath = Path.Combine(configurationRootPath, testAppSettingFileName);
			File.WriteAllText(testAppSettingFilePath, appSettingJsonString);
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(configurationRootPath, true);
			Environment.SetEnvironmentVariable("CARGOWISEWEB_CONFIGROOT", originalEnvVariable, EnvironmentVariableTarget.Process);
		}

		[Test]
		public void TestLoadTheConfiguratiion_CorrectlyLoadSettingsFromJsonFile()
		{
			using var factory = new AppServerWebApplicationFactory();
			IConfiguration config = factory.Services.GetService<IConfiguration>();
			var result = config?.GetSection("CargoWiseOptions").Get<CargoWiseOptions>();

			Assert.That(
				result.DatabaseName,
				Is.EqualTo("testDB"),
				"DatabaseName is overrided");

			//Serilog:Async
			Assert.That(
				config?.GetValue<string>("Serilog:WriteTo:0:Name"),
				Is.EqualTo("Async"));
			Assert.That(
				config?.GetValue<string>("Serilog:WriteTo:0:Args:configure:1:Name"),
				Is.EqualTo("Console"),
				"Async Write To Console configuration should merge into first item.");

			//Serilog:Kafka
			Assert.That(
				config?.GetValue<string>("Serilog:WriteTo:1:Name"),
				Is.EqualTo("Kafka"),
				"Kafka setting should be added.");
			Assert.That(
				config?.GetValue<string>("Serilog:WriteTo:1:Args:brokers"),
				Is.EqualTo("abc.test-1.kafka.wtg.ws:9093"));
			Assert.That(
				config?.GetValue<string>("Serilog:WriteTo:1:Args:topic"),
				Is.EqualTo("test-topic-xyz"));

			//Serilog:Dummy-X
			Assert.That(
				config?.GetValue<string>("Serilog:WriteTo:2:Name"),
				Is.EqualTo("Dummy-X"),
				"Additional dummy setting should be added.");

			//Serilog:MinimumLevel
			Assert.That(
				config?.GetValue<string>("Serilog:MinimumLevel:Override:Microsoft.Hosting.Lifetime"),
				Is.EqualTo("Information"));
		}
	}
}
