using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDocAddressAddressOnlySynchroniserTest : TestCaseWithFactory
	{
		public void TestSynchroniser()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			var declaration = Factory.New<BaseJobDeclaration>();
			var supplierDoc = declaration.SupplierDocumentaryAddress;
			supplierDoc.E2_AddressOverride = ZBool.True;

			var synchroniser = new JobDocAddressAddressOnlySynchroniser(supplierDoc, (ZPropertyInfoGuid)consol.JK_OA_ShippingLineAddressInfo);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise(true);
			AssertEquals(true, supplierDoc.ReadOnly);
			AssertEquals(false, supplierDoc.E2_AddressOverride);
			AssertEquals(ZGuid.Empty, supplierDoc.E2_OA_Address);

			consol.JK_OA_ShippingLineAddress = org1.MainAddress.PK;
			AssertEquals(true, supplierDoc.ReadOnly);
			AssertEquals(false, supplierDoc.E2_AddressOverride);
			AssertEquals(org1.MainAddress.PK, supplierDoc.E2_OA_Address);

			consol.JK_OA_ShippingLineAddress = org2.MainAddress.PK;
			AssertEquals(true, supplierDoc.ReadOnly);
			AssertEquals(false, supplierDoc.E2_AddressOverride);
			AssertEquals(org2.MainAddress.PK, supplierDoc.E2_OA_Address);

			synchroniser.SetEnabled(false, false);
			consol.JK_OA_ShippingLineAddress = org1.MainAddress.PK;
			AssertEquals(false, supplierDoc.ReadOnly);
			AssertEquals(false, supplierDoc.E2_AddressOverride);
			AssertEquals(org2.MainAddress.PK, supplierDoc.E2_OA_Address);
		}
	}
}
