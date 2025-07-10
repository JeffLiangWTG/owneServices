using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.eHubMessageDownloader.Test
{
	[TestFixture]
	public class DownloaderFixture
	{
		[Test]
		public void SourceDataCreate()
		{
			var repoMock = new Mock<IStagingRepository>();
			var adapterMock = new Mock<IeHubAdapter>();
			var inboxMock = new Mock<IMessageInbox>();
			inboxMock.Setup(x => x.Count).Returns(1);
			adapterMock.Setup(x => x.Inbox).Returns(inboxMock.Object);

			var messageMock = new Mock<IeHubMessage>();
			messageMock.Setup(x => x.SchemaName).Returns(Constants.SupportedSchemaName.ZACustoms);
			messageMock.Setup(x => x.MessageStream).Returns(new MemoryStream(Encoding.UTF8.GetBytes(
@"UNB+UNOB:4+SARSINF+WISETECHGLOBAL::WTGWTGWTGWTGWTGW:WTGAS2+20160825:1502+43++PRODAT'
UNH+1+PRODAT:D:96B:UN:ZZZ01'
BGM+6+0+9'
DTM+302:20160825:102'
~~THE REST~~~")));
			inboxMock.Setup(x => x.GetEnumerator()).Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());
			var downloader = new Downloader(repoMock.Object, adapterMock.Object);

			downloader.Run();

			repoMock.Verify(x => x.Add(It.Is<SourceData>(s => s.SDA_Source == DataSourceConstants.Source.eHubZACustomsRepositoryQueue &&
				s.SDA_Filetype == DataSourceConstants.FileType.TXT.ToString() && s.SDA_ContentText ==
@"UNB+UNOB:4+SARSINF+WISETECHGLOBAL::WTGWTGWTGWTGWTGW:WTGAS2+20160825:1502+43++PRODAT'
UNH+1+PRODAT:D:96B:UN:ZZZ01'
BGM+6+0+9'
DTM+302:20160825:102'
~~THE REST~~~" && s.SDA_ContentType == DataSourceConstants.ContentType.ZA_ProDat)));
			inboxMock.Verify(x => x.MarkAsRead());
		}

		[Test]
		public void RetryOnServerTooBusyException()
		{
			var retryTimes = Convert.ToInt32(ApplicationConfig.RetryTimes, CultureInfo.InvariantCulture);

			var adapterMock = new Mock<IeHubAdapter>();
			adapterMock.Setup(x => x.RetrieveMessages()).Throws(new ServerTooBusyException(It.IsAny<string>()));
			var repoMock = new Mock<IStagingRepository>();

			var downloader = new Downloader(repoMock.Object, adapterMock.Object);
			var result = 0;
			Assert.DoesNotThrow(() => result = downloader.Run());
			adapterMock.Verify(x => x.RetrieveMessages(), Times.Exactly(retryTimes));
			Assert.That(result, Is.EqualTo(0));

			var adapterMockNormalException = new Mock<IeHubAdapter>();
			adapterMockNormalException.Setup(x => x.RetrieveMessages()).Throws(new Exception());
			downloader = new Downloader(repoMock.Object, adapterMockNormalException.Object);

			Assert.Throws<Exception>(() => downloader.Run());
		}

		[Test]
		public void RetryOnEndpointNotFoundException()
		{
			var retryTimes = Convert.ToInt32(ApplicationConfig.RetryTimes, CultureInfo.InvariantCulture);

			var adapterMock = new Mock<IeHubAdapter>();
			adapterMock.Setup(x => x.RetrieveMessages()).Throws(new EndpointNotFoundException(It.IsAny<string>()));
			var repoMock = new Mock<IStagingRepository>();

			var downloader = new Downloader(repoMock.Object, adapterMock.Object);
			var result = 0;
			Assert.DoesNotThrow(() => result = downloader.Run());
			adapterMock.Verify(x => x.RetrieveMessages(), Times.Exactly(retryTimes));
			Assert.That(result, Is.EqualTo(0));
		}

		[Test]
		public void TestLogUnknownMessageIfMessageCouldNotBeDecoded()
		{
			var defOut = Console.Out;
			try
			{
				using (var sw = new StringWriter())
				{
					Console.SetOut(sw);
					var repoMock = new Mock<IStagingRepository>();
					var adapterMock = new Mock<IeHubAdapter>();
					var inboxMock = new Mock<IMessageInbox>();
					inboxMock.Setup(x => x.Count).Returns(1);
					adapterMock.Setup(x => x.Inbox).Returns(inboxMock.Object);

					var messageMock = new Mock<IeHubMessage>();
					messageMock.Setup(x => x.SchemaName).Returns(Constants.SupportedSchemaName.GenericMessageDelivery);
					messageMock.Setup(x => x.MessageStream)
						.Returns(new MemoryStream(Encoding.UTF8.GetBytes(TestMessage.BadStructuredMessage)));
					inboxMock.Setup(x => x.GetEnumerator())
						.Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());
					var downloader = new Downloader(repoMock.Object, adapterMock.Object);
					downloader.Run();

					Assert.That(sw.ToString(),
						Does.Contain($"SchemaName: {messageMock.Object.SchemaName}".Replace("\r\n", "\t")));
					Assert.That(sw.ToString(), Does.Contain(TestMessage.BadStructuredMessage.Replace("\r\n", "\t")));
				}
			}
			finally
			{
				Console.SetOut(defOut);
			}
		}

		[Test]
		public void TestLogUnknownMessageIfMessageSchemaNameIsNotSupported()
		{
			var defOut = Console.Out;
			try
			{
				using (var sw = new StringWriter())
				{
					Console.SetOut(sw);
					var repoMock = new Mock<IStagingRepository>();
					var adapterMock = new Mock<IeHubAdapter>();
					var inboxMock = new Mock<IMessageInbox>();
					inboxMock.Setup(x => x.Count).Returns(1);
					adapterMock.Setup(x => x.Inbox).Returns(inboxMock.Object);

					var messageMock = new Mock<IeHubMessage>();
					messageMock.Setup(x => x.SchemaName).Returns("Wrong Schema Name");
					messageMock.Setup(x => x.MessageStream).Returns(new MemoryStream(Encoding.UTF8.GetBytes(TestMessage.NormalStructuredMessage)));
					inboxMock.Setup(x => x.GetEnumerator()).Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());
					var downloader = new Downloader(repoMock.Object, adapterMock.Object);
					downloader.Run();

					Assert.That(sw.ToString(), Does.Contain($"SchemaName: {messageMock.Object.SchemaName} from eHubMessage is not supported".Replace("\r\n", "\t")));
				}
			}
			finally
			{
				Console.SetOut(defOut);
			}
		}
	}
}
