using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Billing.API;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Client;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WTG.ErrorReporting;

namespace CargoWise.Billing.CollectorService.Plugin.Tests
{
	[TestFixture]
	public class PluginsManagerTest
	{
		[Test]
		public void TestStartWhenLastRunWasLessThanIntervalAgo()
		{
			var settingsPath = Path.GetTempFileName();
			string statePath = Path.GetTempFileName();
			try
			{
				var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>10</IntervalMinutes>
    <RetryIntervalSeconds>1</RetryIntervalSeconds>
    <MaxRetryAttempts>0</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				File.WriteAllText(settingsPath, settingsXml);
				var now = DateTime.UtcNow;
				var clock = new TestClock { UtcNow = now };
				var transaction = new BillingTransaction
				{
					BillableCount = 2,
					ClientID = "ABCDEFXYZ",
					ClientNumber = "98765432100123456789",
					ClientStaffCode = "ABC",
					PriceItemCode = "DEF",
					Reference1 = "REFERENCE 1",
					Reference2 = "REFERENCE 2",
					Reference3 = "REFERENCE 3",
					ReportingSource = "XYZ",
					ServiceOccuredUTC = now
				};
				DummyBillingTransactionsPlugin.Transactions = new[] { new TimeStampedTransaction(transaction.ServiceOccuredUTC, transaction) };

				var initialLastSuccessfulRun = now - TimeSpan.FromMinutes(9) - TimeSpan.FromSeconds(56);

				// create state file, make sure the object goes out of scope to ensure we are reading from the file
				var state = new PluginControllerState(clock);
				state.Key = "DummyBillingTransactionsPlugin";
				state.LastSuccessfulRun = initialLastSuccessfulRun;

				var serviceState = new ServiceState();
				serviceState.Plugins.Add(state);
				statePath = Path.GetTempFileName();
				serviceState.WriteToFile(statePath);

				var interval = TimeSpan.FromMinutes(10);
				var mockPluginsManager = new Mock<PluginsManager>(Assembly.GetExecutingAssembly(),
					settingsPath,
					statePath,
					globalSettings,
					new LoggerFactory(),
					() => serviceClientMock.Object,
					() => kafkaClientMock.Object,
					errorReportingClientMock.Object,
					null,
					null)
				{
					CallBase = true
				};

				mockPluginsManager.Setup(_ => _.CreateClock()).Returns(clock);

				using (var manager = mockPluginsManager.Object)
				{
					manager.Start();
					Assert.That(DummyBillingTransactionsPlugin.LastInstance.GetTransactionsCallsCount, Is.EqualTo(0));
					clock.UtcNow = now.AddSeconds(4);
					Assert.That(DummyBillingTransactionsPlugin.LastInstance.GetTransactionsCallsCount, Is.EqualTo(1));
					Assert.That(manager.serviceState.Plugins[0].LastSuccessfulRun, Is.EqualTo(initialLastSuccessfulRun + interval));
				}
				Assert.That(DummyBillingTransactionsPlugin.InstanceCount, Is.EqualTo(1));
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.IsDisposed);
				serviceClientMock.Verify(_ => _.AddTransaction(It.Is<BillingTransaction>(t =>
					t.BillableCount == transaction.BillableCount &&
					t.ClientID == transaction.ClientID &&
					t.ClientNumber == transaction.ClientNumber &&
					t.ClientStaffCode == transaction.ClientStaffCode &&
					t.PriceItemCode == transaction.PriceItemCode &&
					t.Reference1 == transaction.Reference1 &&
					t.Reference2 == transaction.Reference2 &&
					t.Reference3 == transaction.Reference3 &&
					t.Reference4 == transaction.Reference4 &&
					t.ReportingSource == transaction.ReportingSource &&
					t.ServiceOccuredUTC == transaction.ServiceOccuredUTC)), Times.Once);
				mockPluginsManager.VerifyAll();
			}
			finally
			{
				File.Delete(settingsPath);
				File.Delete(statePath);
			}
		}

