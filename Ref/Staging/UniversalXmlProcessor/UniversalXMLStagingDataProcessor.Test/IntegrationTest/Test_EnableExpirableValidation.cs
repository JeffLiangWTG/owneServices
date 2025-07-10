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
	class Test_EnableExpirableValidation : IXmlProcessIntegrationTest
	{
		readonly Guid tariffTypePK = Guid.Parse("FC6DB681-F5F4-4B2F-9020-84AFF4F8BE98");
		readonly Guid tradeGroupPK = Guid.Parse("3877F8E3-0B74-4F40-AFFE-EEF862BB3EFC");
		readonly Guid tariffPK = Guid.Parse("5F3D0392-3F2B-453B-9B1E-5D3BC4414264");

		public string[] FileNames => ["TestFiles\\TestEnableExpirable_FRTariffData_Chapter85.xml", "TestFiles\\TestEnableExpirable_TaricAndSEDailyEUN.xml"];

		public string TestDescription => "Test EnableExpirable validation works as expected";

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2];

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var safeSql = $@"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (newid(), 23, 'RefCusTariff', 'RefCusTariff','ZZ1',0);

insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'EUN', 'European Union', null),
(newid(), 'FR', 'France', 'EUN');

insert into [dbo].[RefCusTradeGroup] (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
values ('{tradeGroupPK}', 'FR04', 'PAYS DE LUE + 5 DOM', '2005-01-01 00:00:00', '2079-06-06 23:59:00', 'FR');

insert into [dbo].[RefCusTariffType] (ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping)
values ('{tariffTypePK}', 'IMP', 'Import Tariff', '', 'EUN');

insert into [dbo].[RefCusTariff] (ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_CompositeKeyOnZZ5,ZZ1_ZZZ_NKDataGrouping)
values ('{tariffPK}', '{tariffTypePK}', '8523520000', 'Smart cards', '2017-01-01 00:00:00.000', '2079-06-06 23:59:00.000', '', '', 'EUN');
insert into [dbo].[RefCusTariffUOM] (ZZ8_PK,ZZ8_ZZ1_Tariff,ZZ8_Type,ZZ8_UOM,ZZ8_ZZA_TradeGroup,ZZ8_ZZZ_NKDataGrouping,ZZ8_StartDate,ZZ8_EndDate)
values (newid(), '{tariffPK}', 'CU2', 'NAR', '{tradeGroupPK}', 'FR', '2017-01-01 00:00:00.000', '2079-06-06 23:59:00.000');
";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(stagingCommand);
			AssertResult_SafeDb_RefCusTariffUOM_AfterProcessingXml1(safeCommand);
		}

		void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(stagingCommand);
			AssertResult_SafeDb_RefCusTariffUOM_AfterProcessingXml2(safeCommand);
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			stagingCommand.CommandText = "SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='FR Tariff chapter 85 Test' ORDER BY SDA_CreatedTime DESC";
			var status = stagingCommand.ExecuteScalar().ToString();
			Assert.That(status, Is.EqualTo(StatusProvider.GetERRStatus()));
		}

		void AssertResult_SafeDb_RefCusTariffUOM_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var tariffUOMs = new List<Safe.RefCusTariffUOM>();
			var sql = @"SELECT ZZ8_Type,ZZ8_UOM FROM dbo.RefCusTariffUOM JOIN dbo.RefCusTariff ON ZZ8_ZZ1_Tariff=ZZ1_PK
WHERE ZZ1_TariffCode='8523520000' AND ZZ8_ZZZ_NKDataGrouping='FR' AND ZZ8_StartDate IS NOT NULL";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var tariffUOM = new Safe.RefCusTariffUOM();
					tariffUOM.ZZ8_Type = reader.GetString(0);
					tariffUOM.ZZ8_UOM = reader.GetString(1);
					tariffUOMs.Add(tariffUOM);
				}
			}
			Assert.AreEqual(1, tariffUOMs.Count);
			Assert.AreEqual("NAR", tariffUOMs.First(x => x.ZZ8_Type == "CU2").ZZ8_UOM);
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			stagingCommand.CommandText = "SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='Taric Daily Tariffs Test' ORDER BY SDA_CreatedTime DESC";
			var status = stagingCommand.ExecuteScalar().ToString();
			Assert.That(status, Is.EqualTo(StatusProvider.GetMERStatus()));
		}

		void AssertResult_SafeDb_RefCusTariffUOM_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var tariffUOMs = new List<Safe.RefCusTariffUOM>();
			var sql = @"SELECT ZZ8_Type,ZZ8_UOM FROM dbo.RefCusTariffUOM JOIN dbo.RefCusTariff ON ZZ8_ZZ1_Tariff=ZZ1_PK
WHERE ZZ1_TariffCode='8523520000' AND ZZ8_ZZZ_NKDataGrouping='EUN' AND ZZ8_StartDate IS NULL";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var tariffUOM = new Safe.RefCusTariffUOM();
					tariffUOM.ZZ8_Type = reader.GetString(0);
					tariffUOM.ZZ8_UOM = reader.GetString(1);
					tariffUOMs.Add(tariffUOM);
				}
			}
			Assert.AreEqual(1, tariffUOMs.Count);
			Assert.AreEqual("KGM", tariffUOMs.First().ZZ8_UOM);
		}
	}
}
