//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHRJobApplicationDocumentValidation
//
//    This class should be used for overriding validation in AutoHRJobApplicationDocumentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationDocumentValidation : AutoHRJobApplicationDocumentValidation
	{
		public HRJobApplicationDocumentValidation(AutoHRJobApplicationDocument parent) : base(parent)
		{
		}
	}
}
