//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVoyageExRateLookups
//
//    This class should be used for overriding collections in AutoJobVoyageExRateLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business
{
	public class BaseJobVoyageExRateLookups : JobVoyageExRateLookups
	{
		public BaseJobVoyageExRateLookups(AutoJobVoyageExRate parent) : base(parent)
		{
		}
	}
}
