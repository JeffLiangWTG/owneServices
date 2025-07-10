using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusGoodsCatalogLookups : AutoCusGoodsCatalogLookups
	{
		public CusGoodsCatalogLookups(AutoCusGoodsCatalog parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList TypeList => Factory.GetCachedValue<GoodsCatalogTypeList>();

		public override OrgHeaderCollection Owners => new ConsigneeCollection(Factory);

		public virtual CodeDescriptionPairList StatusTypeList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<MessageStatusList>();
	}
}
