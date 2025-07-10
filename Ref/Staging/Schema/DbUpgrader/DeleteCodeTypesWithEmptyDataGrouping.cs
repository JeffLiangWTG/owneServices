using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class DeleteCodeTypesWithEmptyDataGrouping : DataTransformation, IDataTransformationTask
	{
		public DeleteCodeTypesWithEmptyDataGrouping(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"DELETE FROM RefCusCodeType WHERE ZZK_ZZZ_NKDataGrouping = '';";

			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
