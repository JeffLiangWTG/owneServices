using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RuleRemoveWI00205083Transformation : DataTransformation, IDataTransformationTask
	{
		public RuleRemoveWI00205083Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
DELETE uom
FROM dbo.refcustariff t
JOIN dbo.refcustariffuom uom on t.ZZ1_PK = uom.ZZ8_ZZ1_Tariff
WHERE t.ZZ1_TariffCode like '6205%'
AND t.zz1_zzz_nkdatagrouping = 'ZA'
AND uom.ZZ8_Type = 'RU1'
AND uom.ZZ8_UOM = 'KG'

DELETE t
FROM dbo.refcustariffrule t
JOIN dbo.refcustariffuomrule uom on t.ZZ1_PK = uom.ZZ8_ZZ1_Tariff
WHERE t.ZZ1_TariffCode like '6205%'
AND t.zz1_zzz_nkdatagrouping = 'ZA'
AND uom.ZZ8_Type = 'RU1'
AND uom.ZZ8_UOM = 'KG'";

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
