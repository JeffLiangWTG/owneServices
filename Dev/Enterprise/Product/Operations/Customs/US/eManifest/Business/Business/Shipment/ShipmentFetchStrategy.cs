using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class ShipmentFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public ShipmentFetchStrategy(EnterpriseBusinessObject businessObject)
			: base(businessObject)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(Commodity), CusInBondCargoDescSchema.BY_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
		}
	}
}
