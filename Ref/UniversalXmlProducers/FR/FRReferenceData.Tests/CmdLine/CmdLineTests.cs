using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.FRReferenceData.CmdLine;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.CmdLine
{
	[TestFixture]
	class CmdLineTests
	{
		[Test]
		public async Task TestTariffCreatorInResumeGenerationModeAsync()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.Combine(Path.GetTempPath(), "TestFRRefData");
			Directory.CreateDirectory(ApplicationConfig.Instance.OutputDirectory);
			Resumer.WriteOutStatus(Resumer.ResumeGeneration);

			var RITADataProviderMoq = new Mock<RITADataProvider>(new RITADataDownloader());
			RITADataProviderMoq.Setup(x => x.GetEUDeclarableTariffsThatDay(It.IsAny<DateTime>())).Returns(new string[] { "0000000000", "0101210000", "0208903000", "999999999" }.ToList());
			var RITADataProvider = RITADataProviderMoq.Object;

			UniversalDataHelper.SentEmails.Clear();
			await TariffCreator.DownloadAndGenerateURDFiles(RITADataProvider, false, Errors.No);

			Assert.That(IsDirectoryEmpty(ApplicationConfig.Instance.OutputDirectory));
			Assert.That(UniversalDataHelper.SentEmails.Count == 1);
			Assert.That(UniversalDataHelper.SentEmails.Contains(new Email
			{
				From = "donotreply_refservice@wisetechglobal.com",
				To = ApplicationConfig.Instance.EmailRecipients,
				Subject = "FR Reference Data TARIFF_REGEN program failed.",
				Body = @"FR Reference Data TARIFF_REGEN program failed when generating output files.
Not all required raw data files from Customs were available in download directory.
Here are the missing tariffs (only the first 100 items are shown):
0000000000,999999999
You should consider rescheduling the program again."
			}));
		}

		bool IsDirectoryEmpty(string path)
		{
			IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
			using (IEnumerator<string> en = items.GetEnumerator())
			{
				return !en.MoveNext();
			}
		}

		[Test]
		public void TestEmptyArgument()
		{
			AssertArgumentError(string.Empty, "No command line argument found.");
		}

		[Test]
		public void TestInvalidFunction()
		{
			AssertArgumentError("nothing", "Invalid argument entered: NOTHING");
		}

		internal static void AssertArgumentError(string parameters, string expectedErrorMessage)
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var processStartInfo = new ProcessStartInfo(Path.Combine(binPath, "CargoWise.RefDbRepo.FRReferenceData.CmdLine.exe"), parameters)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			processStartInfo.EnvironmentVariables.Add("RefDataRepoTesting", "Test");
			var message = ProcessRunner.RunProcess(processStartInfo, false);
			Assert.That(message, Does.Contain(expectedErrorMessage));
		}
	}
}
