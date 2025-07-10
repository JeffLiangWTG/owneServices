//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemReceiveASNRTUPivotValidation
//
//    This class should be used for overriding validation in AutoWhsItemReceiveASNRTUPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveASNRTUPivotValidation : AutoWhsItemReceiveASNRTUPivotValidation
	{
		public WhsItemReceiveASNRTUPivotValidation(AutoWhsItemReceiveASNRTUPivot parent) : base(parent)
		{
		}
	}
}
