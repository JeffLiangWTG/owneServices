using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00196418TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;

				cmd.CommandText = @"Select Count(1) from RefCusTariffRule Where ZZ1_TariffCode IN ('56031110', '56031190', '56031210', '56031290', '262011')";
				Assert.AreEqual(5, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"Select Count(1) from RefCusTariffRule Where ZZ1_PK IN ('00000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000004')";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"Select Count(1) from RefCusTariffUOMRule Where ZZ8_PK IN ('00000000-0000-0000-0000-000000000020', '00000000-0000-0000-0000-000000000040')";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"Select Count(1) from RefCusTariffUOMRule Where ZZ8_PK IN ('00000000-0000-0000-0000-000000000010', '00000000-0000-0000-0000-000000000030', '00000000-0000-0000-0000-000000000050')";
				Assert.AreEqual(3, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00196418Transformation(16);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
insert into RefCusTariffRule (ZZ1_PK, ZZ1_Applied, ZZ1_TariffCode, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping) values ('00000000-0000-0000-0000-000000000001', 1, '56031110', NULL, 'ZA')
insert into RefCusTariffUOMRule (ZZ8_PK, ZZ8_Type, ZZ8_UOM, ZZ8_ZZ1_Tariff, ZZ8_ZZA_TradeGroup, ZZ8_ZZW_TariffNationalCode, ZZ8_ZZZ_NKDataGrouping) values ('00000000-0000-0000-0000-000000000010', 'RU1', 'SM', '00000000-0000-0000-0000-000000000001', NULL, NULL, 'ZA')
insert into RefCusTariffAttributeRule (ZZ3_Pk, ZZ3_ZZ1_Tariff, ZZ3_Name, ZZ3_Value) values ('00000000-0000-0000-0000-000000000100', '00000000-0000-0000-0000-000000000001', 'CheckDigit', '3')

insert into RefCusTariffRule (ZZ1_PK, ZZ1_Applied, ZZ1_TariffCode, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping) values ('00000000-0000-0000-0000-000000000002', 1, '56031110', NULL, 'ZA')
insert into RefCusTariffUOMRule (ZZ8_PK, ZZ8_Type, ZZ8_UOM, ZZ8_ZZ1_Tariff, ZZ8_ZZA_TradeGroup, ZZ8_ZZW_TariffNationalCode, ZZ8_ZZZ_NKDataGrouping) values ('00000000-0000-0000-0000-000000000020', 'RU1', 'SM', '00000000-0000-0000-0000-000000000002', NULL, NULL, '')

insert into RefCusTariffRule (ZZ1_PK, ZZ1_Applied, ZZ1_TariffCode, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping) values ('00000000-0000-0000-0000-000000000003', 1, '56031190', NULL, 'ZA')
insert into RefCusTariffUOMRule (ZZ8_PK, ZZ8_Type, ZZ8_UOM, ZZ8_ZZ1_Tariff, ZZ8_ZZA_TradeGroup, ZZ8_ZZW_TariffNationalCode, ZZ8_ZZZ_NKDataGrouping) values ('00000000-0000-0000-0000-000000000030', 'RU1', 'SM', '00000000-0000-0000-0000-000000000003', NULL, NULL, 'ZA')
insert into RefCusTariffAttributeRule (ZZ3_Pk, ZZ3_ZZ1_Tariff, ZZ3_Name, ZZ3_Value) values ('00000000-0000-0000-0000-000000000300', '00000000-0000-0000-0000-000000000003', 'CheckDigit', '3')

insert into RefCusTariffRule (ZZ1_PK, ZZ1_Applied, ZZ1_TariffCode, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping) values ('00000000-0000-0000-0000-000000000004', 1, '56031190', NULL, 'ZA')
insert into RefCusTariffUOMRule (ZZ8_PK, ZZ8_Type, ZZ8_UOM, ZZ8_ZZ1_Tariff, ZZ8_ZZA_TradeGroup, ZZ8_ZZW_TariffNationalCode, ZZ8_ZZZ_NKDataGrouping) values ('00000000-0000-0000-0000-000000000040', 'RU1', 'SM', '00000000-0000-0000-0000-000000000004', NULL, NULL, '')

insert into RefCusTariffRule (ZZ1_PK, ZZ1_Applied, ZZ1_TariffCode, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping) values ('00000000-0000-0000-0000-000000000005', 1, '56031210', NULL, 'ZA')
insert into RefCusTariffUOMRule (ZZ8_PK, ZZ8_Type, ZZ8_UOM, ZZ8_ZZ1_Tariff, ZZ8_ZZA_TradeGroup, ZZ8_ZZW_TariffNationalCode, ZZ8_ZZZ_NKDataGrouping) values ('00000000-0000-0000-0000-000000000050', 'RU1', 'SM', '00000000-0000-0000-0000-000000000005', NULL, NULL, 'ZA')

insert into RefCusTariffRule (ZZ1_PK, ZZ1_Applied, ZZ1_TariffCode, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping) values ('00000000-0000-0000-0000-000000000006', 1, '56031290', NULL, 'ZA')

insert into RefCusTariffRule (ZZ1_PK, ZZ1_Applied, ZZ1_TariffCode, ZZ1_ZZI_TariffType, ZZ1_ZZZ_NKDataGrouping) values ('00000000-0000-0000-0000-000000000007', 1, '262011', NULL, 'ZA')
insert into RefCusTariffAttributeRule (ZZ3_Pk, ZZ3_ZZ1_Tariff, ZZ3_Name, ZZ3_Value) values ('00000000-0000-0000-0000-000000000700', '00000000-0000-0000-0000-000000000007', 'CheckDigit', '3')
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
