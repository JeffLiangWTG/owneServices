using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class Test_RefCusTariffUOM_WithStartEndDate : IXmlProcessIntegrationTest
	{
		readonly Guid tariffPK = Guid.Parse("F5146E9D-F310-4158-8D0F-E4C3E156A37F");
		readonly Guid tariffTypePK = Guid.Parse("31BE09B9-8D92-48BC-81CF-CFD36EDED55A");
		readonly Guid tradeGroupPK1 = Guid.Parse("0D964E12-336E-4E6E-B9C7-B54EB507788C");
		readonly Guid tradeGroupPK2 = Guid.Parse("D42B786E-72DF-458A-BD9D-EE8B3273CE7C");

		public string[] FileNames => ["TestFiles\\RefCusTariffUOM_WithStartEndDate.xml"];

		public string TestDescription => "Test RefCusTariffUOM with EnableExpirable='true' can be merged successfully when existing record has null start/end date.";

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1];

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var safeSql = $@"
	insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
	values (newid(), 23, 'RefCusTariff', 'RefCusTariff','ZZ1',0);

	insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
	values (newid(), 'GB', 'United Kingdom', null),
	(newid(), 'FR', 'France', null);

	insert into [dbo].[RefCusTariffType] (ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping)
	values ('{tariffTypePK}', 'IMP', 'Import Tariff', '', 'GB');

	insert into [dbo].[RefCusTradeGroup] (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
	values ('{tradeGroupPK2}', '1010',' European Union + UK', '1958-01-01 00:00:00', '2079-06-06 23:59:00','GB'),
	('{tradeGroupPK1}', '1011',' ERGA OMNES1', '1958-01-01 00:00:00', '2079-06-06 23:59:00','GB');

	insert into [dbo].[RefCusTariff] (ZZ1_PK,ZZ1_ZZI_TariffType,ZZ1_TariffCode,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_CompositeKeyOnZZ5,ZZ1_ZZZ_NKDataGrouping)
	values ('{tariffPK}', '{tariffTypePK}', '8523520000', 'Smart cards', '2017-01-01 00:00:00.000', '2079-06-06 23:59:00.000', '', '', 'GB');
	insert into [dbo].[RefCusTariffUOM] (ZZ8_PK,ZZ8_ZZ1_Tariff,ZZ8_Type,ZZ8_UOM,ZZ8_ZZA_TradeGroup,ZZ8_ZZZ_NKDataGrouping,ZZ8_StartDate,ZZ8_EndDate)
	values (newid(), '{tariffPK}', 'CU1', 'NAR', '{tradeGroupPK1}', 'FR', NULL, NULL);
	";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(stagingCommand);
			AssertResult_StagingDb_RefCusTariffUOM_AfterProcessingXml1(stagingCommand);

			AssertResult_SafeDb_RefCusTariff_AfterProcessingXml1(safeCommand);
			AssertResult_SafeDb_RefCusTariffUOM_AfterProcessingXml1(safeCommand);
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var status = string.Empty;
			stagingCommand.CommandText = "SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='TariffUOM Test' ORDER BY SDA_CreatedTime DESC";
			using (var reader = stagingCommand.ExecuteReader())
			{
				if (reader.Read())
				{
					status = reader.GetString(0);
				}
			}
			Assert.That(status, Is.EqualTo(StatusProvider.GetMERStatus()));
		}

		void AssertResult_StagingDb_RefCusTariffUOM_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var tariffUOMs = new List<RefCusTariffUOM>();
			var sql = @"SELECT ZZ8_Type,ZZ8_ZZA_NKTradeGroup FROM dbo.RefCusTariffUOM JOIN dbo.RefCusTariff ON ZZ8_ZZ1_Tariff=ZZ1_PK
	WHERE ZZ1_TariffCode='0101210000' AND ZZ1_ZZZ_NKDataGrouping='GB'";
			stagingCommand.CommandText = sql;
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var tariffUOM = new RefCusTariffUOM();
					tariffUOM.ZZ8_Type = reader.GetString(0);
					if (!reader.IsDBNull(1))
					{
						tariffUOM.ZZ8_ZZA_NKTradeGroup = reader.GetString(1);
					}
					tariffUOMs.Add(tariffUOM);
				}
			}
			Assert.That(tariffUOMs.Count, Is.EqualTo(2));
			Assert.That(string.IsNullOrEmpty(tariffUOMs.First(x => x.ZZ8_Type == "CU1").ZZ8_ZZA_NKTradeGroup), Is.True);
			Assert.That(tariffUOMs.First(x => x.ZZ8_Type == "CU2").ZZ8_ZZA_NKTradeGroup, Is.EqualTo("1011"));
		}

		void AssertResult_SafeDb_RefCusTariff_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var tariffCode = string.Empty;
			var tariffType = Guid.Empty;
			safeCommand.CommandText = "SELECT ZZ1_TariffCode,ZZ1_ZZI_TariffType FROM dbo.RefCusTariff WHERE ZZ1_TariffCode='0101210000' AND ZZ1_ZZZ_NKDataGrouping='GB';";
			using (var reader = safeCommand.ExecuteReader())
			{
				if (reader.Read())
				{
					tariffCode = reader.GetString(0);
					tariffType = reader.GetGuid(1);
				}
			}
			Assert.That(tariffCode, Is.EqualTo("0101210000"));
			Assert.That(tariffType, Is.EqualTo(tariffTypePK));
		}

		void AssertResult_SafeDb_RefCusTariffUOM_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var allTariffUOMs = new List<Safe.RefCusTariffUOM>();
			var sql = @"SELECT ZZ8_Type,ZZ8_ZZ1_Tariff,ZZ8_ZZA_TradeGroup,ZZ8_StartDate,ZZ8_EndDate FROM dbo.RefCusTariffUOM JOIN dbo.RefCusTariff ON ZZ8_ZZ1_Tariff=ZZ1_PK
	WHERE ZZ1_TariffCode IN ('0101210000','8523520000') AND ZZ1_ZZZ_NKDataGrouping='GB'";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var tariffUOM = new Safe.RefCusTariffUOM();
					tariffUOM.ZZ8_Type = reader.GetString(0);
					tariffUOM.ZZ8_ZZ1_Tariff = reader.GetGuid(1);
					if (!reader.IsDBNull(2))
					{
						tariffUOM.ZZ8_ZZA_TradeGroup = reader.GetGuid(2);
					}
					if (!reader.IsDBNull(3))
					{
						tariffUOM.ZZ8_StartDate = reader.GetDateTime(3);
					}
					if (!reader.IsDBNull(3))
					{
						tariffUOM.ZZ8_EndDate = reader.GetDateTime(4);
					}
					allTariffUOMs.Add(tariffUOM);
				}
			}

			var tariffUOMs_0101210000 = allTariffUOMs.Where(x => x.ZZ8_ZZ1_Tariff != tariffPK).ToList();
			Assert.That(tariffUOMs_0101210000.Count, Is.EqualTo(2));
			Assert.That(tariffUOMs_0101210000.First(x => x.ZZ8_Type == "CU1").ZZ8_ZZA_TradeGroup, Is.Null);
			Assert.That(tariffUOMs_0101210000.First(x => x.ZZ8_Type == "CU2").ZZ8_ZZA_TradeGroup, Is.EqualTo(tradeGroupPK1));
			Assert.That(tariffUOMs_0101210000.Select(x => x.ZZ8_StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Distinct(), Is.EquivalentTo(new[] { "2012-01-01 00:00:00", "2013-01-01 00:00:00" }));
			Assert.That(tariffUOMs_0101210000.Select(x => x.ZZ8_EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Distinct(), Is.EquivalentTo(new[] { "2024-01-01 00:00:00", "2025-01-01 00:00:00" }));

			var tariffUOMs_8523520000 = allTariffUOMs.Where(x => x.ZZ8_ZZ1_Tariff == tariffPK).ToList();
			Assert.That(tariffUOMs_0101210000.Count, Is.EqualTo(2));
			Assert.That(tariffUOMs_8523520000.First(x => x.ZZ8_Type == "CU1").ZZ8_StartDate, Is.Null);
			Assert.That(tariffUOMs_8523520000.First(x => x.ZZ8_Type == "CU1").ZZ8_EndDate, Is.Null);
			Assert.That(tariffUOMs_8523520000.First(x => x.ZZ8_Type == "CU2").ZZ8_StartDate.Value.DateTime, Is.EqualTo(new DateTime(2017, 1, 1)));
			Assert.That(tariffUOMs_8523520000.First(x => x.ZZ8_Type == "CU2").ZZ8_EndDate.Value.DateTime, Is.EqualTo(new DateTime(2025, 12, 31, 23, 59, 0)));

		}
	}
}
