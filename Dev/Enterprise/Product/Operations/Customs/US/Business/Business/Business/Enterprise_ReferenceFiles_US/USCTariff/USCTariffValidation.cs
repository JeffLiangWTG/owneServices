//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCTariffValidation
//
//    This class should be used for overriding validation in AutoUSCTariffValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCTariffValidation : AutoUSCTariffValidation
	{
		public USCTariffValidation(AutoUSCTariff parent)
			: base(parent)
		{
		}
	}
}
