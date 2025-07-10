using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class FormulaChangeWI00196892Transformation : DataTransformation, IDataTransformationTask
	{
		public FormulaChangeWI00196892Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
update r set r.ZZ2_RateFormula = 'MIN(MAX((ROUND(0.00003 * VFD, 3) - 0.75), 0) * VFD/100, 0.3 * VFD)' 
from refcustariff t 
join refcustarifftype tt on tt.ZZI_PK = t.ZZ1_ZZI_TariffType
join refcusrate r on r.zz2_zz1_tariff = t.zz1_pk 
where tt.ZZI_TariffType = '12B'
and t.ZZ1_ZZZ_NKDataGrouping = 'ZA'
and r.zz2_rateformula = 'MIN(MAX((ROUND(0.00003 * VFD, 3) - 0.75), 0) * VFD/100, 0.25 * VFD)' 
and zz2_startdate >= '2018-04-01 00:00:00'
";

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
