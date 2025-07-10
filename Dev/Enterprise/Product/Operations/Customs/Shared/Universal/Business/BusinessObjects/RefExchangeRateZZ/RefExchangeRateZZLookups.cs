//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefExchangeRateZZLookups
//
//    This class should be used for overriding collections in AutoRefExchangeRateZZLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefExchangeRateZZLookups : AutoRefExchangeRateZZLookups
	{
		public RefExchangeRateZZLookups(AutoRefExchangeRateZZ parent) : base(parent)
		{
		}
	}
}
