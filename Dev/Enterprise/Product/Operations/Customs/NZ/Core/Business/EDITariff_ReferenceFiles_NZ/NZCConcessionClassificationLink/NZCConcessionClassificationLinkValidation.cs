//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCConcessionClassificationLinkValidation
//
//    This class should be used for overriding validation in AutoNZCConcessionClassificationLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCConcessionClassificationLinkValidation : AutoNZCConcessionClassificationLinkValidation
	{
		public NZCConcessionClassificationLinkValidation(AutoNZCConcessionClassificationLink parent)
			: base(parent)
		{
		}
	}
}
