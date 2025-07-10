using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixApplicabilityDataSetPKs : DataTransformation, IDataTransformationTask
	{
		public DataFixApplicabilityDataSetPKs(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = $@"
with data
as
(
select ZZT_PK, ZX1_DataSetCode, ZX1_DataSetPK from RefCusCondition 
join RefCusApplicability on ZZT_ZX1_Conditions = ZX1_PK
join RefCusPreference on ZZT_DataSetPK = ZZS_PK
where ZX1_ZZ1_Tariff is not null and ZX1_ZZS_Preference is not null
and ZZT_DataSetCode = 'ZZ1'
)

UPDATE a
SET a.ZZT_DataSetPK = d.ZX1_DataSetPK
FROM RefCusApplicability a
JOIN data d ON d.ZZT_PK = a.ZZT_PK
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
