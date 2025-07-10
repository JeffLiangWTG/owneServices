using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class DataFixCheckConstraintsTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"select count(1) from RefCusCondition where ZX1_Comment = 'Invalid Record'";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select is_not_trusted from sys.check_constraints where name = 'CK_RefCusCondition_ZX1_StartDate_ZX1_EndDate'";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(1) from RefCusCondition where ZX1_Comment = 'Wrong tariff'";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select is_not_trusted from sys.foreign_keys where name = 'FK_RefCusCondition_RefCusTariff'";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"select count(1) from RefCusApplicability where ZZT_PK = '199920C7-353B-4AEE-897B-13C90BD3AEA0'";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixCheckConstraintsTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
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

IF EXISTS(SELECT * FROM sys.foreign_keys WHERE name='FK_RefCusCondition_RefCusTariff' AND parent_object_id = OBJECT_ID(N'[dbo].[RefCusCondition]'))
ALTER TABLE [dbo].[RefCusCondition] NOCHECK CONSTRAINT [FK_RefCusCondition_RefCusTariff];

INSERT INTO RefCusCondition(ZX1_PK, ZX1_ZZ1_Tariff, ZX1_Comment, ZX1_ZX2_NKConditionType, ZX1_ZZZ_NKDataGrouping)
VALUES(newid(), newid(), 'Wrong tariff', 'ANY', 'AU')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
