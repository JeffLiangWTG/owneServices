//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusProfileQuestionValidation
//
//    This class should be used for overriding validation in AutoRefCusProfileQuestionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileQuestionValidation : AutoRefCusProfileQuestionValidation
	{
		public RefCusProfileQuestionValidation(AutoRefCusProfileQuestion parent) : base(parent)
		{
		}
	}
}
