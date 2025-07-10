//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemTransferHeaderValidation
//
//    This class should be used for overriding validation in AutoWhsItemTransferHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemTransferHeaderValidation : AutoWhsItemTransferHeaderValidation
	{
		public WhsItemTransferHeaderValidation(AutoWhsItemTransferHeader parent) : base(parent)
		{
		}
	}
}
