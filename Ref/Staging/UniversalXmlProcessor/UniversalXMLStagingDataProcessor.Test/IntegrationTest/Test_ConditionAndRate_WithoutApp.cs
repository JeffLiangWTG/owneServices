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
	class Test_ConditionAndRate_WithoutApp : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\Test_ConditionAndRate_WithoutApp_Insert.xml", "TestFiles\\Test_ConditionAndRate_WithoutApp_UpdateExpire.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2];

		public string TestDescription => "Test Condition and Rate without RefCusApplicability can be merged successfully";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var safeSql = @"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (NEWID(), 23, 'RefCusTariff', 'RefCusTariff', 'ZZ1', 0);

insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'CH', 'Switzerland', NULL);

insert into [dbo].[RefCusTariffType] (ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping)
values ('F6A408EE-AD0F-4052-9981-EA942A79B37F', 'IMP', 'Import Tariff', '', 'CH');

insert into [dbo].[RefCusPreference] (ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
values (newid(), 'PR', 'Preferential tariff', 'CH');

insert into [dbo].[RefCusRateType] (ZZR_PK,ZZR_RateType,ZZR_Description,ZZR_IsPayable,ZZR_ZZZ_NKDataGrouping,ZZR_RX_NKFormulaCurrency,ZZR_CustomsValueFormula)
values ('19469B57-55A7-477C-96A9-EE03F482204C', 'DTY', 'Duty',1, 'CH', '', '');

insert into [dbo].[RefCusRateCode] (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
values (newid(), 'DTY', '19469B57-55A7-477C-96A9-EE03F482204C', 'Customs Duties');

insert into [dbo].[RefCusTradeGroup] (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
values (newid(), '200000', 'All countries', '2004-01-01 00:00:00', '2079-06-06 23:59:00', 'CH');

insert into [dbo].[RefCusConditionType] (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
values (newid(), 'CLASS', 'MVC', 'Mean value check', 'CH');

insert into [dbo].[RefCusConditionValueType] (ZX4_PK, ZX4_ValueType, ZX4_Description, ZX4_IsFormula, ZX4_ZZZ_NKDataGrouping)
values(newid(), 'FRM', 'Formula', 1, 'CH');

insert into [dbo].[RefLanguageType] (ZX6_PK, ZX6_Language, ZX6_Description)
values (newid(), 'EN', 'English');
";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();

			Console.WriteLine("Preparing Data Successfully");
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(stagingCommand);
				AssertResult_DPR_AfterProcessingXml1(stagingCommand, safeCommand);

				AssertResult_SafeDb_RefCusCondition_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusConditionValue_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusConditionLanguage_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusRate_AfterProcessingXml1(safeCommand);
			});
		}

		void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(stagingCommand);
				AssertResult_DPR_AfterProcessingXml2(stagingCommand);

				AssertResult_SafeDb_RefCusCondition_AfterProcessingXml2(safeCommand);
				AssertResult_SafeDb_RefCusConditionValue_AfterProcessingXml2(safeCommand);
				AssertResult_SafeDb_RefCusConditionLanguage_AfterProcessingXml2(safeCommand);
				AssertResult_SafeDb_RefCusRate_AfterProcessingXml2(safeCommand);
			});
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			stagingCommand.CommandText = @"SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='CH Tariff IMP Without App Test'
ORDER BY SDA_CreatedTime desc";
			var status = (string)stagingCommand.ExecuteScalar();
			Assert.AreEqual(StatusProvider.GetMERStatus(), status);
		}

		void AssertResult_DPR_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var pkListInSafe = new List<Guid>();
			var parentPkListInDPR = new List<Guid>();
			safeCommand.CommandText = @"
