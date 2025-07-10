using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RemoveDuplicateRefCusVATApplicabilityWI00627080Transformation : DataTransformation, IDataTransformationTask
	{
		public RemoveDuplicateRefCusVATApplicabilityWI00627080Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
WITH duplicates1 (Pk, Row_Id) AS (
  SELECT
    ZX5_PK,
    ROW_NUMBER() OVER(PARTITION BY ZX5_ZZ1_Tariff, ZX5_ZZW_TariffNationalCode, ZX5_ZZF_NKTaxOrFeeCode, ZX5_AdditionalCode, ZX5_EndDate, ZX5_ZZA_TradeGroup ORDER BY ZX5_SysStartTime ASC)
  FROM RefCusVATApplicability
)
DELETE vat 
FROM RefCusVATApplicability vat
JOIN duplicates1
ON Pk = ZX5_PK
WHERE Row_Id > 1;

WITH duplicates2 (Pk, Row_Id) AS (
  SELECT
    ZX5_PK,
    ROW_NUMBER() OVER(PARTITION BY ZX5_ZZ1_Tariff, ZX5_ZZW_TariffNationalCode, ZX5_ZZF_NKTaxOrFeeCode, ZX5_AdditionalCode, ZX5_StartDate, ZX5_ZZA_TradeGroup ORDER BY ZX5_SysStartTime ASC)
  FROM RefCusVATApplicability
)
DELETE vat 
FROM RefCusVATApplicability vat
JOIN duplicates2
ON Pk = ZX5_PK
WHERE Row_Id > 1
";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
