using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class BillJobDocAddressRequirementTest : TestCaseWithFactory
	{
		public void TestGetJobDocAddressRequirement()
		{
			var billRequirement = new BillJobDocAddressRequirement();
			var requirement = billRequirement.GetJobDocAddressRequirement(DocAddressType.BuyingParty);
			var requirement2 = billRequirement.GetJobDocAddressRequirement(DocAddressType.BuyingParty);
			AssertEquals(requirement, requirement2);
		}

		public void TestValidationForForeignShipper()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var shipper = bill.ForeignShipper;
			var message = @"US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			AssertDocAddress(shipper, true);
			var parentBill = (CusInBondBill)shipper.Parent;
			parentBill.ValidationModes = ValidationModes.InventoryRecord;
			var organisationPKRequiredMessage = ValidationConstants.JobDocAddress.GetOrganisationPKRequired(shipper.AddressCaption);
			shipper.E2_AddressOverride = true;
			CombineAssertions(() =>
			{
				shipper.E2_CompanyName = "COMPANYNAME ?WTG";
				AssertNoWarningContaining(shipper.E2_CompanyNameInfo, "Foreign Shipper Documentary Address: Company Name : " + message);
				shipper.E2_CompanyName = "COMPAN¢NAME ?WTG";
				AssertHasWarningContaining(shipper.E2_CompanyNameInfo, "Foreign Shipper Documentary Address: Company Name : " + message);
				shipper.E2_Address1 = "BOB THE BUILDER";
				AssertNoWarningContaining(shipper.E2_Address1Info, "Foreign Shipper Documentary Address: Address Line 1 : " + message);
				shipper.E2_Address1 = "BO*B ¢THE BUILDER";
				AssertHasWarningContaining(shipper.E2_Address1Info, "Foreign Shipper Documentary Address: Address Line 1 : " + message);
				shipper.E2_Address2 = "BOB THE BUILDER2";
				AssertNoWarningContaining(shipper.E2_Address2Info, "Foreign Shipper Documentary Address: Address Line 2 : " + message);
				shipper.E2_Address2 = "BO*B ¢THE BUILDER2";
				AssertHasWarningContaining(shipper.E2_Address2Info, "Foreign Shipper Documentary Address: Address Line 2 : " + message);
			});
		}

		public void TestValidationForConsignee()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var docAddress = bill.Consignee;
			var message = @"US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			AssertDocAddress(docAddress, true);
			var billConsignee = (CusInBondBill)docAddress.Parent;
			billConsignee.ValidationModes = ValidationModes.InventoryRecord;
			var organisationPKRequiredMessage = ValidationConstants.JobDocAddress.GetOrganisationPKRequired(docAddress.AddressCaption);
			docAddress.E2_AddressOverride = true;
			CombineAssertions(() =>
			{
				docAddress.E2_CompanyName = "COMPANYNAME ?WTG";
				AssertNoWarningContaining(docAddress.E2_CompanyNameInfo, "Consignee Address: Company Name : " + message);
				docAddress.E2_CompanyName = "COMP¢N*YNAME ?WTG";
				AssertHasWarningContaining(docAddress.E2_CompanyNameInfo, "Consignee Address: Company Name : " + message);
				docAddress.E2_Address1 = "BOB THE BUILDER";
				AssertNoWarningContaining(docAddress.E2_Address1Info, "Consignee Address: Address Line 1 : " + message);
				docAddress.E2_Address1 = "BO*B ¢THE BUILDER";
				AssertHasWarningContaining(docAddress.E2_Address1Info, "Consignee Address: Address Line 1 : " + message);
				docAddress.E2_Address2 = "BOB THE BUILDER2";
				AssertNoWarningContaining(docAddress.E2_Address2Info, "Consignee Address: Address Line 2 : " + message);
				docAddress.E2_Address2 = "BO*B ¢THE BUILDER2";
				AssertHasWarningContaining(docAddress.E2_Address2Info, "Consignee Address: Address Line 2 : " + message);
			});
		}

		public void TestValidationForNotifyParty1()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var notifyParty = bill.NotifyParty1;
			AssertDocAddress(notifyParty);
			var parentBill = (CusInBondBill)notifyParty.Parent;
			parentBill.ValidationModes = ValidationModes.InventoryRecord;
			var message = @"US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			var organisationPKRequiredMessage = ValidationConstants.JobDocAddress.GetOrganisationPKRequired(notifyParty.AddressCaption);
			notifyParty.E2_AddressOverride = true;
			CombineAssertions(() =>
			{
				notifyParty.E2_CompanyName = "COMPANYNAME ?WTG";
				AssertNoWarningContaining(notifyParty.E2_CompanyNameInfo, "Notify Party: Company Name : " + message);
				notifyParty.E2_CompanyName = "COMPAN¢YNAME ?WTG";
				AssertHasWarningContaining(notifyParty.E2_CompanyNameInfo, "Notify Party: Company Name : " + message);
				notifyParty.E2_Address1 = "BOB THE BUILDER";
				AssertNoWarningContaining(notifyParty.E2_Address1Info, "Notify Party: Address Line 1 : " + message);
				notifyParty.E2_Address1 = "BO*B ¢THE BUILDER";
				AssertHasWarningContaining(notifyParty.E2_Address1Info, "Notify Party: Address Line 1 : " + message);
				notifyParty.E2_Address2 = "BOB THE BUILDER2";
				AssertNoWarningContaining(notifyParty.E2_Address2Info, "Notify Party: Address Line 2 : " + message);
				notifyParty.E2_Address2 = "BO*B ¢THE BUILDER2";
				AssertHasWarningContaining(notifyParty.E2_Address2Info, "Notify Party: Address Line 2 : " + message);
			});
		}

		public void TestValidationForNotifyParty2()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var notifyParty = bill.NotifyParty2;
			AssertDocAddress(notifyParty);
			var parentBill = (CusInBondBill)notifyParty.Parent;
			parentBill.ValidationModes = ValidationModes.InventoryRecord;
			var message = @"US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			var organisationPKRequiredMessage = ValidationConstants.JobDocAddress.GetOrganisationPKRequired(notifyParty.AddressCaption);
			notifyParty.E2_AddressOverride = true;
			notifyParty.E2_AddressOverride = true;
			CombineAssertions(() =>
			{
				notifyParty.E2_CompanyName = "COMPANYNAME ?WTG";
				AssertNoWarningContaining(notifyParty.E2_CompanyNameInfo, "Notify Party 2: Company Name : " + message);
				notifyParty.E2_CompanyName = "COMPAN¢YNAME ?WTG";
				AssertHasWarningContaining(notifyParty.E2_CompanyNameInfo, "Notify Party 2: Company Name : " + message);
				notifyParty.E2_Address1 = "BOB THE BUILDER";
				AssertNoWarningContaining(notifyParty.E2_Address1Info, "Notify Party 2: Address Line 1 : " + message);
				notifyParty.E2_Address1 = "BO*B ¢THE BUILDER";
				AssertHasWarningContaining(notifyParty.E2_Address1Info, "Notify Party 2: Address Line 1 : " + message);
				notifyParty.E2_Address2 = "BOB THE BUILDER2";
				AssertNoWarningContaining(notifyParty.E2_Address2Info, "Notify Party 2: Address Line 2 : " + message);
				notifyParty.E2_Address2 = "BO*B ¢THE BUILDER2";
				AssertHasWarningContaining(notifyParty.E2_Address2Info, "Notify Party 2: Address Line 2 : " + message);
			});
		}

		public void TestValidationForCustomsBroker()
		{
			var org = Factory.New<OrgHeader>();
			var header = Factory.New<CusInBondHeader>();
			header.ValidationModes = ValidationModes.InventoryRecord;
			var bill = header.Bills.AddNew();
			var customsBroker = bill.CustomsBroker;
			AssertEquals("E2_GovRegNum", ZString.Empty, customsBroker.E2_GovRegNum);
			AssertEquals("E2_GovRegNumType", "DEF", customsBroker.E2_GovRegNumType);
			AssertNoMessageError(customsBroker.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.CustomsBrokerAddressRequiresABIRouting.ToString());
			customsBroker.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("E2_GovRegNum", ZString.Empty, customsBroker.E2_GovRegNum);
			AssertEquals("E2_GovRegNumType", OrgCusCode.USACodeTypes.ABIRoutingCode, customsBroker.E2_GovRegNumType);
			AssertHasMessageError(customsBroker.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.CustomsBrokerAddressRequiresABIRouting.ToString());
			header.ValidationModes = ValidationModes.SubsequentInBond;
			customsBroker.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("E2_GovRegNum", ZString.Empty, customsBroker.E2_GovRegNum);
			AssertEquals("E2_GovRegNumType", OrgCusCode.USACodeTypes.ABIRoutingCode, customsBroker.E2_GovRegNumType);
			AssertHasMessageError(customsBroker.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.CustomsBrokerAddressRequiresABIRouting.ToString());
			customsBroker.E2_OA_Address = ZGuid.Empty;
			AssertEquals("E2_GovRegNum", ZString.Empty, customsBroker.E2_GovRegNum);
			AssertEquals("E2_GovRegNumType", "DEF", customsBroker.E2_GovRegNumType);
			AssertNoMessageError(customsBroker.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.CustomsBrokerAddressRequiresABIRouting.ToString());
			org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ABIRoutingCode, "1234SV2", Core.Constants.CountryCodes.UnitedStates);
			customsBroker.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("E2_GovRegNum", "1234SV2", customsBroker.E2_GovRegNum);
			AssertEquals("E2_GovRegNumType", OrgCusCode.USACodeTypes.ABIRoutingCode, customsBroker.E2_GovRegNumType);
			AssertNoMessageError(customsBroker.E2_OA_AddressInfo, ValidationConstants.JobDocAddress.CustomsBrokerAddressRequiresABIRouting.ToString());
		}

		void AssertDocAddress(JobDocAddress docAddress, bool assertOrganisationPK = false)
		{
			var bill = (CusInBondBill)docAddress.Parent;
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var organisationPKRequiredMessage = ValidationConstants.JobDocAddress.GetOrganisationPKRequired(docAddress.AddressCaption);
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "BOB THE BUILDER";
			AssertNoMessageError(docAddress.E2_CompanyNameInfo, ValidationConstants.JobDocAddress.CompanyNameRequired.ToString());
			docAddress.E2_CompanyName = ZString.Empty;
			AssertHasMessageError(docAddress.E2_CompanyNameInfo, ValidationConstants.JobDocAddress.CompanyNameRequired.ToString());
			docAddress.E2_Address1 = "ADDRESS 1";
			AssertNoMessageError(docAddress.E2_Address1Info, ValidationConstants.JobDocAddress.AddressRequired.ToString());
			docAddress.E2_Address1 = ZString.Empty;
			AssertHasMessageError(docAddress.E2_Address1Info, ValidationConstants.JobDocAddress.AddressRequired.ToString());
			if (assertOrganisationPK)
			{
				AssertNoMessageError(docAddress.OrganisationPKInfo, organisationPKRequiredMessage.ToString());
				docAddress.E2_AddressOverride = false;
				AssertHasMessageError(docAddress.OrganisationPKInfo, organisationPKRequiredMessage.ToString());
			}

			bill.ValidationModes = ValidationModes.None;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "BOB THE BUILDER";
			AssertNoMessageError(docAddress.E2_CompanyNameInfo, ValidationConstants.JobDocAddress.CompanyNameRequired.ToString());
			docAddress.E2_CompanyName = ZString.Empty;
			AssertNoMessageError(docAddress.E2_CompanyNameInfo, ValidationConstants.JobDocAddress.CompanyNameRequired.ToString());
			docAddress.E2_Address1 = "ADDRESS 1";
			AssertNoMessageError(docAddress.E2_Address1Info, ValidationConstants.JobDocAddress.AddressRequired.ToString());
			docAddress.E2_Address1 = ZString.Empty;
			AssertNoMessageError(docAddress.E2_Address1Info, ValidationConstants.JobDocAddress.AddressRequired.ToString());
			if (assertOrganisationPK)
			{
				AssertNoMessageError(docAddress.OrganisationPKInfo, organisationPKRequiredMessage.ToString());
				docAddress.E2_AddressOverride = false;
				AssertNoMessageError(docAddress.OrganisationPKInfo, organisationPKRequiredMessage.ToString());
			}
		}
	}
}
