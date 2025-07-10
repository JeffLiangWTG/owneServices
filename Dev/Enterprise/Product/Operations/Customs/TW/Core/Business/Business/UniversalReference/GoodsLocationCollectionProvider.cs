
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class GoodsLocationCollectionProvider : CollectionProviderWithCodeSupport, Integration.Customs.TW.ITWGoodsLocationCollectionProvider
	{
		public GoodsLocationCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			var factory = new BusinessObjectFactory();
			return TWRefCusCodeListTypes.GetGoodsLocationCollection(factory, ZString.Empty);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;

		public override int MaxLength => ZZRefCusCodeListSchema.ZZD_Code.MaxLength;
	}
}
