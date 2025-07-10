using System.Collections.Generic;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class EditOrderTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.EditOrder;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new EditOrderForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebForwardingOrdersModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebOrdersAddEdit; }
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerOrderManager }; }
		}
	}
}
