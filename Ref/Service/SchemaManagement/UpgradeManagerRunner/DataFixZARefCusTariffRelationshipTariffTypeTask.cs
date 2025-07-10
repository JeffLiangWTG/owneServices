using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixZARefCusTariffRelationshipTariffTypeTask : DataTransformation, IDataTransformationTask
	{
		public DataFixZARefCusTariffRelationshipTariffTypeTask(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
UPDATE r SET r.ZZH_ZZI_TariffType = tt.ZZI_PK
FROM RefCusTariff t
JOIN RefCusTariffRelationship r ON r.ZZH_ZZ1_Tariff = t.ZZ1_PK
,RefCusTariffType tt
WHERE tt.ZZI_TariffType = '1P1' 
AND t.ZZ1_ZZZ_NKDataGrouping = 'ZA'
AND tt.ZZI_ZZZ_NKDataGrouping = 'ZA'
AND r.ZZH_ZZI_TariffType <> tt.ZZI_PK
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
