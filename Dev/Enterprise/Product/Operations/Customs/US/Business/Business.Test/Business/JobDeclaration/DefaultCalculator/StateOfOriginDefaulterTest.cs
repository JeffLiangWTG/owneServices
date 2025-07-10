using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class StateOfOriginDefaulterTest : TestCaseWithFactory
	{
		public void TestDefault()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "USLAX";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			var address2 = org.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "AUSYD";
			var address3 = org.Addresses.AddNew();
			address3.OA_State = USStatesList.Codes.Colorado;

			var declaration = Factory.New<JobDeclaration>();

			DummyBusinessObject obj = Factory.New<DummyBusinessObject>();
			StateOfOriginDefaulter.Default(obj.Z0_FK_CodeInfo, null);
			AssertEquals(ZString.Empty, obj.Z0_FK_Code);

			StateOfOriginDefaulter.Default(obj.Z0_FK_CodeInfo, declaration.SupplierPickupAddress);
			AssertEquals(ZString.Empty, obj.Z0_FK_Code);

			declaration.SupplierPickupAddress.E2_OA_Address = org.MainAddress.PK;
			StateOfOriginDefaulter.Default(obj.Z0_FK_CodeInfo, declaration.SupplierPickupAddress);
			AssertEquals(USStatesList.Codes.California, obj.Z0_FK_Code);

			declaration.SupplierPickupAddress.E2_OA_Address = address3.PK;
			StateOfOriginDefaulter.Default(obj.Z0_FK_CodeInfo, declaration.SupplierPickupAddress);
			AssertEquals(USStatesList.Codes.Colorado, obj.Z0_FK_Code);

			declaration.SupplierPickupAddress.E2_OA_Address = address2.PK;
			StateOfOriginDefaulter.Default(obj.Z0_FK_CodeInfo, declaration.SupplierPickupAddress);
			AssertEquals(USStatesList.Codes.Colorado, obj.Z0_FK_Code);

			var address4 = org.Addresses.AddNew();
			address4.OA_RL_NKRelatedPortCode = "MX2NB";
			address4.OA_State = "AGU";
			declaration.SupplierPickupAddress.E2_OA_Address = address4.PK;
			StateOfOriginDefaulter.Default(obj.Z0_FK_CodeInfo, declaration.SupplierPickupAddress);
			AssertEquals("AG", obj.Z0_FK_Code);

			declaration.SupplierPickupAddress.E2_AddressOverride = true;
			declaration.SupplierPickupAddress.E2_RN_NKCountryCode = "US";
			declaration.SupplierPickupAddress.E2_State = USStatesList.Codes.Tennessee;
			StateOfOriginDefaulter.Default(obj.Z0_FK_CodeInfo, declaration.SupplierPickupAddress);
			AssertEquals("TN", obj.Z0_FK_Code);
		}
	}
}
