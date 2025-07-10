using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCarrierAccount))]
	sealed class OrgCarrierAccountTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOrgCarrierAccount()
		{
			var carrier = Factory.New<OrgHeader>();
			var billToParty = Factory.New<OrgHeader>();
			var can1 = Factory.New<OrgCarrierAccount>();

			can1.OAN_AccountNumber = "ACCNO1";
			can1.OAN_DepotID = "DEPID1";
			can1.OAN_MerchantNumber = "MERCNO1";
			can1.OAN_OH_Carrier = carrier.PK;

			AssertEquals("ACCNO1", carrier.CarrierAccounts[0].OAN_AccountNumber);
			AssertEquals("DEPID1", carrier.CarrierAccounts[0].OAN_DepotID);
			AssertEquals("MERCNO1", carrier.CarrierAccounts[0].OAN_MerchantNumber);
		}

		public void TestCarrierName()
		{
			var carrier = NewTestHeaderForTests();
			var can1 = Factory.New<OrgCarrierAccount>();
			can1.OAN_OH_Carrier = carrier.PK;
			AssertEquals(carrier.PK, can1.Carrier.PK);
			AssertEquals(carrier.OH_FullName, can1.Carrier.OH_FullName);
		}

		public void TestOAN_OH_Carrier()
		{
			var carrier = NewTestHeaderForTests();
			var can1 = Factory.New<OrgCarrierAccount>();
			can1.OAN_OH_Carrier = carrier.PK;
			AssertEquals("Precondition", carrier.PK, can1.OAN_OH_Carrier);
			AssertEquals(carrier.PK, can1.OAN_OH_BillToParty);
		}

		public void TestGetReadOnlySecurity()
		{
			var orgCarrierAccount = Factory.NewWithValidTestData<DummyOrgCarrierAccount>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var security = new SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);

			security.OrgCarrierModify.IsAllowed = true;
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				orgCarrierAccount.OAN_OH_Carrier = ZGuid.Empty;
				AssertEquals("When Carrier is null, readonly should return false.", false, orgCarrierAccount.GetReadOnlySecurityForTesting);

				orgCarrierAccount.OAN_OH_Carrier = carrier.PK;
				AssertEquals("When Carrier is defined but not in the database, readonly should return false.", false, orgCarrierAccount.GetReadOnlySecurityForTesting);

				Factory.Save();
				AssertEquals("When Carrier is defined and in the database and OrgCarrierModify is allowed, readonly should return false.", false, orgCarrierAccount.GetReadOnlySecurityForTesting);
			}

			orgCarrierAccount = Factory.NewWithValidTestData<DummyOrgCarrierAccount>();
			carrier = Factory.NewWithValidTestData<OrgHeader>();
			security.OrgCarrierModify.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				orgCarrierAccount.OAN_OH_Carrier = ZGuid.Empty;
				AssertEquals("When Carrier is null, readonly should return false.", false, orgCarrierAccount.GetReadOnlySecurityForTesting);

				orgCarrierAccount.OAN_OH_Carrier = carrier.PK;
				AssertEquals("When Carrier is defined but not in the database, readonly should return false.", false, orgCarrierAccount.GetReadOnlySecurityForTesting);

				Factory.Save();
				AssertEquals("When Carrier is defined and in the database and OrgCarrierModify is not allowed, readonly should return true.", true, orgCarrierAccount.GetReadOnlySecurityForTesting);
			}
		}

		#region Implementation

		OrgHeader NewTestHeaderForTests()
		{
			var headerForTest = Factory.New<OrgHeader>();
			headerForTest.OH_FullName = "Test Org 1 2 3";
			headerForTest.OH_RL_NKClosestPort = "AUSYD";
			headerForTest.MainAddress.OA_Address1 = "26 Test Street";
			headerForTest.MainAddress.OA_City = "Prospect";
			headerForTest.OH_Code = "Test123";
			headerForTest.OH_IsShippingProvider = ZBool.True;
			headerForTest.OH_IsAirCTO = ZBool.True;
			headerForTest.OH_IsSeaCTO = ZBool.True;
			headerForTest.OH_IsRailHead = ZBool.True;
			headerForTest.OH_IsRoadFreightDepot = ZBool.True;
			headerForTest.Factory.Save();
			return headerForTest;
		}

		class DummyOrgCarrierAccount : OrgCarrierAccount
		{
			public DummyOrgCarrierAccount(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool GetReadOnlySecurityForTesting => GetReadOnlySecurity(null);
		}

		#endregion
	}
}
