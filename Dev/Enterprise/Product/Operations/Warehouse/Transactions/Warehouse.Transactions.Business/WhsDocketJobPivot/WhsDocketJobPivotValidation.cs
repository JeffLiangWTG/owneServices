//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsDocketJobPivotValidation
//
//    This class should be used for overriding validation in AutoWhsDocketJobPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketJobPivotValidation : AutoWhsDocketJobPivotValidation
	{
		public WhsDocketJobPivotValidation(AutoWhsDocketJobPivot parent) : base(parent)
		{
		}
	}
}
