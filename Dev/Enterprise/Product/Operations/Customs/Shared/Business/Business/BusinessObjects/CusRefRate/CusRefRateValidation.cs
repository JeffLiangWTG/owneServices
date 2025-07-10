//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefRateValidation
//
//    This class should be used for overriding validation in AutoCusRefRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusRefRateValidation : AutoCusRefRateValidation
	{
		public CusRefRateValidation(AutoCusRefRate parent) : base(parent)
		{
		}
	}
}
