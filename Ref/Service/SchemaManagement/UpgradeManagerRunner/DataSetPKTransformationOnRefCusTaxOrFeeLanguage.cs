using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataSetPKTransformationOnRefCusTaxOrFeeLanguage : DataTransformation, IDataTransformationTask
	{
		public DataSetPKTransformationOnRefCusTaxOrFeeLanguage(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlTextRefCusTaxOrFeeLanguage = @"
	DISABLE TRIGGER ALL ON RefCusTaxOrFeeLanguage;

	UPDATE t 
	SET ZXU_DataSetPK = ZX0_PK, ZXU_DataSetCode = 'ZX0'
FROM RefCusTaxOrFeeLanguage t
 JOIN RefCusTaxOrFee ON t.ZXU_ZZF_TaxOrFee = ZZF_PK
 JOIN RefCusTaxOrFeetype ON ZZF_ZX0_NKTaxOrFeeType = ZX0_TaxOrFeeType;

	ENABLE TRIGGER ALL ON RefCusTaxOrFeeLanguage;
";

			DbHelper.ExecuteNonQuery(trans, sqlTextRefCusTaxOrFeeLanguage);
		}
	}
}
