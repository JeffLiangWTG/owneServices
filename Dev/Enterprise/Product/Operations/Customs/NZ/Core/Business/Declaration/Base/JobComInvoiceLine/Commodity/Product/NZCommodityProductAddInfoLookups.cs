//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCommodityProductAddInfoLookups
//
//    This class should be used for overriding collections in AutoNZCommodityProductAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCommodityProductAddInfoLookups : AutoNZCommodityProductAddInfoLookups
	{
		public NZCommodityProductAddInfoLookups(AutoNZCommodityProductAddInfo parent) : base(parent)
		{
		}

		public ProductNameTypeList NameTypeList
		{
			get { return Factory.GetCachedValue<ProductNameTypeList>(); }
		}

		public IdentityTypeList IDTypeList
		{
			get { return Factory.GetCachedValue<IdentityTypeList>(); }
		}
	}
}
