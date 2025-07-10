using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class UpdateRefCusRateCode_InternalUseTransformation : DataTransformation, IDataTransformationTask
	{
		public UpdateRefCusRateCode_InternalUseTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"UPDATE RefCusRateCode 
SET ZY1_InternalUse = 1 
FROM RefCusRateCode INNER JOIN RefCusRateType ON ZY1_ZZR_RateType =ZZR_PK
WHERE ZZR_ZZZ_NKDataGrouping = 'EUN' AND ZY1_RateCode IN ('ADFM','ADFMR','ADSZ','ADSZR','EA','EAR')";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
