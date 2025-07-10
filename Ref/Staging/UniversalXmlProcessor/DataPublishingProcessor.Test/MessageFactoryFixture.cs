using System.Collections.Generic;
using System.Linq;
using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DataPublishingProcessor.Test
{
	[TestFixture]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public class MessageFactoryFixture
	{
		[Test]
		public void AddBatchToMessageOnlyIncludesIsPushDataSets()
		{
			var messageFactory = new MessageFactory(_mockAdapter.Object);
			using (var cmd = DbTestHelper.CreateCommand(_conn, _trans))
			{
				messageFactory.AddBatchToMessage(cmd, new List<short> { 1 });
				Assert.False(messageFactory.HasMessagesToSend());
			}

			using (var cmd = DbTestHelper.CreateCommand(_conn, _trans))
			{
				messageFactory.AddBatchToMessage(cmd, new List<short> { 2 });
				Assert.True(messageFactory.HasMessagesToSend());
			}
		}

		[Test]
		public void SendMessageToSubscribersOnly()
		{
			SubscribeTo("TST", 2);

			var messageFactory = new MessageFactory(_mockAdapter.Object);
			using (var cmd = DbTestHelper.CreateCommand(_conn, _trans))
			{
				messageFactory.AddBatchToMessage(cmd, new List<short>() { 2, 3 });
			}

			using (var cmd = DbTestHelper.CreateCommand(_conn, _trans))
			{
				var subscribers = messageFactory.GetSubscribers(cmd);
				CollectionAssert.IsNotEmpty(subscribers);
				Assert.That(subscribers.Count, Is.EqualTo(1));
				Assert.That(subscribers.First().Key, Is.EqualTo("TST"));
				Assert.That(subscribers.First().Value, Has.Count.EqualTo(1));
				Assert.That(subscribers.First().Value.First(), Is.EqualTo(2));
			}

			using (var cmd = DbTestHelper.CreateCommand(_conn, _trans))
			{
				messageFactory.SendMessage(cmd, 10);
			}

			_mockMessageOutbox.Verify(x => x.AddMessage(It.IsAny<IeHubMessage>()), Times.Once());
			_mockAdapter.Verify(x => x.SendMessages(), Times.Once);
		}

		[Test]
		public void BatchMessages()
		{
			SubscribeTo("TST", 2);
			SubscribeTo("ABC", 2);
			SubscribeTo("DEF", 2);

			var messageFactory = new MessageFactory(_mockAdapter.Object);
			using (var cmd = DbTestHelper.CreateCommand(_conn, _trans))
			{
				messageFactory.AddBatchToMessage(cmd, new List<short> { 2, 3 });
			}

			using (var cmd = DbTestHelper.CreateCommand(_conn, _trans))
			{
				var subscribers = messageFactory.GetSubscribers(cmd);
				CollectionAssert.IsNotEmpty(subscribers);
			}

			using (var cmd = DbTestHelper.CreateCommand(_conn, _trans))
			{
				messageFactory.SendMessage(cmd, 2);
			}

			_mockMessageOutbox.Verify(x => x.AddMessage(It.IsAny<IeHubMessage>()), Times.Exactly(3));
			_mockAdapter.Verify(x => x.SendMessages(), Times.Exactly(2));
		}

		SqlTransaction _trans;
		SqlConnection _conn;
		Mock<IeHubAdapter> _mockAdapter;
		Mock<IMessageOutbox> _mockMessageOutbox;

		[SetUp]
		public void SetUp()
		{
			_mockMessageOutbox = new Mock<IMessageOutbox>();
			_mockAdapter = new Mock<IeHubAdapter>();
			_mockAdapter.SetupGet(x => x.Outbox).Returns(_mockMessageOutbox.Object);

			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			var connString = TestConnectionString.GetAdmin(dbName);
			_conn = new SqlConnection(connString);
			_conn.Open();
			_trans = _conn.BeginTransaction();

			PrepareData();
		}

		[TearDown]
		public void TearDown()
		{
			DbTestHelper.ExecuteNonQuery(_conn, "DELETE FROM RefDataSetInformation", _trans);
			DbTestHelper.ExecuteNonQuery(_conn, "DELETE FROM DataPushSubscription", _trans);
			_trans.Commit();
			_trans.Dispose();

			_conn.Close();
			_conn.Dispose();
		}

		void SubscribeTo(string clientId, short dataSetId)
		{
			DbTestHelper.ExecuteNonQuery(_conn, $@"
INSERT INTO DataPushSubscription (DPS_PK, DPS_ClientRecipientId, DPS_DataSetId)
VALUES (newid(), '{clientId}', {dataSetId})", _trans);
		}

		void PrepareData()
		{
			DbTestHelper.ExecuteNonQuery(_conn, $@"
INSERT RefDataSetInformation(RDS_PK, RDS_DataSetId, RDS_TableName, RDS_DataSetName, RDS_DataSetTableCode, RDS_PriorityLevel, RDS_IsPush, RDS_LastUpdatedUTC)
	VALUES(newid(), 1, 'RefCusCodeList', 'RefCusCodeList', 'ZZD', 0, 0, '2020-07-07 06:44:54.3458577')
,(newid(), 2, 'RefCusCodeList', 'FRFallback', 'ZZD', 1, 1, '2020-07-07 06:44:54.3458577')
,(newid(), 3, 'RefCusTariff', 'RefCusTariffSpecial', 'ZZ1', 1, 0, '2020-07-07 06:44:54.3458577')


IF OBJECT_ID('tempdb..{ApplicationConfig.TempDataSetIdTableName}') IS NOT NULL
BEGIN
	DROP TABLE {ApplicationConfig.TempDataSetIdTableName}
END
CREATE TABLE {ApplicationConfig.TempDataSetIdTableName} (DataSetId tinyint)", _trans);
		}
	}
}
