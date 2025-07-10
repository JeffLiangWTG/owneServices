using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class RegionDistrictPortCodeMessageProcessorTest
	{
		[Test]
		public void TestParseToXml()
		{
			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Input\RegionDistrictPortCodesResponseMessage.txt");
			var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Output\");
			var expectFile = Path.Combine(outputPath, @"USRegionDistrictPortCodeExample.xml");
			var outputFileName = @"_USRegionDistrictPortCode.xml";
			var actualFilePath = Path.Combine(outputPath, DateTime.UtcNow.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + outputFileName);

			var adapterMock = new Mock<IeHubAdapter>();
			var inboxMock = new Mock<IMessageInbox>();
			inboxMock.Setup(x => x.Count).Returns(1);
			adapterMock.Setup(x => x.Inbox).Returns(inboxMock.Object);
			var messageMock = new Mock<IeHubMessage>();
			messageMock.Setup(x => x.MessageStream).Returns(new MemoryStream(File.ReadAllBytes(inputPath)));
			inboxMock.Setup(x => x.GetEnumerator()).Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());

			var downloader = new USIncomingMessageDownloader(adapterMock.Object, outputPath, "");

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
				var publicationTime = Regex.Match(actualXML, @"\d{4}(\-|\/|\.)\d{1,2}\1\d{1,2}T00:00:00").Value;
				actualXML = actualXML.Replace(publicationTime, "2024-01-16T00:00:00");
			}

			using (FileStream fileStream = new FileStream(actualFilePath, FileMode.Truncate))
			using (StreamWriter writer = new StreamWriter(fileStream))
			{
				writer.Write(actualXML);
			}

			using (var actualXmlStream = new FileStream(actualFilePath, FileMode.Open))
			using (var expectedXmlStream = new FileStream(Path.Combine(outputPath, @"USRegionDistrictPortCodeExample.xml"), FileMode.Open))
			{
				var actualXml = new XmlDocument();
				actualXml.Load(actualXmlStream);

				var expectedXml = new XmlDocument();
				expectedXml.Load(expectedXmlStream);

				Assert.AreEqual(expectedXml.InnerXml, actualXml.InnerXml);
			}
		}

		[Test]
		public void TestParseToXml_RegionDistrictPortCodeNotMatch()
		{
			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Input\RegionDistrictPortCodesResponseMessage_RegionDistrictPortCodeNotMatchy.txt");
			var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Output\");
			var expectFile = Path.Combine(outputPath, @"USRegionDistrictPortCodeExample.xml");

			var adapterMock = new Mock<IeHubAdapter>();
			var inboxMock = new Mock<IMessageInbox>();
			inboxMock.Setup(x => x.Count).Returns(1);
			adapterMock.Setup(x => x.Inbox).Returns(inboxMock.Object);
			var messageMock = new Mock<IeHubMessage>();
			messageMock.Setup(x => x.MessageStream).Returns(new MemoryStream(File.ReadAllBytes(inputPath)));
			inboxMock.Setup(x => x.GetEnumerator()).Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());

			var downloader = new USIncomingMessageDownloader(adapterMock.Object, outputPath, "");

			var exception = Assert.Throws<InvalidOperationException>(() => downloader.Run());
			Assert.AreEqual("The Region/District/Port Code 10022 in F201 does not match the code 10021 in F101. The message structure may be changed.", exception.Message);
		}

		[Test]
		public void TestParseToXml_TransportModeNotMatch()
		{
			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Input\RegionDistrictPortCodesResponseMessage_TransportModeNotMatch.txt");
			var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Output\");
			var expectFile = Path.Combine(outputPath, @"USRegionDistrictPortCodeExample.xml");

			var adapterMock = new Mock<IeHubAdapter>();
			var inboxMock = new Mock<IMessageInbox>();
			inboxMock.Setup(x => x.Count).Returns(1);
			adapterMock.Setup(x => x.Inbox).Returns(inboxMock.Object);
			var messageMock = new Mock<IeHubMessage>();
			messageMock.Setup(x => x.MessageStream).Returns(new MemoryStream(File.ReadAllBytes(inputPath)));
			inboxMock.Setup(x => x.GetEnumerator()).Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());

			var downloader = new USIncomingMessageDownloader(adapterMock.Object, outputPath, "");

			var exception = Assert.Throws<InvalidOperationException>(() => downloader.Run());
			Assert.AreEqual("Transport mode X does not match. The new transport mode code is introduced.", exception.Message);
		}
	}
}
