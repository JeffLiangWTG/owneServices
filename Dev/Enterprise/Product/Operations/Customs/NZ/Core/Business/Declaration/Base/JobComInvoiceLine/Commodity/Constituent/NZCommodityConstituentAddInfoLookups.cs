//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCommodityConstituentAddInfoLookups
//
//    This class should be used for overriding collections in AutoNZCommodityConstituentAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCommodityConstituentAddInfoLookups : AutoNZCommodityConstituentAddInfoLookups
	{
		public NZCommodityConstituentAddInfoLookups(AutoNZCommodityConstituentAddInfo parent) : base(parent)
		{
		}
	}
}
