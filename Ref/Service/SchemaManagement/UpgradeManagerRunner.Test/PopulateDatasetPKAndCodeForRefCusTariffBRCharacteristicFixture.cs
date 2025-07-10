using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class PopulateDatasetPKAndCodeForRefCusTariffBRCharacteristicFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusTariffBRCharacteristic WHERE ZB1_ZZ1_Tariff IS NOT NULL AND ZB1_DataSetPK IS NULL";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 0);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new PopulateDatasetPKAndCodeForRefCusTariffBRCharacteristic(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
DISABLE TRIGGER RefCusTariffBRCharacteristic_Combined ON RefCusTariffBRCharacteristic;

INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (NEWID(), 'XXX', 'X');

INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping)
VALUES ('13D797BF-1A04-425D-9277-370886576F8D', 'XX', 'XX', 'XXX');

INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZF_NKTaxOrFeeCode, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZZ_NKDataGrouping)
VALUES ('9D3AC63B-C1CC-4941-BB9B-DD2177382274', '13D797BF-1A04-425D-9277-370886576F8D', '7000', 'X', '', '2002-01-01 00:00:00', '2079-06-06 23:59:00', 'XXX');

INSERT INTO RefCusTariffBRCharacteristic (ZB1_PK,ZB1_CharacteristicType,ZB1_ZZ1_Tariff,ZB1_ZZ5_Nomenclature,ZB1_Style,ZB1_MaxLength,ZB1_DecimalPlaces,ZB1_Code,ZB1_Text,ZB1_StartDate,ZB1_EndDate,ZB1_IsImport,ZB1_IsExport,ZB1_IsMandatory,ZB1_IsConditioningAttribute)
VALUES(NEWID(), 'NVE', '9D3AC63B-C1CC-4941-BB9B-DD2177382274', NULL, 'LIST', 10, 4, 'A1', 'A1', '2019-12-14 00:00:00', '2079-06-06 23:59:00', 1, 0, 1, 1);

ENABLE TRIGGER RefCusTariffBRCharacteristic_Combined ON RefCusTariffBRCharacteristic;
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}

