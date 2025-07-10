using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DuplicatedEndDatesRemoval : DataTransformation, IDataTransformationTask
	{
		public DuplicatedEndDatesRemoval(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"
UPDATE RefCusTariff SET ZZ1_EndDate = ZZ1_StartDate
WHERE ZZ1_PK IN (SELECT ZZ1_PK
FROM ( SELECT
	ZZ1_PK,
	RANK() OVER (PARTITION BY ZZ1_ZZZ_NKDataGrouping, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_IAmUnique, ZZ1_EndDate order by RVC_Deleted, ZZ1_StartDate desc) as rk
	FROM RefCusTariff
	JOIN RefDbVersionControl on RVC_ParentPK = ZZ1_PK
	) dupTariffs
WHERE rk > 1)
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
