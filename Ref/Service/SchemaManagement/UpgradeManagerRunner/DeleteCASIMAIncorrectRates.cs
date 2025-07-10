using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DeleteCASIMAIncorrectRates : DataTransformation, IDataTransformationTask
	{
		public DeleteCASIMAIncorrectRates(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
DELETE ap
FROM RefCusApplicability ap
JOIN RefCusTradeGroup ON ZZT_ZZA_TradeGroup = ZZA_PK
JOIN RefCusRate ON ZZT_ZZ2_Rate = ZZ2_PK
JOIN RefCusTariff ON ZZ2_ZZ1_Tariff = ZZ1_PK
JOIN RefCusTariffType ON ZZ1_ZZI_TariffType = ZZI_PK
WHERE ZZ1_TariffCode = 'CRS2018' AND ZZ1_ZZZ_NKDataGrouping = 'CA'
AND ZZI_TariffType = 'SIMA' and ZZI_ZZZ_NKDataGrouping = 'CA' AND ZZA_TradeGroup IN ('KR', 'VN')
";

			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
