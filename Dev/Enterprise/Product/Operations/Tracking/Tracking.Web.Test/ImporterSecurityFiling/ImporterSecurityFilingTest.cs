using System.Collections.Generic;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ImporterSecurityFilingTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.ImporterSecurityFiling;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new ImporterSecurityFilingForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebISFModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebISFView; }
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Env.Licence.ImporterSecurityFiling }; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			foreach (OrgSecurityContacts securityContact in TestPage.SiteUser.LoggedInUser.SecurityRightsForBindingOnly)
			{
				if (securityContact.Security.SecurityKey == WebSecurityRightsList.WebISFView.Code)
				{
					securityContact.OZ_Granted = true;
					break;
				}
			}
		}

		class ImporterSecurityFilingForTest : ImporterSecurityFiling.ImporterSecurityFiling
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}
	}
}
