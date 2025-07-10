//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCClassificationChapterValidation
//
//    This class should be used for overriding validation in AutoNZCClassificationChapterValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCClassificationChapterValidation : AutoNZCClassificationChapterValidation
	{
		public NZCClassificationChapterValidation(AutoNZCClassificationChapter parent)
			: base(parent)
		{
		}
	}
}
