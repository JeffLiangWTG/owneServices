using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.SEReferenceData.CmdLine;
using CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Services;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SEReferenceData.ExchangeRates.Tests
{
	[TestFixture]
	class ExchangeRatesProgramTest
	{
		[Test]
		public void IncrementalFileNameStructureException()
		{
			using (var stringWriter = new StringWriter())
			{
				Console.SetOut(stringWriter);
				Console.SetError(stringWriter);
				var downloaderMock = new Mock<IDownloadExchangeRates>();
				downloaderMock.Setup(x => x.FindLatestIncrementFile(ApplicationConfig.IncrementalDailyRepositoryUrl)).Returns(string.Empty);
				ExchangeRatesProgram.Run(outputPath, downloaderMock.Object);
				Assert.That(stringWriter.ToString(), Does.StartWith("Unable to find any Incremental Files for the Exchange Rates going back two months."));
			}
		}

		[Test]
		public void MultipleFilesProduced()
		{
			const string incrementalFile = "IncrementalObjectTraderExport_80a5d811-cf30-4773-8733-638de5364a3d_210104.xml.gz.pgp";

			var downloaderMock = new Mock<IDownloadExchangeRates>();
			downloaderMock.Setup(x => x.FindLatestIncrementFile(ApplicationConfig.IncrementalDailyRepositoryUrl)).Returns(incrementalFile);
			var monetaryExchangeRates = new monetaryExchangeRate[]
			{
				new monetaryExchangeRate
				{
					calculationUnit = 1,
					monetaryConversionRate = 2.56278831m,
					monetaryUnitCode = "CZK",
					national = 1
				}
			};

			var monetaryExchangePeriod1 = new monetaryExchangePeriod
			{
				national = 1,
				monetaryUnitCode = "SEK",
				SID = -468,
				dateStart = new DateTime(2020, 12, 01),
				changeType = "U",
				monetaryExchangeRate = monetaryExchangeRates
			};

			var monetaryExchangePeriod2 = new monetaryExchangePeriod
			{
				national = 1,
				monetaryUnitCode = "SEK",
				SID = 2996,
				dateStart = new DateTime(2021, 01, 01),
				changeType = "U",
				monetaryExchangeRate = monetaryExchangeRates
			};
			var monetaryExchangePeriods = new monetaryExchangePeriod[] { monetaryExchangePeriod1, monetaryExchangePeriod2 };
			downloaderMock.Setup(x => x.DownloadLatestIncrementalAndExtract(Path.Combine(ApplicationConfig.IncrementalDailyRepositoryUrl, incrementalFile))).Returns(monetaryExchangePeriods);
			var expectedfile1 = Path.Combine(outputPath, "RefExchangeRateZZ_SE_-468.xml");
			var expectedfile2 = Path.Combine(outputPath, "RefExchangeRateZZ_SE_2996.xml");
			try
			{
				ExchangeRatesProgram.Run(outputPath, downloaderMock.Object);
				Assert.That(File.Exists(expectedfile1), Is.True);
				Assert.That(File.Exists(expectedfile2), Is.True);
				Assert.That(new FileInfo(expectedfile2).Length, Is.GreaterThan(0));
			}
			finally
			{
				foreach (var file in new[] { expectedfile1, expectedfile2 })
				{
					if (File.Exists(file))
					{
						File.Delete(file);
					}
				}
			}
		}

		[OneTimeSetUp]
		public void Setup()
		{
			var assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ExchangeRates\TestFiles\Output");
		}
		string outputPath;
	}
}
