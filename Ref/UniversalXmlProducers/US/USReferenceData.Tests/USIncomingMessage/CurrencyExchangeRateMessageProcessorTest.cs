using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor;
using CargoWise.RefDbRepo.USReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	internal class CurrencyExchangeRateMessageProcessorTest
	{
		[Test]
		public void TestParseToXml()
		{
			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Input\CurrencyExchangeRateMessage.txt");
			var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Output\");

			var adapterMock = new Mock<IeHubAdapter>();
			var inboxMock = new Mock<IMessageInbox>();
			inboxMock.Setup(x => x.Count).Returns(1);
			adapterMock.Setup(x => x.Inbox).Returns(inboxMock.Object);
			var messageMock = new Mock<IeHubMessage>();
			messageMock.Setup(x => x.MessageStream).Returns(new MemoryStream(File.ReadAllBytes(inputPath)));
			inboxMock.Setup(x => x.GetEnumerator()).Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());

			var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.Instance.ScheduleKCsvFileName);
			var downloader = new USIncomingMessageDownloader(adapterMock.Object, outputPath, filePath);

			var filePathPair1 = GetFilePathPairAndCleanFile("20230818", outputPath);
			var filePathPair2 = GetFilePathPairAndCleanFile("20230819", outputPath);
			var filePathPair3 = GetFilePathPairAndCleanFile("20230820", outputPath);

			Assert.DoesNotThrow(() => downloader.Run());

			AssertXMLIsSame(filePathPair1.expectFilePath, filePathPair1.actualFilePath);
			AssertXMLIsSame(filePathPair2.expectFilePath, filePathPair2.actualFilePath);
			AssertXMLIsSame(filePathPair3.expectFilePath, filePathPair3.actualFilePath);
		}

		void AssertXMLIsSame(string expectFilePath, string actualFilePath)
		{
			var expectXML = string.Empty;
			using (var stream = new MemoryStream(File.ReadAllBytes(expectFilePath)))
			using (var reader = new StreamReader(stream))
			{
				expectXML = reader.ReadToEnd();
			}

			var actualXML = string.Empty;
			using (var stream = new MemoryStream(File.ReadAllBytes(actualFilePath)))
			using (var reader = new StreamReader(stream))
			{
				actualXML = reader.ReadToEnd();
			}

			Assert.AreEqual(expectXML, actualXML);
		}

		(string expectFilePath, string actualFilePath) GetFilePathPairAndCleanFile(string date, string outputPath)
		{
			var result = (Path.Combine(outputPath, $"{date}_RefExchangeRateZZ_US_CBP_CUSExample.xml"), Path.Combine(outputPath, $"{date}_RefExchangeRateZZ_US_CBP_CUS_Message.xml"));
			if (File.Exists(result.Item2))
			{
				File.Delete(result.Item2);
			}
			return result;
		}
	}
}
