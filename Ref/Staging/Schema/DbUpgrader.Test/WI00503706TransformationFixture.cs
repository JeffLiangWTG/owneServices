using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class WI00503706TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusTariffUOM";
				Assert.AreEqual(2, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusTariffUOM WHERE ZZ8_Type NOT IN ('CU1','RU1','AD1','CU2','CU3','CU4','CU5')";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_AdditionalCode IS NULL OR  ZZT_OrderNumber IS NULL";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusApplicability";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"SELECT ZZT_AdditionalCode FROM RefCusApplicability";
				Assert.AreEqual("", Convert.ToString(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusConditionValueTypeLanguage WHERE ZXX_Description IS NULL";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusConditionValueTypeLanguage";
				Assert.AreEqual(3, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"SELECT ZXX_Description FROM RefCusConditionValueTypeLanguage WHERE ZXX_ZX4_ValueType = 'DF17CA9E-AD7A-4C0D-B1E0-C6057BC15D7E'";
				Assert.AreEqual("", Convert.ToString(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new WI00503706Transformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
ALTER TABLE RefCusTariffUOM DROP CONSTRAINT CK_RefCusTariffUOM_ZZ8_Type;
INSERT INTO RefCusTariffUOM (ZZ8_PK,ZZ8_ZZ1_Tariff,ZZ8_Type,ZZ8_UOM,ZZ8_ZZW_TariffNationalCode,ZZ8_ZZZ_NKDataGrouping)
VALUES 
(NEWID(), NULL, 'CU1', 'A1', NULL, 'EUN'),
(NEWID(), NULL, 'CU2', 'RU1', NULL, 'EUN'),
(NEWID(), NULL, 'FF', 'RU1', NULL, 'EUN');


INSERT INTO RefCusTariffType (ZZI_PK,ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
VALUES ('B8DA230C-BDDE-4B49-A17B-C42055D3769D','1P1', '1P1', 'AU')

INSERT INTO RefCusTariff (ZZ1_PK,ZZ1_ZZI_NKTariffType, ZZ1_StartDate, ZZ1_EndDate, ZZ1_TariffCode)
 VALUES ('15F534A1-E364-46A3-8ADC-952D685EDA1A', '1P1', '1900-01-01', '2079-06-06', 'Test');

IF EXISTS(SELECT * FROM sys.check_constraints WHERE name='CK_RefCusCondition_ZX1_StartDate_ZX1_EndDate' AND parent_object_id = OBJECT_ID(N'[dbo].[RefCusCondition]'))
ALTER TABLE [dbo].[RefCusCondition] NOCHECK CONSTRAINT [CK_RefCusCondition_ZX1_StartDate_ZX1_EndDate];

INSERT INTO RefCusCondition(ZX1_PK, ZX1_ZZ1_Tariff, ZX1_Comment, ZX1_ZX2_NKConditionType, ZX1_ZZZ_NKDataGrouping, ZX1_StartDate, ZX1_EndDate)
VALUES('EEDE1D46-7C0A-42EC-AEAE-A3EA5CD4C4D5', '15F534A1-E364-46A3-8ADC-952D685EDA1A', 'Invalid Record', 'ANY', 'AU', '2079-06-06', '1900-01-01')

INSERT INTO RefCusApplicability(ZZT_PK, ZZT_ZX1_Conditions, ZZT_ZZA_NKTradeGroup, ZZT_ZZA_ZZZ_NKDataGrouping, ZZT_StartDate, ZZT_EndDate)
VALUES('199920C7-353B-4AEE-897B-13C90BD3AEA0', 'EEDE1D46-7C0A-42EC-AEAE-A3EA5CD4C4D5', 'AU', 'AU', '1900-01-01', '2079-06-06')


ALTER TABLE RefCusConditionValueTypeLanguage ALTER COLUMN ZXX_Description NVARCHAR(500) NULL;

INSERT RefCusConditionValueType (ZX4_PK, ZX4_ValueType, ZX4_Description, ZX4_IsFormula, ZX4_ZZZ_NKDataGrouping)
VALUES ('DF17CA9E-AD7A-4C0D-B1E0-C6057BC15D7E', 'AAA', 'Test1', 1, 'ZA'),
('F94C22D5-57BC-43DE-B680-9CF1DF50CB59', 'BBB', 'Test2', 1, 'ZA'),
('A3554EC4-DBDF-4FD3-9113-8A5C17C37F5C', 'DDD', 'Test4', 1, 'ZA')

INSERT RefCusConditionValueTypeLanguage (ZXX_PK, ZXX_ZX4_ValueType, ZXX_ZX6_NKLanguage, ZXX_Description)
VALUES(NEWID(),'DF17CA9E-AD7A-4C0D-B1E0-C6057BC15D7E','WW', NULL),
(NEWID(),'F94C22D5-57BC-43DE-B680-9CF1DF50CB59','WW', NULL),
(NEWID(),'A3554EC4-DBDF-4FD3-9113-8A5C17C37F5C','WW', 'DDD')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
