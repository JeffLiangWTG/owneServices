//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemReceiveConsignmentValidation
//
//    This class should be used for overriding validation in AutoWhsItemReceiveConsignmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveConsignmentValidation : AutoWhsItemReceiveConsignmentValidation
	{
		public WhsItemReceiveConsignmentValidation(AutoWhsItemReceiveConsignment parent) : base(parent)
		{
		}
	}
}
