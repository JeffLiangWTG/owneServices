using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;
using CargoWise.eHub.Portal.Helpers;
using Common.Logging;
using Moq;
using NUnit.Framework;
using static CargoWise.eHub.Portal.Controllers.Service.OceanCarrierMessagingController.TypeNames;

namespace CargoWise.eHub.Portal.Tests.HelperTests
{
	public class OcmConfigBackupManagerTest
	{
		private Mock<OcmConfigBackupManager> _managerMock;
		private Mock<HttpServerUtilityBase> _serverMock;
		private Mock<ILog> _loggerMock;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<ILog>();

			_managerMock = new Mock<OcmConfigBackupManager>{ CallBase = true };
			_managerMock.Object.Logger = _loggerMock.Object;
			_managerMock.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));
			_managerMock.Setup(x => x.Delete(It.IsAny<string>()));
			_managerMock.Setup(x => x.GetFiles(It.IsAny<string>())).Returns<string>(typeId => new []
			{
				Path.Combine("Temp", "OCMRoutingRuleBackups", typeId, "20230722021918605_SplitingBlackList.csv")
			});

			_serverMock = new Mock<HttpServerUtilityBase>();
			_serverMock.Setup(x => x.MapPath(It.IsAny<string>())).Returns<string>(input => Path.Combine("Temp", input));
		}

		[Test]
		public void TestBackup_Success()
		{
			var content = "This is a test content";
			var encodedContent = Encoding.Default.GetBytes(content);
			var capturedPath = string.Empty;
			var capturedContent = string.Empty;
			_managerMock.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
				.Callback<string, string>((path, ctnt) =>
				{
					capturedPath = path;
					capturedContent = ctnt;
				});

			_managerMock.Object.Backup(_serverMock.Object, CarrierBookingAgent, encodedContent);

			_managerMock.Verify(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			_managerMock.Verify(x => x.GetFiles(It.IsAny<string>()), Times.Once);
			_managerMock.Verify(x => x.Delete(It.IsAny<string>()), Times.AtLeastOnce);
			Assert.That(capturedPath, Does.StartWith(Path.Combine("Temp", "~", "OCMRoutingRuleBackups", "CarrierBookingAgent")));
			Assert.That(capturedPath, Does.EndWith("_CarrierBookingAgent.csv"));
			Assert.That(capturedContent, Is.EqualTo(content));
		}

		[TestCase(SplitingBlackList)]
		[TestCase(SplitingWhiteList)]
		[TestCase(MultipleRecipients)]
		[TestCase(CarrierHandlingAgent)]
		[TestCase(CarrierBookingAgent)]
		[TestCase(DefaultCarrier)]
		public void TestBack_SubfolderPerType(string typeId)
		{
			string capturedPath = string.Empty;
			_managerMock.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
				.Callback<string, string>((saveFilePath, _) => capturedPath = saveFilePath);

			var content = Encoding.Default.GetBytes("This is a test content");
			_managerMock.Object.Backup(_serverMock.Object, typeId, content);

			Assert.That(capturedPath, Is.Not.NaN);
			Assert.That(capturedPath, Is.Not.Empty);
			Assert.That(capturedPath, Does.Contain(typeId));
		}

		[Test]
		public void TestBackup_DeleteFilesOneMonthEarlier()
		{
			var basePath = Path.Combine("Temp", "OCMRoutingRuleBackups");
			var yesterday = Path.Combine(basePath, "yesterday_SplitingBlackList.csv");
			var oneMonthAgo = Path.Combine(basePath, "oneMonthAgo_SplitingBlackList.csv");
			var oneYearAgo = Path.Combine(basePath, "oneYearAgo_SplitingBlackList.csv");

			_managerMock.Setup(x => x.GetFiles(It.IsAny<string>())).Returns(new []
			{
				yesterday,
				oneMonthAgo,
				oneYearAgo
			});

			_managerMock.Setup(x => x.GetFileInfoCreationTimeUtc(It.IsAny<string>())).Returns<string>(file =>
			{
				DateTime createTime = DateTime.UtcNow;
				if (file == yesterday) createTime = DateTime.UtcNow.AddDays(-1);
				if (file == oneMonthAgo) createTime = DateTime.UtcNow.AddMonths(-1).AddTicks(-1);
				if (file == oneYearAgo) createTime = DateTime.UtcNow.AddYears(-1).AddTicks(-1);

				return createTime;
			});

			_managerMock.Object.Backup(_serverMock.Object, DefaultCarrier, Encoding.Default.GetBytes("File content"));

			_managerMock.Verify(x => x.Delete(yesterday), Times.Never);
			_managerMock.Verify(x => x.Delete(oneMonthAgo), Times.Once);
			_managerMock.Verify(x => x.Delete(oneYearAgo), Times.Once);
		}

		[TestCase("")]
		[TestCase("CustomBackupFolder")]
		public void TestBackup_ConfigurableSubFolder(string appSettingsValue)
		{
			ConfigurationManager.AppSettings[OcmConfigBackupManager.BackupFolderKey] = appSettingsValue;
			string capturedPath = string.Empty;
			_managerMock.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
				.Callback<string, string>((saveFilePath, _) => capturedPath = saveFilePath);

			_managerMock.Object.Backup(_serverMock.Object, DefaultCarrier, Encoding.Default.GetBytes("File content"));

			if (string.IsNullOrWhiteSpace(appSettingsValue))
			{
				Assert.That(capturedPath, Does.Contain(OcmConfigBackupManager.BackupFolderDefault));
			}
			else
			{
				Assert.That(capturedPath, Does.Contain(appSettingsValue));
			}

			ConfigurationManager.AppSettings[OcmConfigBackupManager.BackupFolderKey] = string.Empty;
		}

		[TestCaseSource(nameof(BackupTestCases))]
		public void TestBackup_BackupPathIsFromRoot(string appSettingsValue)
		{
			ConfigurationManager.AppSettings[OcmConfigBackupManager.BackupFolderKey] = appSettingsValue;
			string capturedPath = string.Empty;
			_serverMock.Setup(x => x.MapPath(It.IsAny<string>())).Callback<string>(s => capturedPath = s).Returns<string>(s => s);

			_managerMock.Object.Backup(_serverMock.Object, DefaultCarrier, Encoding.Default.GetBytes("File content"));

			Assert.That(capturedPath.StartsWith("~" + Path.DirectorySeparatorChar));
		}

		public static string[] BackupTestCases =
		{
			"",
			"CustomBackupFolder",
			Path.Combine("..", "CustomBackupFolder"),
			Path.Combine(".", "CustomBackupFolder")
		};
	}
}
