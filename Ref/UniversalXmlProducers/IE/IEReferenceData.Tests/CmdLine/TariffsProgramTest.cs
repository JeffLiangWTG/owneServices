using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.IEReferenceData.Tariffs.CmdLine;
using CargoWise.RefDbRepo.IEReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.CmdLine.Tests
{
	[TestFixture]
	class TariffsProgramTest
	{
		[Test]
		public void TestEmailSending()
		{
			var directory = TestHelper.GetRunningDirectory();
			var configMock = TestHelper.GetBaseMock().SetupTariffTestInputPaths();
			configMock.Setup(x => x.ExciseDuty_Url_Alcohol_Products).Returns(Path.Combine(directory, TestHelper.ExciseDutyTariffTestInputPath, "Alcohol Products Tax - With Unrecognizable Item.html"));
			var config = configMock.Object;

			var logger = new Logger();
			TariffsProgram.Run(config, logger);

			Assert.AreEqual(1, EmailNotificationHelper.SentEmails.Count);
			var sentEmail = EmailNotificationHelper.SentEmails.First();
			Assert.AreEqual("Excise Duty Rate: Unrecognizable item detected", sentEmail.Subject);
			Assert.AreEqual(ApplicationConfig.Instance.EmailSender, sentEmail.From);
			Assert.AreEqual(ApplicationConfig.Instance.ExciseDuty_EmailNotificationRecipients, sentEmail.To);
		}

		[SetUp]
		public void SetUp()
		{
			TestHelper.ClearRuntimeData();
		}

		[TearDown]
		public void TearDown()
		{
			TestHelper.ClearRuntimeData();
		}
	}
}
