//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusCodeListAttributeNameLanguageLookups
//
//    This class should be used for overriding collections in AutoRefCusCodeListAttributeNameLanguageLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusCodeListAttributeNameLanguageLookups : AutoRefCusCodeListAttributeNameLanguageLookups
	{
		public RefCusCodeListAttributeNameLanguageLookups(AutoRefCusCodeListAttributeNameLanguage parent) : base(parent)
		{
		}
	}
}
