//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVoyCountryLookups
//
//    This class should be used for overriding collections in AutoJobVoyCountryLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business
{
	public class BaseJobVoyCountryLookups : JobVoyCountryLookups
	{
		public BaseJobVoyCountryLookups(AutoJobVoyCountry parent) : base(parent)
		{
		}
	}
}
