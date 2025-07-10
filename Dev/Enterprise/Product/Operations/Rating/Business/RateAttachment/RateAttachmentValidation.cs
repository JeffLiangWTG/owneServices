//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRateAttachmentValidation
//
//    This class should be used for overriding validation in AutoRateAttachmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Rating.Business
{
	public class RateAttachmentValidation : AutoRateAttachmentValidation
	{
		public RateAttachmentValidation(AutoRateAttachment parent)
			: base(parent)
		{
		}
	}
}

