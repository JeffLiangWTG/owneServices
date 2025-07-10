//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewRelatedActivityPivotValidation
//
//    This class should be used for overriding validation in AutoViewRelatedActivityPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class ViewRelatedActivityPivotValidation : AutoViewRelatedActivityPivotValidation
	{
		public ViewRelatedActivityPivotValidation(AutoViewRelatedActivityPivot parent) : base(parent)
		{
		}
	}
}
