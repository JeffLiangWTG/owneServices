using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.CmdLine;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.CmdLine
{
	[TestFixture]
	class ResumerTest
	{
		[Test]
		public void TestGetProcessStartdate()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			var testDownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.DownloadDirectory = testDownloadDirectory;
			var exectedOutputFile = Path.Combine(testDownloadDirectory, "LastRun.txt");
			File.Delete(exectedOutputFile);
			Assert.IsFalse(File.Exists(exectedOutputFile));
			Assert.AreEqual(Resumer.GetProcessStartDate(), DateTime.Today.AddDays(-1));

			var date = DateTime.Today.AddDays(-1);
			Resumer.StoreProcessStartDate(date);
			Assert.IsTrue(File.Exists(exectedOutputFile));
			Assert.AreEqual(Resumer.GetProcessStartDate(), date);
		}

		[Test]
		public void TestStoreProcessStartDate()
		{
			var testDownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.DownloadDirectory = testDownloadDirectory;
			var date = DateTime.Today;
			Resumer.StoreProcessStartDate(date);
			var exectedOutputFile = Path.Combine(testDownloadDirectory, "LastRun.txt");
			Assert.IsTrue(File.Exists(exectedOutputFile));
			var exectedOutputFileContent = File.ReadAllText(exectedOutputFile);
			Assert.IsTrue(exectedOutputFileContent.Contains(DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
		}

		[Test]
		public void TestWriteOutStatus()
		{
			var testDownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.DownloadDirectory = testDownloadDirectory;
			Resumer.WriteOutStatus(Resumer.DoNotResume);
			var exectedOutputFile = Path.Combine(testDownloadDirectory, "Resume.txt");
			Assert.IsTrue(File.Exists(exectedOutputFile));
			var exectedOutputFileContent = File.ReadAllText(exectedOutputFile);
			Assert.IsTrue(exectedOutputFileContent.Contains(Resumer.DoNotResume));
		}

		[Test]
		public void TestParseStatus()
		{
			var testDownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.DownloadDirectory = testDownloadDirectory;

			if (File.Exists(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "Resume.txt")))
			{
				File.Delete(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "Resume.txt"));
			}

			Assert.AreEqual(Resumer.DoNotResume, Resumer.ParseStatus());
			Resumer.WriteOutStatus(Resumer.ResumeDownload + " 1234567890");
			Assert.AreEqual(Resumer.ResumeDownload + " 1234567890", Resumer.ParseStatus());
		}

		[Test]
		public void TestGetTariffFromStatus()
		{
			var testDownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.DownloadDirectory = testDownloadDirectory;
			Assert.AreEqual("1234567890", Resumer.GetTariffFromStatus(Resumer.ResumeDownload + " 1234567890"));
			Assert.AreEqual("", Resumer.GetTariffFromStatus(Resumer.ResumeGeneration));
		}
	}
}
