using CargoWise.Definitions;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDocketReferenceValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestValidateWX_RefType

		public void TestValidateWX_RefType()
		{
			TestCodePairList(Reference.WX_RefTypeInfo, ErrorCheckType.HasErrors, false, Reference.Lookups.ReferenceTypes);
		}

		public void TestValidateWX_RefType_TPCShouldBeUnique()
		{
			var order = Factory.New<WhsOrder>();
			var tranportBillTo = Helper.CreateClient("BILLER");
			order.TransportBillToDocAddress.E2_AddressOverride = false;
			order.TransportBillToDocAddress.OrganisationPK = tranportBillTo.PK;

			var references = order.References;
			var reference1 = references.AddNew();
			reference1.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.OrderTypeCode;
			reference1.WX_Reference = "AD1234";
			var reference2 = references.AddNew();
			reference2.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber;
			reference2.WX_Reference = "123";
			AssertNoErrors(reference2.WX_RefTypeInfo);
			var reference3 = references.AddNew();
			reference3.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber;
			reference3.WX_Reference = "456";
			AssertHasError(reference3.WX_RefTypeInfo, "Duplicate TPC Reference Type.");
			references.RemoveAndDelete(reference2);
			reference3.Validation.ValidateAll();
			AssertNoErrors(reference3.WX_RefTypeInfo);
		}

		public void TestValidateWX_RefType_TPCRequiresTransportBillToAddress()
		{
			var order = Factory.New<WhsOrder>();
			Assert(order.TransportBillToDocAddress.IsEmpty);

			var references = order.References;
			var reference = references.AddNew();
			reference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber;
			reference.WX_Reference = "AD1234";
			AssertHasError(reference.WX_RefTypeInfo, "Third party carrier account number requires a transport bill to address.");

			var tranportBillTo = Helper.CreateClient("BILLER");
			order.TransportBillToDocAddress.E2_AddressOverride = false;
			order.TransportBillToDocAddress.OrganisationPK = tranportBillTo.PK;

			Assert(!order.TransportBillToDocAddress.IsEmpty);
			reference.Validation.ValidateAll();
			AssertNoErrors(reference.WX_RefTypeInfo);
		}

		public void TestValidateWX_RefType_TPCRequiresTransportBillToAddress_OverridenAddress()
		{
			var order = Factory.New<WhsOrder>();
			Assert(order.TransportBillToDocAddress.IsEmpty);

			var references = order.References;
			var reference = references.AddNew();
			reference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber;
			reference.WX_Reference = "AD1234";
			AssertHasError(reference.WX_RefTypeInfo, "Third party carrier account number requires a transport bill to address.");

			order.TransportBillToDocAddress.E2_AddressOverride = true;
			order.TransportBillToDocAddress.E2_CompanyName = "Test Company";
			order.TransportBillToDocAddress.E2_Address1 = "Test Address";
			order.TransportBillToDocAddress.E2_City = "Sydney";
			order.TransportBillToDocAddress.E2_State = "NSW";
			order.TransportBillToDocAddress.E2_RN_NKCountryCode = "AU";

			Assert(!order.TransportBillToDocAddress.IsEmpty);
			reference.Validation.ValidateAll();
			AssertNoErrors(reference.WX_RefTypeInfo);
		}

		#endregion

		#region TestValidateWX_Reference

		public void TestValidateWX_Reference_OrderTypeCode()
		{
			TestMandatoryString(Reference.WX_ReferenceInfo, ErrorCheckType.HasErrors);

			Reference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.OrderTypeCode;
			Reference.WX_Reference = "A";
			AssertHasError(Reference.WX_ReferenceInfo, "Order Type Code reference must be six characters.");

			Reference.WX_Reference = "AD1234";
			AssertNoErrors(Reference.WX_ReferenceInfo);

			Reference.WX_Reference = "AD12345";
			AssertHasError(Reference.WX_ReferenceInfo, "Order Type Code reference must be six characters.");

			Reference.WX_RefType = "";
			Reference.WX_Reference = "A";
			AssertNoErrors(Reference.WX_ReferenceInfo);
		}

		public void TestValidateWX_Reference_ThirdPartyCarrierAccount()
		{
			TestMandatoryString(Reference.WX_ReferenceInfo, ErrorCheckType.HasErrors);

			Reference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber;
			Reference.WX_Reference = "12345678901";
			AssertHasError(Reference.WX_ReferenceInfo, "Third party carrier account number cannot be more than 10 characters.");

			Reference.WX_Reference = "1234567890";
			AssertNoErrors(Reference.WX_ReferenceInfo);

			Reference.WX_Reference = "";
			AssertHasError(Reference.WX_ReferenceInfo, "Please enter a Reference.");

			Reference.WX_RefType = "";
			Reference.WX_Reference = "1";
			AssertNoErrors(Reference.WX_ReferenceInfo);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Reference = Factory.New<WhsDocketReference>();
		}
		protected WhsDocketReference Reference;

		#endregion
	}
}
