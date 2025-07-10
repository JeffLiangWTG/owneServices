//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemDTUReturnDetailValidation
//
//    This class should be used for overriding validation in AutoWhsItemDTUReturnDetailValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDTUReturnDetailValidation : AutoWhsItemDTUReturnDetailValidation
	{
		public WhsItemDTUReturnDetailValidation(AutoWhsItemDTUReturnDetail parent) : base(parent)
		{
		}
	}
}
