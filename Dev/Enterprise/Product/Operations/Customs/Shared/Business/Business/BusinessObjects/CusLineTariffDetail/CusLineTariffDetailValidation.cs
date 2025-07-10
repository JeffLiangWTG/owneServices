//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusLineTariffDetailValidation
//
//    This class should be used for overriding validation in AutoCusLineTariffDetailValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusLineTariffDetailValidation : AutoCusLineTariffDetailValidation
	{
		public CusLineTariffDetailValidation(AutoCusLineTariffDetail parent)
			: base(parent)
		{
		}
	}
}