DECLARE @tariffPK UNIQUEIDENTIFIER = (SELECT ZZ1_PK FROM RefCusTariff WHERE ZZ1_TariffCode='01022110000911' AND ZZ1_ZZZ_NKDataGrouping='CH')
SELECT ZZ2_PK FROM RefCusRate WHERE ZZ2_ZZ1_Tariff=@tariffPK
UNION
SELECT ZX1_PK FROM RefCusCondition WHERE ZX1_ZZ1_Tariff=@tariffPK
UNION
SELECT ZZT_PK FROM RefCusApplicability WHERE ZZT_DataSetPK=@tariffPK";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var pk = (Guid)reader[0];
					pkListInSafe.Add(pk);
				}
			}

			stagingCommand.CommandText = "SELECT DPR_ParentPK FROM DataProcessingResult WHERE DPR_SubSource='CH Tariff IMP Without App Test' AND DPR_ParentTableCode IN ('ZZ2', 'ZX1', 'ZZT');";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var pk = (Guid)reader[0];
					parentPkListInDPR.Add(pk);
				}
			}
			Assert.AreEqual(6, parentPkListInDPR.Count);
			CollectionAssert.AreEquivalent(pkListInSafe, parentPkListInDPR);
		}

		void AssertResult_SafeDb_RefCusCondition_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var conditions = new List<Safe.RefCusCondition>();
			safeCommand.CommandText = @"
DECLARE @tariffPK UNIQUEIDENTIFIER = (SELECT ZZ1_PK FROM RefCusTariff WHERE ZZ1_TariffCode='01022110000911' AND ZZ1_ZZZ_NKDataGrouping='CH')
SELECT * FROM RefCusCondition LEFT JOIN RefCusApplicability ON ZX1_PK=ZZT_ZX1_Conditions
WHERE ZX1_ZZ1_Tariff=@tariffPK";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var condition = new Safe.RefCusCondition();
					condition.ZX1_Comment = reader[nameof(Safe.RefCusCondition.ZX1_Comment)].ToString();
					if (reader[nameof(Safe.RefCusApplicability.ZZT_PK)] != DBNull.Value)
					{
						var applicability = new Safe.RefCusApplicability();
						applicability.ZZT_PK = (Guid)reader[nameof(Safe.RefCusApplicability.ZZT_PK)];
						condition.RefCusApplicabilities.Add(applicability);
					}
					conditions.Add(condition);
				}
			}
			Assert.AreEqual(2, conditions.Count);
			Assert.AreEqual(0, conditions.First(x => x.ZX1_Comment == "Condition without app").RefCusApplicabilities.Count);
			Assert.AreEqual(1, conditions.First(x => x.ZX1_Comment == "Condition with app").RefCusApplicabilities.Count);
		}

		void AssertResult_SafeDb_RefCusConditionValue_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var conditions = new List<Safe.RefCusCondition>();
			safeCommand.CommandText = @"
DECLARE @tariffPK UNIQUEIDENTIFIER = (SELECT ZZ1_PK FROM RefCusTariff WHERE ZZ1_TariffCode='01022110000911' AND ZZ1_ZZZ_NKDataGrouping='CH')
SELECT * FROM RefCusCondition JOIN RefCusConditionValue ON ZX1_PK=ZX3_ZX1_Condition
WHERE ZX1_ZZ1_Tariff=@tariffPK";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var condition = new Safe.RefCusCondition();
					condition.ZX1_Comment = reader[nameof(Safe.RefCusCondition.ZX1_Comment)].ToString();
					var conditionValue = new Safe.RefCusConditionValue();
					conditionValue.ZX3_Value = reader[nameof(Safe.RefCusConditionValue.ZX3_Value)].ToString();
					condition.RefCusConditionValues.Add(conditionValue);
					conditions.Add(condition);
				}
			}
			Assert.AreEqual(2, conditions.Count);
			var conditionValues = conditions.First(x => x.ZX1_Comment == "Condition without app").RefCusConditionValues;
			Assert.AreEqual(1, conditionValues.Count);
			Assert.AreEqual("0", conditionValues[0].ZX3_Value);
			conditionValues = conditions.First(x => x.ZX1_Comment == "Condition with app").RefCusConditionValues;
			Assert.AreEqual(1, conditionValues.Count);
			Assert.AreEqual("1", conditionValues[0].ZX3_Value);
		}

		void AssertResult_SafeDb_RefCusConditionLanguage_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var conditions = new List<Safe.RefCusCondition>();
			safeCommand.CommandText = @"
