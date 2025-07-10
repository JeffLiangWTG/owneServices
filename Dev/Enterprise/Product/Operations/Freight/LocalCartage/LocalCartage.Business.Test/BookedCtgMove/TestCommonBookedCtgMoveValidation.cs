using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	internal class TestCommonBookedCtgMoveValidation : BusinessObjectValidationTestCase
	{
		public void TestEW_F3_NKPackType()
		{
			var move = Factory.New<CommonBookedCtgMove>();
			move.EW_F3_NKPackType = "";
			AssertEquals(true, move.EW_F3_NKPackTypeInfo.HasErrors());
			move.EW_F3_NKPackType = "PLT";
			AssertEquals(false, move.EW_F3_NKPackTypeInfo.HasErrors());
			move.EW_F3_NKPackType = "XXX";
			AssertEquals(false, move.EW_F3_NKPackTypeInfo.HasErrors());
		}

		public void TestEW_RequestedPickupTimeStart()
		{
			ZDateTime now = ZDateTime.Now;
			CommonBookedCtgMove move = Factory.New<CommonBookedCtgMove>();
			move.EW_RequestedPickupTimeEnd = now;
			move.EW_RequestedPickupTimeStart = now.AddDays(-1);
			Assert("Before: Should be no errors", !move.EW_RequestedPickupTimeStartInfo.HasErrors());
			move.EW_RequestedPickupTimeStart = now;
			Assert("Same: Should be no errors", !move.EW_RequestedPickupTimeStartInfo.HasErrors());
			move.EW_RequestedPickupTimeStart = now.AddDays(1);
			Assert("After: Should be error", move.EW_RequestedPickupTimeStartInfo.HasErrors());
		}

		public void TestEW_RequestedPickupTimeEnd()
		{
			ZDateTime now = ZDateTime.Now;
			CommonBookedCtgMove move = Factory.New<CommonBookedCtgMove>();
			move.EW_RequestedPickupTimeStart = now;
			move.EW_RequestedPickupTimeEnd = now.AddDays(1);
			Assert("After: Should be no errors", !move.EW_RequestedPickupTimeEndInfo.HasErrors());
			move.EW_RequestedPickupTimeEnd = now;
			Assert("Same: Should be no errors", !move.EW_RequestedPickupTimeEndInfo.HasErrors());
			move.EW_RequestedPickupTimeEnd = now.AddDays(-1);
			Assert("Before: Should be error", move.EW_RequestedPickupTimeEndInfo.HasErrors());
		}

		public void TestEW_RequestedDeliveryTimeStart()
		{
			ZDateTime now = ZDateTime.Now;
			CommonBookedCtgMove move = Factory.New<CommonBookedCtgMove>();
			move.EW_RequestedDeliveryTimeEnd = now;
			move.EW_RequestedDeliveryTimeStart = now.AddDays(-1);
			Assert("Before: Should be no errors", !move.EW_RequestedDeliveryTimeStartInfo.HasErrors());
			move.EW_RequestedDeliveryTimeStart = now;
			Assert("Same: Should be no errors", !move.EW_RequestedDeliveryTimeStartInfo.HasErrors());
			move.EW_RequestedDeliveryTimeStart = now.AddDays(1);
			Assert("After: Should be error", move.EW_RequestedDeliveryTimeStartInfo.HasErrors());
		}

		public void TestEW_RequestedDeliveryTimeEnd()
		{
			ZDateTime now = ZDateTime.Now;
			CommonBookedCtgMove move = Factory.New<CommonBookedCtgMove>();
			move.EW_RequestedDeliveryTimeStart = now;
			move.EW_RequestedDeliveryTimeEnd = now.AddDays(1);
			Assert("After: Should be no errors", !move.EW_RequestedDeliveryTimeEndInfo.HasErrors());
			move.EW_RequestedDeliveryTimeEnd = now;
			Assert("Same: Should be no errors", !move.EW_RequestedDeliveryTimeEndInfo.HasErrors());
			move.EW_RequestedDeliveryTimeEnd = now.AddDays(-1);
			Assert("Before: Should be error", move.EW_RequestedDeliveryTimeEndInfo.HasErrors());
		}

		public void TestEW_E2PickupAddressID()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.RefreshAddresses();
			JobDocAddress address1 = cartage.DocAddresses[0];
			address1.E2_AddressOverride = true;
			address1.E2_Address1 = "Address1";
			JobDocAddress address2 = cartage.DocAddresses[1];
			address2.E2_AddressOverride = true;
			address2.E2_Address1 = "Address2";
			JobDocAddress address3 = cartage.DocAddresses[2];
			address3.E2_AddressOverride = true;
			address3.E2_Address1 = "Address3";
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.EW_E2PickupAddressID = address1.PK;
			move.EW_E2WaitPointAddressID = address2.PK;
			move.EW_E2DeliveryAddressID = address3.PK;
			move.Validation.ValidateEW_E2PickupAddressID();
			AssertNoErrors(move.EW_E2PickupAddressIDInfo);
			move.EW_E2PickupAddressID = address2.PK;
			Assert(move.EW_E2PickupAddressIDInfo.HasError("First Rating Party cannot be the same as Second Rating Party"));
			move.EW_E2PickupAddressID = address1.PK;
			move.EW_E2WaitPointAddressID = address2.PK;
			move.EW_E2DeliveryAddressID = address1.PK;
			move.Validation.ValidateEW_E2PickupAddressID();
			AssertNoErrors(move.EW_E2PickupAddressIDInfo);
			move.EW_E2WaitPointAddressID = ZGuid.Empty;
			AssertNoErrors(move.EW_E2PickupAddressIDInfo);
			move.EW_E2PickupAddressID = ZGuid.Empty;
			Assert(move.EW_E2PickupAddressIDInfo.HasWarning("First Rating Party should have a value if Second or Third Rating Party does."));
		}

		public void TestEW_E2WaitPointAddressID()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.RefreshAddresses();
			JobDocAddress address1 = cartage.DocAddresses[0];
			address1.E2_AddressOverride = true;
			address1.E2_Address1 = "Address1";
			JobDocAddress address2 = cartage.DocAddresses[1];
			address2.E2_AddressOverride = true;
			address2.E2_Address1 = "Address2";
			JobDocAddress address3 = cartage.DocAddresses[2];
			address3.E2_AddressOverride = true;
			address3.E2_Address1 = "Address3";
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.EW_E2PickupAddressID = address1.PK;
			move.EW_E2WaitPointAddressID = address2.PK;
			move.EW_E2DeliveryAddressID = address3.PK;
			move.Validation.ValidateEW_E2WaitPointAddressID();
			AssertNoErrors(move.EW_E2WaitPointAddressIDInfo);
			move.EW_E2WaitPointAddressID = address1.PK;
			Assert(move.EW_E2WaitPointAddressIDInfo.HasError("Second Rating Party cannot be the same as First or Third Rating Party"));
			move.EW_E2WaitPointAddressID = address3.PK;
			Assert(move.EW_E2WaitPointAddressIDInfo.HasError("Second Rating Party cannot be the same as First or Third Rating Party"));
			move.EW_E2WaitPointAddressID = ZGuid.Empty;
			Assert(move.EW_E2WaitPointAddressIDInfo.HasWarning("Second Rating Party should have a value if Third Rating Party does."));
			move.EW_E2DeliveryAddressID = ZGuid.Empty;
			move.Validation.ValidateEW_E2WaitPointAddressID();
			AssertNoErrors(move.EW_E2WaitPointAddressIDInfo);
		}

		public void TestEW_E2DeliveryAddressID()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.RefreshAddresses();
			JobDocAddress address1 = cartage.DocAddresses[0];
			address1.E2_AddressOverride = true;
			address1.E2_Address1 = "Address1";
			JobDocAddress address2 = cartage.DocAddresses[1];
			address2.E2_AddressOverride = true;
			address2.E2_Address1 = "Address2";
			JobDocAddress address3 = cartage.DocAddresses[2];
			address3.E2_AddressOverride = true;
			address3.E2_Address1 = "Address3";
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.EW_E2PickupAddressID = address1.PK;
			move.EW_E2WaitPointAddressID = address2.PK;
			move.EW_E2DeliveryAddressID = address3.PK;
			move.Validation.ValidateEW_E2DeliveryAddressID();
			AssertNoErrors(move.EW_E2DeliveryAddressIDInfo);
			move.EW_E2DeliveryAddressID = address2.PK;
			Assert(move.EW_E2DeliveryAddressIDInfo.HasError("Third Rating Party cannot be the same as Second Rating Party"));
		}
	}
}
