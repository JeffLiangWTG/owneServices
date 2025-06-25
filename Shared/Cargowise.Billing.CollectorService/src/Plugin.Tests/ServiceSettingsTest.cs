using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Org.XmlUnit.Constraints;

namespace CargoWise.Billing.CollectorService.Plugin.Tests
{
	[TestFixture]
	class ServiceSettingsTest
	{
		[Test]
		public void TestSerialization()
		{
			var serializer = new XmlSerializer(typeof(ServiceSettings));
			using (var stream = new MemoryStream())
			using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = Encoding.UTF8}))
			{
				serializer.Serialize(writer, new ServiceSettings(SettingsExample));

				Assert.That(stream.ToArray(), CompareConstraint.IsIdenticalTo(SettingsSerializationStringExample));
			}
		}

		[Test]
		public void TestDeserialization()
		{
			var exampleBytes = Encoding.UTF8.GetBytes(SettingsDeserializationStringExample);
			var serializer = new XmlSerializer(typeof(ServiceSettings));
			using (var stream = new MemoryStream(exampleBytes))
			{
				Assert.That(serializer.Deserialize(stream), Is.EqualTo(new ServiceSettings(SettingsExample)));
			}
		}

		#region SettingsExample

		public static ServiceSettings SettingsExample
		{
			get
			{
				return new ServiceSettings
				{
					Plugins = new[]
					{
						new PluginControllerSettings
						{
							Key = "TestPlugin1.Key",
							Active = true,
							TypeName = "TestPlugin1",
							IntervalMinutes = 1440,
							RetryIntervalSeconds = 60,
							MaxRetryAttempts = 3,
							SendBillingTransaction = true,
							SendUsageTransaction = false,
							PluginSettings = new PluginSettings
							{
								Parameters = new[]
								{
									new PluginParameter("ConnectionString", "Server=#server#;Database=#database#;User Id=#userid#;Password=#password#;")
								}
							}
						},
						new PluginControllerSettings
						{
							Key = "TestPlugin2.Key",
							Active = false,
							TypeName = "TestPlugin2",
							IntervalMinutes = 60,
							RetryIntervalSeconds = 20,
							MaxRetryAttempts = 10,
							SendBillingTransaction = true,
							SendUsageTransaction = true,
							PluginSettings = new PluginSettings
							{
								Parameters = new[]
								{
									new PluginParameter("FirstParameter", "SomeValue"),
									new PluginParameter("SecondParameter", "1000")
								}
							}
						},
						new PluginControllerSettings
						{
							Key = "TestWithoutSettings.Key",
							Active = true,
							TypeName = "TestWithoutSettings",
							IntervalMinutes = 1440,
							RetryIntervalSeconds = 60,
							MaxRetryAttempts = 3,
							SendBillingTransaction = false,
							SendUsageTransaction = false,
						}
					}
				};
			}
		}

		#endregion // SettingsExample

		#region SettingsDeserializationStringExample
		const string SettingsDeserializationStringExample = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Plugins xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Plugin>
    <Key>TestPlugin1.Key</Key>
    <Active>true</Active>
    <TypeName>TestPlugin1</TypeName>
    <IntervalMinutes>1440</IntervalMinutes>
    <RetryIntervalSeconds>60</RetryIntervalSeconds>
    <MaxRetryAttempts>3</MaxRetryAttempts>
    <Settings>
      <Param Name=""ConnectionString"">Server=#server#;Database=#database#;User Id=#userid#;Password=#password#;</Param>
    </Settings>
  </Plugin>
  <Plugin>
    <Key>TestPlugin2.Key</Key>
    <Active>false</Active>
    <TypeName>TestPlugin2</TypeName>
    <IntervalMinutes>60</IntervalMinutes>
    <RetryIntervalSeconds>20</RetryIntervalSeconds>
    <MaxRetryAttempts>10</MaxRetryAttempts>
    <SendBillingTransaction>true</SendBillingTransaction>
    <SendUsageTransaction>true</SendUsageTransaction>
    <Settings>
      <Param Name=""FirstParameter"">SomeValue</Param>
      <Param Name=""SecondParameter"">1000</Param>
    </Settings>
  </Plugin>
  <Plugin>
    <Key>TestWithoutSettings.Key</Key>
    <Active>true</Active>
    <TypeName>TestWithoutSettings</TypeName>
    <IntervalMinutes>1440</IntervalMinutes>
    <RetryIntervalSeconds>60</RetryIntervalSeconds>
    <MaxRetryAttempts>3</MaxRetryAttempts>
    <SendBillingTransaction>false</SendBillingTransaction>
    <SendUsageTransaction>false</SendUsageTransaction>
  </Plugin>
</Plugins>";
		#endregion // SettingsDeserializationStringExample

		#region SettingsSerializationStringExample
		const string SettingsSerializationStringExample = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Plugins xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Plugin>
    <Key>TestPlugin1.Key</Key>
    <Active>true</Active>
    <TypeName>TestPlugin1</TypeName>
    <IntervalMinutes>1440</IntervalMinutes>
    <RetryIntervalSeconds>60</RetryIntervalSeconds>
    <MaxRetryAttempts>3</MaxRetryAttempts>
    <SendBillingTransaction>true</SendBillingTransaction>
    <SendUsageTransaction>false</SendUsageTransaction>
    <Settings>
      <Param Name=""ConnectionString"">Server=#server#;Database=#database#;User Id=#userid#;Password=#password#;</Param>
    </Settings>
  </Plugin>
  <Plugin>
    <Key>TestPlugin2.Key</Key>
    <Active>false</Active>
    <TypeName>TestPlugin2</TypeName>
    <IntervalMinutes>60</IntervalMinutes>
    <RetryIntervalSeconds>20</RetryIntervalSeconds>
    <MaxRetryAttempts>10</MaxRetryAttempts>
    <SendBillingTransaction>true</SendBillingTransaction>
    <SendUsageTransaction>true</SendUsageTransaction>
    <Settings>
      <Param Name=""FirstParameter"">SomeValue</Param>
      <Param Name=""SecondParameter"">1000</Param>
    </Settings>
  </Plugin>
  <Plugin>
    <Key>TestWithoutSettings.Key</Key>
    <Active>true</Active>
    <TypeName>TestWithoutSettings</TypeName>
    <IntervalMinutes>1440</IntervalMinutes>
    <RetryIntervalSeconds>60</RetryIntervalSeconds>
    <MaxRetryAttempts>3</MaxRetryAttempts>
    <SendBillingTransaction>false</SendBillingTransaction>
    <SendUsageTransaction>false</SendUsageTransaction>
  </Plugin>
</Plugins>";
		#endregion // SettingsDeserializationStringExamples
	}
}
