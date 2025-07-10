//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCGoldPriceValidation
//
//    This class should be used for overriding validation in AutoUSCGoldPriceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCGoldPriceValidation : AutoUSCGoldPriceValidation
	{
		public USCGoldPriceValidation(AutoUSCGoldPrice parent)
			: base(parent)
		{
		}
	}
}
