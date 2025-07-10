using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00234455Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00234455Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
UPDATE uom SET ZZ8_UOM = REPLACE(ZZ8_UOM,' ','')
FROM RefCusTariff t
JOIN RefCusTariffUOM uom on uom.ZZ8_ZZ1_Tariff = t.ZZ1_PK
Where uom.ZZ8_ZZZ_NKDataGrouping = 'EUN'
AND t.ZZ1_ZZZ_NKDataGrouping = 'EUN'
AND uom.ZZ8_UOM like '% %';
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
