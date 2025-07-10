//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewLocalTransportScheduleValidation
//
//    This class should be used for overriding validation in AutoViewLocalTransportScheduleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class ViewLocalTransportScheduleValidation : AutoViewLocalTransportScheduleValidation
	{
		public ViewLocalTransportScheduleValidation(AutoViewLocalTransportSchedule parent) : base(parent)
		{
		}
	}
}