DECLARE @tariffPK UNIQUEIDENTIFIER = (SELECT ZZ1_PK FROM RefCusTariff WHERE ZZ1_TariffCode='01022110000911' AND ZZ1_ZZZ_NKDataGrouping='CH')
SELECT * FROM RefCusCondition JOIN RefCusConditionLanguage ON ZX1_PK=ZXJ_ZX1_Condition
WHERE ZX1_ZZ1_Tariff=@tariffPK";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var condition = new Safe.RefCusCondition();
					condition.ZX1_Comment = reader[nameof(Safe.RefCusCondition.ZX1_Comment)].ToString();
					var conditionLanguage = new Safe.RefCusConditionLanguage();
					conditionLanguage.ZXJ_ZX6_NKLanguage = reader[nameof(Safe.RefCusConditionLanguage.ZXJ_ZX6_NKLanguage)].ToString();
					condition.RefCusConditionLanguages.Add(conditionLanguage);
					conditions.Add(condition);
				}
			}
			Assert.AreEqual(2, conditions.Count);
			var conditionLanguages = conditions.First(x => x.ZX1_Comment == "Condition without app").RefCusConditionLanguages;
			Assert.AreEqual(1, conditionLanguages.Count);
			Assert.AreEqual("EN", conditionLanguages[0].ZXJ_ZX6_NKLanguage);
			conditionLanguages = conditions.First(x => x.ZX1_Comment == "Condition with app").RefCusConditionLanguages;
			Assert.AreEqual(1, conditionLanguages.Count);
			Assert.AreEqual("EN", conditionLanguages[0].ZXJ_ZX6_NKLanguage);
		}

		void AssertResult_SafeDb_RefCusRate_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var rates = new List<Safe.RefCusRate>();
			safeCommand.CommandText = @"
DECLARE @tariffPK UNIQUEIDENTIFIER = (SELECT ZZ1_PK FROM RefCusTariff WHERE ZZ1_TariffCode='01022110000911' AND ZZ1_ZZZ_NKDataGrouping='CH')
SELECT * FROM RefCusRate LEFT JOIN RefCusApplicability ON ZZ2_PK=ZZT_ZZ2_Rate
WHERE ZZ2_ZZ1_Tariff=@tariffPK";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var rate = new Safe.RefCusRate();
					rate.ZZ2_RateFormula = reader[nameof(Safe.RefCusRate.ZZ2_RateFormula)].ToString();
					if (reader[nameof(Safe.RefCusApplicability.ZZT_PK)] != DBNull.Value)
					{
						var applicability = new Safe.RefCusApplicability();
						applicability.ZZT_PK = (Guid)reader[nameof(Safe.RefCusApplicability.ZZT_PK)];
						rate.RefCusApplicabilities.Add(applicability);
					}
					rates.Add(rate);
				}
			}
			Assert.AreEqual(2, rates.Count);
			Assert.NotNull(rates.First(x => x.RefCusApplicabilities.Count == 0));
			Assert.NotNull(rates.First(x => x.RefCusApplicabilities.Count == 1));
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			stagingCommand.CommandText = @"SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='CH Tariff IMP Without App Test'
ORDER BY SDA_CreatedTime desc";
			var status = (string)stagingCommand.ExecuteScalar();
			Assert.AreEqual(StatusProvider.GetMERStatus(), status);
		}

		void AssertResult_DPR_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var dprList = new List<DataProcessingResult>();
			stagingCommand.CommandText = "SELECT DPR_ParentPK, DPR_ParentTableCode, DPR_PublicationTime, DPR_Status FROM DataProcessingResult WHERE DPR_SubSource='CH Tariff IMP Without App Test' AND DPR_ParentTableCode IN ('ZZ2', 'ZX1', 'ZZT');";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var dpr = new DataProcessingResult();
					dpr.DPR_ParentPK = (Guid)reader[0];
					dpr.DPR_ParentTableCode = (string)reader[1];
					dpr.DPR_PublicationTime = (DateTime)reader[2];
					dpr.DPR_Status = (string)reader[3];
					dprList.Add(dpr);
				}
			}

			Assert.AreEqual(7, dprList.Count);
			var expiredDPR = dprList.FirstOrDefault(x => x.DPR_Status == "PRS");
			Assert.NotNull(expiredDPR);
			Assert.AreEqual("ZZ2", expiredDPR.DPR_ParentTableCode);
			Assert.AreEqual(new DateTime(2025, 4, 15, 1, 0, 0), expiredDPR.DPR_PublicationTime);
			dprList.Remove(expiredDPR);
			Assert.True(dprList.All(x => x.DPR_PublicationTime == new DateTime(2025, 4, 15, 2, 0, 0)));
		}

		void AssertResult_SafeDb_RefCusCondition_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var conditions = new List<Safe.RefCusCondition>();
			safeCommand.CommandText = @"
