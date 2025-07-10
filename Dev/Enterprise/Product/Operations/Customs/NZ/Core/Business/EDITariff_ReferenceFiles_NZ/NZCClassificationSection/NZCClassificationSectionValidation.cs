//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCClassificationSectionValidation
//
//    This class should be used for overriding validation in AutoNZCClassificationSectionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCClassificationSectionValidation : AutoNZCClassificationSectionValidation
	{
		public NZCClassificationSectionValidation(AutoNZCClassificationSection parent)
			: base(parent)
		{
		}
	}
}
