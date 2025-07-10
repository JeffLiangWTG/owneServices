//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewSalesRelationActivityDataValidation
//
//    This class should be used for overriding validation in AutoViewSalesRelationActivityDataValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class ViewSalesRelationActivityDataValidation : AutoViewSalesRelationActivityDataValidation
	{
		public ViewSalesRelationActivityDataValidation(AutoViewSalesRelationActivityData parent) : base(parent)
		{
		}
	}
}
