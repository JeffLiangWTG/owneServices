using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(CusSeaManTranHead), "SlotCharterers")]
	public class CusSeaManSlotOrg : AutoCusSeaManSlotOrg
	{
		public CusSeaManSlotOrg(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusSeaManSlotOrgTypeDecider TypeDecider = new CusSeaManSlotOrgTypeDecider();

		#region Fetch Strategy

		protected class CusSeaManSlotOrgFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public CusSeaManSlotOrgFetchStrategy(CusSeaManSlotOrg slot)
				: base(slot)
			{
				this.slot = slot;
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(OrgHeaderSchema.PK, slot.BS_OH_SlotCharterer);
			}

			readonly CusSeaManSlotOrg slot;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusSeaManSlotOrgFetchStrategy(this);
		}

		#endregion

		#region Related Business Objects

		public CusSeaManTranHead Header
		{
			get { return Factory.Load<CusSeaManTranHead>(BS_BT); }
		}

		#endregion
	}
}
