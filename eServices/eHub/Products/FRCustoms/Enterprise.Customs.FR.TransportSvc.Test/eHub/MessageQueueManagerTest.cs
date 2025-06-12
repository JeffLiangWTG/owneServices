using System.IO;
using Enterprise.Customs.FR.TransportSvc.Messages;
using Enterprise.Customs.FR.TransportSvc.Utilities;
using NUnit.Framework;

namespace Enterprise.Customs.FR.TransportSvc.Test
{
	[TestFixture]
	class MessageQueueManagerTest
	{
		[Test]
		public void TestDownloadIncomingMessages()
		{
			ApplicationConfig.Instance.WorkingDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";
			ApplicationConfig.Instance.EHubTestEnabled = "1";

			var log = new Logger();
			Logger.DumpLog();

			Assert.DoesNotThrow(() => MessagesQueueManager.DownloadIncomingMessages(log));
			var logFileContent = File.ReadAllText(Logger.CurrentDateLogFilePath);
			Assert.That(logFileContent, Does.Contain("Retrieving test messages from production eHub."));
			Assert.That(logFileContent, Does.Contain("Retrieving test messages from test eHub."));
		}

		[Test]
		public void TestDownloadIncomingMessagesReportsIfeHubNotConfigured()
		{
			ApplicationConfig.Instance.WorkingDirectory = TestHelper.CreateTestDirectory();
			ApplicationConfig.Instance.ExecuteForDebugging = "1";
			ApplicationConfig.Instance.EHubProductionEnabled = "0";
			ApplicationConfig.Instance.EHubTestEnabled = "1";
			ApplicationConfig.Instance.EHubForTestAddress = string.Empty;

			var log = new Logger();
			Logger.DumpLog();

			Assert.DoesNotThrow(() => MessagesQueueManager.DownloadIncomingMessages(log));
			var logFileContent = File.ReadAllText(Logger.CurrentDateLogFilePath);
			Assert.That(logFileContent, Does.Contain("Retrieving test messages from production eHub."));
			Assert.That(logFileContent, Does.Contain("Retrieving test messages from test eHub."));
			Assert.That(logFileContent, Does.Contain("Invalid URI: The URI is empty."));

			Logger.DumpLog();
			ApplicationConfig.Instance.EHubForTestAddress = "https://ehub-ausyd-test.cargowise.net/eHubGateway/eHubStreamedService.svc";
			Assert.DoesNotThrow(() => MessagesQueueManager.DownloadIncomingMessages(log));
			logFileContent = File.ReadAllText(Logger.CurrentDateLogFilePath);
			Assert.That(logFileContent, Does.Not.Contain("Invalid URI: The URI is empty."));
		}

		[Test]
		public void TesteHubForTestAddressConfigured()
		{
			Assert.That(ApplicationConfig.Instance.EHubForTestAddress, Is.EqualTo("https://ehub-ausyd-test.cargowise.net/eHubGateway/eHubStreamedService.svc"));
		}
	}
}
