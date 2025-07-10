//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemReceiveTransportationUnitValidation
//
//    This class should be used for overriding validation in AutoWhsItemReceiveTransportationUnitValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveTransportationUnitValidation : AutoWhsItemReceiveTransportationUnitValidation
	{
		public WhsItemReceiveTransportationUnitValidation(AutoWhsItemReceiveTransportationUnit parent) : base(parent)
		{
		}

		protected override void CheckWRH_SignedByIsNotEmpty()
		{
			// remove mandatory check for CW1 desktop.
		}
	}
}
