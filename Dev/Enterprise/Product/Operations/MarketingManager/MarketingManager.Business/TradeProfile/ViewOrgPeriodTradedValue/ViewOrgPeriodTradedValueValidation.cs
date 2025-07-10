//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewOrgPeriodTradedValueValidation
//
//    This class should be used for overriding validation in AutoViewOrgPeriodTradedValueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MarketingManager.Business
{
	public class ViewOrgPeriodTradedValueValidation : AutoViewOrgPeriodTradedValueValidation
	{
		public ViewOrgPeriodTradedValueValidation(AutoViewOrgPeriodTradedValue parent) : base(parent)
		{
		}
	}
}
