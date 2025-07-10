//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsDockDoorAssignmentValidation
//
//    This class should be used for overriding validation in AutoWhsDockDoorAssignmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDockDoorAssignmentValidation : AutoWhsDockDoorAssignmentValidation
	{
		public WhsDockDoorAssignmentValidation(AutoWhsDockDoorAssignment parent) : base(parent)
		{
		}
	}
}