DECLARE @tariffPK UNIQUEIDENTIFIER = (SELECT ZZ1_PK FROM RefCusTariff WHERE ZZ1_TariffCode='01022110000911' AND ZZ1_ZZZ_NKDataGrouping='CH')
SELECT * FROM RefCusCondition LEFT JOIN RefCusApplicability ON ZX1_PK=ZZT_ZX1_Conditions
WHERE ZX1_ZZ1_Tariff=@tariffPK";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var condition = new Safe.RefCusCondition();
					condition.ZX1_Comment = reader[nameof(Safe.RefCusCondition.ZX1_Comment)].ToString();
					if (reader[nameof(Safe.RefCusApplicability.ZZT_PK)] != DBNull.Value)
					{
						var applicability = new Safe.RefCusApplicability();
						applicability.ZZT_PK = (Guid)reader[nameof(Safe.RefCusApplicability.ZZT_PK)];
						condition.RefCusApplicabilities.Add(applicability);
					}
					conditions.Add(condition);
				}
			}
			Assert.AreEqual(2, conditions.Count);
			Assert.AreEqual(0, conditions.First(x => x.ZX1_Comment == "Condition without app (update)").RefCusApplicabilities.Count);
			Assert.AreEqual(1, conditions.First(x => x.ZX1_Comment == "Condition with app (update)").RefCusApplicabilities.Count);
		}

		void AssertResult_SafeDb_RefCusConditionValue_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var conditions = new List<Safe.RefCusCondition>();
			safeCommand.CommandText = @"
DECLARE @tariffPK UNIQUEIDENTIFIER = (SELECT ZZ1_PK FROM RefCusTariff WHERE ZZ1_TariffCode='01022110000911' AND ZZ1_ZZZ_NKDataGrouping='CH')
SELECT * FROM RefCusCondition JOIN RefCusConditionValue ON ZX1_PK=ZX3_ZX1_Condition
WHERE ZX1_ZZ1_Tariff=@tariffPK";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var condition = new Safe.RefCusCondition();
					condition.ZX1_Comment = reader[nameof(Safe.RefCusCondition.ZX1_Comment)].ToString();
					var conditionValue = new Safe.RefCusConditionValue();
					conditionValue.ZX3_Value = reader[nameof(Safe.RefCusConditionValue.ZX3_Value)].ToString();
					condition.RefCusConditionValues.Add(conditionValue);
					conditions.Add(condition);
				}
			}
			Assert.AreEqual(2, conditions.Count);
			var conditionValues = conditions.First(x => x.ZX1_Comment == "Condition without app (update)").RefCusConditionValues;
			Assert.AreEqual(1, conditionValues.Count);
			Assert.AreEqual("10", conditionValues[0].ZX3_Value);
			conditionValues = conditions.First(x => x.ZX1_Comment == "Condition with app (update)").RefCusConditionValues;
			Assert.AreEqual(1, conditionValues.Count);
			Assert.AreEqual("1", conditionValues[0].ZX3_Value);
		}

		void AssertResult_SafeDb_RefCusConditionLanguage_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var conditions = new List<Safe.RefCusCondition>();
			safeCommand.CommandText = @"
