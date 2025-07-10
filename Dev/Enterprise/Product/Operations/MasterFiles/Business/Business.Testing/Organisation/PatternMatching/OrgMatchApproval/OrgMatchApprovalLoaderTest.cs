using System.Collections;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgMatchApprovalLoaderTest : TestCaseWithDummyOrgMatchApproval
	{
		public void TestLoadAll()
		{
			OrgMatchApproval matchApproval1 = Loader.LoadOrCreate(DummyParent.PK, OrgMatchApprovalType.DummyType);
			OrgMatchApproval matchApproval2 = Loader.LoadOrCreate(DummyParent.PK, OrgMatchApprovalType.DummyType2);

			OrgMatchApproval[] allMatchApprovalsAroundParent = Loader.LoadAll(DummyParent.PK);
			AssertEquals("There should be 2 match approvals attached to the parent", 2, allMatchApprovalsAroundParent.Length);
			AssertEquals("The correct match approvals should be returned", true, ((IList)allMatchApprovalsAroundParent).Contains(matchApproval1));
			AssertEquals("The correct match approvals should be returned", true, ((IList)allMatchApprovalsAroundParent).Contains(matchApproval2));
		}

		public void TestLoad()
		{
			OrgMatchApproval createdMatchApproval = Loader.LoadOrCreate(DummyParent.PK, OrgMatchApprovalType.DummyType);
			OrgMatchApproval loadedMatchApproval = Loader.Load(DummyParent.PK, OrgMatchApprovalType.DummyType);

			AssertEquals("Loaded match approval should be the same as the one created", createdMatchApproval.PK, loadedMatchApproval.PK);
		}

		public void TestLoadOrCreate()
		{
			OrgMatchApproval createdMatchApproval = Loader.LoadOrCreate(DummyParent.PK, OrgMatchApprovalType.DummyType);
			OrgMatchApproval createdMatchApproval2 = Loader.LoadOrCreate(DummyParent.PK, OrgMatchApprovalType.DummyType2);
			OrgMatchApproval loadedMatchApproval = Loader.LoadOrCreate(DummyParent.PK, OrgMatchApprovalType.DummyType);
			OrgMatchApproval loadedMatchApproval2 = Loader.LoadOrCreate(DummyParent.PK, OrgMatchApprovalType.DummyType2);

			AssertEquals("Loaded match approval should be the same as the one created", createdMatchApproval.PK, loadedMatchApproval.PK);
			AssertEquals("Loaded match approval should be the same as the one created", createdMatchApproval2.PK, loadedMatchApproval2.PK);
			Assert("Different organisation type codes should load different organisation match approval records", createdMatchApproval.PK != loadedMatchApproval2.PK);

			AssertEquals("P2_ParentID should be set correctly when created", DummyParent.PK, createdMatchApproval.AddressToBeMatched.P3_ParentID);
			AssertEquals("P2_ParentTableCode should be set correctly when created", "P3", createdMatchApproval.P2_ParentTableCode);
			AssertEquals("P2_MatchType should be set correctly when created", OrgMatchApprovalType.DummyType.Code, createdMatchApproval.P2_MatchType);

			AssertEquals("AddressToBeMatched.P3_AddressType should be set correctly when created", OrgMatchApprovalType.DummyType.Code, createdMatchApproval.AddressToBeMatched.P3_AddressType);
			AssertEquals("AddressToBeMatched.P3_ParentTableCode should be set correctly when created", "Z0", createdMatchApproval.AddressToBeMatched.P3_ParentTableCode);
			AssertEquals("Address details should be copied to AddressToBeMatched so they can be easily filtered on a filter screen", "OwnerCode", createdMatchApproval.AddressToBeMatched.P3_Code);
		}

		public void TestLoadOrCreate_WithPreCreatedAddress()
		{
			DummyMatchApproval.Delete();

			OrgPatternMatchAddress preCreatedAddress = Factory.New<OrgPatternMatchAddress>();
			preCreatedAddress.P3_ParentID = DummyParent.PK;
			OrgMatchApproval matchApproval = Loader.LoadOrCreate(preCreatedAddress, OrgMatchApprovalType.DummyType);

			AssertEquals(
				"The created OrgMatchApproval record should be linked to our pre-created OrgPatternMatchAddressRecord",
				preCreatedAddress.PK, matchApproval.AddressToBeMatched.PK);
			AssertEquals("OrgMatchApproval.Parent should be correctly linked", DummyParent.PK, matchApproval.Parent.PK);
		}
	}
}
