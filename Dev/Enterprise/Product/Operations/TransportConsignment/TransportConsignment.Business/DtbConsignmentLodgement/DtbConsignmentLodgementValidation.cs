//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbConsignmentLodgementValidation
//
//    This class should be used for overriding validation in AutoDtbConsignmentLodgementValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentLodgementValidation : AutoDtbConsignmentLodgementValidation
	{
		public DtbConsignmentLodgementValidation(AutoDtbConsignmentLodgement parent) : base(parent)
		{
		}
	}
}

