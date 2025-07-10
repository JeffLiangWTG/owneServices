using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eTail.Business
{
	public class HVLVClusterKeyNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
	{
		public HVLVClusterKeyNumberFountainUniqueIndexFailureHandler(string uniqueIndex, BusinessObject bizObjCausingError) : base(uniqueIndex, bizObjCausingError)
		{
		}

		protected override INumberFountainProxy NumberFountainToFix => Env.NumberFountains.HVLVConsignmentClusterKey;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected override DbCommand CommandToFindMaxValueInDatabase(DbConnection connection)
		{
			var sqlText = @"WITH MaxValues AS (
	SELECT
		ISNULL((SELECT MAX(HCH_ClusterKey) FROM dbo.HVLVConsignmentHeader), 0) AS MaxHCH,
		ISNULL((SELECT MAX(HVH_ClusterKey) FROM dbo.HVLVBookingHeader), 0) AS MaxHVH
)
SELECT CASE
	WHEN MaxHCH >= MaxHVH THEN MaxHCH
	ELSE MaxHVH
END AS MaxValue
FROM MaxValues";

			return connection.Command(sqlText);
		}
	}
}
