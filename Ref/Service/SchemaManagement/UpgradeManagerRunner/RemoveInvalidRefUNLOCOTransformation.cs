using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RemoveInvalidRefUNLOCOTransformation : DataTransformation, IDataTransformationTask
	{
		public RemoveInvalidRefUNLOCOTransformation(int version) : base(version)
		{ }

		public void Run(IDbTransaction trans)
		{
			var sql = @"DELETE FROM dbo.RefUNLOCO WHERE RL_Code NOT LIKE '[A-Z][A-Z][A-Z0-9][A-Z0-9][A-Z0-9]'";

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
