//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCTariffDutyRateValidation
//
//    This class should be used for overriding validation in AutoUSCTariffDutyRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCTariffDutyRateValidation : AutoUSCTariffDutyRateValidation
	{
		public USCTariffDutyRateValidation(AutoUSCTariffDutyRate parent)
			: base(parent)
		{
		}
	}
}
