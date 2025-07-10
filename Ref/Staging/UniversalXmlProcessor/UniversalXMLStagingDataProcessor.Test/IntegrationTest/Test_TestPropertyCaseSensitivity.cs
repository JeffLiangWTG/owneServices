using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class Test_PropertyValueCaseSensitivity : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\TestPropertyValueCaseSensitivity.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1];

		public string TestDescription => "Test property value is case sensitive during merging";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var safeSql = $@"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (newid(), 16, 'UNDGSubstance', 'UNDGSubstance','DG',0);

INSERT INTO UNDGSubstance (DG_PK, DG_UNNO, DG_Class, DG_PSN, DG_PG, DG_LQMaxAmt, DG_LQMaxAmtUQ, DG_PackIns, DG_ExceptedQuantityCode, DG_Standard, DG_IsNotOtherwiseSpecified, DG_LQMaxAmtType, DG_PaxPackIns, DG_LQ2OrPaxMaxAmtType, DG_LQ2OrPaxMaxAmt, DG_LQ2OrPaxMaxAmtUQ,DG_CargoPackAmtType, DG_CargoPackIns, DG_CargoMaxAmt, DG_CargoMaxAmtUQ, DG_EmergencyResponseGuide, DG_Hazards, DG_SpecialHandlingCodes, DG_UniqueRecordId, DG_IsActive)
VALUES('428371DC-6D0E-4FAB-8CD5-0F381C504DF6', '1088', '3', 'acetal', 'II', 1, 'L', 'Y341', 'E2', 'IAT', 0, 'NLM', '353', 'NLM', 5.000, 'L', 'NLM',	'364',	60.000,	'L',	'3H',	'Flammable Liquid;Package Orientation',	'RFL',	'7001', 1)

";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_SafeDb_UNDGSubstance_AfterProcessingXml1(safeCommand);
			});
		}

		void AssertResult_SafeDb_UNDGSubstance_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var uNDGSubstances = new List<Safe.UNDGSubstance>();
			var sql = @"SELECT
DG_UNNO, DG_PSN
FROM dbo.UNDGSubstance";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var uNDGSubstance = new Safe.UNDGSubstance
					{
						DG_UNNO = reader.GetString(0),
						DG_PSN = reader.GetString(1)
					};
					uNDGSubstances.Add(uNDGSubstance);
				}
			}
			Assert.AreEqual(1, uNDGSubstances.Count);
			Assert.AreEqual("1088", uNDGSubstances.First().DG_UNNO);
			Assert.AreEqual("Acetal", uNDGSubstances.First().DG_PSN);
		}
	}
}
