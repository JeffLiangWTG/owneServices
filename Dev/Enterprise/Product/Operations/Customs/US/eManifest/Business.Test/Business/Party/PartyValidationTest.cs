using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	class PartyValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheck_ValidationStatus()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			party.E2_ValidationStatus = AddressValidationStatus.Invalid;
			Assert("precondition", !party.E2_SuppressAddressValidationError);
			party.RunPreSaveValidation();
			AssertHasErrors("precondition", party.E2_ValidationStatusInfo);
			party.ClearAllNotifications();
			party.E2_SuppressAddressValidationError = true;
			party.RunPreSaveValidation();
			AssertNoErrors(party.E2_ValidationStatusInfo);
			ErrorReporter.Clear();
		}

		public void TestCheckE2_AddressType()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(party.E2_AddressTypeInfo, "???", PartyTypes.Codes.Consignee);
			ValidationTestHelper.AssertErrorIfNotEntered(party.E2_AddressTypeInfo);
		}

		public void TestCheckE2_GovRegNumType()
		{
			party.E2_AddressOverride = true;
			ValidationTestHelper.AssertErrorIfInvalidCode(party.E2_GovRegNumTypeInfo, "???", PartyIdTypes.Codes.SCAC);
		}

		public void TestCheckE2_RN_NKCountryCode()
		{
			party.E2_AddressOverride = true;
			ValidationTestHelper.AssertErrorIfNotEntered(party.E2_RN_NKCountryCodeInfo);
		}

		public void TestCheckOrganisationPK()
		{
			const string messageError = "Please specify a valid Carrier for this job.";
			party.Validation.ValidateOrganisationPK();
			AssertHasMessageError(party.OrganisationPKInfo, messageError);
			party.OrganisationPK = ZGuid.NewZGuid();
			AssertHasMessageError(party.OrganisationPKInfo, messageError);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			party.OrganisationPK = org.PK;
			AssertNoMessageError(party.OrganisationPKInfo, messageError);
			OrgValidationTest.AssertJobDocAddressUsesOrgValidationIfOrgSpecified(party.OrganisationPKInfo);
			party.Shipment.B0_ShipmentType = ShipmentTypes.Codes.SplitShipment;
			party.OrganisationPK = ZGuid.NewZGuid();
			AssertNoMessageErrors(party.OrganisationPKInfo);
			party.Shipment.B0_ShipmentType = ZString.Empty;
			party.Shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.LodgedWithOtherTrip;
			party.Validation.ValidateOrganisationPK();
			AssertNoMessageErrors(party.OrganisationPKInfo);
		}

		public void TestCheckE2_CompanyName()
		{
			party.E2_CompanyName = "ABC";
			AssertNoMessageErrors(party.E2_CompanyNameInfo);
			party.E2_CompanyName = "ÄBC";
			AssertNoMessageErrors(party.E2_CompanyNameInfo);
			party.E2_CompanyName = "哈";
			AssertHasMessageErrorContaining(party.E2_CompanyNameInfo, party.E2_CompanyNameInfo.HumanReadableName + OrgValidation.InvalidCharactersMessageError);
		}

		public void TestCheckE2_Address1()
		{
			party.E2_Address1 = "ABC";
			AssertNoMessageErrors(party.E2_Address1Info);
			party.E2_Address1 = "ÄBC";
			AssertNoMessageErrors(party.E2_Address1Info);
			party.E2_Address1 = "哈";
			AssertHasMessageErrorContaining(party.E2_Address1Info, party.E2_Address1Info.HumanReadableName + OrgValidation.InvalidCharactersMessageError);
		}

		public void TestCheckE2_Address2()
		{
			party.E2_Address2 = "ABC";
			AssertNoMessageErrors(party.E2_Address2Info);
			party.E2_Address2 = "ÄBC";
			AssertNoMessageErrors(party.E2_Address2Info);
			party.E2_Address2 = "哈";
			AssertHasMessageErrorContaining(party.E2_Address2Info, party.E2_Address2Info.HumanReadableName + OrgValidation.InvalidCharactersMessageError);
		}

		public void TestCheckE2_City()
		{
			party.E2_City = "ABC";
			AssertNoMessageErrors(party.E2_CityInfo);
			party.E2_City = "ÄBC";
			AssertNoMessageErrors(party.E2_CityInfo);
			party.E2_City = "哈";
			AssertHasMessageErrorContaining(party.E2_CityInfo, party.E2_CityInfo.HumanReadableName + OrgValidation.InvalidCharactersMessageError);
		}

		public void TestCheckE2_Postcode()
		{
			party.E2_AddressType = PartyTypes.Codes.Consignee;
			party.E2_Postcode = "";
			party.Validation.ValidateE2_Postcode();
			AssertHasMessageErrorContaining(party.E2_PostcodeInfo, "You have not entered a Consignee: Postcode.");

			party.E2_Postcode = "2023";
			party.Validation.ValidateE2_Postcode();
			AssertNoMessageErrorContaining(party.E2_PostcodeInfo, "You have not entered a Consignee: Postcode.");

			party.E2_AddressType = PartyTypes.Codes.Carrier;
			party.E2_Postcode = "1111A";
			AssertNoMessageErrors(party.E2_PostcodeInfo);
			party.E2_Postcode = "1111Ä";
			AssertNoMessageErrors(party.E2_PostcodeInfo);
			party.E2_Postcode = "1111哈";
			AssertHasMessageErrorContaining(party.E2_PostcodeInfo, party.E2_PostcodeInfo.HumanReadableName + OrgValidation.InvalidCharactersMessageError);
		}

		public void TestCheckE2_State()
		{
			party.E2_AddressType = PartyTypes.Codes.Consignee;
			party.E2_State = "";
			party.Validation.ValidateE2_State();
			AssertHasMessageErrorContaining(party.E2_StateInfo, "You have not entered a Consignee: State.");

			party.E2_State = "NSW";
			party.Validation.ValidateE2_State();
			AssertNoMessageErrorContaining(party.E2_StateInfo, "You have not entered a Consignee: State.");

			party.E2_AddressType = PartyTypes.Codes.Carrier;
			party.E2_State = "NY";
			AssertNoMessageErrors(party.E2_StateInfo);
			party.E2_State = "NÄ";
			AssertNoMessageErrors(party.E2_StateInfo);
			party.E2_State = "N哈";
			AssertHasMessageErrorContaining(party.E2_StateInfo, party.E2_StateInfo.HumanReadableName + OrgValidation.InvalidCharactersMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			party = shipment.Parties.AddNew();
			party.E2_AddressType = PartyTypes.Codes.Carrier;
		}

		Party party;
	}
}
