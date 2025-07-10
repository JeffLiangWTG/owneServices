using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Data.SqlClient;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DataPublishingProcessor.Test
{
	[TestFixture]
	public class DataPublisherFixture
	{
		Mock<IMessageFactory> _messageFactoryMock;

		[SetUp]
		public void SetUp()
		{
			_messageFactoryMock = new Mock<IMessageFactory>();
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void Run()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			var connString = TestConnectionString.GetAdmin(dbName);
			using var connection = new SqlConnection(connString);
			connection.Open();
			var publisher = new DataPublisher(connection);
			DbTestHelper.ExecuteNonQuery(connection, @"
INSERT RefDataSetInformation(RDS_PK, RDS_DataSetId, RDS_TableName, RDS_DataSetName, RDS_DataSetTableCode, RDS_PriorityLevel)
	VALUES(newid(), 1, 'X1Table', 'X1Table', 'X1', 0) , (newid(), 2, 'X2Table', 'X2Table', 'X2', 0)

INSERT INTO RefDbVersionControl (RVC_ParentPK, RVC_ParentCode)
VALUES (newid(), 'X1'),
	(newid(), 'X2'),
	(newid(), 'X1'),
	(newid(), 'X2'),
	(newid(), 'X1');"
			);

			publisher.PublishData(false, _messageFactoryMock.Object);
			Assert.AreEqual(5, DbTestHelper.ExecuteScalar(connection, "SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_IsPublished = 1"));

			DbTestHelper.ExecuteNonQuery(connection, @"UPDATE TOP (1) RefDbVersionControl SET RVC_IsPublished = 0 WHERE RVC_ParentCode = 'X2'");
			Thread.Sleep(10);
			publisher.PublishData(false, _messageFactoryMock.Object);

			Assert.AreEqual(1, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentCode = 'X1' AND RVC_IsPublished = 1"));
			Assert.AreEqual(2, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentCode = 'X2' AND RVC_IsPublished = 1"));
		}

		[Test]
		public void DoesNotRetryMoreThan3TimesUpdateDataSetInformation()
		{
			var connectionMock = new Mock<IDbConnection>();
			var transMock = new Mock<IDbTransaction>();
			var commandMock = new Mock<IDbCommand>();
			var readerMock = new Mock<IDataReader>();

			var errorMessage = "Execution Timeout Expired.";
			var ex = new SqlExceptionBuilder().WithErrorMessage(errorMessage).Build();
			var stackTrace = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
			stackTrace.SetValue(ex, "UpdateDataSetInformation");

			connectionMock.SetupSequence(x => x.CreateCommand()).Throws(ex).Throws(ex).Returns(commandMock.Object).Returns(commandMock.Object);
			connectionMock.Setup(x => x.BeginTransaction()).Returns(transMock.Object);
			transMock.Setup(x => x.Commit());
			commandMock.Setup(x => x.ExecuteNonQuery());
			commandMock.Setup(x => x.ExecuteScalar());
			commandMock.Setup(x => x.ExecuteReader()).Returns(readerMock.Object);
			readerMock.Setup(x => x.Read()).Returns(null);

			var originalOut = Console.Out;
			try
			{
				using var sw = new StringWriter();
				Console.SetOut(sw);
				var publisher = new DataPublisher(connectionMock.Object);
				publisher.PublishData();
				Assert.That(sw.ToString(), Does.Contain("1 trial(s) for UpdateDataSetInformation failed"));
				Assert.That(sw.ToString(), Does.Contain($"{ApplicationConfig.RetryCount - 1} trial(s) for UpdateDataSetInformation failed"));
				Assert.That(sw.ToString(), Does.Not.Contain($"{ApplicationConfig.RetryCount} trial(s) for UpdateDataSetInformation failed"));
			}
			finally
			{
				Console.SetOut(originalOut);
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void RunWithCustomisedDataSets()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			var connString = TestConnectionString.GetAdmin(dbName);
			using var connection = new SqlConnection(connString);
			connection.Open();
			var publisher = new DataPublisher(connection);

			DbTestHelper.ExecuteNonQuery(connection, @"
			INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
			VALUES(newid(), 'AU', 'Australia'), (newid(), 'FR', 'France')

			INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
			VALUES (newid(), 'ANY', 'Any Code Type AU', 0, 0, 'AU')
			INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
			VALUES (newid(), 'ANY', 'Any Code Type FR', 0, 0, 'FR')

			INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
			VALUES('C9924078-3C28-4DE3-BFE2-20AC73042BF3', 'ANY', 'ZZZXXX', 'ANY Code List', 'AU', '1900-01-01 00:00:00', '2079-06-06 00:00:00')

			INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
			VALUES (newid(), 'FBK', 'French Fallback', 0, 0, 'FR')

			INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
			VALUES('E9CBB0F3-060A-47C2-8946-EE24D36D8163', 'FBK', 'XXXZZZ', 'TESTING', 'FR', '1900-01-01 00:00:00', '2079-06-06 00:00:00')

			INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
			VALUES('A3BE36AA-E159-49CE-8400-0C4C4F88D32F', 'FBK', 'AAAZZZ', 'TESTING2', 'FR', '1900-01-01 00:00:00', '2079-06-06 00:00:00')

			INSERT RefDataSetInformation(RDS_PK, RDS_DataSetId, RDS_TableName, RDS_DataSetName, RDS_DataSetTableCode, RDS_PriorityLevel)
				VALUES(newid(), 1, 'RefCusCodeList', 'RefCusCodeList', 'ZZD', 0)
					,(newid(), 2, 'RefCusCodeList', 'FRFallback', 'ZZD', 1)
					,(newid(), 3, 'RefCusCodeList', 'FRDatasetFallbackSpecific', 'ZZD', 2)

			INSERT INTO RefDataSetInformationDefinition (RDD_PK, RDD_DataSetId, RDD_ColumnName, RDD_ColumnValue)
			VALUES (newid(), 2, 'ZZD_ZZZ_NKDataGrouping','FR'),
			(newid(), 2, 'ZZD_ZZK_NKCodeType', 'FBK'),
			(newid(), 3, 'ZZD_ZZZ_NKDataGrouping','FR'),
			(newid(), 3, 'ZZD_ZZK_NKCodeType', 'FBK'),
			(newid(), 3, 'ZZD_Code', 'AAAZZZ')
			");
			publisher.PublishData(false, _messageFactoryMock.Object);

			Assert.AreEqual(3, DbTestHelper.ExecuteScalar(connection, "SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_IsPublished = 1"));

			Assert.AreEqual(1, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = 'C9924078-3C28-4DE3-BFE2-20AC73042BF3'"));
			Assert.AreEqual(2, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = 'E9CBB0F3-060A-47C2-8946-EE24D36D8163'"));
			Assert.AreEqual(3, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = 'A3BE36AA-E159-49CE-8400-0C4C4F88D32F'"));

			PublishAfterUpdate(connection);
		}

		void PublishAfterUpdate(SqlConnection connection)
		{
			var publisher = new DataPublisher(connection);

			DbTestHelper.ExecuteNonQuery(connection, @"
			UPDATE RefCusCodeList SET ZZD_ZZK_NKCodeType = 'ANY', ZZD_Code = 'AAABBB', ZZD_ZZZ_NKDataGrouping = 'AU' WHERE ZZD_PK = 'A3BE36AA-E159-49CE-8400-0C4C4F88D32F';
			UPDATE RefCusCodeList SET ZZD_ZZK_NKCodeType = 'FBK', ZZD_Code = 'AAAZZZ', ZZD_ZZZ_NKDataGrouping = 'FR' WHERE ZZD_PK = 'C9924078-3C28-4DE3-BFE2-20AC73042BF3';
			");
			Assert.AreEqual(1, DbTestHelper.ExecuteScalar(connection, "SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_IsPublished = 1"));

			publisher.PublishData(false, _messageFactoryMock.Object);
			Assert.AreEqual(3, DbTestHelper.ExecuteScalar(connection, "SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_IsPublished = 1"));

			Assert.AreEqual(3, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = 'C9924078-3C28-4DE3-BFE2-20AC73042BF3'"));
			Assert.AreEqual(2, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = 'E9CBB0F3-060A-47C2-8946-EE24D36D8163'"));
			Assert.AreEqual(1, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = 'A3BE36AA-E159-49CE-8400-0C4C4F88D32F'"));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void TestPublishingPush()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			var connString = TestConnectionString.GetAdmin(dbName);
			using var connection = new SqlConnection(connString);
			connection.Open();
			var publisher = new DataPublisher(connection);

			DbTestHelper.ExecuteNonQuery(connection, @"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (newid(), 'AU', 'Australia'), (newid(), 'FR', 'France')

INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES (newid(), 'ANY', 'Any Code Type AU', 0, 0, 'AU')

INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
VALUES ('C9924078-3C28-4DE3-BFE2-20AC73042BF3', 'ANY', 'ZZZXXX', 'ANY Code List', 'AU', '1900-01-01 00:00:00', '2079-06-06 00:00:00')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
VALUES ('9467EE9E-8D69-43D7-8302-0FD34A68A54D', '2P1', 'Schedule 2 Part 1', 'AU')

INSERT INTO RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
VALUES ('2D8D2354-3A0D-4FB1-AC15-3DFBA7332F7E', 'NL', 'Dutch')

INSERT RefDataSetInformation (RDS_PK, RDS_DataSetId, RDS_TableName, RDS_DataSetName, RDS_DataSetTableCode, RDS_PriorityLevel, RDS_IsPush)
VALUES
(newid(), 200, 'RefCusCodeList', 'RefCusCodeList', 'ZZD', 0, 1),
(newid(), 9, 'RefCusTariffType', 'RefCusTariffType', 'ZZI', 0, 0),
(newid(), 2, 'RefLanguageType', 'RefLanguageType', 'ZX6', 0, 0),
(newid(), 201, 'RefCusTariff', 'RefCusTariff', 'ZZ1', 1, 0),
(newid(), 202, 'RefAirline', 'InactiveRefAirline', 'RM', 1, 0)

INSERT INTO RefDataSetInformationDefinition (RDD_PK, RDD_DataSetId, RDD_ColumnName, RDD_ColumnValue)
VALUES
(newid(), 200, 'ZZD_ZZK_NKCodeType', 'FBK'),
(newid(), 200, 'ZZD_ZZZ_NKDataGrouping','FR'),
(newid(), 201, 'ZZ1_ZZZ_NKDataGrouping','GB'),
(newid(), 202, 'RM_IsActive', '0')");

			publisher.PublishData(true, _messageFactoryMock.Object);

			_messageFactoryMock.Verify(x => x.AddBatchToMessage(It.IsAny<SqlCommand>(), It.IsAny<IEnumerable<short>>()), Times.AtLeastOnce);
			_messageFactoryMock.Verify(x => x.SendMessage(It.IsAny<SqlCommand>(), It.IsAny<int>()), Times.Once);

			Assert.AreEqual(200, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = 'C9924078-3C28-4DE3-BFE2-20AC73042BF3'"));
			Assert.AreEqual(true, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_IsPublished FROM RefDbVersionControl WHERE RVC_ParentPK = 'C9924078-3C28-4DE3-BFE2-20AC73042BF3'"));

			Assert.AreEqual(DBNull.Value, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = '9467EE9E-8D69-43D7-8302-0FD34A68A54D'"));
			Assert.AreEqual(false, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_IsPublished FROM RefDbVersionControl WHERE RVC_ParentPK = '9467EE9E-8D69-43D7-8302-0FD34A68A54D'"));

			Assert.AreEqual(DBNull.Value, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = '2D8D2354-3A0D-4FB1-AC15-3DFBA7332F7E'"));
			Assert.AreEqual(false, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_IsPublished FROM RefDbVersionControl WHERE RVC_ParentPK = '2D8D2354-3A0D-4FB1-AC15-3DFBA7332F7E'"));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void TestPublishingAuto()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			var connString = TestConnectionString.GetAdmin(dbName);
			using var connection = new SqlConnection(connString);
			connection.Open();
			var publisher = new DataPublisher(connection);
			DbTestHelper.ExecuteNonQuery(connection, @"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (newid(), 'AU', 'Australia'), (newid(), 'FR', 'France')

INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES (newid(), 'ANY', 'Any Code Type AU', 0, 0, 'AU')

INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
VALUES ('C9924078-3C28-4DE3-BFE2-20AC73042BF3', 'ANY', 'ZZZXXX', 'ANY Code List', 'AU', '1900-01-01 00:00:00', '2079-06-06 00:00:00')

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
VALUES ('9467EE9E-8D69-43D7-8302-0FD34A68A54D', '2P1', 'Schedule 2 Part 1', 'AU')

INSERT INTO RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
VALUES ('2D8D2354-3A0D-4FB1-AC15-3DFBA7332F7E', 'NL', 'Dutch')

INSERT RefDataSetInformation (RDS_PK, RDS_DataSetId, RDS_TableName, RDS_DataSetName, RDS_DataSetTableCode, RDS_PriorityLevel, RDS_IsPush)
VALUES
(newid(), 200, 'RefCusCodeList', 'RefCusCodeList', 'ZZD', 0, 1),
(newid(), 9, 'RefCusTariffType', 'RefCusTariffType', 'ZZI', 0, 0),
(newid(), 2, 'RefLanguageType', 'RefLanguageType', 'ZX6', 0, 0),
(newid(), 201, 'RefCusTariff', 'RefCusTariff', 'ZZ1', 1, 0),
(newid(), 202, 'RefAirline', 'InactiveRefAirline', 'RM', 1, 0)

INSERT INTO RefDataSetInformationDefinition (RDD_PK, RDD_DataSetId, RDD_ColumnName, RDD_ColumnValue)
VALUES
(newid(), 200, 'ZZD_ZZK_NKCodeType', 'FBK'),
(newid(), 200, 'ZZD_ZZZ_NKDataGrouping','FR'),
(newid(), 201, 'ZZ1_ZZZ_NKDataGrouping','GB'),
(newid(), 202, 'RM_IsActive', '0')");

			publisher.PublishData(false, _messageFactoryMock.Object);

			Assert.AreEqual(DBNull.Value, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = 'C9924078-3C28-4DE3-BFE2-20AC73042BF3'"));
			Assert.AreEqual(false, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_IsPublished FROM RefDbVersionControl WHERE RVC_ParentPK = 'C9924078-3C28-4DE3-BFE2-20AC73042BF3'"));

			Assert.AreEqual(9, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = '9467EE9E-8D69-43D7-8302-0FD34A68A54D'"));
			Assert.AreEqual(true, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_IsPublished FROM RefDbVersionControl WHERE RVC_ParentPK = '9467EE9E-8D69-43D7-8302-0FD34A68A54D'"));

			Assert.AreEqual(2, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = '2D8D2354-3A0D-4FB1-AC15-3DFBA7332F7E'"));
			Assert.AreEqual(true, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_IsPublished FROM RefDbVersionControl WHERE RVC_ParentPK = '2D8D2354-3A0D-4FB1-AC15-3DFBA7332F7E'"));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void PublishRefAirline()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			var connString = TestConnectionString.GetAdmin(dbName);
			using var connection = new SqlConnection(connString);
			connection.Open();
			var publisher = new DataPublisher(connection);
			DbTestHelper.ExecuteNonQuery(connection, @"
			INSERT INTO RefAirline  (RM_PK, RM_EagleAddedAirlinePrefixOrAccountingCode, RM_ThreeLetterCode, RM_AirlineName1, RM_IsActive, RM_AccountingCode, RM_AirlineName2, RM_TwoCharacterCode, RM_DuplicateFlagIndicator, RM_AddressLine1, RM_AddressLine2,
				RM_AirlineCity, RM_AirlineState, RM_AirlineCountry, RM_AirlinePostalCode, RM_ReservationsDeptTeletype, RM_ReservationsContactName, RM_ReservationsContactTitle, RM_ReservationsContactTeletype, RM_EmergencyTeletype, RM_EmergencyContactName,
				RM_EmergencyContactTitle, RM_MembershipFlagSITA, RM_MembershipFlagARINC, RM_MembershipFlagIATA, RM_MembershipFlagATA, RM_TypeOfOperationsCode, RM_AccountingSecondaryFlag, RM_AirlinePrefix, RM_AirlinePrefixSecondaryFlag)
			VALUES  ('6C22D427-C0F8-4200-99E9-ACD2F424D4AA', '123', 'ABC', 'AAL Inc.', 1, '123', '', 'AB', 0, '1 SkyviewDrive, MD 8D155', '', 'Dallas', 'Texas', 'USA', '75261-9616', 'HDQRZAA', 'L.L. Curtis', 'Vice President Res.', 'HDQRZAA', '', '', '', 1, 1, 1, 1, 'I', '', '391', ''),
					('01FEF126-6910-4525-B024-262D977214E9', '456', 'XYZ', 'MYD Inc.', 0, '456', '', 'XY', 0, 'Lusaka International Airport', '', 'Lusaka', '', 'Zambia', '', 'LUNRCQ3', 'M. Chali', 'Res. Control Officer', 'LUNRCQ3', '', '', '', 1, 0, 1, 0, 'I', '', '22', '');

			INSERT RefDataSetInformation(RDS_PK, RDS_DataSetId, RDS_TableName, RDS_DataSetName, RDS_DataSetTableCode, RDS_PriorityLevel)
				VALUES	(newid(), 43, 'RefAirline', 'RefAirline', 'RM', 0),
						(newid(), 202, 'RefAirline', 'InactiveRefAirline', 'RM', 1);

			INSERT INTO RefDataSetInformationDefinition (RDD_PK, RDD_DataSetId, RDD_ColumnName, RDD_ColumnValue)
				VALUES	(newid(), 202, 'RM_IsActive','0');");
			Assert.AreEqual(0, DbTestHelper.ExecuteScalar(connection, "SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_IsPublished = 1"));

			publisher.PublishData(false, _messageFactoryMock.Object);
			Assert.AreEqual(2, DbTestHelper.ExecuteScalar(connection, "SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_IsPublished = 1"));
			Assert.AreEqual(43, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));
			Assert.AreEqual(202, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = '01FEF126-6910-4525-B024-262D977214E9'"));

			DbTestHelper.ExecuteNonQuery(connection, @"
			UPDATE RefAirline SET RM_IsActive = 0 WHERE RM_PK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA';
			UPDATE RefAirline SET RM_IsActive = 1 WHERE RM_PK = '01FEF126-6910-4525-B024-262D977214E9';");
			Assert.AreEqual(0, DbTestHelper.ExecuteScalar(connection, "SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_IsPublished = 1"));

			publisher.PublishData(false, _messageFactoryMock.Object);
			Assert.AreEqual(2, DbTestHelper.ExecuteScalar(connection, "SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_IsPublished = 1"));
			Assert.AreEqual(202, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));
			Assert.AreEqual(43, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = '01FEF126-6910-4525-B024-262D977214E9'"));

			DbTestHelper.ExecuteNonQuery(connection, @"
			UPDATE RefAirline SET RM_IsActive = 1 WHERE RM_PK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA';
			UPDATE RefAirline SET RM_IsActive = 0 WHERE RM_PK = '01FEF126-6910-4525-B024-262D977214E9';");
			Assert.AreEqual(0, DbTestHelper.ExecuteScalar(connection, "SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_IsPublished = 1"));

			publisher.PublishData(false, _messageFactoryMock.Object);
			Assert.AreEqual(2, DbTestHelper.ExecuteScalar(connection, "SELECT COUNT(*) FROM RefDbVersionControl WHERE RVC_IsPublished = 1"));
			Assert.AreEqual(43, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = '6C22D427-C0F8-4200-99E9-ACD2F424D4AA'"));
			Assert.AreEqual(202, DbTestHelper.ExecuteScalar(connection, "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = '01FEF126-6910-4525-B024-262D977214E9'"));
		}
	}
}
