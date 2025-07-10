using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	[HttpContextEnabledTest]
	sealed class EditBillOfLadingTest : BillOfLadingBasePageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.EditBillOfLading;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new EditFwdInstructionForTest();
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebLinerAndAgencyFwdInstructionsEdit; }
		}

		class EditFwdInstructionForTest : EditFwdInstruction
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}
	}
}
