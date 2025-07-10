//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCClassificationLevyRateValidation
//
//    This class should be used for overriding validation in AutoNZCClassificationLevyRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCClassificationLevyRateValidation : AutoNZCClassificationLevyRateValidation
	{
		public NZCClassificationLevyRateValidation(AutoNZCClassificationLevyRate parent)
			: base(parent)
		{
		}
	}
}
