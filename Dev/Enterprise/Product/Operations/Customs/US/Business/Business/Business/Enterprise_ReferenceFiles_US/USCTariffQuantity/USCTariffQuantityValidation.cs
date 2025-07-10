//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCTariffQuantityValidation
//
//    This class should be used for overriding validation in AutoUSCTariffQuantityValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCTariffQuantityValidation : AutoUSCTariffQuantityValidation
	{
		public USCTariffQuantityValidation(AutoUSCTariffQuantity parent)
			: base(parent)
		{
		}
	}
}
