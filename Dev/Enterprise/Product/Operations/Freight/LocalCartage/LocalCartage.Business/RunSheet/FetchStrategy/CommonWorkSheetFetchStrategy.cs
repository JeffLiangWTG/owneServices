using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonWorkSheetFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CommonWorkSheetFetchStrategy(CommonWorkSheet runSheet)
			: base(runSheet)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(GlbStaff), GlbStaffSchema.GS_Code, RunSheet.EY_GS_NKTruckDriver);
			Factory.AddFetchHint(typeof(OrgHeader), OrgHeaderSchema.PK, RunSheet.EY_OH_TransportCo);
			Factory.AddFetchHint(typeof(RefEquipment), RefEquipmentSchema.PK, RunSheet.EY_RQ_Truck);

			Factory.AddFetchHint(typeof(CommonCartageLeg), JobContainerLegsSchema.JU_EY_RunSheet, RunSheet.PK);
			Factory.AddFetchHint(typeof(StmALog), StmALogSchema.SL_Parent, RunSheet.PK);
		}

		CommonWorkSheet RunSheet
		{
			get { return (CommonWorkSheet)BusinessObject; }
		}
	}
}
