//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbConsignmentLegValidation
//
//    This class should be used for overriding validation in AutoDtbConsignmentLegValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentLegValidation : AutoDtbConsignmentLegValidation
	{
		public DtbConsignmentLegValidation(AutoDtbConsignmentLeg parent) : base(parent)
		{
		}
	}
}
