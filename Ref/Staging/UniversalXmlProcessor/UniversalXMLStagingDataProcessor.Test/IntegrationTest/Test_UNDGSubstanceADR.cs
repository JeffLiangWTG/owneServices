using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class Test_UNDGSubstanceADR : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\UNDG_ADR_List.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1];

		public string TestDescription => "Test merging UNDGSubstanceADR";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var safeSql = $@"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (newid(), 19, 'UNDGSubstanceADR', 'UNDGSubstanceADR','ADR',0);
";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(stagingCommand);
				AssertResult_StagingDb_UNDGAttributeZZ_AfterProcessingXml1(stagingCommand);
				AssertResult_SafeDb_UNDGAttributeZZ_AfterProcessingXml1(safeCommand);
			});
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var status = string.Empty;
			stagingCommand.CommandText = "SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='UNDG ADR List' ORDER BY SDA_CreatedTime DESC";
			using (var reader = stagingCommand.ExecuteReader())
			{
				if (reader.Read())
				{
					status = reader.GetString(0);
				}
			}
			Assert.That(status, Is.EqualTo(StatusProvider.GetMERStatus()), "Please also check the following tables if this test fails: UNDGSubstanceRID, UNDGSubstanceADR, UNDGSubstanceADN, UNDGSubstanceJTT, UNDGSubstanceCFR.");
		}

		void AssertResult_StagingDb_UNDGAttributeZZ_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var attributeZZs = new List<UNDGAttributeZZ>();
			var sql = @"SELECT
DAZ_Type
FROM dbo.UNDGAttributeZZ
JOIN UNDGSubstanceADR ON DAZ_ParentPK = ADR_PK
WHERE ADR_UNNO ='0004' AND ADR_Variant = 'a'";
			stagingCommand.CommandText = sql;
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var attributeZZ = new UNDGAttributeZZ
					{
						DAZ_Type = reader.GetString(0)
					};
					attributeZZs.Add(attributeZZ);
				}
			}
			Assert.AreEqual(1, attributeZZs.Count);
			Assert.AreEqual("QDT", attributeZZs.First().DAZ_Type);
		}

		void AssertResult_SafeDb_UNDGAttributeZZ_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var attributeZZs = new List<Safe.UNDGAttributeZZ>();
			var sql = @"SELECT
DAZ_Type
FROM dbo.UNDGAttributeZZ
JOIN UNDGSubstanceADR ON DAZ_ParentPK = ADR_PK
WHERE ADR_UNNO ='0004' AND ADR_Variant = 'a'";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var attributeZZ = new Safe.UNDGAttributeZZ
					{
						DAZ_Type = reader.GetString(0)
					};
					attributeZZs.Add(attributeZZ);
				}
			}
			Assert.AreEqual(1, attributeZZs.Count);
			Assert.AreEqual("QDT", attributeZZs.First().DAZ_Type);
		}
	}
}
