//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewValueAnalysisValidation
//
//    This class should be used for overriding validation in AutoViewValueAnalysisValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MarketingManager.Business
{
	public class ViewValueAnalysisValidation : AutoViewValueAnalysisValidation
	{
		public ViewValueAnalysisValidation(AutoViewValueAnalysis parent) : base(parent)
		{
		}
	}
}
