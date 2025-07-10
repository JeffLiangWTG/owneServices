using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class RemoveDuplicateRefCusExcludedTradeGroupTransformation : DataTransformation, IDataTransformationTask
	{
		public RemoveDuplicateRefCusExcludedTradeGroupTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"DELETE a FROM RefCusExcludedTradeGroup a 
JOIN RefCusExcludedTradeGroup b
ON a.ZZC_ZZA_TradeGroup = b.ZZC_ZZA_TradeGroup AND  a.ZZC_ZZT_Applicability = b.ZZC_ZZT_Applicability
AND a.ZZC_PK < b.ZZC_PK";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
