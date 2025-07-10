//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobDocumentDataValidation
//
//    This class should be used for overriding validation in AutoJobDocumentDataValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentDataValidation : AutoJobDocumentDataValidation
	{
		public JobDocumentDataValidation(AutoJobDocumentData parent) : base(parent)
		{
		}
	}
}
