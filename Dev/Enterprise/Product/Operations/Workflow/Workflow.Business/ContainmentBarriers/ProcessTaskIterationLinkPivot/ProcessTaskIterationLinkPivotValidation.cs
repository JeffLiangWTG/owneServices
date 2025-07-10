//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessTaskIterationLinkPivotValidation
//
//    This class should be used for overriding validation in AutoProcessTaskIterationLinkPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Workflow.Business
{
	public class ProcessTaskIterationLinkPivotValidation : AutoProcessTaskIterationLinkPivotValidation
	{
		public ProcessTaskIterationLinkPivotValidation(AutoProcessTaskIterationLinkPivot parent) : base(parent)
		{
		}
	}
}
