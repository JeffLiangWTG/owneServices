using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00514782Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00514782Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF 3 = (SELECT COUNT(*) Names 
    from sys.columns 
    WHERE OBJECT_ID = OBJECT_ID('RefShippingLineMessagingRequirement')
    AND Name IN ('RSR_IsBookingRequest', 'RSR_IsShippingInstruction', 'RSR_IsShippingOrder')
    )
BEGIN
	DELETE FROM RefShippingLineMessagingRequirement
	WHERE RSR_IsBookingRequest = 0 AND RSR_IsShippingInstruction = 0 AND RSR_IsShippingOrder = 0
END
";
			DbHelper.ExecuteNonQuery(trans, sql, 600);
		}
	}
}
