using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class UpdateRefShippingLineTransformation : DataTransformation, IDataTransformationTask
	{
		public UpdateRefShippingLineTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"UPDATE r
SET RSL_BookingRequestAvailable = 1
FROM RefShippingLine r
JOIN RefShippingLineMessagingRequirement ON RSR_RSL_ShippingLine = RSL_PK
WHERE RSR_IsBookingRequest = 1 AND RSL_BookingRequestAvailable = 0;

UPDATE r
SET RSL_ShippingInstructionAvailable = 1
FROM RefShippingLine r
JOIN RefShippingLineMessagingRequirement ON RSR_RSL_ShippingLine = RSL_PK
WHERE RSR_IsShippingInstruction = 1 AND RSL_ShippingInstructionAvailable = 0;";
			DbHelper.ExecuteNonQuery(trans, sql);
		}
	}
}
