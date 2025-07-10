using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class TariffTypeNKTransformationTask : DataTransformation, IDataTransformationTask
	{
		public TariffTypeNKTransformationTask(int version) : base(version) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ZZ1_ZZI_TariffType' AND object_id = OBJECT_ID('dbo.RefCusTariff'))
BEGIN
	UPDATE t
	SET t.ZZ1_ZZI_NKTariffType = tt.ZZI_TariffType
	FROM RefCusTariff t
	JOIN RefCusTariffType tt ON tt.ZZI_PK = t.ZZ1_ZZI_TariffType
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ZZH_ZZI_TariffType' AND object_id = OBJECT_ID('dbo.RefCusTariffRelationship'))
BEGIN
	UPDATE r
	SET r.ZZH_ZZI_NKTariffType = tt.ZZI_TariffType
	FROM RefCusTariffRelationship r
	JOIN RefCusTariffType tt ON tt.ZZI_PK = r.ZZH_ZZI_TariffType
END
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
