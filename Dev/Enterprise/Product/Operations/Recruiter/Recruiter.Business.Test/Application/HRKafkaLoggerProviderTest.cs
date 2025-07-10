using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NLog;
using NLog.Layouts;
using WTG.Logging.NLog.Kafka;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRKafkaLoggerProviderTest : TestCaseWithFactory
	{
		public void TestGetLogger_ProductionSystem()
		{
			var licenceType = DatabaseTypes.Codes.Production;
			LicenceTypeChanger.SetSystemLicence(licenceType);

			var provider = new HRKafkaLoggerProvider(GetType().Name);
			var logger = provider.GetLogger();
			var target = LogManager.Configuration.FindTargetByName<KafkaTarget>(logger.Name);

			AssertEquals("topic-au2-prod-hrm-logs-prod", target.Topic.ToString());
			AssertEquals(brokers, target.Brokers.ToString());
			AssertEquals("${message}", target.Layout.ToString());
		}

		public void TestGetLogger_NonProductionSystem()
		{
			var licenceType = DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);

			var provider = new HRKafkaLoggerProvider(GetType().Name);
			var logger = provider.GetLogger();
			var target = LogManager.Configuration.FindTargetByName<KafkaTarget>(logger.Name);

			AssertEquals("topic-au1-test-hrm-logs-test", target.Topic.ToString());
			AssertEquals(brokers, target.Brokers.ToString());
			AssertEquals("${message}", target.Layout.ToString());
		}

		public void TestLogger_CustomLayout()
		{
			var jsonLayout = new JsonLayout
			{
				IncludeEventProperties = true
			};

			jsonLayout.Attributes.Add(new JsonAttribute("message", "${message}"));
			jsonLayout.Attributes.Add(new JsonAttribute("foo", "${event-properties:item=foo}"));
			jsonLayout.Attributes.Add(new JsonAttribute("bar", "${event-properties:item=bar}"));

			var provider = new HRKafkaLoggerProvider(GetType().Name, jsonLayout);
			var logger = provider.GetLogger();
			var target = LogManager.Configuration.FindTargetByName<KafkaTarget>(logger.Name);

			var layoutString = target.Layout.ToString();
			AssertEquals(typeof(JsonLayout), target.Layout.GetType());
			AssertContains("message-${message}", layoutString);
			AssertContains("foo-${event-properties:item=foo}", layoutString);
			AssertContains("bar-${event-properties:item=bar}", layoutString);
		}

		public void TestLogger_CustomMinLoggingLevel()
		{
			RecruiterDataRegistry.Instance.LoggingMinLevel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, LogLevel.Warn.Name);

			var provider = new HRKafkaLoggerProvider(GetType().Name);
			var logger = provider.GetLogger();

			Assert(logger.IsWarnEnabled);
			Assert(!logger.IsInfoEnabled);
		}

		protected override void TearDown()
		{
			base.TearDown();
			LogManager.Shutdown();
		}

		readonly string brokers = RecruiterDataRegistry.Instance.LoggingKafkaBrokers.Value;
	}
}
