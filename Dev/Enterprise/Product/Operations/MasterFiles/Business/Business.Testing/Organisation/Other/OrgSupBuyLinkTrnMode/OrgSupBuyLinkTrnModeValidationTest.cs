using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSupBuyLinkTrnModeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPF_RS_NKDefaultServiceLevel()
		{
			LinkMode.PF_RS_NKDefaultServiceLevel = "ABC";
			AssertHasErrors(LinkMode.PF_RS_NKDefaultServiceLevelInfo);

			LinkMode.PF_RS_NKDefaultServiceLevel = "A";
			AssertHasErrors(LinkMode.PF_RS_NKDefaultServiceLevelInfo);

			LinkMode.PF_RS_NKDefaultServiceLevel = "12";
			AssertHasErrors(LinkMode.PF_RS_NKDefaultServiceLevelInfo);

			var refServiceLevel = Factory.New<RefServiceLevel>();
			refServiceLevel.RS_Code = "ABB";
			refServiceLevel.RS_IsActive = false;
			LinkMode.PF_RS_NKDefaultServiceLevel = "ABB";
			AssertHasErrors(LinkMode.PF_RS_NKDefaultServiceLevelInfo);

			LinkMode.PF_RS_NKDefaultServiceLevel = null;
			AssertNoErrors(LinkMode.PF_RS_NKDefaultServiceLevelInfo);

			refServiceLevel.RS_Code = "AAA";
			refServiceLevel.RS_IsActive = true;
			LinkMode.PF_RS_NKDefaultServiceLevel = "AAA";
			AssertNoErrors(LinkMode.PF_RS_NKDefaultServiceLevelInfo);
		}

		public void TestCheckPF_RL_NKPlaceOfDeliveryPort()
		{
			LinkMode.PF_RL_NKPlaceOfDeliveryPort = "abc";
			AssertHasErrors(LinkMode.PF_RL_NKPlaceOfDeliveryPortInfo);

			LinkMode.PF_RL_NKPlaceOfDeliveryPort = "AUSYD";
			AssertNoErrors(LinkMode.PF_RL_NKPlaceOfDeliveryPortInfo);
		}

		public void TestCheckPF_RL_NKPlaceOfReceivalPort()
		{
			LinkMode.PF_RL_NKPlaceOfReceivalPort = "abc";
			AssertHasErrors(LinkMode.PF_RL_NKPlaceOfReceivalPortInfo);

			LinkMode.PF_RL_NKPlaceOfReceivalPort = "AUSYD";
			AssertNoErrors(LinkMode.PF_RL_NKPlaceOfReceivalPortInfo);
		}

		public void TestCheckPF_ContainerMode()
		{
			LinkMode.PF_ContainerMode = Constants.ContainerModes.FCL;
			AssertNoErrors("Container Mode should be valid", LinkMode.PF_ContainerModeInfo);

			LinkMode.PF_ContainerMode = "";
			AssertNoErrors("Container Mode should be valid", LinkMode.PF_ContainerModeInfo);

			LinkMode.PF_ContainerMode = "XXX";
			AssertHasErrors("Container Mode should NOT be valid", LinkMode.PF_ContainerModeInfo);
		}

		public void TestCheckPF_TransportMode()
		{
			LinkMode.PF_TransportMode = Constants.TransportModes.Air;
			AssertNoErrors("Transport Mode should be valid", LinkMode.PF_TransportModeInfo);

			LinkMode.PF_TransportMode = "XXX";
			AssertHasErrors("Transport Mode should NOT be valid", LinkMode.PF_TransportModeInfo);
		}

		public void TestCheckPF_IncoTerm()
		{
			LinkMode.PF_IncoTerm = "DDD";
			LinkMode.Validation.ValidatePF_IncoTerm();
			Assert("Company is Consignee, invalid code, has errors", LinkMode.PF_IncoTermInfo.HasErrors());

			LinkMode.PF_IncoTerm = "FOB";
			LinkMode.Validation.ValidatePF_IncoTerm();
			Assert("Company is Consignee, valid code, no errors", !LinkMode.PF_IncoTermInfo.HasErrors());

			LinkMode.PF_IncoTerm = "DAT";
			LinkMode.Validation.ValidatePF_IncoTerm();
			AssertEquals("Incoterm 'DAT' should have warning", true, LinkMode.PF_IncoTermInfo.HasWarning("This Incoterm is obsolete from 1 January 2020 according to the International Chamber of Commerce rules."));
		}

		public void TestCheckPF_IncoTermMode()
		{
			LinkMode.PF_IncoTermMode = "";
			AssertNoErrors("Empty code, no errors", LinkMode.PF_IncoTermModeInfo);

			LinkMode.PF_IncoTermMode = OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.THS;
			AssertNoErrors("Valid code, no errors", LinkMode.PF_IncoTermModeInfo);

			LinkMode.PF_IncoTermMode = "ABC";
			AssertHasErrors("Invalid code, has errors", LinkMode.PF_IncoTermModeInfo);
		}

		public void TestDuplicateMode()
		{
			LinkMode.PF_TransportMode = "AIR";
			LinkMode.PF_ContainerMode = LinkMode.Lookups.ContainerModeList[0].Code;

			OrgSupBuyLinkTrnMode linkMode2 = SupplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();
			linkMode2.PF_TransportMode = "SEA";
			linkMode2.PF_ContainerMode = linkMode2.Lookups.ContainerModeList[1].Code;

			Assert("Link should have no errors", !LinkMode.HasRowErrors);
			Assert("Link2 should have no errors", !linkMode2.HasRowErrors);

			linkMode2.PF_TransportMode = "AIR";
			linkMode2.PF_ContainerMode = linkMode2.Lookups.ContainerModeList[1].Code;

			Assert("Link should have no errors", !LinkMode.HasRowErrors);
			Assert("Link2 should have no errors", !linkMode2.HasRowErrors);

			linkMode2.PF_ContainerMode = linkMode2.Lookups.ContainerModeList[0].Code;

			Assert("Link should have no errors", !LinkMode.HasRowErrors);
			Assert("Link2 should have errors", linkMode2.HasRowErrors);
		}

		public void TestCheckPF_OH_ControllingCustomer_EnableControllingCustomerFunctionalityAndValidationsIsTrue()
		{
			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var controllingCustomerOrg = Factory.New<OrgHeader>();
			LinkMode.PF_OH_ControllingCustomer = controllingCustomerOrg.PK;
			controllingCustomerOrg.OH_IsControllingCustomer = false;

			LinkMode.Validation.ValidatePF_OH_ControllingCustomer();
			AssertHasError(LinkMode.PF_OH_ControllingCustomerInfo, "Only an organization flagged as Controlling Customer can be used as a Controlling Customer.");

			controllingCustomerOrg.OH_IsControllingCustomer = true;
			LinkMode.Validation.ValidatePF_OH_ControllingCustomer();
			AssertNoErrors(LinkMode.PF_OH_ControllingCustomerInfo);
		}

		public void TestCheckPF_OH_ControllingCustomer_EnableControllingCustomerFunctionalityAndValidationsIsFalse()
		{
			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var controllingCustomerOrg = Factory.New<OrgHeader>();
			LinkMode.PF_OH_ControllingCustomer = controllingCustomerOrg.PK;
			controllingCustomerOrg.OH_IsControllingCustomer = false;

			LinkMode.Validation.ValidatePF_OH_ControllingCustomer();
			AssertNoErrors(LinkMode.PF_OH_ControllingCustomerInfo);

			controllingCustomerOrg.OH_IsControllingCustomer = true;
			LinkMode.Validation.ValidatePF_OH_ControllingCustomer();
			AssertNoErrors(LinkMode.PF_OH_ControllingCustomerInfo);
		}

		public void TestCheckPF_OC_OverrideNotifyParty()
		{
			var notifyParty = Factory.New<OrgHeader>();
			var contact = notifyParty.Contacts.AddNew();

			LinkMode.PF_OA_OverrideNotifyPartyAddress = ZGuid.Invalid;
			LinkMode.PF_OC_OverrideNotifyParty = ZGuid.Empty;
			LinkMode.Validation.ValidatePF_OC_OverrideNotifyParty();
			AssertNoErrors(LinkMode.PF_OC_OverrideNotifyPartyInfo);

			LinkMode.PF_OA_OverrideNotifyPartyAddress = ZGuid.Empty;
			LinkMode.PF_OC_OverrideNotifyParty = ZGuid.Empty;
			LinkMode.Validation.ValidatePF_OC_OverrideNotifyParty();
			AssertNoErrors(LinkMode.PF_OC_OverrideNotifyPartyInfo);

			LinkMode.PF_OA_OverrideNotifyPartyAddress = notifyParty.MainAddress.PK;
			LinkMode.PF_OC_OverrideNotifyParty = ZGuid.Empty;
			LinkMode.Validation.ValidatePF_OC_OverrideNotifyParty();
			AssertHasError(LinkMode.PF_OC_OverrideNotifyPartyInfo, "Please enter a Notify Party Contact.");

			LinkMode.PF_OA_OverrideNotifyPartyAddress = ZGuid.Invalid;
			LinkMode.PF_OC_OverrideNotifyParty = contact.PK;
			LinkMode.Validation.ValidatePF_OC_OverrideNotifyParty();
			AssertNoErrors(LinkMode.PF_OC_OverrideNotifyPartyInfo);

			LinkMode.PF_OA_OverrideNotifyPartyAddress = ZGuid.Empty;
			LinkMode.PF_OC_OverrideNotifyParty = contact.PK;
			LinkMode.Validation.ValidatePF_OC_OverrideNotifyParty();
			AssertNoErrors(LinkMode.PF_OC_OverrideNotifyPartyInfo);

			LinkMode.PF_OA_OverrideNotifyPartyAddress = notifyParty.MainAddress.PK;
			LinkMode.PF_OC_OverrideNotifyParty = contact.PK;
			LinkMode.Validation.ValidatePF_OC_OverrideNotifyParty();
			AssertNoErrors(LinkMode.PF_OC_OverrideNotifyPartyInfo);

			notifyParty.Contacts.RemoveAndDeleteAll();
			LinkMode.PF_OC_OverrideNotifyParty = ZGuid.Empty;
			LinkMode.Validation.ValidatePF_OC_OverrideNotifyParty();
			AssertNoErrors(LinkMode.PF_OC_OverrideNotifyPartyInfo);
		}

		#region Implementation

		OrgSupBuyLinkTrnMode LinkMode;
		OrgSupplierBuyerLink SupplierBuyerLink;
		OrgHeader Supplier;
		OrgHeader Buyer;

		protected override void SetUp()
		{
			base.SetUp();
			Buyer = Factory.New<OrgHeader>();
			Buyer.OH_IsConsignee = true;
			Supplier = Factory.New<OrgHeader>();
			Supplier.OH_RL_NKClosestPort = "AUSYD";
			Supplier.OH_IsConsignor = true;

			SupplierBuyerLink = Buyer.SupplierLinks.AddNew();
			LinkMode = SupplierBuyerLink.OrgSupBuyLinkTrnModes[0];
		}

		#endregion
	}
}
