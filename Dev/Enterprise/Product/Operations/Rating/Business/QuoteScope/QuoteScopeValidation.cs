//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoQuoteScopeValidation
//
//    This class should be used for overriding validation in AutoQuoteScopeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise
{
	public class QuoteScopeValidation : AutoQuoteScopeValidation
	{
		public QuoteScopeValidation(AutoQuoteScope parent) : base(parent)
		{
		}
	}
}
