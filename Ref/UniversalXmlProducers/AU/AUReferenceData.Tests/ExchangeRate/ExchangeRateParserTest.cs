using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AUReferenceData.Business.ExchangeRate;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate
{
	public abstract class ExchangeRateParserTest<TEntity>
		where TEntity : RefExchangeRateZZ
	{
		#region GetSourceLocation

		[Test]
		public void TestGetSourceLocation()
		{
			var webPageUrl = "https://www.ccf.customs.gov.au/reference/production/main/_reference_production_main_.html";

			using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate.TestFiles._reference_production_main_.html"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
					var parserForTesting = new MockXCHAGRATEParser();
					parserForTesting.MockHttpClientHelper.Setup(x => x.GetWebPageAsync(webPageUrl)).Returns(Task.FromResult(reader.ReadToEnd()));
					var remoteFileUrl = parserForTesting.GetSourceLocation(webPageUrl);

					Assert.False(parserForTesting.HasErrorNotification);
					Assert.AreEqual(MockTextFileName, Path.GetFileName(remoteFileUrl));
			}
		}

		[Test]
		public void TestGetSourceLocationDownloadFailure()
		{
			var webPageUrl = "https://www.ccf.customs.gov.au/reference/production/main/_reference_production_main_.html";
			var parserForTesting = new MockXCHAGRATEParser();
			parserForTesting.MockHttpClientHelper.Setup(x => x.GetWebPageAsync(webPageUrl)).Returns(Task.FromResult("Dummytext"));

			var badwebPageUrl = "https://www.ccf.customs.gov.au/reference/production/main/" + "XXX" + "_reference_production_main_.html";
			var remoteFileUrl = parserForTesting.GetSourceLocation(badwebPageUrl);

			Assert.AreEqual(string.Empty, Path.GetFileName(remoteFileUrl));
			Assert.True(parserForTesting.HasErrorNotification);
			Assert.That(parserForTesting.GetErrorNotification(), Does.Contain($"Error Retrieving Source from {badwebPageUrl}: Could not retrieve any content."));		
		}

		[Test]
		public void TestGetSourceLocationInvalidOperation()
		{
			string invalidHTML = @"
<HTML>
<HEAD>
	<TITLE>Index of /reference/production/main/</TITLE>
</HEAD>
<BODY>
	<H2>Index of directory /reference/production/main/</H2>
	<TABLE CELLPADDING=5>
		<TR>    <TD COLSPAN=2><B>File</B></TD>    <TD><B>Size</B></TD>    <TD><B>Type</TD></B></TR>
		<TR><TD><A HREF=""..""><IMG BORDER=0 SRC=""/icons/back.gif""></A></TD><TD><A HREF="".."">Up one level</A></TD><TD>-</TD><TD>Directory</TD></TR>
		<TR><TD><A HREF=""AHECCSS-P1-EEMAIN-1907130015.txt""><IMG BORDER=0 SRC=""/icons/text.gif""></A></TD><TD><A HREF=""AHECCSS-P1-EEMAIN-1907130015.txt"">AHECCSS-P1-EEMAIN-1907130015.txt</A></TD><TD>1239KB</TD><TD>text/plain</TD></TR>
		<TR><TD><A HREF=""AQSCMDTY-P1-EDMAIN-2002130203.txt""><IMG BORDER=0 SRC=""/icons/text.gif""></A></TD><TD><A HREF=""AQSCMDTY-P1-EDMAIN-2002130203.txt"">AQSCMDTY-P1-EDMAIN-2002130203.txt</A></TD><TD>404</TD><TD>text/plain</TD></TR>
	</TABLE>
</BODY>
</HTML>
";

			var webPageUrl = "https://www.ccf.customs.gov.au/reference/production/main/_reference_production_main_.html";
			var parserForTesting = new MockXCHAGRATEParser();
			parserForTesting.MockHttpClientHelper.Setup(x => x.GetWebPageAsync(webPageUrl)).Returns(Task.FromResult(invalidHTML));

			var remoteFileUrl = parserForTesting.GetSourceLocation(webPageUrl);

			Assert.AreEqual(string.Empty, Path.GetFileName(remoteFileUrl));
			Assert.True(parserForTesting.HasErrorNotification);
			Assert.That(parserForTesting.GetErrorNotification(), Does.Contain($"Error Retrieving Source from {webPageUrl}: No link to {parserForTesting.FileNamePrefix} defined on node"));
		}

		[Test]
		public void TestGetSourceLocationGeneralException()
		{
			var webPageUrl = "https://www.ccf.customs.gov.au/reference/production/main/_reference_production_main_.html";
			var parserForTesting = new MockXCHAGRATEParser();
			parserForTesting.MockHttpClientHelper.Setup(x => x.GetWebPageAsync(webPageUrl)).Throws(new System.Net.Sockets.SocketException(10060));

			var exception = Assert.Catch<InvalidOperationException>(() => parserForTesting.GetSourceLocation(webPageUrl));
			Assert.That(exception.Message, Does.Contain($"Error Retrieving Source from {webPageUrl}: A connection attempt failed because the connected party did not properly respond after a period of time"));
		}

		#endregion

		protected abstract string MockTextFileName { get; }
		protected abstract string MockWebPageFileName { get; }

		#region GetPublicationDate

		[Test]
		public void TestGetPublicationDate()
		{
			var auCulture = System.Globalization.CultureInfo.GetCultureInfo("en-AU");
			var today = DateTime.Today;
			var lastNight = today.AddHours(-6);
			var thisMorning = today.AddHours(6);

			var publicationDate = new MockXCHAGRATEParser().GetPublicationDate($"SOME FILE{lastNight.ToString("yyMMddHHmm", auCulture)}.txt");
			Assert.AreEqual(today, publicationDate);

			publicationDate = new MockXCHAGRATEParser().GetPublicationDate($"SOME FILE{today.ToString("yyMMddHHmm", auCulture)}.txt");
			Assert.AreEqual(today, publicationDate);

			publicationDate = new MockXCHAGRATEParser().GetPublicationDate($"SOME FILE{thisMorning.ToString("yyMMddHHmm", auCulture)}.txt");
			Assert.AreEqual(thisMorning, publicationDate);

			publicationDate = new MockXCHAGRATEParser().GetPublicationDate($@"1234567890\SOME FILE{thisMorning.ToString("yyMMddHHmm", auCulture)}.txt");
			Assert.AreEqual(thisMorning, publicationDate);

			var parser = new MockXCHAGRATEParser();

			var ex = Assert.Throws<UnhandledApplicationException>(() => parser.GetPublicationDate("SOME FILE1234567890.txt"));
			Assert.IsTrue(ex.Message.Equals("Invalid format of publication time string: 1234567890, remote file url: SOME FILE1234567890.txt", StringComparison.Ordinal), "Message in exception should contains publication time string and remote file url");
		}

		#endregion

		#region ReadFromResource

		[Test]
		public void TestReadFromResource()
		{
			var remoteFileUrl = "https://www.ccf.customs.gov.au/reference/production/main/" + MockTextFileName;
			var parserForTesting = new MockXCHAGRATEParser();
			using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate.TestFiles.XCHGRATE-P1-EDMAIN-2002220144.txt"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				parserForTesting.MockHttpClientHelper.Setup(x => x.GetWebPageAsync(remoteFileUrl)).Returns(Task.FromResult(reader.ReadToEnd()));

				var filePath = Path.Combine(ExchangeRateTestHelper.TestFilesPath, MockTextFileName);
				var localFilePath = Path.GetTempFileName();
				var result = parserForTesting.ReadFromResource(remoteFileUrl, localFilePath);

				Assert.True(result);
				Assert.False(parserForTesting.HasErrorNotification);
				Assert.AreEqual(File.ReadAllText(filePath), File.ReadAllText(localFilePath));
			}
		}

		[Test]
		public void TestReadFromResourceDownloadFailure()
		{
			var remoteFileUrl = "https://www.ccf.customs.gov.au/reference/production/main/" + MockTextFileName;
			var parserForTesting = new MockXCHAGRATEParser();
			parserForTesting.MockHttpClientHelper.Setup(x => x.GetWebPageAsync(remoteFileUrl)).Returns(Task.FromResult("DummyText"));

			var localFilePath = Path.GetTempFileName();
			var badRemoteFileUrl = "https://www.ccf.customs.gov.au/reference/production/main/" + "XXX" + MockTextFileName;
			var result = parserForTesting.ReadFromResource(badRemoteFileUrl, localFilePath);

			Assert.False(result);
			Assert.True(parserForTesting.HasErrorNotification);
			Assert.That(parserForTesting.GetErrorNotification(), Does.Contain($"Error Downloading from {badRemoteFileUrl}: Could not retrieve any content."));
		}

		[Test]
		public void TestParseExceptionAtReadFromResource()
		{
			AssertTempFileIsDeleted(parser => parser.ReadFromResourceAction = () => throw new UnhandledApplicationException());
		}

		#endregion

		#region ConvertData

		[Test]
		public void TestConvertData()
		{
			var textFilePath = Path.Combine(ExchangeRateTestHelper.TestFilesPath, MockTextFileName);
			var entities = new MockXCHAGRATEParser().ConvertData(textFilePath).ToList();

			AssertSameEntities(ExpectedEntities, (IEnumerable<TEntity>)entities);
		}

		void AssertTempFileIsDeleted(Action<MockXCHAGRATEParser> setupParser)
		{
			var tempFileName = Path.GetTempFileName();
			var parserForTesting = new MockXCHAGRATEParser
			{
				GetTempFileNameAction = () => tempFileName,
				ConvertDataAction = () => throw new UnhandledApplicationException(),
				WebPageUrlForTest = "https://www.ccf.customs.gov.au/reference/production/main/"
			};
			setupParser.Invoke(parserForTesting);

			using (var streamHtml = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate.TestFiles._reference_production_main_.html"))
			using (var streamTxt = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate.TestFiles.XCHGRATE-P1-EDMAIN-2002220144.txt"))
			using (var readerHtml = new StreamReader(streamHtml, Encoding.UTF8))
			using (var readerTxt = new StreamReader(streamTxt, Encoding.UTF8))
			{
				parserForTesting.MockHttpClientHelper
					.Setup(x => x.GetWebPageAsync("https://www.ccf.customs.gov.au/reference/production/main/"))
					.Returns(Task.FromResult(readerHtml.ReadToEnd()));
				parserForTesting.MockHttpClientHelper
					.Setup(x => x.GetWebPageAsync("https://www.ccf.customs.gov.au/reference/production/main/XCHGRATE-P1-EDMAIN-2002220144.txt"))
					.Returns(Task.FromResult(readerTxt.ReadToEnd()));

				Assert.Throws<UnhandledApplicationException>(() => parserForTesting.Parse(ExchangeRateTestHelper.TestFilesPath));
				Assert.False(File.Exists(tempFileName), "Temp file {0} is deleted", tempFileName);
			}
		}

		[Test]
		public void TestParseExceptionAtConvertData()
		{
			AssertTempFileIsDeleted(parser => parser.ConvertDataAction = () => throw new UnhandledApplicationException());
		}

		protected abstract IEnumerable<TEntity> ExpectedEntities { get; }

		protected void AssertSameEntities(IEnumerable<TEntity> expected, IEnumerable<TEntity> actual)
		{
			var expectedList = expected.ToList();
			var actualList = actual.ToList();
			Assert.AreEqual(expectedList.Count, actualList.Count);
			for (var i = 0; i < expectedList.Count; i++)
			{
				CompareEntity(expectedList[i], actualList[i]);
			}
		}

		protected abstract void CompareEntity(TEntity expected, TEntity actual);

		#endregion

		#region WriteToDestination

		[Test]
		public void TestWriteToDestination()
		{
			var auCulture = System.Globalization.CultureInfo.GetCultureInfo("en-AU");
			var expectedXmlFilePath = Path.Combine(ExchangeRateTestHelper.TestFilesPath, MockXmlFileName);
			var expectedXml = File.ReadAllText(expectedXmlFilePath);
			expectedXml = expectedXml.Replace("$PUBLICATIONTIME", ExpectedPublicationDateTime.ToString("s", auCulture)).Replace("$DEFAULTENDDATE", DateTime.Today.ToString("s", auCulture));

			string outputFilePath = null;
			try
			{
				var entities = ExpectedEntities;
				var parser = new MockXCHAGRATEParser();
				parser.WriteToDestination(entities, ExchangeRateTestHelper.TestFilesPath, ExpectedPublicationDateTime);

				outputFilePath = Path.Combine(ExchangeRateTestHelper.TestFilesPath, parser.OutputFileName);
				var actualXml = File.ReadAllText(outputFilePath);

				Assert.AreEqual(expectedXml, actualXml);
			}
			finally
			{
				if (!string.IsNullOrEmpty(outputFilePath))
				{
					File.Delete(outputFilePath);
				}
			}
		}

		[Test]
		public void TestParseExceptionAtWriteToDestination()
		{
			AssertTempFileIsDeleted(parser => parser.WriteToDestinationAction = () => throw new UnhandledApplicationException());
		}

		protected abstract string MockXmlFileName { get; }
		protected abstract DateTime ExpectedPublicationDateTime { get; }

		#endregion

		#region EndToEnd

		[Test]
		public void TestParse()
		{
			var auCulture = System.Globalization.CultureInfo.GetCultureInfo("en-AU");
			var expectedXmlFilePath = Path.Combine(ExchangeRateTestHelper.TestFilesPath, MockXmlFileName);
			var expectedXml = File.ReadAllText(expectedXmlFilePath);
			expectedXml = expectedXml.Replace("$PUBLICATIONTIME", DateTime.Today.ToString("s", auCulture)).Replace("$DEFAULTENDDATE", DateTime.Today.ToString("s", auCulture));
			string outputFilePath = null;
			try
			{
				var tempFileName = Path.GetTempFileName();
				var parserForTesting = new MockXCHAGRATEParser { GetTempFileNameAction = () => tempFileName };
				parserForTesting.WebPageUrlForTest = "https://www.ccf.customs.gov.au/reference/production/main/";
				using (var streamHtml = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate.TestFiles._reference_production_main_.html"))
				using (var streamTxt = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.AUReferenceData.Tests.ExchangeRate.TestFiles.XCHGRATE-P1-EDMAIN-2002220144.txt"))
				using (var readerHtml = new StreamReader(streamHtml, Encoding.UTF8))
				using (var readerTxt = new StreamReader(streamTxt, Encoding.UTF8))
				{
					parserForTesting.MockHttpClientHelper.Setup(x => x.GetWebPageAsync("https://www.ccf.customs.gov.au/reference/production/main/")).Returns(Task.FromResult(readerHtml.ReadToEnd()));
					parserForTesting.MockHttpClientHelper.Setup(x => x.GetWebPageAsync("https://www.ccf.customs.gov.au/reference/production/main/XCHGRATE-P1-EDMAIN-2002220144.txt")).Returns(Task.FromResult(readerTxt.ReadToEnd()));

					parserForTesting.Parse(ExchangeRateTestHelper.TestFilesPath);
					outputFilePath = Path.Combine(ExchangeRateTestHelper.TestFilesPath, parserForTesting.OutputFileName);
					var actualXml = File.ReadAllText(outputFilePath);

					Assert.AreEqual(expectedXml, actualXml);
					Assert.False(File.Exists(tempFileName), "Temp file {0} is deleted", tempFileName);
				}
			}
			finally
			{
				if (!string.IsNullOrEmpty(outputFilePath))
				{
					File.Delete(outputFilePath);
				}
			}
		}

		#endregion

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		Assembly assembly;
	}

	public class MockXCHAGRATEParser : XCHAGRATEParser
	{
		public MockXCHAGRATEParser()
		{
			mockHttpMessageHandler = new Mock<HttpMessageHandler>();
			mockHttpClientHelper = new Mock<IHttpClientHelper>();
		}

		public string WebPageUrlForTest { get; set; }

		public bool IgnoreFilter { get; set; } = true;

		protected override string WebPageUrl => WebPageUrlForTest ?? Path.Combine(ExchangeRateTestHelper.TestFilesPath, LocationWebPageFileName);

		protected override Func<RefExchangeRateZZ, bool> Filter => IgnoreFilter ? x => true : base.Filter;

		public Action ReadFromResourceAction { get; set; }

		public Action ConvertDataAction { get; set; }

		public Action WriteToDestinationAction { get; set; }

		public Func<string> GetTempFileNameAction { get; set; }

		public override bool ReadFromResource(string remoteFileUrl, string localFilePath)
		{
			ReadFromResourceAction?.Invoke();
			return base.ReadFromResource(remoteFileUrl, localFilePath);
		}

		public override IEnumerable<RefExchangeRateZZ> ConvertData(string localFilePath)
		{
			ConvertDataAction?.Invoke();
			return base.ConvertData(localFilePath);
		}

		public override void WriteToDestination(IEnumerable<RefExchangeRateZZ> entities, string outputFilePath, DateTime publicationDate)
		{
			WriteToDestinationAction?.Invoke();
			base.WriteToDestination(entities, outputFilePath, publicationDate);
		}

		protected override string GetTempFileName()
		{
			return GetTempFileNameAction?.Invoke() ?? base.GetTempFileName();
		}

		public override string GetSourceLocation(string webPageUrl)
		{
			var location = base.GetSourceLocation(webPageUrl);
			return location.Replace(LocationWebPageFileName + @"\", "");
		}

		public new DateTime GetLastModifiedDate(string remoteFileUrl)
		{
			return base.GetLastModifiedDate(remoteFileUrl);
		}

		public string WebPageUrlForExplictTest => base.WebPageUrl;

		public new string FileNamePrefix => base.FileNamePrefix;

		public const string LocationWebPageFileName = "_reference_production_main_.html";

		public Mock<HttpMessageHandler> MockHttpMessageHandler => mockHttpMessageHandler ?? new Mock<HttpMessageHandler>();
		Mock<HttpMessageHandler> mockHttpMessageHandler;

		protected override HttpClient GetHttpClient() => new HttpClient(MockHttpMessageHandler.Object);

		public Mock<IHttpClientHelper> MockHttpClientHelper => mockHttpClientHelper ?? new Mock<IHttpClientHelper>();
		Mock<IHttpClientHelper> mockHttpClientHelper;

		protected override IHttpClientHelper GetHttpClientHelper() => MockHttpClientHelper?.Object;
	}
}
