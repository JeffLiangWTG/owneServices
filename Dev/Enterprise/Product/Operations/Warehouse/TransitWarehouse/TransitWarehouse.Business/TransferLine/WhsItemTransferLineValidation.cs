//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemTransferLineValidation
//
//    This class should be used for overriding validation in AutoWhsItemTransferLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemTransferLineValidation : AutoWhsItemTransferLineValidation
	{
		public WhsItemTransferLineValidation(AutoWhsItemTransferLine parent) : base(parent)
		{
		}
	}
}
