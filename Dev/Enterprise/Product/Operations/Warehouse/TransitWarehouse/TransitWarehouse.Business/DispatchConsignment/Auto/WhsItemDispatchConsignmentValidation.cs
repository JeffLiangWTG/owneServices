//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsTransitDispatchConsignmentValidation
//
//    This class should be used for overriding validation in AutoWhsTransitDispatchConsignmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchConsignmentValidation : AutoWhsItemDispatchConsignmentValidation
	{
		public WhsItemDispatchConsignmentValidation(AutoWhsItemDispatchConsignment parent) : base(parent)
		{
		}
	}
}
