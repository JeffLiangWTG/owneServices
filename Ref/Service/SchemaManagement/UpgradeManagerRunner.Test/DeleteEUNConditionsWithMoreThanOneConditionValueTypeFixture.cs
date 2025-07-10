using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DeleteEUNConditionsWithMoreThanOneConditionValueTypeFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = @"SELECT COUNT(*) FROM RefCusConditionValue WHERE ZX3_ZX1_Condition = '{0}'";
			Assert.AreEqual(0, DbHelper.ExecuteScalar(Transaction, string.Format(CultureInfo.InvariantCulture, sql, "B6E8E1BB-09B9-4BB0-8B84-3E23B9CD303C")));
			Assert.AreEqual(1, DbHelper.ExecuteScalar(Transaction, string.Format(CultureInfo.InvariantCulture, sql, "02D25351-8EF8-4752-8A43-81FCFAA83FE8")));
			Assert.AreEqual(2, DbHelper.ExecuteScalar(Transaction, string.Format(CultureInfo.InvariantCulture, sql, "DF4DCECC-4412-4AD3-946D-FAC61E04C2F1")));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DeleteEUNConditionsWithMoreThanOneConditionValueType(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('1AF726AC-BE39-4BE6-A638-2E95BDF3A6B5','EUN','EUN', NULL)

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_HasFormulaSpecificQuestions])
VALUES ('5E2AD85E-10F0-4FBF-ABB1-1B2B03E5B4C4','IMP','IMP','EUN',1);

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('3714350B-06A1-4C25-87FB-D9E92D02D64C','5E2AD85E-10F0-4FBF-ABB1-1B2B03E5B4C4','213030208',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','EUN','');

INSERT INTO [dbo].[RefCusConditionType] (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES	('D8EAB791-B14D-460A-99E5-E7EC3A7CA0A2', 'CTRL', '724', 'XXX', 'EUN')

INSERT INTO [dbo].[RefCusConditionValueType] (ZX4_PK, ZX4_ValueType, ZX4_Description, ZX4_IsFormula, ZX4_ZZZ_NKDataGrouping)
VALUES
	('DD6AB09A-C4B6-430F-B594-400989664B9F', 'D', 'XXX', 0, 'EUN'),
	('0AAD41CD-9CDA-486E-8688-2C6EA80FBF3C', 'R', 'YYY', 1, 'EUN')

INSERT INTO [dbo].[RefCusCondition] (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_Source, ZX1_Comment, ZX1_ZZZ_NKDataGrouping)
VALUES
	('B6E8E1BB-09B9-4BB0-8B84-3E23B9CD303C', 'D8EAB791-B14D-460A-99E5-E7EC3A7CA0A2', '3714350B-06A1-4C25-87FB-D9E92D02D64C', '1900-01-01', '2079-06-06', '', '', 'EUN'),
	('02D25351-8EF8-4752-8A43-81FCFAA83FE8', 'D8EAB791-B14D-460A-99E5-E7EC3A7CA0A2', '3714350B-06A1-4C25-87FB-D9E92D02D64C', '1900-01-01', '2079-06-06', '', '', 'EUN'),
	('DF4DCECC-4412-4AD3-946D-FAC61E04C2F1', 'D8EAB791-B14D-460A-99E5-E7EC3A7CA0A2', '3714350B-06A1-4C25-87FB-D9E92D02D64C', '1900-01-01', '2079-06-06', '', '', 'EUN')

INSERT INTO [dbo].[RefCusConditionValue] (ZX3_PK, ZX3_ZX4_ValueType, ZX3_ZX1_Condition, ZX3_Value, ZX3_LogicalORWithinGroup)
VALUES
	(newid(), 'DD6AB09A-C4B6-430F-B594-400989664B9F', 'B6E8E1BB-09B9-4BB0-8B84-3E23B9CD303C', '1', 0),
	(newid(), '0AAD41CD-9CDA-486E-8688-2C6EA80FBF3C', 'B6E8E1BB-09B9-4BB0-8B84-3E23B9CD303C', '1', 0),
	(newid(), '0AAD41CD-9CDA-486E-8688-2C6EA80FBF3C', '02D25351-8EF8-4752-8A43-81FCFAA83FE8', '1', 0),
	(newid(), 'DD6AB09A-C4B6-430F-B594-400989664B9F', 'DF4DCECC-4412-4AD3-946D-FAC61E04C2F1', '1', 0),
	(newid(), 'DD6AB09A-C4B6-430F-B594-400989664B9F', 'DF4DCECC-4412-4AD3-946D-FAC61E04C2F1', '2', 0)

";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
