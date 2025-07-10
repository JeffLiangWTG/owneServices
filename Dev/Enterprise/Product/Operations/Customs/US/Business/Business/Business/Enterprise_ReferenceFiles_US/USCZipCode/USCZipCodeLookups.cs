//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCZipCodeLookups
//
//    This class should be used for overriding collections in AutoUSCZipCodeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCZipCodeLookups : AutoUSCZipCodeLookups
	{
		public USCZipCodeLookups(AutoUSCZipCode parent)
			: base(parent)
		{
		}
	}
}
