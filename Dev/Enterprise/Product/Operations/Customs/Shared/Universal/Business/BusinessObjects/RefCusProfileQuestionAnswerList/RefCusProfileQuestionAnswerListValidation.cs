//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusProfileQuestionAnswerListValidation
//
//    This class should be used for overriding validation in AutoRefCusProfileQuestionAnswerListValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileQuestionAnswerListValidation : AutoRefCusProfileQuestionAnswerListValidation
	{
		public RefCusProfileQuestionAnswerListValidation(AutoRefCusProfileQuestionAnswerList parent) : base(parent)
		{
		}
	}
}
