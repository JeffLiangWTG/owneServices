using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00189669Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00189669Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
UPDATE r SET r.ZZ2_ZZS_Preference = Pcorrect.ZZS_PK
FROM RefCusTariff t
JOIN RefCusRate r on r.ZZ2_ZZ1_Tariff = t.zz1_pk
JOIN RefCusPreference p on p.ZZS_PK = r.ZZ2_ZZS_Preference
JOIN RefCusPreference Pcorrect on Pcorrect.ZZS_Preference = p.ZZS_Preference and Pcorrect.ZZS_ZZZ_NKDataGrouping = 'ZA'
WHERE t.zz1_zzz_nkdatagrouping = 'ZA'
AND r.ZZ2_ZZZ_NKDataGrouping = 'ZA'
AND p.ZZS_ZZZ_NKDataGrouping <> 'ZA'
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
