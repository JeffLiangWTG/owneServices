using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgMatchApprovalNewOrgCreatorTest : TestCaseWithDummyOrgMatchApproval
	{
		public void TestSetOrgDetailsAndApproveMatchOnSave()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			OrgMatchApprovalNewOrgCreator creator = new OrgMatchApprovalNewOrgCreator(DummyMatchApproval, newOrganisation);
			creator.SetOrgDetailsAndApproveMatchOnSave();
			AssertEquals("Defaults should be set on the new organisation", "Organisation CompanyName", newOrganisation.OH_FullName);

			Factory.Save();
			AssertEquals("OrgMatchApproval should be approved now", true, DummyMatchApproval.IsApproved);
		}

		public void TestCannotSaveExceptionThrownOnSaveIfMatchAlreadyApproved()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			DummyMatchApproval.Match(organisation.PK, "OP1");
			DummyMatchApproval.Match(organisation.PK, "OP2");

			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			OrgMatchApprovalNewOrgCreator creator = new OrgMatchApprovalNewOrgCreator(DummyMatchApproval, newOrganisation);
			creator.SetOrgDetailsAndApproveMatchOnSave();

			try
			{
				Factory.Save();
				Fail("Expected exception of type " + typeof(ZCannotSaveException).FullName + " so a message is shown to the user");
			}
			catch (ZCannotSaveException ex)
			{
				AssertEquals("The exception should be thrown for the right reason", true, ex.Message.IndexOf("already been approved") != -1);
			}
		}
	}
}