		[Test]
		public void TestStartWhenLastRunWasMoreThanIntervalAgo()
		{
			var settingsPath = Path.GetTempFileName();
			string statePath = Path.GetTempFileName();
			try
			{
				var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>10</IntervalMinutes>
    <RetryIntervalSeconds>1</RetryIntervalSeconds>
    <MaxRetryAttempts>0</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				File.WriteAllText(settingsPath, settingsXml);
				var now = new DateTime(2018, 2, 2, 2, 0, 0);
				var clock = new TestClock { UtcNow = now };
				var initialLastSuccessfulRun = now - TimeSpan.FromMinutes(19) - TimeSpan.FromSeconds(56);

				// create state file, make sure the object goes out of scope to ensure we are reading from the file
				var state = new PluginControllerState(clock);
				state.Key = "DummyBillingTransactionsPlugin";
				state.LastSuccessfulRun = initialLastSuccessfulRun;

				var serviceState = new ServiceState();
				serviceState.Plugins.Add(state);
				serviceState.WriteToFile(statePath);

				var interval = TimeSpan.FromMinutes(10);
				var mockPluginsManager = new Mock<PluginsManager>(Assembly.GetExecutingAssembly(),
					settingsPath,
					statePath,
					globalSettings,
					new LoggerFactory(),
					() => serviceClientMock.Object,
					() => kafkaClientMock.Object,
					errorReportingClientMock.Object,
					null,
					null) { CallBase = true };
				mockPluginsManager.Setup(_ => _.CreateClock()).Returns(clock);

				using (var manager = mockPluginsManager.Object)
				{
					manager.Start();
					Assert.That(DummyBillingTransactionsPlugin.LastInstance.GetTransactionsCallsCount, Is.EqualTo(1));
					Assert.That(manager.serviceState.Plugins[0].LastSuccessfulRun, Is.EqualTo(initialLastSuccessfulRun + interval));
					clock.UtcNow = now.AddSeconds(4);
					Assert.That(DummyBillingTransactionsPlugin.LastInstance.GetTransactionsCallsCount, Is.EqualTo(2));
					Assert.That(manager.serviceState.Plugins[0].LastSuccessfulRun, Is.EqualTo(initialLastSuccessfulRun + interval + interval));
				}
				Assert.That(DummyBillingTransactionsPlugin.InstanceCount, Is.EqualTo(1));
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.IsDisposed);

				mockPluginsManager.VerifyAll();
			}
			finally
			{
				File.Delete(settingsPath);
				File.Delete(statePath);
			}
		}

		[Test]
		public void TestPluginThrowsException()
		{
			var settingsPath = Path.GetTempFileName();
			string statePath = Path.GetTempFileName();
			try
			{
				var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>10</IntervalMinutes>
    <RetryIntervalSeconds>2</RetryIntervalSeconds>
    <MaxRetryAttempts>2</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				File.WriteAllText(settingsPath, settingsXml);
				var now = new DateTime(2018, 2, 2, 2, 0, 0);
				var clock = new TestClock { UtcNow = now };
				var initialLastSuccessfulRun = now - TimeSpan.FromMinutes(15);

				// create state file, make sure the object goes out of scope to ensure we are reading from the file
				var state = new PluginControllerState(clock);
				state.Key = "DummyBillingTransactionsPlugin";
				state.LastSuccessfulRun = initialLastSuccessfulRun;

				var serviceState = new ServiceState();
				serviceState.Plugins.Add(state);
				serviceState.WriteToFile(statePath);

				var interval = TimeSpan.FromMinutes(10);
				DummyBillingTransactionsPlugin.MaxExceptionsCount = 2;
				var mockPluginsManager = new Mock<PluginsManager>(Assembly.GetExecutingAssembly(),
					settingsPath,
					statePath,
					globalSettings,
					new LoggerFactory(),
					() => serviceClientMock.Object,
					() => kafkaClientMock.Object,
					errorReportingClientMock.Object,
					null,
					null)
				{
					CallBase = true
				};
				mockPluginsManager.Setup(_ => _.CreateClock()).Returns(clock);

				using (var manager = mockPluginsManager.Object)
				{
					manager.Start();
					Assert.That(manager.serviceState.Plugins[0].LastSuccessfulRun, Is.EqualTo(initialLastSuccessfulRun));
					clock.UtcNow = now.AddSeconds(2);
					Assert.That(manager.serviceState.Plugins[0].LastSuccessfulRun, Is.EqualTo(initialLastSuccessfulRun));
					clock.UtcNow = now.AddSeconds(4);
					Assert.That(manager.serviceState.Plugins[0].LastSuccessfulRun, Is.EqualTo(initialLastSuccessfulRun + interval));
				}
				Assert.That(DummyBillingTransactionsPlugin.InstanceCount, Is.EqualTo(1));
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.IsDisposed);

				mockPluginsManager.VerifyAll();
			}
			finally
			{
				File.Delete(settingsPath);
				File.Delete(statePath);
			}
		}

		[Test]
		public void TestPluginsDoNotShareBillingServiceClient()
		{
			// delete the file so it generates its own.
			var statePath = Path.GetTempFileName();
			File.Delete(statePath);

			var settingsPath = Path.GetTempFileName();
			try
			{
				var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin1</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>1440</IntervalMinutes>
    <RetryIntervalSeconds>60</RetryIntervalSeconds>
    <MaxRetryAttempts>5</MaxRetryAttempts>
  </Plugin>
  <Plugin>
    <Key>DummyBillingTransactionsPlugin2</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>1440</IntervalMinutes>
    <RetryIntervalSeconds>60</RetryIntervalSeconds>
    <MaxRetryAttempts>5</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				File.WriteAllText(settingsPath, settingsXml);

				var billingServiceClientsCount = 0;
				var mockPluginsManager = new Mock<PluginsManager>(Assembly.GetExecutingAssembly(),
					settingsPath,
					statePath,
					globalSettings,
					new LoggerFactory().AddLog4Net(),
					() =>
					{
						billingServiceClientsCount++;
						return serviceClientMock.Object;
					},
					() => kafkaClientMock.Object,
					errorReportingClientMock.Object,
					null,
					null)
				{
					CallBase = true
				};
				using (var manager = mockPluginsManager.Object)
				{
					manager.Start();
					Assert.That(billingServiceClientsCount, Is.EqualTo(2));
				}

				mockPluginsManager.VerifyAll();
			}
			finally
			{
				File.Delete(settingsPath);
				File.Delete(statePath);
			}
		}

		[Test]
		public void TestDetectSettingsChange()
		{
			// delete the file so it generates its own.
			var statePath = Path.GetTempFileName();
			File.Delete(statePath);

			var settingsPath = Path.GetTempFileName();
			try
			{
				var settingsXml1 = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>false</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>1440</IntervalMinutes>
    <RetryIntervalSeconds>60</RetryIntervalSeconds>
    <MaxRetryAttempts>3</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				var settingsXml2 = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>1440</IntervalMinutes>
    <RetryIntervalSeconds>60</RetryIntervalSeconds>
    <MaxRetryAttempts>3</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				var settingsXml3 = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>1440</IntervalMinutes>
    <RetryIntervalSeconds>60</RetryIntervalSeconds>
    <MaxRetryAttempts>5</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				var settingsXml4 = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
</Plugins>";

				// setup initial settings: disabled plugin
				File.WriteAllText(settingsPath, settingsXml1);
				var pluginsManager = new PluginsManager(Assembly.GetExecutingAssembly(), settingsPath, statePath, globalSettings, new LoggerFactory(), () => serviceClientMock.Object, () => kafkaClientMock.Object, errorReportingClientMock.Object);
				pluginsManager.Start();
				// initially there is one instance
				Assert.That(DummyBillingTransactionsPlugin.InstanceCount, Is.EqualTo(1));
				// update is called once during initialization of the PluginController
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.UpdateSettingsCallsCount, Is.EqualTo(1));
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.IsDisposed, Is.False);

				// modify settings: enabled plugin
				File.WriteAllText(settingsPath, settingsXml2);
				Thread.Sleep(5000);
				// the existing instance should have been only updated, keeping the count at 1
				Assert.That(DummyBillingTransactionsPlugin.InstanceCount, Is.EqualTo(1));
				// update is called again to handle the changes to the file.
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.UpdateSettingsCallsCount, Is.EqualTo(2));
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.IsDisposed, Is.False);

				// modify settings: MaxRetryAttempts to 5
				File.WriteAllText(settingsPath, settingsXml3);
				Thread.Sleep(5000);
				// the existing instance should have been only updated, keeping the count at 1
				Assert.That(DummyBillingTransactionsPlugin.InstanceCount, Is.EqualTo(1));
				// update is called again to handle the changes to the file.
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.UpdateSettingsCallsCount, Is.EqualTo(3));
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.IsDisposed, Is.False);

				// modify settings: Remove plugin
				File.WriteAllText(settingsPath, settingsXml4);
				Thread.Sleep(5000);
				Assert.That(DummyBillingTransactionsPlugin.InstanceCount, Is.EqualTo(1));
				// no plugins so we still keep the cache of settingsXml3, UpdateSettings is not called
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.UpdateSettingsCallsCount, Is.EqualTo(3));
				// empty plugins should not cause the service to stop
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.IsDisposed, Is.False);

				File.WriteAllText(settingsPath, settingsXml3);
				Thread.Sleep(5000);
				Assert.That(DummyBillingTransactionsPlugin.InstanceCount, Is.EqualTo(1));
				// should only Update settings when new settings is not equal to old settings
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.UpdateSettingsCallsCount, Is.EqualTo(3));
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.IsDisposed, Is.False);

				File.WriteAllText(settingsPath, settingsXml2);
				Thread.Sleep(5000);
				Assert.That(DummyBillingTransactionsPlugin.InstanceCount, Is.EqualTo(1));
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.UpdateSettingsCallsCount, Is.EqualTo(4));
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.IsDisposed, Is.False);
			}
			finally
			{
				File.Delete(settingsPath);
				File.Delete(statePath);
			}
		}

		[Test]
		public void TestDetectStateChange()
		{
			var settingsPath = Path.GetTempFileName();
			string statePath = Path.GetTempFileName();
			try
			{
				var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>10</IntervalMinutes>
    <RetryIntervalSeconds>600</RetryIntervalSeconds>
    <MaxRetryAttempts>0</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				File.WriteAllText(settingsPath, settingsXml);
				var now = new DateTime(2018, 2, 2, 2, 0, 0);
				var clock = new TestClock { UtcNow = now };
				var initialLastSuccessfulRun = now - TimeSpan.FromMinutes(10) - TimeSpan.FromSeconds(3);

				// create state
				var state = new PluginControllerState(clock);
				state.Key = "DummyBillingTransactionsPlugin";
				state.LastSuccessfulRun = initialLastSuccessfulRun;
				var serviceState = new ServiceState();
				serviceState.Plugins.Add(state);

				// setup initial settings: disabled plugin
				serviceState.WriteToFile(statePath);
				var mockPluginsManager = new Mock<PluginsManager>(Assembly.GetExecutingAssembly(),
					settingsPath,
					statePath,
					globalSettings,
					new LoggerFactory(),
					() => serviceClientMock.Object,
					() => kafkaClientMock.Object,
					errorReportingClientMock.Object,
					null,
					null)
				{
					CallBase = true
				};
				mockPluginsManager.Setup(_ => _.CreateClock()).Returns(clock);
				using (var pluginsManager = mockPluginsManager.Object)
				{
					pluginsManager.Start();
					Thread.Sleep(5000);
					Assert.That(DummyBillingTransactionsPlugin.LastInstance.GetTransactionsCallsCount, Is.EqualTo(1));

					// overwrite state file causing it to be hot reloaded and thus rerun the GetTransactionsCallsCount
					serviceState.WriteToFile(statePath);
					Thread.Sleep(5000);
					Assert.That(DummyBillingTransactionsPlugin.LastInstance.GetTransactionsCallsCount, Is.EqualTo(2));
				}

				mockPluginsManager.VerifyAll();
			}
			finally
			{
				File.Delete(settingsPath);
				File.Delete(statePath);
			}
		}

		[Test]
		public void TestOnStateFileChanged_ReadFromFile_FileIsOccupied_ShouldRetryAndReportToIssueManager()
		{
			var settingsPath = Path.GetTempFileName();
			var statePath = Path.GetTempFileName();

			try
			{
				var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>10</IntervalMinutes>
    <RetryIntervalSeconds>600</RetryIntervalSeconds>
    <MaxRetryAttempts>0</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				File.WriteAllText(settingsPath, settingsXml);

				var now = new DateTime(2018, 2, 2, 2, 0, 0);
				var clock = new TestClock { UtcNow = now };
				var initialLastSuccessfulRun = now - TimeSpan.FromMinutes(10) - TimeSpan.FromSeconds(3);

				// create state
				var state = new PluginControllerState(clock)
				{
					Key = "DummyBillingTransactionsPlugin",
					LastSuccessfulRun = initialLastSuccessfulRun
				};
				var serviceState = new ServiceState();
				serviceState.Plugins.Add(state);
				serviceState.WriteToFile(statePath);

				var mockPluginsManager = new Mock<PluginsManager>(Assembly.GetExecutingAssembly(),
					settingsPath,
					statePath,
					globalSettings,
					new LoggerFactory().AddLog4Net(),
					() => serviceClientMock.Object,
					() => kafkaClientMock.Object,
					errorReportingClientMock.Object,
					null,
					null)
				{
					CallBase = true
				};

				mockPluginsManager.Setup(_ => _.CreateClock()).Returns(clock);

				using (var pluginsManager = mockPluginsManager.Object)
				{
					pluginsManager.Start();

					using (File.Open(statePath, FileMode.Create))
					{
						Thread.Sleep(5000);

						memoryAppender = GetMemoryAppender();
						var errorLogMessages = memoryAppender.GetEvents().Where(e => e.Level == Level.Error).ToList();
						Assert.That(errorLogMessages.Count, Is.EqualTo(4));
						Assert.That(errorLogMessages[0].MessageObject.ToString(), Is.EqualTo("Failed to load state xml. Retrying..."));
						Assert.That(errorLogMessages[1].MessageObject.ToString(), Is.EqualTo("Failed to load state xml. Retrying..."));
						Assert.That(errorLogMessages[2].MessageObject.ToString(), Is.EqualTo("Failed to load state xml. Retrying..."));
						Assert.That(errorLogMessages[3].MessageObject.ToString(), Is.EqualTo("Failed to load state xml after retrying for 3 times"));
						Assert.That(errorLogMessages[3].ExceptionObject, Is.TypeOf<IOException>());
						Assert.That(errorLogMessages[3].ExceptionObject.Message, Does.StartWith("The process cannot access the file"));

						var warningLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Warn).ToList().Single();
						Assert.That(warningLogMessage.MessageObject.ToString(), Is.EqualTo("Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'"));
					}
				}

				mockPluginsManager.VerifyAll();
			}
			finally
			{
				File.Delete(settingsPath);
				File.Delete(statePath);
			}
		}

		[Test]
		public void TestOnSettingsFileChanged_ReadFromFile_FileIsOccupied_ShouldRetryAndReportToIssueManager()
		{
			var settingsPath = Path.GetTempFileName();
			var statePath = Path.GetTempFileName();

			try
			{
				var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>10</IntervalMinutes>
    <RetryIntervalSeconds>600</RetryIntervalSeconds>
    <MaxRetryAttempts>0</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				File.WriteAllText(settingsPath, settingsXml);

				var now = new DateTime(2018, 2, 2, 2, 0, 0);
				var clock = new TestClock { UtcNow = now };
				var initialLastSuccessfulRun = now - TimeSpan.FromMinutes(10) - TimeSpan.FromSeconds(3);

				// create state
				var state = new PluginControllerState(clock)
				{
					Key = "DummyBillingTransactionsPlugin",
					LastSuccessfulRun = initialLastSuccessfulRun
				};
				var serviceState = new ServiceState();
				serviceState.Plugins.Add(state);
				serviceState.WriteToFile(statePath);

				var mockPluginsManager = new Mock<PluginsManager>(Assembly.GetExecutingAssembly(),
					settingsPath,
					statePath,
					globalSettings,
					new LoggerFactory().AddLog4Net(),
					() => serviceClientMock.Object,
					() => kafkaClientMock.Object,
					errorReportingClientMock.Object,
					null,
					null)
				{
					CallBase = true
				};

				mockPluginsManager.Setup(_ => _.CreateClock()).Returns(clock);

				using (var pluginsManager = mockPluginsManager.Object)
				{
					pluginsManager.Start();

					using (File.Open(settingsPath, FileMode.Create))
					{
						Thread.Sleep(5000);

						memoryAppender = GetMemoryAppender();
						var errorLogMessages = memoryAppender.GetEvents().Where(e => e.Level == Level.Error).ToList();
						Assert.That(errorLogMessages.Count, Is.EqualTo(4));
						Assert.That(errorLogMessages[0].MessageObject.ToString(), Is.EqualTo("Failed to load settings xml. Retrying..."));
						Assert.That(errorLogMessages[1].MessageObject.ToString(), Is.EqualTo("Failed to load settings xml. Retrying..."));
						Assert.That(errorLogMessages[2].MessageObject.ToString(), Is.EqualTo("Failed to load settings xml. Retrying..."));
						Assert.That(errorLogMessages[3].MessageObject.ToString(), Is.EqualTo("Failed to load settings xml after retrying for 3 times"));
						Assert.That(errorLogMessages[3].ExceptionObject, Is.TypeOf<IOException>());
						Assert.That(errorLogMessages[3].ExceptionObject.Message, Does.StartWith("The process cannot access the file"));

						var warningLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Warn).ToList().Single();
						Assert.That(warningLogMessage.MessageObject.ToString(), Is.EqualTo("Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'"));
					}
				}

				mockPluginsManager.VerifyAll();
			}
			finally
			{
				File.Delete(settingsPath);
				File.Delete(statePath);
			}
		}

		[Test]
		public void TestOnPluginLastSuccessfulRunChanged_WriteToFile_FileIsOccupied_ShouldRetryAndReportToIssueManager()
		{
			var settingsPath = Path.GetTempFileName();
			var statePath = Path.GetTempFileName();

			try
			{
				var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>DummyBillingTransactionsPlugin</Key>
    <Active>true</Active>
    <TypeName>CargoWise.Billing.CollectorService.Plugin.Tests.DummyBillingTransactionsPlugin</TypeName>
    <IntervalMinutes>1</IntervalMinutes>
    <RetryIntervalSeconds>600</RetryIntervalSeconds>
    <MaxRetryAttempts>0</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				File.WriteAllText(settingsPath, settingsXml);

				var now = new DateTime(2018, 2, 2, 2, 0, 0);
				var clock = new TestClock { UtcNow = now };
				var initialLastSuccessfulRun = now - TimeSpan.FromSeconds(55);

				// create state
				var state = new PluginControllerState(clock)
				{
					Key = "DummyBillingTransactionsPlugin",
					LastSuccessfulRun = initialLastSuccessfulRun
				};
				var serviceState = new ServiceState();
				serviceState.Plugins.Add(state);
				serviceState.WriteToFile(statePath);

				var mockPluginsManager = new Mock<PluginsManager>(Assembly.GetExecutingAssembly(),
					settingsPath,
					statePath,
					globalSettings,
					new LoggerFactory().AddLog4Net(),
					() => serviceClientMock.Object,
					() => kafkaClientMock.Object,
					errorReportingClientMock.Object,
					null,
					null)
				{
					CallBase = true
				};
				mockPluginsManager.Setup(_ => _.CreateClock()).Returns(clock);

				using (var pluginsManager = mockPluginsManager.Object)
				{
					pluginsManager.Start();

					using (File.Open(statePath, FileMode.Open, FileAccess.Write))
					{
						clock.UtcNow = now.AddSeconds(5);
						Thread.Sleep(1500);

						memoryAppender = GetMemoryAppender();
						var errorLogMessages = memoryAppender.GetEvents().Where(e => e.Level == Level.Error).OrderBy(x => x.TimeStamp).ToList();
						Assert.That(errorLogMessages.Count, Is.EqualTo(4));
						Assert.That(errorLogMessages[0].MessageObject.ToString(), Is.EqualTo("Failed to write to state file. Retrying..."));
						Assert.That(errorLogMessages[1].MessageObject.ToString(), Is.EqualTo("Failed to write to state file. Retrying..."));
						Assert.That(errorLogMessages[2].MessageObject.ToString(), Is.EqualTo("Failed to write to state file. Retrying..."));
						Assert.That(errorLogMessages[3].MessageObject.ToString(), Is.EqualTo("Failed to write to state file after retrying for 3 times"));
						Assert.That(errorLogMessages[3].ExceptionObject, Is.TypeOf<IOException>());
						Assert.That(errorLogMessages[3].ExceptionObject.Message, Does.StartWith("The process cannot access the file"));

						var warningLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Warn).ToList().Single();
						Assert.That(warningLogMessage.MessageObject.ToString(), Is.EqualTo("Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'"));
					}
				}

				mockPluginsManager.VerifyAll();
			}
			finally
			{
				File.Delete(settingsPath);
				File.Delete(statePath);
			}
		}

		[Test]
		public void TestClockIsDelayed()
		{
			// delete the file so it generates its own.
			var statePath = Path.GetTempFileName();
			File.Delete(statePath);

			var settingsPath = Path.GetTempFileName();
			try
			{
				var settingsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
 <Plugins xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" >
  <Plugin>
    <Key>Test1</Key>
    <Active>true</Active>
    <TypeName>CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.Test.Plugin</TypeName>
    <IntervalMinutes>1</IntervalMinutes>
    <RetryIntervalSeconds>30</RetryIntervalSeconds>
    <MaxRetryAttempts>3</MaxRetryAttempts>
  </Plugin>
  <Plugin>
    <Key>Test2</Key>
    <Active>true</Active>
    <TypeName>CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.Test.Plugin</TypeName>
    <IntervalMinutes>2</IntervalMinutes>
    <RetryIntervalSeconds>30</RetryIntervalSeconds>
    <MaxRetryAttempts>3</MaxRetryAttempts>
  </Plugin>
</Plugins>";
				File.WriteAllText(settingsPath, settingsXml);
				using (var manager = new PluginsManager(Assembly.GetExecutingAssembly(), settingsPath, statePath, globalSettings, new LoggerFactory(), () => serviceClientMock.Object, () => kafkaClientMock.Object, errorReportingClientMock.Object))
				using (var clock = manager.CreateClock())
				{
					var expectedUtcNow = DateTime.UtcNow.AddSeconds(-59);
					var now = clock.UtcNow;
					var minRange = expectedUtcNow.AddMilliseconds(-20);
					var maxRange = expectedUtcNow.AddMilliseconds(20);
					Assert.That(now, Is.InRange(minRange, maxRange), "Expected: in range ({0:yyyy-MM-dd HH:mm:ss.fff}, {1:yyyy-MM-dd HH:mm:ss.fff})", minRange, maxRange, now);
				}
			}
			finally
			{
				File.Delete(settingsPath);
				File.Delete(statePath);
			}
		}

		[SetUp]
		public void SetUp()
		{
			memoryAppender = new MemoryAppender();
			BasicConfigurator.Configure(memoryAppender);
			serviceClientMock = new Mock<IBillingServiceClient>();
			kafkaClientMock = new Mock<IBillingKafkaClient>();
			errorReportingClientMock = new Mock<IErrorReportingClient>();
			errorReportingClientMock.Setup(x => x.ServiceUri).Returns(new Uri("", UriKind.Relative));
		}

		[TearDown]
		public void TearDown()
		{
			DummyBillingTransactionsPlugin.Reset();
		}

		MemoryAppender memoryAppender;
		Dictionary<string, string> globalSettings = new()
		{
			{ "UsageELKKafkaTopic", "TestTopic" }
		};
		static MemoryAppender GetMemoryAppender() => log4net.LogManager.GetRepository().GetAppenders().OfType<MemoryAppender>().Single();
		Mock<IBillingServiceClient> serviceClientMock;
		Mock<IBillingKafkaClient> kafkaClientMock;
		Mock<IErrorReportingClient> errorReportingClientMock;
	}
}
