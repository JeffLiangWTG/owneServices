using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor;
using CargoWise.RefDbRepo.USReferenceData.Services;
using Moq;
using NUnit.Framework;


namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	internal class FirmsCodeMessageProcessorTest
	{
		[Test]
		public void TestParseToXml()
		{
			AssertParseToXml("FIRMSCodeResponseMessage.txt", "USFIRMSCodeExample.xml");
		}

		[Test]
		public void TestParseToXmlForErrorFormat()
		{
			AssertParseToXml("FIRMSCodeResponseMessage error format.txt", "USFIRMSCodeExample2.xml");
		}

		void AssertParseToXml(string inputFile, string outputFile)
		{
			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Input\{inputFile}");
			var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Output\");
			var outputFileName = @"_USFirmsCodes.xml";
			var actualFilePath = Path.Combine(outputPath, DateTime.UtcNow.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + outputFileName);

			var adapterMock = new Mock<IeHubAdapter>();
			var inboxMock = new Mock<IMessageInbox>();
			inboxMock.Setup(x => x.Count).Returns(1);
			adapterMock.Setup(x => x.Inbox).Returns(inboxMock.Object);
			var messageMock = new Mock<IeHubMessage>();
			messageMock.Setup(x => x.MessageStream).Returns(new MemoryStream(File.ReadAllBytes(inputPath)));
			inboxMock.Setup(x => x.GetEnumerator()).Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());

			var downloader = new USIncomingMessageDownloader(adapterMock.Object, outputPath, string.Empty);

			if (File.Exists(actualFilePath))
			{
				File.Delete(actualFilePath);
			}

			Assert.DoesNotThrow(() => downloader.Run());

			var actualXML = string.Empty;
			using (FileStream fileStream = new FileStream(actualFilePath, FileMode.Open))
			using (StreamReader reader = new StreamReader(fileStream))
			{
				actualXML = reader.ReadToEnd();
				var pattern = @"20\d{2}(\-|\/|\.)\d{1,2}\1\d{1,2}T00:00:00";
				actualXML = Regex.Replace(actualXML, pattern, "2024-01-16T00:00:00");
			}

			using (FileStream fileStream = new FileStream(actualFilePath, FileMode.Truncate))
			using (StreamWriter writer = new StreamWriter(fileStream))
			{
				writer.Write(actualXML);
			}

			using (var actualXmlStream = new FileStream(actualFilePath, FileMode.Open))
			using (var expectedXmlStream = new FileStream(Path.Combine(outputPath, $@"{outputFile}"), FileMode.Open))
			{
				var actualXml = new XmlDocument();
				actualXml.Load(actualXmlStream);

				var expectedXml = new XmlDocument();
				expectedXml.Load(expectedXmlStream);

				Assert.AreEqual(expectedXml.InnerXml, actualXml.InnerXml);
			}
		}

		[SetUp]
		public void SetUp()
		{
			ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.USReferenceData.Tests.config.json");
		}
	}
}
