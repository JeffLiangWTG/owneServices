//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbConsignmentLodgementPivotValidation
//
//    This class should be used for overriding validation in AutoDtbConsignmentLodgementPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentLodgementPivotValidation : AutoDtbConsignmentLodgementPivotValidation
	{
		public DtbConsignmentLodgementPivotValidation(AutoDtbConsignmentLodgementPivot parent) : base(parent)
		{
		}
	}
}

