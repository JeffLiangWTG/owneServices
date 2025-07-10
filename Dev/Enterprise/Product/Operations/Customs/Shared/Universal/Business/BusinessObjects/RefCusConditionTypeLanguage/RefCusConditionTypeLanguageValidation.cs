//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusConditionTypeLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusConditionTypeLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusConditionTypeLanguageValidation : AutoRefCusConditionTypeLanguageValidation
	{
		public RefCusConditionTypeLanguageValidation(AutoRefCusConditionTypeLanguage parent) : base(parent)
		{
		}
	}
}
