using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DeleteInvalidRefLocoMapAndRefUNLOCOUtcOffset : DataTransformation, IDataTransformationTask
	{
		public DeleteInvalidRefLocoMapAndRefUNLOCOUtcOffset(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"DELETE t FROM RefLocoMap t 
LEFT JOIN RefUNLOCO ON RY_RL_NKLocoPort = RL_Code
WHERE RL_PK IS NULL";
			DbHelper.ExecuteNonQuery(trans, sql);

			sql = @"DELETE t FROM RefUNLOCOUtcOffset t 
LEFT JOIN RefUNLOCO ON RLO_RL_NKCode = RL_Code
WHERE RL_PK IS NULL";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
