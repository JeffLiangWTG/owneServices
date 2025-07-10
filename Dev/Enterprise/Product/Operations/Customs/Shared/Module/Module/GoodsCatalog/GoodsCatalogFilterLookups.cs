using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class GoodsCatalogFilterLookups : CommonFilterLookups
	{
		public GoodsCatalogFilterLookups(GoodsCatalogFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		protected new GoodsCatalogFilterBusinessObject FilterBizObj => (GoodsCatalogFilterBusinessObject)base.FilterBizObj;

		public CodeDescriptionPairList TypeList => GoodsCatalog.Lookups.TypeList;

		public CodeDescriptionPairList StatusTypeList => GoodsCatalog.Lookups.StatusTypeList;

		protected override CodeDescriptionPairList GetMainMessageStatusList() => GoodsCatalog.Lookups.MessageStatusList;

		BaseCusGoodsCatalog GoodsCatalog => goodsCatalog ??= Factory.GetNull<BaseCusGoodsCatalog>();
		BaseCusGoodsCatalog goodsCatalog;
	}
}
