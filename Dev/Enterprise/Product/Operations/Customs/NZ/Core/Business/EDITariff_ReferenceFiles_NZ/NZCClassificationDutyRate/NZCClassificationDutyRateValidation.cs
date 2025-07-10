//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCClassificationDutyRateValidation
//
//    This class should be used for overriding validation in AutoNZCClassificationDutyRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCClassificationDutyRateValidation : AutoNZCClassificationDutyRateValidation
	{
		public NZCClassificationDutyRateValidation(AutoNZCClassificationDutyRate parent)
			: base(parent)
		{
		}
	}
}
