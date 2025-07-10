//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusProfileQuestionAttributeValidation
//
//    This class should be used for overriding validation in AutoRefCusProfileQuestionAttributeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileQuestionAttributeValidation : AutoRefCusProfileQuestionAttributeValidation
	{
		public RefCusProfileQuestionAttributeValidation(AutoRefCusProfileQuestionAttribute parent) : base(parent)
		{
		}
	}
}
