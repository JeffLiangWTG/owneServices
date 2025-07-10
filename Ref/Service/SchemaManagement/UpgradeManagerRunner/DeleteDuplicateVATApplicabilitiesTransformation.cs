using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DeleteDuplicateVATApplicabilitiesTransformation : DataTransformation, IDataTransformationTask
	{
		public DeleteDuplicateVATApplicabilitiesTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var cmd = $@"
SELECT ZX5_ZZ1_Tariff, ZX5_ZZF_NKTaxOrFeeCode
INTO #VATApplicability
FROM RefCusVATApplicability
WHERE ZX5_ZZZ_NKDataGrouping='IT'
AND ZX5_ZZF_NKTaxOrFeeCode IN (SELECT ZZF_Code FROM RefCusTaxOrFee WHERE ZZF_ZZZ_NKDataGrouping='IT')
GROUP BY ZX5_ZZ1_Tariff,ZX5_ZZF_NKTaxOrFeeCode HAVING COUNT(ZX5_ZZ1_Tariff)>1 AND COUNT(ZX5_ZZF_NKTaxOrFeeCode)>1

DELETE FROM RefCusVATApplicability WHERE ZX5_PK IN (
SELECT A.ZX5_PK FROM RefCusVATApplicability A JOIN #VAtApplicability B
ON A.ZX5_ZZ1_Tariff = B.ZX5_ZZ1_Tariff AND A.ZX5_ZZF_NKTaxOrFeeCode = B.ZX5_ZZF_NKTaxOrFeeCode
WHERE A.ZX5_AdditionalCode = '')

DROP TABLE #VATApplicability
";

			DbHelper.ExecuteNonQuery(trans, cmd);
		}
	}
}
