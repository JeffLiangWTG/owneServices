using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAirlineBranchAccount))]
	sealed class OrgAirlineBranchAccountTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidationAndLookups()
		{
			var airlineBranchAccount = Factory.New<OrgAirlineBranchAccount>();
			AssertEquals(typeof(OrgAirlineBranchAccountValidation), airlineBranchAccount.Validation.GetType());
			AssertEquals(typeof(OrgAirlineBranchAccountLookups), airlineBranchAccount.Lookups.GetType());
		}

		public void TestGetReadOnlySecurity()
		{
			var orgAirlineBranchAccount = Factory.NewWithValidTestData<DummyOrgAirlineBranchAccount>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);

			security.OrgCarrierModifyAir.IsAllowed = true;
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				orgAirlineBranchAccount.OAA_OH_Carrier = ZGuid.Empty;
				AssertEquals("When Carrier is null, readonly should return false.", false, orgAirlineBranchAccount.GetReadOnlySecurityForTesting);

				orgAirlineBranchAccount.OAA_OH_Carrier = carrier.PK;
				AssertEquals("When Carrier is defined but not in the database, readonly should return false.", false, orgAirlineBranchAccount.GetReadOnlySecurityForTesting);

				Factory.Save();
				AssertEquals("When Carrier is defined and in the database and OrgCarrierModifyAir is allowed, readonly should return false.", false, orgAirlineBranchAccount.GetReadOnlySecurityForTesting);
			}

			orgAirlineBranchAccount = Factory.NewWithValidTestData<DummyOrgAirlineBranchAccount>();
			carrier = Factory.NewWithValidTestData<OrgHeader>();
			security.OrgCarrierModifyAir.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				orgAirlineBranchAccount.OAA_OH_Carrier = ZGuid.Empty;
				AssertEquals("When Carrier is null, readonly should return false.", false, orgAirlineBranchAccount.GetReadOnlySecurityForTesting);

				orgAirlineBranchAccount.OAA_OH_Carrier = carrier.PK;
				AssertEquals("When Carrier is defined but not in the database, readonly should return false.", false, orgAirlineBranchAccount.GetReadOnlySecurityForTesting);

				Factory.Save();
				AssertEquals("When Carrier is defined and in the database and OrgCarrierModifyAir is not allowed, readonly should return true.", true, orgAirlineBranchAccount.GetReadOnlySecurityForTesting);
			}
		}
	}
}
