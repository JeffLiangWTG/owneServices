//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTariffAdditionalCodeViewValidation
//
//    This class should be used for overriding validation in AutoTariffAdditionalCodeViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class TariffAdditionalCodeViewValidation : AutoTariffAdditionalCodeViewValidation
	{
		public TariffAdditionalCodeViewValidation(AutoTariffAdditionalCodeView parent)
			: base(parent)
		{
		}
	}
}
