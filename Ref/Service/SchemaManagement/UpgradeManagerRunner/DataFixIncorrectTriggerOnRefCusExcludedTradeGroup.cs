using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixIncorrectTriggerOnRefCusExcludedTradeGroup : DataTransformation, IDataTransformationTask
	{
		public DataFixIncorrectTriggerOnRefCusExcludedTradeGroup(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
UPDATE e SET e.ZZC_DataSetPK = a.ZZT_DataSetPK, e.ZZC_DataSetCode = a.ZZT_DataSetCode
FROM RefCusExcludedTradeGroup e
JOIN RefCusApplicability a ON a.ZZT_PK = e.ZZC_ZZT_Applicability
WHERE e.ZZC_DataSetPK <> a.ZZT_DataSetPK
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sqlText;
				cmd.CommandTimeout = 300;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
