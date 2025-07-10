using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor;
using CargoWise.RefDbRepo.USReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class USIncomingMessageDownloaderTest
	{
		[Test]
		public void TestDownloaderMessagesWithEndpointNotFoundException()
		{
			var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Output\");
			var adapterMock = new Mock<IeHubAdapter>();
			var inboxMock = new Mock<IMessageInbox>();
			adapterMock.Setup(x => x.Inbox).Throws(new EndpointNotFoundException("EndpointNotFoundException"));
			var adapterMockOB = adapterMock.Object;

			Assert.DoesNotThrow(() => new USIncomingMessageDownloader(adapterMockOB, outputPath, "").Run());
			Assert.That(consoleOutput.ToString(), Does.Contain("EndpointNotFoundException"));
		}

		[Test]
		public void RetryOnException()
		{
			var retryTimes = ApplicationConfig.Instance.USIncomingMessageRetryTimes;
			if (retryTimes == 0)
			{
				retryTimes = 1;
			}

			var adapterMock = new Mock<IeHubAdapter>();
			adapterMock.Setup(x => x.RetrieveMessages()).Throws(new Exception());

			var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Output\");
			var downloader = new USIncomingMessageDownloader(adapterMock.Object, outputPath, "");
			Assert.DoesNotThrow(() => downloader.Run());
			adapterMock.Verify(x => x.RetrieveMessages(), Times.Exactly(retryTimes));
		}

		[Test]
		public void TestMethod()
		{
			var originalConsoleError = Console.Error;
			try
			{
				using (var consoleError = new StringWriter())
				{
					Console.SetError(consoleError);

					var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"USIncomingMessage\TestFiles\Output\");
					var adapterMock = new Mock<IeHubAdapter>();
					var inboxMock = new Mock<IMessageInbox>();
					inboxMock.Setup(x => x.Count).Returns(1);
					adapterMock.Setup(x => x.Inbox).Returns(inboxMock.Object);
					var messageMock = new Mock<IeHubMessage>();
					messageMock.Setup(x => x.MessageStream).Returns(new MemoryStream(Encoding.UTF8.GetBytes("Test String")));
					inboxMock.Setup(x => x.GetEnumerator()).Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());
					new USIncomingMessageDownloader(adapterMock.Object, outputPath, "").Run();
					Assert.True(string.IsNullOrEmpty(consoleError.ToString()));
				}
			}
			finally
			{
				Console.SetOut(originalConsoleError);
			}
		}

		[TearDown]
		public void Cleanup()
		{
			Console.SetError(originalConsoleOutput);
			consoleOutput.Dispose();
		}

		[SetUp]
		public virtual void SetUp()
		{
			originalConsoleOutput = Console.Error;
			consoleOutput = new StringWriter();
			Console.SetError(consoleOutput);
		}
		StringWriter consoleOutput;
		TextWriter originalConsoleOutput;
	}
}