DECLARE @tariffPK UNIQUEIDENTIFIER = (SELECT ZZ1_PK FROM RefCusTariff WHERE ZZ1_TariffCode='01022110000911' AND ZZ1_ZZZ_NKDataGrouping='CH')
SELECT * FROM RefCusCondition JOIN RefCusConditionLanguage ON ZX1_PK=ZXJ_ZX1_Condition
WHERE ZX1_ZZ1_Tariff=@tariffPK";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var condition = new Safe.RefCusCondition();
					condition.ZX1_Comment = reader[nameof(Safe.RefCusCondition.ZX1_Comment)].ToString();
					var conditionLanguage = new Safe.RefCusConditionLanguage();
					conditionLanguage.ZXJ_Comment = reader[nameof(Safe.RefCusConditionLanguage.ZXJ_Comment)].ToString();
					condition.RefCusConditionLanguages.Add(conditionLanguage);
					conditions.Add(condition);
				}
			}
			Assert.AreEqual(2, conditions.Count);
			var conditionLanguages = conditions.First(x => x.ZX1_Comment == "Condition without app (update)").RefCusConditionLanguages;
			Assert.AreEqual(1, conditionLanguages.Count);
			Assert.AreEqual("Condition without app (update)", conditionLanguages[0].ZXJ_Comment);
			conditionLanguages = conditions.First(x => x.ZX1_Comment == "Condition with app (update)").RefCusConditionLanguages;
			Assert.AreEqual(1, conditionLanguages.Count);
			Assert.AreEqual("Condition with app", conditionLanguages[0].ZXJ_Comment);
		}

		void AssertResult_SafeDb_RefCusRate_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var rates = new List<Safe.RefCusRate>();
			safeCommand.CommandText = @"
DECLARE @tariffPK UNIQUEIDENTIFIER = (SELECT ZZ1_PK FROM RefCusTariff WHERE ZZ1_TariffCode='01022110000911' AND ZZ1_ZZZ_NKDataGrouping='CH')
SELECT * FROM RefCusRate WHERE ZZ2_ZZ1_Tariff=@tariffPK";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var safeRate = new Safe.RefCusRate();
					safeRate.ZZ2_RateFormula = reader[nameof(Safe.RefCusRate.ZZ2_RateFormula)].ToString();
					safeRate.ZZ2_StartDate = (DateTime)reader[nameof(Safe.RefCusRate.ZZ2_StartDate)];
					safeRate.ZZ2_EndDate = (DateTime)reader[nameof(Safe.RefCusRate.ZZ2_EndDate)];
					rates.Add(safeRate);
				}
			}

			var startDate = new DateTime(2012, 1, 1);
			var endDate = new DateTime(2079, 6, 6, 23, 59, 0);
			Assert.AreEqual(3, rates.Count);
			var rate = rates.First(x => x.ZZ2_RateFormula == "0");
			Assert.AreEqual(startDate.AddMinutes(-1), rate.ZZ2_EndDate.DateTime);
			rate = rates.First(x => x.ZZ2_RateFormula == "10");
			Assert.AreEqual(startDate, rate.ZZ2_StartDate.DateTime);
			Assert.AreEqual(endDate, rate.ZZ2_EndDate.DateTime);
			rate = rates.FirstOrDefault(x => x.ZZ2_RateFormula == "20");
			Assert.NotNull(rate);
		}
	}
}
