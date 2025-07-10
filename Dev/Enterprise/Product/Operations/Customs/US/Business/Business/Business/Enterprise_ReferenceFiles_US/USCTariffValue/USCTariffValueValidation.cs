//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCTariffValueValidation
//
//    This class should be used for overriding validation in AutoUSCTariffValueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCTariffValueValidation : AutoUSCTariffValueValidation
	{
		public USCTariffValueValidation(AutoUSCTariffValue parent)
			: base(parent)
		{
		}
	}
}
