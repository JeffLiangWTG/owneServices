//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCClassificationValidation
//
//    This class should be used for overriding validation in AutoNZCClassificationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCClassificationValidation : AutoNZCClassificationValidation
	{
		public NZCClassificationValidation(AutoNZCClassification parent)
			: base(parent)
		{
		}
	}
}
