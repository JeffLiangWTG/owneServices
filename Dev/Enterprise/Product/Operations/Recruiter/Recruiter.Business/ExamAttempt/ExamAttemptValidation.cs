//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExamAttemptValidation
//
//    This class should be used for overriding validation in AutoExamAttemptValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Recruiter.Business
{
	public class ExamAttemptValidation : AutoExamAttemptValidation
	{
		public ExamAttemptValidation(AutoExamAttempt parent) : base(parent)
		{
		}
	}
}
