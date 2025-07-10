using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	abstract class TariffParserTests
	{
		[Test]
		public void DownloadAndConvertCodesToXMLFile()
		{
			using (var webServiceMockStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.Tariffs.TestFiles.Input." + WebServiceMockResultFileName))
			{
				var expectXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.USReferenceData.Tests.Tariffs.TestFiles.Output." + ExpectFileName);
				var clientMock = new Mock<IDownLoadService>();
				var mockPublicationTime = (true, new DateTime(2024, 1, 1));
				clientMock.Setup(x => x.GetDateTimeFromHtmlNode(null)).Returns(mockPublicationTime);
				clientMock.Setup(x => x.DownloadFile(string.Empty, ApplicationConfig.Instance.CensusEndPoint, DownloadFilePath)).Returns(true);
				SaveToFileOfDownloadPath(webServiceMockStream);

				var codesParser = (TariffParser)Activator.CreateInstance(ParserObjectType, clientMock.Object, new DateTime(2024, 01, 05, 0, 0, 0));
				codesParser.DownloadAndConvertCodesToXMLFile(TestOutputFilePath);
				Assert.That(expectXML, Is.EqualTo(File.ReadAllText(Path.Combine(TestOutputFilePath, OutputFileName))));
			}
		}

		[Test]
		public void FileDownloadFailed()
		{
			using (var webServiceMockStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.Tariffs.TestFiles.Input." + WebServiceMockResultFileName))
			{
				var clientMock = new Mock<IDownLoadService>();
				var mockPublicationTime = (true, new DateTime(2024, 1, 1));
				clientMock.Setup(x => x.GetDateTimeFromHtmlNode(null)).Returns(mockPublicationTime);
				clientMock.Setup(x => x.DownloadFile(string.Empty, ApplicationConfig.Instance.CensusEndPoint, DownloadFilePath)).Returns(false);
				SaveToFileOfDownloadPath(webServiceMockStream);

				var codesParser = (TariffParser)Activator.CreateInstance(ParserObjectType, clientMock.Object, new DateTime(2024, 01, 05, 0, 0, 0));
				var error = codesParser.DownloadAndConvertCodesToXMLFile(null);
				Assert.That(error, Does.Contain($"Unable to download file from the website. Processing has failed for Tariff Type: {TariffType}"));
			}
		}

		[Test]
		public void InvalidPublicationDate()
		{
			var clientMock = new Mock<IDownLoadService>();

			var mockPublicationTime = (false, DateTime.MinValue);
			clientMock.Setup(x => x.GetDateTimeFromHtmlNode(null)).Returns(mockPublicationTime);

			var codesParser = (TariffParser)Activator.CreateInstance(ParserObjectType, clientMock.Object, new DateTime(2024, 01, 05, 0, 0, 0));
			var error = codesParser.DownloadAndConvertCodesToXMLFile(null);
			Assert.That(error, Does.Contain($"Unable to get file update date from the website. Processing has failed for Tariff Type: {TariffType}"));
		}

		[Test]
		public void InvalidTariffDataRecord()
		{
			using (var webServiceMockStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.Tariffs.TestFiles.Input.Invalid.txt"))
			{
				var clientMock = new Mock<IDownLoadService>();

				var mockPublicationTime = (true, new DateTime(2024, 1, 1));
				clientMock.Setup(x => x.GetDateTimeFromHtmlNode(null)).Returns(mockPublicationTime);
				clientMock.Setup(x => x.DownloadFile(string.Empty, ApplicationConfig.Instance.CensusEndPoint, DownloadFilePath)).Returns(true);
				SaveToFileOfDownloadPath(webServiceMockStream);

				var codesParser = (TariffParser)Activator.CreateInstance(ParserObjectType, clientMock.Object, new DateTime(2024, 01, 05, 0, 0, 0));
				var error = codesParser.DownloadAndConvertCodesToXMLFile(TestOutputFilePath);
				Assert.That(error, Does.Contain($"Tariff Type: {TariffType}"));
				Assert.That(error, Does.Contain("Code: 6406901530"));
				Assert.That(error, Does.Contain("Description: LEG WARMERS"));
				Assert.That(error, Does.Contain("First UOM: DOZ"));
				Assert.That(error, Does.Contain("Second UOM: KG"));
			}
		}

		[Test]
		public void TestDownloadAndConvertCodesToXMLFile_WithPGA()
		{
			using (var webServiceMockStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.Tariffs.TestFiles.Input." + WebServiceMockResultFileName))
			{
				var expectXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.USReferenceData.Tests.Tariffs.TestFiles.Output." + ExpectWithPGAFileName);
				var clientMock = new Mock<IDownLoadService>();
				var mockPublicationTime = (true, new DateTime(2019, 12, 31));
				clientMock.Setup(x => x.GetDateTimeFromHtmlNode(null)).Returns(mockPublicationTime);
				clientMock.Setup(x => x.DownloadFile(string.Empty, ApplicationConfig.Instance.CensusEndPoint, DownloadFilePath)).Returns(true);
				SaveToFileOfDownloadPath(webServiceMockStream);

				var ev1List = new List<EV1>();
				ev1List.Add(new EV1("0101210000", true));
				ev1List.Add(new EV1("0101210010", true));
				ev1List.Add(new EV1("0101290010", false));
				ev1List.Add(new EV1("0101300000", false));
				var codesParser = (TariffParser)Activator.CreateInstance(ParserObjectType, clientMock.Object, new DateTime(2020, 01, 07, 0, 0, 0), GenerateTariff4PGAData(), ev1List);
				codesParser.DownloadAndConvertCodesToXMLFile(TestOutputFilePath);
				var actual = File.ReadAllText(Path.Combine(TestOutputFilePath, OutputFileName));
				Assert.That(expectXML, Is.EqualTo(actual));
			}
		}

		[Test]
		public void TestCensusPageNotFound()
		{
			var site = new HtmlDocument();
			var sitehtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.USReferenceData.Tests.Tariffs.TestFiles.Input.AESUpdateInfoPageNotFound.html");
			site.LoadHtml(sitehtml);
			var htmlNodeWithDate = site.DocumentNode.Descendants().FirstOrDefault(node => node.Name == Constants.HtmlNodeNames.H3 && node.InnerText.Contains(UpdateInfoNodeKeyword));

			var clientMock = new Mock<IDownLoadService>();
			clientMock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() => htmlNodeWithDate);

			var codesParser = (TariffParser)Activator.CreateInstance(ParserObjectType, clientMock.Object, new DateTime(2024, 01, 05, 0, 0, 0));
			var error = codesParser.DownloadAndConvertCodesToXMLFile(null);
			Assert.That(error, Does.Contain($"Unable to get file update date from the website. Processing has failed for Tariff Type: {TariffType}"));
		}

		void SaveToFileOfDownloadPath(Stream webServiceMockStream)
		{
			byte[] srcBuf = new byte[webServiceMockStream.Length];
			webServiceMockStream.Read(srcBuf, 0, srcBuf.Length);
			webServiceMockStream.Seek(0, SeekOrigin.Begin);
			using (var fs = new FileStream(DownloadFilePath, FileMode.Create, FileAccess.Write))
			{
				fs.Write(srcBuf, 0, srcBuf.Length);
				fs.Close();
			}
		}

		Dictionary<string, Tariff4PGA[]> GenerateTariff4PGAData()
		{
			var resutl = new Dictionary<string, Tariff4PGA[]>
			{
				{ "0101210010", new Tariff4PGA[] { new Tariff4PGA("0101210010", true, "AMS") } },
				{ "0101210000", new Tariff4PGA[] { new Tariff4PGA("0101210000", false, "EPA"), new Tariff4PGA("0101210000", true, "AMS"), new Tariff4PGA("0101210000", true, "NMFS") } } ,
				{ "9999999999", new Tariff4PGA[] { new Tariff4PGA("9999999999", true, "AMS") } },
				{ "010190", new Tariff4PGA[] { new Tariff4PGA("010190", true, "TTB") } },
			};
			return resutl;
		}

		string TestOutputFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Tariffs\TestFiles", @"Output");

		string DownloadFilePath => Path.Combine(Path.GetTempPath(), DownloadFileName);

		protected abstract string TariffType { get; }

		protected abstract string WebServiceMockResultFileName { get; }

		protected abstract Type ParserObjectType { get; }

		protected abstract string DownloadFileName { get; }

		protected abstract string UpdateInfoNodeKeyword { get; }

		protected virtual string ExpectedTestFileName => "RefCusTariffZZ_US_" + TariffType + "_TYPE.xml";
		protected virtual string OutputFileName => "RefCusTariffZZ_US_" + TariffType + "_TYPE.xml";

		protected virtual string ExpectFileName => OutputFileName;

		protected virtual string ExpectWithPGAFileName => "RefCusTariffZZ_US_" + TariffType + "_TYPE_PGA.xml";

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		Assembly assembly;
	}
}
