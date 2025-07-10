using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class CommodityFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CommodityFetchStrategy(EnterpriseBusinessObject businessObject)
			: base(businessObject)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, BusinessObject.PK);
		}
	}
}
