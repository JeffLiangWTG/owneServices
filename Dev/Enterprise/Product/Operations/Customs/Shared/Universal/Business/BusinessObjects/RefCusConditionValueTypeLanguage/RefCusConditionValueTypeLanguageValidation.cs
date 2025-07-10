//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusConditionValueTypeLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusConditionValueTypeLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusConditionValueTypeLanguageValidation : AutoRefCusConditionValueTypeLanguageValidation
	{
		public RefCusConditionValueTypeLanguageValidation(AutoRefCusConditionValueTypeLanguage parent) : base(parent)
		{
		}
	}
}
