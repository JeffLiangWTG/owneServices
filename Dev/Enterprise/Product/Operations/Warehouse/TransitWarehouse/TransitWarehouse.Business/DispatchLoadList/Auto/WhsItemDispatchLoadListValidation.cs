//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemDispatchLoadListValidation
//
//    This class should be used for overriding validation in AutoWhsItemDispatchLoadListValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchLoadListValidation : AutoWhsItemDispatchLoadListValidation
	{
		public WhsItemDispatchLoadListValidation(AutoWhsItemDispatchLoadList parent) : base(parent)
		{
		}
	}
}