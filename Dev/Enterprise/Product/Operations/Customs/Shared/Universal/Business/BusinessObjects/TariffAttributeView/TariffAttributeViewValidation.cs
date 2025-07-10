//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTariffAttributeViewValidation
//
//    This class should be used for overriding validation in AutoTariffAttributeViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class TariffAttributeViewValidation : AutoTariffAttributeViewValidation
	{
		public TariffAttributeViewValidation(AutoTariffAttributeView parent) : base(parent)
		{
		}
	}
}
