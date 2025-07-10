//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCommodityAddInfoLookups
//
//    This class should be used for overriding collections in AutoNZCommodityAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class NZCommodityAddInfoLookups : AutoNZCommodityAddInfoLookups
	{
		public NZCommodityAddInfoLookups(AutoNZCommodityAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ClassTypeList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("NZ NZCommodityAddInfoLookups ClassTypeList", delegate
				{
					classTypeList = new ClassificationTypeList();
					classTypeList.RemoveCode(ClassificationTypeList.Codes.HS);
					classTypeList.RemoveCode(ClassificationTypeList.Codes.CV);
					classTypeList.RemoveCode(ClassificationTypeList.Codes.SSO);
					return classTypeList;
				});
			}
		}
		ClassificationTypeList classTypeList;
	}
}
