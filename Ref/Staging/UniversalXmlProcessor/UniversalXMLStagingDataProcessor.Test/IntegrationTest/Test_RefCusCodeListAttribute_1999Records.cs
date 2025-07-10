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
	class Test_RefCusCodeListAttribute_1999Records : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\Test_RefCusCodeListAttribute_1999Records.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1];

		public string TestDescription => "Test RefCusCodeListAttribute without startdate or enddate specified, and the RefCusCodeListAttribute is included in the second batch of 1000 RefCusCodeLists.";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var safeSql = $@"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (newid(), 10, 'RefCusCodeList', 'RefCusCodeList','ZZD',0);

insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'EUN', 'European Union', null),
(newid(), 'ES', 'Spain', 'EUN');

insert into [dbo].[RefCusCodeType] (ZZK_PK,ZZK_CodeType,ZZK_Description,ZZK_ZZZ_NKDataGrouping)
values (newid(), 'DC44N', 'Document Type (EU Box 44 NCTS)', 'ES'),
(newid(), 'DC44I', 'Document Type (EU Box 44 Exports)', 'ES'),
(newid(), 'DC44E', 'Document Type (EU Box 44 Exports)', 'ES');

INSERT INTO [dbo].[RefCusCodeListAttributeName] (ZXE_PK, ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping)
VALUES(newid(), 'Level', 'Where the code can be used: Header, House or Item', 'DC44N', 'ES'),
(newid(), 'Complement', 'Complement', 'DC44N', 'ES'),
(newid(), 'Reference', 'Reference', 'DC44N', 'ES'),
(newid(), 'ItemNumber', 'ItemNumber', 'DC44N', 'ES');
";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(stagingCommand);
				AssertResult_StagingDb_RefCusCodeList_AfterProcessingXml1(stagingCommand);
				AssertResult_StagingDb_RefCusCodeListAttribute_AfterProcessingXml1(stagingCommand);
				AssertResult_SafeDb_RefCusCodeList_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusCodeListAttribute_AfterProcessingXml1(safeCommand);
			});
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

		void AssertResult_StagingDb_RefCusCodeList_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var codeLists = new List<RefCusCodeList>();
			var sql = @"SELECT ZZD_Code from dbo.RefCusCodeList;";
			stagingCommand.CommandText = sql;
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeList = new RefCusCodeList
					{
						ZZD_Code = reader.GetString(0)
					};
					codeLists.Add(codeList);
				}
			}
			Assert.AreEqual(1999, codeLists.Count);
		}

		void AssertResult_StagingDb_RefCusCodeListAttribute_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var codeListAttributes = new List<RefCusCodeListAttribute>();
			var sql = @"SELECT ZZE_Value FROM dbo.RefCusCodeListAttribute;";
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
			Assert.AreEqual(18, codeListAttributes.Count);
		}

		void AssertResult_SafeDb_RefCusCodeList_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var codeLists = new List<Safe.RefCusCodeList>();
			var sql = @"SELECT ZZD_Code from dbo.RefCusCodeList;";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeList = new Safe.RefCusCodeList
					{
						ZZD_Code = reader.GetString(0)
					};
					codeLists.Add(codeList);
				}
			}
			Assert.AreEqual(1999, codeLists.Count);
		}

		void AssertResult_SafeDb_RefCusCodeListAttribute_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var codeListAttributes = new List<Safe.RefCusCodeListAttribute>();
			var sql = @"SELECT ZZE_Value FROM dbo.RefCusCodeListAttribute;";
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
			Assert.AreEqual(18, codeListAttributes.Count);
		}
	}
}
