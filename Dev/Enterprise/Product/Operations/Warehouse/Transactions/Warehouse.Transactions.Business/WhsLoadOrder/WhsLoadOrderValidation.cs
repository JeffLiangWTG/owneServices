//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsLoadOrderValidation
//
//    This class should be used for overriding validation in AutoWhsLoadOrderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsLoadOrderValidation : AutoWhsLoadOrderValidation
	{
		public WhsLoadOrderValidation(AutoWhsLoadOrder parent) : base(parent)
		{
		}
	}
}
