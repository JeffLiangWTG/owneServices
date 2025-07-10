using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class Test_RefCusCodeListAttribute : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\RefCusCodeListAttribute_WithoutStartEndDate.xml", "TestFiles\\RefCusCodeListAttribute_WithStartEndDateInsert.xml", "TestFiles\\RefCusCodeListAttribute_WithStartEndDateExpire.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2, AssertResult_AfterProcessingXml3];

		public string TestDescription => "Test RefCusCodeListAttribute without startdate or enddate specified";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var safeSql = $@"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (newid(), 10, 'RefCusCodeList', 'RefCusCodeList','ZZD',0);

insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'EUN', 'European Union', null);
insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'DE', 'Germany', 'EUN');

insert into [dbo].[RefLanguageType] (ZX6_PK,ZX6_Language,ZX6_Description)
values (newid(), 'DE', 'German');

insert into [dbo].[RefCusCodeType] (ZZK_PK,ZZK_CodeType,ZZK_Description,ZZK_ZZZ_NKDataGrouping)
values (newid(), 'EMCPK', 'Exise Movement Control System (EMCS) Pack Types', 'EUN');
insert into [dbo].[RefCusCodeType] (ZZK_PK,ZZK_CodeType,ZZK_Description,ZZK_ZZZ_NKDataGrouping)
values (newid(), 'EMCPC', 'Exise Movement Control With Date Range', 'EUN');

INSERT INTO [dbo].[RefCusCodeListAttributeName] (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping)
VALUES(newid(), 'Message Number', 'Message Number', 'EMCPK', 'EUN')
INSERT INTO [dbo].[RefCusCodeListAttributeName] (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsDateRangeUsed)
VALUES(newid(), 'Date Range', 'Date Range', 'EMCPC', 'EUN', 1)
INSERT INTO [dbo].[RefCusCodeListAttributeName] (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsDateRangeUsed)
VALUES(newid(), 'ExpireRecord', 'ExpireRecord', 'EMCPC', 'EUN', 1)

INSERT INTO [dbo].[RefCusCodeList] (ZZD_PK, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate, ZZD_ZZK_NKCodeType)
VALUES(newid(), 'AF', 'Afosol', 'EUN', '1900-01-01T00:00:00', '2079-06-06T23:59:00', 'EMCPC')

INSERT INTO [dbo].[RefCusCodeListAttribute] (ZZE_PK, ZZE_ZZD_CodeList, ZZE_ZXE_NKName, ZZE_Value, ZZE_StartDate, ZZE_EndDate)
VALUES(newid(), (SELECT ZZD_PK FROM dbo.RefCusCodeList WHERE ZZD_Code='AF' AND ZZD_ZZZ_NKDataGrouping='EUN'), 'ExpireRecord', '100010', '1900-01-01T00:00:00', '2079-06-06T23:59:00')

";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(stagingCommand);
				AssertResult_StagingDb_RefCusCodeListAttribute_AfterProcessingXml1(stagingCommand);
				AssertResult_SafeDb_RefCusCodeListAttribute_AfterProcessingXml1(safeCommand);
			});
		}

		void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_RefCusCodeListAttribute_AfterProcessingXml2(stagingCommand);
				AssertResult_SafeDb_RefCusCodeListAttribute_AfterProcessingXml2(safeCommand);
			});
		}

		void AssertResult_AfterProcessingXml3(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_SafeDb_RefCusCodeListAttribute_Expiration(safeCommand);
			});
		}

		private void AssertResult_SafeDb_RefCusCodeListAttribute_Expiration(IDbCommand safeCommand)
		{
			var codeListAttributes = new List<Safe.RefCusCodeListAttribute>();
			var sql = @"SELECT
ZZE_Value, ZZE_StartDate, ZZE_EndDate
FROM dbo.RefCusCodeListAttribute
JOIN dbo.RefCusCodeList ON ZZE_ZZD_CodeList=ZZD_PK
WHERE ZZD_Code='AF' AND ZZD_ZZZ_NKDataGrouping='EUN'
AND ZZE_ZXE_NKName = 'ExpireRecord'
ORDER BY ZZE_StartDate";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeListAttribute = new Safe.RefCusCodeListAttribute
					{
						ZZE_Value = reader.GetString(0),
						ZZE_StartDate = reader.GetDateTime(1),
						ZZE_EndDate = reader.GetDateTime(2)
					};
					codeListAttributes.Add(codeListAttribute);
				}
			}
			Assert.AreEqual(2, codeListAttributes.Count);
			Assert.That(codeListAttributes[0].ZZE_Value, Is.EqualTo("100010"));
			Assert.That(codeListAttributes[0].ZZE_StartDate, Is.EqualTo(new DateTimeOffset(new DateTime(1900, 1, 1))));
			Assert.That(codeListAttributes[0].ZZE_EndDate, Is.EqualTo(new DateTimeOffset(new DateTime(2019, 12, 31, 23, 59, 0))));

			Assert.That(codeListAttributes[1].ZZE_Value, Is.EqualTo("200010"));
			Assert.That(codeListAttributes[1].ZZE_StartDate, Is.EqualTo(new DateTimeOffset(new DateTime(2020, 1, 1))));
			Assert.That(codeListAttributes[1].ZZE_EndDate, Is.EqualTo(new DateTimeOffset(new DateTime(2079, 6, 6, 23, 59, 0))));
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var status = string.Empty;
			stagingCommand.CommandText = "SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='RefCusCodeListAttribute Test' ORDER BY SDA_CreatedTime DESC";
			using (var reader = stagingCommand.ExecuteReader())
			{
				if (reader.Read())
				{
					status = reader.GetString(0);
				}
			}
			Assert.That(status, Is.EqualTo(StatusProvider.GetMERStatus()));
		}

		void AssertResult_StagingDb_RefCusCodeListAttribute_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var codeListAttributes = new List<RefCusCodeListAttribute>();
			var sql = @"SELECT
