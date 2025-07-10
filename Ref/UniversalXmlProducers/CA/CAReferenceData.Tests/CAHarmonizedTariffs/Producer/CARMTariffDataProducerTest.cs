using System;
using System.Globalization;
using System.IO;
using System.Web;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.CAReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class CARMTariffDataProducerTest : TariffProducerTest
	{
		Mock<IWebServiceCaller> callerMock;
		string WorkingDirectory;

		protected override string FunctionCode => Constants.ProgramFunctions.CATariff;

		protected override string NothingNewPublishedMessage => $"CARM Tariff: Nothing new published since last process. Skip processing this time.";

		[Test]
		public override void TestNothingNewPublished()
		{
			var checker = new PreProcessChecker(FunctionCode);
			checker.MarkAsProcessRequired();
			var expectedMessage = NothingNewPublishedMessage;
			LogBuilder.Clear();
			ConsoleOutput.GetStringBuilder().Clear();
			DownloaderMock.Setup(x => x.DownloadFile(null, It.IsAny<string>())).Returns(true);
			callerMock.Setup(x => x.QueryAndDownloadXmlFiles(WorkingDirectory, It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
			callerMock.Setup(x => x.GetLatestUpdateOnDate(It.IsAny<string>())).Returns(DateTime.Now);

			TariffProducer.QueryDataAndParseToXMLFile();
			Assert.That(LogBuilder.ToString(), Does.Not.Contain(expectedMessage));
			Assert.That(ConsoleOutput.ToString(), Does.Not.Contain(expectedMessage));

			LogBuilder.Clear();
			ConsoleOutput.GetStringBuilder().Clear();
			callerMock.Setup(x => x.GetLatestUpdateOnDate(It.IsAny<string>())).Returns(DateTime.Now.AddHours(-1));
			TariffProducer.QueryDataAndParseToXMLFile();
			Assert.That(LogBuilder.ToString(), Does.Not.Contain(expectedMessage));
			Assert.That(ConsoleOutput.ToString(), Does.Contain(expectedMessage));
		}

		[Test]
		public void TestQueryLatestUpdateTime()
		{
			ConsoleOutput.GetStringBuilder().Clear();
			callerMock.Setup(x => x.QueryAndDownloadXmlFiles(WorkingDirectory, It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);
			callerMock.Setup(x => x.GetLatestUpdateOnDate(It.IsAny<string>())).Returns((DateTime?)null);

			TariffProducer.QueryDataAndParseToXMLFile();
			Assert.That(ConsoleOutput.ToString(), Does.Contain("CARM Tariff: Latest update time not found"));

			ConsoleOutput.GetStringBuilder().Clear();
			var dateTime = new DateTime(2024, 12, 13);
			callerMock.Setup(x => x.GetLatestUpdateOnDate(It.IsAny<string>())).Returns(dateTime);
			TariffProducer.QueryDataAndParseToXMLFile();
			Assert.That(ConsoleOutput.ToString(), Does.Contain("CARM Tariff: Latest update time " + dateTime.ToString(CultureInfo.InvariantCulture)));

			ConsoleOutput.GetStringBuilder().Clear();
			var dateTime2 = new DateTime(2024, 12, 14);
			callerMock.Setup(x => x.GetLatestUpdateOnDate(It.IsAny<string>())).Returns((DateTime?)null);
			callerMock.Setup(x => x.GetLatestUpdateOnDate(CARMAPIQueryTypes.TariffQueryType)).Returns(dateTime);
			callerMock.Setup(x => x.GetLatestUpdateOnDate(CARMAPIQueryTypes.ExciseTaxesQueryType)).Returns(dateTime2);
			TariffProducer.QueryDataAndParseToXMLFile();
			Assert.That(ConsoleOutput.ToString(), Does.Contain("CARM Tariff: Latest update time " + dateTime2.ToString(CultureInfo.InvariantCulture)));
		}

		[Test]
		public void TestFolderClearedFinally()
		{
			DownloaderMock.Setup(x => x.DownloadFile(null, It.IsAny<string>())).Returns(true);
			Directory.CreateDirectory(WorkingDirectory);
			var testFile = TestHelper.CreateTempFile(WorkingDirectory, "Test.txt", "TEST");
			Assert.IsTrue(File.Exists(testFile));

			callerMock.Setup(x => x.QueryAndDownloadXmlFiles(WorkingDirectory, It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), "EN", "")).Returns(true);
			TariffProducer.QueryDataAndParseToXMLFile();
			Assert.IsFalse(File.Exists(testFile));
			Assert.IsFalse(Directory.Exists(WorkingDirectory));
		}

		[Test]
		public void TestQueryFilterParam()
		{
			Assert.True(DateTime.TryParse("2021-05-25", out DateTime lastPublishDate));

			var lastPublishDateTime = lastPublishDate.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
			Console.WriteLine($"CARM Tariff: Downloading files from webservice, last publish time [{0}]", lastPublishDateTime);

			var filter = string.IsNullOrEmpty(lastPublishDateTime) ? string.Empty : $"{nameof(CARMTariff.UpdateOn)} gt datetime'{HttpUtility.UrlEncode(lastPublishDateTime)}'";
			Assert.AreEqual("UpdateOn gt datetime'2021-05-25T00%3a00%3a00'", filter);
		}

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			WorkingDirectory = Path.Combine(Path.GetTempPath(), FunctionCode);
		}

		protected override IProducer CreateNewProducer()
		{
			var conditionFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CAHarmonizedTariff\all-pga-programs.xlsx");
			return new CARMTariffDataProducer(callerMock.Object, LogBuilder, DownloaderMock.Object, conditionFileName);
		}

		protected override void CreateDownloaderMockData()
		{
			callerMock = new Mock<IWebServiceCaller>();
			callerMock.Setup(x => x.QueryAndDownloadXmlFiles(WorkingDirectory, "", new string[] { }, new string[] { }, null, null)).Verifiable();
			DownloaderMock.Setup(x => x.DownloadFile(null, It.IsAny<string>())).Returns(false);
		}
	}
}
