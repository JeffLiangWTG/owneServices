using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.eHubMessageDownloader.Test
{
	class DownloaderIntegrationFixture
	{
		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void SaveSourceData()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connectionString = TestConnectionString.GetAdmin(dbName);
			using (var repo = new StagingRepository(connectionString))
			{
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
DTM+302:20150205:102'
~~THE REST~~~")));
				inboxMock.Setup(x => x.GetEnumerator()).Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());
				var downloader = new Downloader(repo, adapterMock.Object);
				downloader.Run();
			}
			using (var repo = new StagingRepository(connectionString))
			{
				Assert.That(repo.Get<SourceData>().ToArray().Length, Is.EqualTo(1));
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void TestNotPopulateSourceDataIfMessageCouldNotBeDecoded()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connectionString = TestConnectionString.GetAdmin(dbName);
			using (var repo = new StagingRepository(connectionString))
			{
				var adapterMock = new Mock<IeHubAdapter>();
				var inboxMock = new Mock<IMessageInbox>();
				inboxMock.Setup(x => x.Count).Returns(1);
				adapterMock.Setup(x => x.Inbox).Returns(inboxMock.Object);

				var messageMock = new Mock<IeHubMessage>();
				messageMock.Setup(x => x.SchemaName).Returns(Constants.SupportedSchemaName.GenericMessageDelivery);
				messageMock.Setup(x => x.MessageStream).Returns(new MemoryStream(Encoding.UTF8.GetBytes(TestMessage.BadStructuredMessage)));
				inboxMock.Setup(x => x.GetEnumerator()).Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());
				var downloader = new Downloader(repo, adapterMock.Object);
				downloader.Run();
			}
			using (var repo = new StagingRepository(connectionString))
			{
				var sourceDataArr = repo.Get<SourceData>().ToArray();
				Assert.That(sourceDataArr, Has.Length.EqualTo(0));
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void TestNotPopulateSourceDataIfMessageSchemaNameIsNotSupported()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var connectionString = TestConnectionString.GetAdmin(dbName);
			using (var repo = new StagingRepository(connectionString))
			{
				var adapterMock = new Mock<IeHubAdapter>();
				var inboxMock = new Mock<IMessageInbox>();
				inboxMock.Setup(x => x.Count).Returns(1);
				adapterMock.Setup(x => x.Inbox).Returns(inboxMock.Object);

				var messageMock = new Mock<IeHubMessage>();
				messageMock.Setup(x => x.SchemaName).Returns("Wrong Schema Name");
				messageMock.Setup(x => x.MessageStream).Returns(new MemoryStream(Encoding.UTF8.GetBytes(TestMessage.NormalStructuredMessage)));
				inboxMock.Setup(x => x.GetEnumerator()).Returns(new List<IeHubMessage>(new[] { messageMock.Object }).GetEnumerator());
				var downloader = new Downloader(repo, adapterMock.Object);
				downloader.Run();
			}
			using (var repo = new StagingRepository(connectionString))
			{
				var sourceDataArr = repo.Get<SourceData>().ToArray();
				Assert.That(sourceDataArr, Has.Length.EqualTo(0));
			}
		}
	}
}
