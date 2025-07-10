using CargoWise.EntityFramework;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageLegFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CommonCartageLegFetchStrategy(CommonCartageLeg leg)
			: base(leg)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(CommonWorkSheet), Leg.JU_EY_RunSheet);
			Factory.AddFetchHint(typeof(CommonBookedCtgMove), Leg.JU_EW);
			Factory.AddFetchHint(typeof(GPSSupporterActivity), LocalCartageVehicleActivitySchema.EN_JU, Leg.PK);
			Factory.AddFetchHint(typeof(GenCustomAddOnValue), GenCustomAddOnValueSchema.XV_ParentID, Leg.PK);
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, Leg.PK);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			// Tested in Leg planner Filter control
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(typeof(JobDocAddress), Leg.JU_E2PickupAddressID);
			Factory.AddFetchHint(typeof(JobDocAddress), Leg.JU_E2WaitPointAddressID);
			Factory.AddFetchHint(typeof(JobDocAddress), Leg.JU_E2DeliveryAddressID);
			Factory.AddFetchHint(typeof(StmALog), StmALogSchema.SL_Parent, Leg.PK);
		}

		CommonCartageLeg Leg
		{
			get { return (CommonCartageLeg)BusinessObject; }
		}
	}
}
