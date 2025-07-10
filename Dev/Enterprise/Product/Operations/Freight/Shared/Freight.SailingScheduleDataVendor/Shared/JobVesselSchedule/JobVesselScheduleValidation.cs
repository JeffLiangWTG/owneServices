//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVesselScheduleValidation
//
//    This class should be used for overriding validation in AutoJobVesselScheduleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class JobVesselScheduleValidation : AutoJobVesselScheduleValidation
	{
		public JobVesselScheduleValidation(AutoJobVesselSchedule parent) : base(parent)
		{
		}
	}
}
