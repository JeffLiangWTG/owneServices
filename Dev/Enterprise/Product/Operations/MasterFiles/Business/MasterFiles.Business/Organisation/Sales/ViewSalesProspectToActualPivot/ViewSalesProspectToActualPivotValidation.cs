//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewSalesProspectToActualPivotValidation
//
//    This class should be used for overriding validation in AutoViewSalesProspectToActualPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class ViewSalesProspectToActualPivotValidation : AutoViewSalesProspectToActualPivotValidation
	{
		public ViewSalesProspectToActualPivotValidation(AutoViewSalesProspectToActualPivot parent) : base(parent)
		{
		}
	}
}
