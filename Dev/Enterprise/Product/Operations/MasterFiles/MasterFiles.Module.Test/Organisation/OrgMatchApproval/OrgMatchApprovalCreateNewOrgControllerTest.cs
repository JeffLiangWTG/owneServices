using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgMatchApprovalCreateNewOrgControllerTest : TestCaseWithDummyOrgMatchApproval
	{
		public void TestShowNewForm_CommitsToParentRecord()
		{
			Factory.Save();
			OrgMatchApprovalCreateNewOrgController controller = new OrgMatchApprovalCreateNewOrgController();
			controller.UnmatchedOrgMatchApproval = DummyMatchApproval;

			using (ZOrganisationsForm form = (ZOrganisationsForm)controller.ShowNewForm())
			{
				OrgHeader newConsigneeOrg = (OrgHeader)form.BusinessEntity;
				newConsigneeOrg.OH_Code = "splaty";
				form.BusinessEntity.Factory.Save();
			}

			AssertEquals("New consignee organisation should be committed to the parent record (by calling OrgMatchApproval.CommitMatch)", "match committed", DummyParent.Z0_Description);
		}

		public void TestShowNewForm_IsInEditMode_SaveButtonEnabled()
		{
			Factory.Save();
			OrgMatchApprovalCreateNewOrgController controller = new OrgMatchApprovalCreateNewOrgController();
			controller.UnmatchedOrgMatchApproval = DummyMatchApproval;

			using (ZOrganisationsForm form = (ZOrganisationsForm)controller.ShowNewForm())
			{
				AssertEquals(ODisplayMode.Edit, form.DisplayMode);
			}
		}
	}
}
