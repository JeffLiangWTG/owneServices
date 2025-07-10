using System;
using System.Globalization;
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
	public class UnitedNationsStandardProductAndServiceCodesParserTest
	{
		[Test]
		public void TestUnitedNationsStandardProductAndServiceCodesParser()
		{
			using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.AMSCATAIRGuidelinesMay2022.pdf"))
			using (var stream2 = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.CopyOfCEMSActiveUNSPSCCodesWithCreatedOn20150724.xlsx"))
			{
				var outputFileDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RefCusCodeList\TestFiles");
				var parser = new UNSPCParserTest(outputFileDirectoryPath);
				var inputPDFFilePath = parser.InputPDFFilePath_Exposed;
				SaveToFileOfDownloadPath(stream, inputPDFFilePath);
				var inputExcelFilePath = parser.InputExcelFilePath_Exposed;
				SaveToFileOfDownloadPath(stream2, inputExcelFilePath);
				if (File.Exists(parser.OutputFilePath_Exposed))
				{
					File.Delete(parser.OutputFilePath_Exposed);
				}
				var logs = parser.ConvertCodeListToXML();

				var exceptXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.ExpectedUNSPCForBothPDFAndExcel.xml");
				Assert.That(exceptXML, Is.EqualTo(File.ReadAllText(parser.OutputFilePath_Exposed)));

				Assert.That(logs, Does.Contain("Have got the Excel file and ready to start converting it to intermediate data."));
				Assert.That(logs, Does.Contain("Intermediate data parsing is complete. (Set USDA_AMS_PGM OTH attribute)"));
				Assert.That(logs, Does.Contain("Intermediate data parsing is complete. (Set USDA_AMS_PGM MO6 attribute)"));

				Assert.That(logs, Does.Contain("Have got the PDF file and ready to start converting it to intermediate data."));
				Assert.That(logs, Does.Contain("Intermediate data parsing is complete. (Set USDA_AMS_PGM EG1 and PN1 attribute)"));
			}
		}

		[Test]
		public void TestExcelFileShouldNotBeUpdated()
		{
			using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.CopyOfCEMSActiveUNSPSCCodesWithCreatedOn20150724.xlsx"))
			{
				var outputFileDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RefCusCodeList\TestFiles");
				var parser = new UNSPCExcelParserTest(outputFileDirectoryPath);
				var inputFilePath = parser.InputExcelFilePath_Exposed;
				SaveToFileOfDownloadPath(stream, inputFilePath);
				var logs = parser.ConvertCodeListToXML();
				Assert.That(logs, Does.Contain("It doesn't need to be updated. The excel file is up to date."));
			}
		}

		[Test]
		public void TestPDFDateTimeNodeIsNull()
		{
			var outputFileDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RefCusCodeList\TestFiles");
			var parser = new UNSPCDownloadAndConvertToXMLParserTest(outputFileDirectoryPath);

			var clientMock = new Mock<IDownLoadService>();
			clientMock.Setup(x => x.FindNode(ApplicationConfig.Instance.UnitedNationsStandardProductAndServiceCodesURLForPDF, It.IsAny<Func<HtmlNode, bool>>()))
					.Returns((string x, Func<HtmlNode, bool> c) =>
					{
						return null;
					});

			parser.ServiceClient = clientMock.Object;
			var logs = parser.ConvertCodeListToXML();
			Assert.That(logs, Does.Contain("The layout of the website has changed. Failed to obtain publication time."));
		}

		[Test]
		public void TestPDFDateTimeIsWrong()
		{
			var outputFileDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RefCusCodeList\TestFiles");
			var parser = new UNSPCDownloadAndConvertToXMLParserTest(outputFileDirectoryPath);

			var site = new HtmlDocument();
			var sitehtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.TestHtmlForUNSPC.html");
			site.LoadHtml(sitehtml);

			var dateTimeNode = site.DocumentNode.Descendants().Where(node => node.Name == Constants.HtmlNodeNames.SPAN && node.InnerText.ToUpper(CultureInfo.InvariantCulture).Contains("LAST MODIFIED:")).ToArray()[1];

			var clientMock = new Mock<IDownLoadService>();
			clientMock.Setup(x => x.FindNode(ApplicationConfig.Instance.UnitedNationsStandardProductAndServiceCodesURLForPDF, It.IsAny<Func<HtmlNode, bool>>()))
					.Returns((string x, Func<HtmlNode, bool> c) =>
					{
						return dateTimeNode;
					});

			parser.ServiceClient = clientMock.Object;
			var logs = parser.ConvertCodeListToXML();
			Assert.That(logs, Does.Contain("Failed to obtain publication time."));
		}

		[Test]
		public void TestDownLoadHtmlNodeIsNull()
		{
			var outputFileDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RefCusCodeList\TestFiles");
			var parser = new UNSPCDownloadAndConvertToXMLParserTest(outputFileDirectoryPath);

			var site = new HtmlDocument();
			var sitehtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.TestHtmlForUNSPC.html");
			site.LoadHtml(sitehtml);

			var dateTimeNode = site.DocumentNode.Descendants().FirstOrDefault(node => node.Name == Constants.HtmlNodeNames.SPAN && node.InnerText.ToUpper(CultureInfo.InvariantCulture).Contains("LAST MODIFIED:"));

			var clientMock = new Mock<IDownLoadService>();
			clientMock.Setup(x => x.FindNode(ApplicationConfig.Instance.UnitedNationsStandardProductAndServiceCodesURLForPDF, It.IsAny<Func<HtmlNode, bool>>()))
					.Returns((string x, Func<HtmlNode, bool> c) =>
					{
						if (c.Invoke(dateTimeNode))
						{
							return dateTimeNode;
						}
						else
						{
							return null;
						}
					});

			parser.ServiceClient = clientMock.Object;
			var logs = parser.ConvertCodeListToXML();
			Assert.That(logs, Does.Contain("The layout of the website has changed. Failed to get PDF download address."));
		}

		[Test]
		public void TestDownLoadPDFAndConvertToXML()
		{
			using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.AMSCATAIRGuidelinesMay2022.pdf"))
			using (var stream2 = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.CopyOfCEMSActiveUNSPSCCodesWithCreatedOn20150724.xlsx"))
			{
				var outputFileDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RefCusCodeList\TestFiles");
				var parser = new UNSPCDownloadAndConvertToXMLParserTest(outputFileDirectoryPath);
				var inputPDFFilePath = parser.InputPDFFilePath_Exposed;
				SaveToFileOfDownloadPath(stream, inputPDFFilePath);
				var inputExcelFilePath = parser.InputExcelFilePath_Exposed;
				SaveToFileOfDownloadPath(stream2, inputExcelFilePath);
				var site = new HtmlDocument();
				var sitehtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.TestHtmlForUNSPC.html");
				site.LoadHtml(sitehtml);

				var dateTimeNode = site.DocumentNode.Descendants().FirstOrDefault(node => node.Name == Constants.HtmlNodeNames.SPAN && node.InnerText.ToUpper(CultureInfo.InvariantCulture).Contains("LAST MODIFIED:"));
				var downloadNode = site.DocumentNode.Descendants().FirstOrDefault(node => node.Name == Constants.HtmlNodeNames.A && node.InnerText.Contains("AMS CATAIR Guidelines"));

				var clientMock = new Mock<IDownLoadService>();
				clientMock.Setup(x => x.FindNode(ApplicationConfig.Instance.UnitedNationsStandardProductAndServiceCodesURLForPDF, It.IsAny<Func<HtmlNode, bool>>()))
						.Returns((string x, Func<HtmlNode, bool> c) =>
						{
							if (c.Invoke(dateTimeNode))
							{
								return dateTimeNode;
							}
							else if (c.Invoke(downloadNode))
							{
								return downloadNode;
							}
							else
							{
								return null;
							}
						});
				clientMock.Setup(x => x.DownloadFile("<a href=\"https\" class=\"survey-processed\">AMS CATAIR Guidelines</a>", ApplicationConfig.Instance.CustomsBorderProtectionGoverment, inputPDFFilePath)).Returns(true);
				parser.ServiceClient = clientMock.Object;

				if (File.Exists(parser.OutputFilePath_Exposed))
				{
					File.Delete(parser.OutputFilePath_Exposed);
				}
				var logs = parser.ConvertCodeListToXML();

				var exceptXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.ExpectedUNSPCForPDF.xml");
				Assert.That(exceptXML, Is.EqualTo(File.ReadAllText(parser.OutputFilePath_Exposed)));
			}
		}

		[Test]
		public void TestThrowNewExceptionWhenExcelFileCanonotOpened()
		{
			using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.CopyOfCEMSActiveUNSPSCCodesCannotOpen.xlsx"))
			{
				var outputFileDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RefCusCodeList\TestFiles");
				var parser = new UNSPCParserForExcelFileCannotOpenTest(outputFileDirectoryPath);
				var inputExcelFilePath = parser.InputExcelFilePath_Exposed;
				SaveToFileOfDownloadPath(stream, inputExcelFilePath);
				if (File.Exists(parser.OutputFilePath_Exposed))
				{
					File.Delete(parser.OutputFilePath_Exposed);
				}

				var exception = Assert.Throws<InvalidOperationException>(() => parser.ConvertCodeListToXML());
				Assert.That(exception.Message, Does.Contain("Error while opening the Excel file."));
			}

		}

		[Test]
		public void TestThrowNewExceptionWhenDownloadIncorrectExcel()
		{
			using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.AMSCATAIRGuidelinesMay2022.pdf"))
			using (var stream2 = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.CopyOfCEMSActiveUNSPSCCodesWithCreatedOn20150724.xlsx"))
			{
				var outputFileDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"RefCusCodeList\TestFiles");
				var parser = new UnitedNationsStandardProductAndServiceCodesParser(outputFileDirectoryPath);
				var site = new HtmlDocument();
				var sitehtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.TestHtmlForUNSPC.html");
				site.LoadHtml(sitehtml);

				var dateTimeNode = site.DocumentNode.Descendants().FirstOrDefault(node => node.Name == Constants.HtmlNodeNames.SPAN && node.GetAttributeValue("class", string.Empty).Contains("field-content"));
				var downloadNode = site.DocumentNode.Descendants().FirstOrDefault(node => node.Name == Constants.HtmlNodeNames.A && node.GetAttributeValue("title", string.Empty).Contains("AMS CATAIR Guidelines"));

				var clientMock = new Mock<IDownLoadService>();
				clientMock.Setup(x => x.FindNode(ApplicationConfig.Instance.UnitedNationsStandardProductAndServiceCodesURLForPDF, It.IsAny<Func<HtmlNode, bool>>()))
						.Returns((string x, Func<HtmlNode, bool> c) =>
						{
							if (c.Invoke(dateTimeNode))
							{
								return dateTimeNode;
							}
							else if (c.Invoke(downloadNode))
							{
								return downloadNode;
							}
							else
							{
								return null;
							}
						});
				clientMock.Setup(x => x.DownloadFile(ApplicationConfig.Instance.UnitedNationsStandardProductAndServiceCodesURLForExcel, "")).Returns(false);
				parser.ServiceClient = clientMock.Object;

				var exception = Assert.Throws<InvalidOperationException>(() => parser.ConvertCodeListToXML());
				Assert.AreEqual("Failed to download correct Excel file.", exception.Message);
			}
		}

		public void SaveToFileOfDownloadPath(Stream stream, string path)
		{
			byte[] srcBuf = new byte[stream.Length];
			stream.Read(srcBuf, 0, srcBuf.Length);
			stream.Seek(0, SeekOrigin.Begin);
			using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				fs.Write(srcBuf, 0, srcBuf.Length);
				fs.Close();
			}
		}

		class UNSPCParserForExcelFileCannotOpenTest : UNSPCDownloadAndConvertToXMLParserTest
		{
			public UNSPCParserForExcelFileCannotOpenTest(string outputFileDirectoryPath) : base(outputFileDirectoryPath)
			{
			}

			protected override bool DownLoadFileAndGetPublicationDateTime()
			{
				return true;
			}

			protected override bool DownLoadExcelFile()
			{
				var assembly = Assembly.GetExecutingAssembly();
				using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.CopyOfCEMSActiveUNSPSCCodesCannotOpen.xlsx"))
				{
					new UnitedNationsStandardProductAndServiceCodesParserTest().SaveToFileOfDownloadPath(stream, InputExcelFilePath);
				}
				return true;
			}
		}

		class UNSPCParserTest : UNSPCDownloadAndConvertToXMLParserTest
		{
			public UNSPCParserTest(string outputFileDirectoryPath) : base(outputFileDirectoryPath)
			{
			}

			protected override DateTime PublicationDateTime => new DateTime(2022, 06, 02);

			protected override bool DownLoadFileAndGetPublicationDateTime()
			{
				return true;
			}

			protected override bool DownLoadExcelFile()
			{
				var assembly = Assembly.GetExecutingAssembly();
				using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.CopyOfCEMSActiveUNSPSCCodesWithCreatedOn20220224.xlsx"))
				{
					new UnitedNationsStandardProductAndServiceCodesParserTest().SaveToFileOfDownloadPath(stream, InputExcelFilePath);
				}
				return true;
			}
		}

		class UNSPCExcelParserTest : UNSPCDownloadAndConvertToXMLParserTest
		{
			public UNSPCExcelParserTest(string outputFileDirectoryPath) : base(outputFileDirectoryPath)
			{
			}

			protected override bool DownLoadFileAndGetPublicationDateTime()
			{
				return true;
			}

			protected override bool DownLoadExcelFile()
			{
				var assembly = Assembly.GetExecutingAssembly();
				using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.CopyOfCEMSActiveUNSPSCCodesWithCreatedOn20150724.xlsx"))
				{
					new UnitedNationsStandardProductAndServiceCodesParserTest().SaveToFileOfDownloadPath(stream, InputExcelFilePath);
				}
				return true;
			}
		}

		class UNSPCDownloadAndConvertToXMLParserTest : UnitedNationsStandardProductAndServiceCodesParser
		{
			public UNSPCDownloadAndConvertToXMLParserTest(string outputFileDirectoryPath) : base(outputFileDirectoryPath)
			{
			}

			protected override DateTime PublicationDateTime => new DateTime(2021, 04, 21);

			public string OutputFilePath_Exposed => base.OutputFilePath;

			public string InputPDFFilePath_Exposed => InputPDFFilePath;

			public string InputExcelFilePath_Exposed => InputExcelFilePath;

			protected override bool DownLoadExcelFile()
			{
				var assembly = Assembly.GetExecutingAssembly();
				using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.CopyOfCEMSActiveUNSPSCCodesWithCreatedOn20220224.xlsx"))
				{
					new UnitedNationsStandardProductAndServiceCodesParserTest().SaveToFileOfDownloadPath(stream, InputExcelFilePath);
				}
				return true;
			}
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		Assembly assembly;
	}
}
