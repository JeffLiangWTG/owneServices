//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRelatedActivityPivotValidation
//
//    This class should be used for overriding validation in AutoRelatedActivityPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RelatedActivityPivotValidation : AutoRelatedActivityPivotValidation
	{
		public RelatedActivityPivotValidation(AutoRelatedActivityPivot parent) : base(parent)
		{
		}
	}
}
