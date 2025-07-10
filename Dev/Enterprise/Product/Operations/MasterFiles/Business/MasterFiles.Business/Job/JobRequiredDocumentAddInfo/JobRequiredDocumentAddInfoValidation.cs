//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobRequiredDocumentAddInfoValidation
//
//    This class should be used for overriding validation in AutoJobRequiredDocumentAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class JobRequiredDocumentAddInfoValidation : AutoJobRequiredDocumentAddInfoValidation
	{
		public JobRequiredDocumentAddInfoValidation(AutoJobRequiredDocumentAddInfo parent) : base(parent)
		{
		}
	}
}
