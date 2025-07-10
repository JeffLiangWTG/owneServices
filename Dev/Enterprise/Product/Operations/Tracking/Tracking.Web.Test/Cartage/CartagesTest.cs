using System.Collections.Generic;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Cartage;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class CartagesTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.Cartages;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new CartagesForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebCartageModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebCartageView; }
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Env.Licence.WebTrackerLocalTransport }; }
		}

		class CartagesForTest : Cartages
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}
	}
}
