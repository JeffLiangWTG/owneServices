//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCConcessionDutyRateValidation
//
//    This class should be used for overriding validation in AutoNZCConcessionDutyRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCConcessionDutyRateValidation : AutoNZCConcessionDutyRateValidation
	{
		public NZCConcessionDutyRateValidation(AutoNZCConcessionDutyRate parent)
			: base(parent)
		{
		}
	}
}
