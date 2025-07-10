//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemReceiveConsignmentRTUDivotValidation
//
//    This class should be used for overriding validation in AutoWhsItemReceiveConsignmentRTUDivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveConsignmentRTUDivotValidation : AutoWhsItemReceiveConsignmentRTUDivotValidation
	{
		public WhsItemReceiveConsignmentRTUDivotValidation(AutoWhsItemReceiveConsignmentRTUDivot parent) : base(parent)
		{
		}
	}
}
