using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class EditWarehouseReceiveTest : WarehousingPageBaseTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.EditWarehouseReceive;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new EditWarehouseReceiveForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebWarehouseReceiptsModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebWarehouseReceiptsAddEdit; }
		}
	}
}
