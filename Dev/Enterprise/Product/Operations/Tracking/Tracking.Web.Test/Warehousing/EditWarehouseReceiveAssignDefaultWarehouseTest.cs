using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class EditWarehouseReceiveAssignDefaultWarehouseTest : AssignDefaultWarehouseTest
	{
		public override WhsDocket GetDocketWithAssignedDefaultWarehouse()
		{
			using (EditWarehouseReceiveForAssignDefaultWarehouseTest page = new EditWarehouseReceiveForAssignDefaultWarehouseTest())
			{
				page.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				return ((TrackingWhsReceive)page.GetNewDataSourceForTest()).WhsReceive;
			}
		}
	}
}
