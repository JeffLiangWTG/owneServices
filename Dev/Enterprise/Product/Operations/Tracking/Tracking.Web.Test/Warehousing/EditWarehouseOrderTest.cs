using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class EditWarehouseOrderTest : WarehousingPageBaseTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.EditWarehouseOrder;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new EditWarehouseOrderForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebWarehouseOrdersModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebWarehouseOrdersAddEdit; }
		}
	}
}
