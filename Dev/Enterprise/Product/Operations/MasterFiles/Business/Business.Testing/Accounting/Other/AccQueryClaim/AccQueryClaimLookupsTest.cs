using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class AccQueryClaimLookupsTest : BusinessObjectLookupsTestCase
	{
		AccQueryClaim fQueryClaim;
		public AccQueryClaim QueryClaim
		{
			get
			{
				if (fQueryClaim == null)
				{
					fQueryClaim = CreateBusinessObjectForTest();
				}
				return fQueryClaim;
			}
		}

		public abstract AccQueryClaim CreateBusinessObjectForTest();

		public void TestHoldOptions()
		{
			Assert(QueryClaim.Lookups.HoldOptions.Count == 2);
			AssertEquals("DNM, ALM", QueryClaim.Lookups.HoldOptions.CodesAsString);
		}

		public void TestDebtors()
		{
			AssertNotNull("Debtors collection should not be null", QueryClaim.Lookups.Debtors);
			Assert("Debtors collection type", QueryClaim.Lookups.Debtors is DebtorCollection);
		}

		public void TestCreditors()
		{
			AssertNotNull("Creditors collection should not be null", QueryClaim.Lookups.Creditors);
			Assert("Creditors collection type", QueryClaim.Lookups.Creditors is CreditorCollection);
		}

		public void TestContacts()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContactDependentCollection orgContacts = org.Contacts;
			OrgContact contact1 = Factory.New<OrgContact>();
			OrgContact contact2 = Factory.New<OrgContact>();

			org.Contacts.Add(contact1);
			org.Contacts.Add(contact2);

			QueryClaim.AY_OH_Debtor = org.PK;
			QueryClaim.Lookups.Contacts.Load();

			bool result = TestContactBelongsToOrg(orgContacts);
			Assert("Contact Test OK", result);

			contact1 = Factory.New<OrgContact>();
			contact2 = Factory.New<OrgContact>();

			OrgHeader org1 = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "FHTRAN");
			org1.Contacts.Load();

			QueryClaim.AY_OH_Debtor = org1.PK;

			result = TestContactBelongsToOrg(orgContacts);
			Assert("Contact did change when changing Debtor", !result);
		}

		public bool TestContactBelongsToOrg(OrgContactDependentCollection orgContacts)
		{
			bool result = true;
			for (int i = 0; i < orgContacts.Count; i++)
			{
				if (!(orgContacts.Contains(QueryClaim.Lookups.Contacts[i].PK)))
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public void TestBranches()
		{
			QueryClaim.Lookups.Branches.Load();

			ZQuery branchFilter = new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
			GlbBranchCollection collection = new GlbBranchCollection(Factory, branchFilter);
			collection.Load();

			Assert("Precondition Current Company has at least one Branch", collection.Count > 0);

			bool result = true;
			for (int i = 0; i < QueryClaim.Lookups.Branches.Count; i++)
			{
				if (!(QueryClaim.Lookups.Branches.Contains(collection[i])))
				{
					result = false;
					break;
				}
			}
			Assert("Test Branches is OK", result);
		}

		public void TestClaimReason()
		{
			AssertNotNull("QueryClaim Not Null", QueryClaim.Lookups.ClaimReason);
			Assert("Claim Reason should have at least one element", QueryClaim.Lookups.ClaimReason.Count > 0);
		}

		public void TestClaimStatus()
		{
			AssertNotNull("QueryClaim Not Null", QueryClaim.Lookups.ClaimStatus);
			Assert("Claim Reason should have at least one element", QueryClaim.Lookups.ClaimStatus.Count > 0);
		}

		public abstract void TestClaimStatusLookups();

		public void TestClaimType()
		{
			AssertNotNull("QueryClaim Not Null", QueryClaim.Lookups.ClaimType);
			Assert("Claim Reason should have at least one element", QueryClaim.Lookups.ClaimType.Count > 0);
		}

		public void TestStaff()
		{
			AssertNotNull("Staff collection should not be null", QueryClaim.Lookups.Staff);
			Assert("Staff collection type", QueryClaim.Lookups.Staff is GlbStaffCollection);
		}
	}
}
