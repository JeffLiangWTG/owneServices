//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsABCCategoryValidation
//
//    This class should be used for overriding validation in AutoWhsABCCategoryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsABCCategoryValidation : AutoWhsABCCategoryValidation
	{
		public WhsABCCategoryValidation(AutoWhsABCCategory parent)
			: base(parent)
		{
		}
	}
}
