using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor;
using CargoWise.RefDbRepo.USReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class ScheduleKPortCodeMessageProcessorTest
	{
		[Test]
		public void TestParseToXml()
		{
			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Input\ScheduleKPortCodesResponseMessage.txt");
			var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Output\");
			var expectFile = Path.Combine(outputPath, @"USForeignPortsExample.xml");
			var outputFileName = @"_USForeignPorts.xml";
			var actualFilePath = Path.Combine(outputPath, DateTime.Now.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + outputFileName);

			var adapterMock = new Mock<IeHubAdapter>();
			var inboxMock = new Mock<IMessageInbox>();
			inboxMock.Setup(x => x.Count).Returns(1);
			adapterMock.Setup(x => x.Inbox).Returns(inboxMock.Object);
			var messageMock = new Mock<IeHubMessage>();
			messageMock.Setup(x => x.MessageStream).Returns(new MemoryStream(File.ReadAllBytes(inputPath)));
			inboxMock.Setup(x => x.GetEnumerator()).Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());

			var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.Instance.ScheduleKCsvFileName);
			var downloader = new USIncomingMessageDownloader(adapterMock.Object, outputPath, filePath);

			if (File.Exists(actualFilePath))
			{
				File.Delete(actualFilePath);
			}

			Assert.DoesNotThrow(() => downloader.Run());

			var expectXML = string.Empty;

			using (var stream = new MemoryStream(File.ReadAllBytes(Path.Combine(outputPath, "USForeignPortsExample.xml"))))
			using (var reader = new StreamReader(stream))
			{
				expectXML = reader.ReadToEnd();
			}

			var actualXML = string.Empty;
			using (var stream = new MemoryStream(File.ReadAllBytes(actualFilePath)))
			using (var reader = new StreamReader(stream))
			{
				actualXML = reader.ReadToEnd();
				var publicationTime = Regex.Match(actualXML, @"\d{4}(\-|\/|\.)\d{1,2}\1\d{1,2}T00:00:00").Value;
				actualXML = actualXML.Replace(publicationTime, "2023-01-05T00:00:00");
			}

			Assert.AreEqual(expectXML, actualXML);
		}
	}
}
