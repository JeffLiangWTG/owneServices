using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class EditWarehouseOrderAssignDefaultWarehouseTest : AssignDefaultWarehouseTest
	{
		public override WhsDocket GetDocketWithAssignedDefaultWarehouse()
		{
			using (EditWarehouseOrderForTest page = new EditWarehouseOrderForTest())
			{
				page.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				return ((TrackingWhsOrder)page.GetNewDataSourceForTest()).WhsOrder;
			}
		}

		class EditWarehouseOrderForTest : EditWarehouseOrder
		{
			public BusinessObject GetNewDataSourceForTest()
			{
				return GetNewDataSource();
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}
	}
}
