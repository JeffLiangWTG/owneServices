using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataSetPKTransformationOnRefCusRateCodeLanguage : DataTransformation, IDataTransformationTask
	{
		public DataSetPKTransformationOnRefCusRateCodeLanguage(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlTextRefCusTaxOrFeeLanguage = @"
	DISABLE TRIGGER ALL ON RefCusRateCodeLanguage;

	UPDATE t 
	SET ZXC_DataSetPK = ZZR_PK, ZXC_DataSetCode = 'ZZR'
FROM RefCusRateCodeLanguage t
JOIN RefCusRateCode on t.ZXC_ZY1_RateCode = ZY1_PK
JOIN RefCusRateType on ZY1_ZZR_RateType = ZZR_PK;

	ENABLE TRIGGER ALL ON RefCusRateCodeLanguage;
";

			DbHelper.ExecuteNonQuery(trans, sqlTextRefCusTaxOrFeeLanguage);
		}
	}
}
