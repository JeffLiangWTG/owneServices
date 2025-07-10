using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class AddingRVC_IsPublishedTranformation : DataTransformation, IDataTransformationTask
	{
		public AddingRVC_IsPublishedTranformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"UPDATE RefDbVersionControl SET RVC_IsPublished = 1";
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

