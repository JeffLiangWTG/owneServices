//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCVisaTariffValidation
//
//    This class should be used for overriding validation in AutoUSCVisaTariffValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCVisaTariffValidation : AutoUSCVisaTariffValidation
	{
		public USCVisaTariffValidation(AutoUSCVisaTariff parent)
			: base(parent)
		{
		}
	}
}
