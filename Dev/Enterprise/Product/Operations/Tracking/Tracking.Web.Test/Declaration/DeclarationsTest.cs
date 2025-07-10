using System.Collections.Generic;
using System.Web.UI;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class DeclarationsTest : BasePageWithAuthorisationTest
	{
		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints => new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerImportBrokerage, Environment.Env.Licence.WebTrackerExportBrokerage };

		protected override BooleanRegistryItem UseWebModule => WebDataRegistry.Instance.UseWebDeclarationModule;

		protected override WebSecurityRight SiteUserSecurityRight => WebSecurityRightsList.WebDeclarationView;

		protected override string GetExpectedPageName() => WebTracker.Pages.Declarations;

		protected override Control GetNewControl() => new DeclarationsWithGetNewTestGlobal();
	}

	class DeclarationsWithGetNewTestGlobal : Declarations
	{
		protected override ZGlobal GetNewTestGlobal() => new TestGlobal();
	}
}
