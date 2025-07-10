//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusConditionLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusConditionLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusConditionLanguageValidation : AutoRefCusConditionLanguageValidation
	{
		public RefCusConditionLanguageValidation(AutoRefCusConditionLanguage parent) : base(parent)
		{
		}
	}
}
