using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class PopulateDatasetPKAndCodeForRefCusTariffBRCharacteristic : DataTransformation, IDataTransformationTask
	{
		public PopulateDatasetPKAndCodeForRefCusTariffBRCharacteristic(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
DISABLE TRIGGER RefCusTariffBRCharacteristic_Combined ON RefCusTariffBRCharacteristic;

IF (SELECT COUNT(*) FROM RefCusTariffBRCharacteristic WHERE ZB1_ZZ1_Tariff IS NOT NULL AND ZB1_DataSetPK IS NULL) > 0
BEGIN
	UPDATE RefCusTariffBRCharacteristic SET ZB1_DataSetPK = ZB1_ZZ1_Tariff, ZB1_DataSetCode = 'ZZ1'
	WHERE ZB1_ZZ1_Tariff IS NOT NULL AND ZB1_DataSetPK IS NULL
END;

ENABLE TRIGGER RefCusTariffBRCharacteristic_Combined ON RefCusTariffBRCharacteristic;
";

			DbHelper.ExecuteNonQuery(trans, sql, 300);
		}
	}
}