ZZE_Value
FROM dbo.RefCusCodeListAttribute
JOIN dbo.RefCusCodeList ON ZZE_ZZD_CodeList=ZZD_PK
WHERE ZZD_Code='AE' AND ZZD_ZZZ_NKDataGrouping='EUN'";
			stagingCommand.CommandText = sql;
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeListAttribute = new RefCusCodeListAttribute
					{
						ZZE_Value = reader.GetString(0)
					};
					codeListAttributes.Add(codeListAttribute);
				}
			}
			Assert.AreEqual(1, codeListAttributes.Count);
			Assert.AreEqual("100010", codeListAttributes.First().ZZE_Value);
		}

		void AssertResult_SafeDb_RefCusCodeListAttribute_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var codeListAttributes = new List<Safe.RefCusCodeListAttribute>();
			var sql = @"SELECT
ZZE_Value
FROM dbo.RefCusCodeListAttribute
JOIN dbo.RefCusCodeList ON ZZE_ZZD_CodeList=ZZD_PK
WHERE ZZD_Code='AE' AND ZZD_ZZZ_NKDataGrouping='EUN'";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeListAttribute = new Safe.RefCusCodeListAttribute
					{
						ZZE_Value = reader.GetString(0)
					};
					codeListAttributes.Add(codeListAttribute);
				}
			}
			Assert.AreEqual(1, codeListAttributes.Count);
			Assert.AreEqual("100010", codeListAttributes.First().ZZE_Value);
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var status = string.Empty;
			stagingCommand.CommandText = "SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='RefCusCodeListAttribute Test' ORDER BY SDA_CreatedTime DESC";
			using (var reader = stagingCommand.ExecuteReader())
			{
				if (reader.Read())
				{
					status = reader.GetString(0);
				}
			}
			Assert.That(status, Is.EqualTo(StatusProvider.GetMERStatus()));
		}

		void AssertResult_StagingDb_RefCusCodeListAttribute_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var codeListAttributes = new List<RefCusCodeListAttribute>();
			var sql = @"SELECT
ZZE_StartDate,
ZZE_EndDate
FROM dbo.RefCusCodeListAttribute
JOIN dbo.RefCusCodeList ON ZZE_ZZD_CodeList=ZZD_PK
WHERE ZZD_Code='AE' AND ZZD_ZZZ_NKDataGrouping='EUN'
AND ZZE_ZXE_NKName = 'Date Range'";
			stagingCommand.CommandText = sql;
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeListAttribute = new RefCusCodeListAttribute
					{
						ZZE_StartDate = reader.GetDateTime(0),
						ZZE_EndDate = reader.GetDateTime(1)
					};
					codeListAttributes.Add(codeListAttribute);
				}
			}
			Assert.True(codeListAttributes.All(x => x.ZZE_StartDate == new DateTime(1900, 1, 1)));
			Assert.True(codeListAttributes.All(x => x.ZZE_EndDate == new DateTime(2079, 6, 6, 23, 59, 0)));
		}

		void AssertResult_SafeDb_RefCusCodeListAttribute_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var codeListAttributes = new List<Safe.RefCusCodeListAttribute>();
			var sql = @"SELECT
ZZE_Value
FROM dbo.RefCusCodeListAttribute
JOIN dbo.RefCusCodeList ON ZZE_ZZD_CodeList=ZZD_PK
WHERE ZZD_Code='AE' AND ZZD_ZZZ_NKDataGrouping='EUN'
AND ZZE_ZXE_NKName = 'Date Range'";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeListAttribute = new Safe.RefCusCodeListAttribute
					{
						ZZE_Value = reader.GetString(0)
					};
					codeListAttributes.Add(codeListAttribute);
				}
			}
			Assert.AreEqual(1, codeListAttributes.Count);
			Assert.AreEqual("100010", codeListAttributes.First().ZZE_Value);
		}
	}
}
