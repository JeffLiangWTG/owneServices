using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00196418Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00196418Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
UPDATE	UR 
SET		UR.ZZ8_ZZZ_NKDataGrouping = 'ZA'
FROM	RefCusTariffUOMRule UR
JOIN	RefCusTariffRule TR 
on		ZZ8_ZZ1_Tariff = ZZ1_PK
	AND ZZ1_ZZZ_NKDataGrouping = 'ZA'
	AND ZZ8_ZZZ_NKDataGrouping = ''

DELETE	TR
FROM	RefCusTariffRule TR
LEFT OUTER JOIN RefCusTariffAttributeRule TAR
ON		ZZ3_ZZ1_Tariff = ZZ1_PK 
	AND ZZ1_ZZZ_NKDataGrouping = 'ZA'
WHERE	ZZ1_TariffCode IN 
		(
			SELECT	ZZ1_TariffCode
			FROM	RefCusTariffRule
			JOIN	RefCusTariffUOMRule 
			ON		ZZ8_ZZ1_Tariff = ZZ1_PK 
				AND	ZZ1_ZZZ_NKDataGrouping = 'ZA'
			GROUP BY ZZ1_TariffCode, ZZ8_Type, ZZ8_UOM
			HAVING COUNT(*) > 1 
		)
	AND ZZ1_ZZZ_NKDataGrouping = 'ZA'
	AND ZZ3_PK IS NULL
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sqlText;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
