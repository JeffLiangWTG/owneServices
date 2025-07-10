//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemDispatchTransportationUnitValidation
//
//    This class should be used for overriding validation in AutoWhsItemDispatchTransportationUnitValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchTransportationUnitValidation : AutoWhsItemDispatchTransportationUnitValidation
	{
		public WhsItemDispatchTransportationUnitValidation(AutoWhsItemDispatchTransportationUnit parent) : base(parent)
		{
		}

		protected override void CheckWDH_SignedByIsNotEmpty()
		{
			// remove mandatory check for CW1 desktop.
		}
	}
}
