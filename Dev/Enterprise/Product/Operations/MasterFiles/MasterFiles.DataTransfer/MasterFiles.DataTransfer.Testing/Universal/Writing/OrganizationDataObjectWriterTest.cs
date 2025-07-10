using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class OrganizationDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestGetDataObjectWithSchema()
		{
			var houseBill = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.ICusSCAHouse>();
			houseBill[CusSCAHouseSchema.CA_ConsigneeName] = "CONSIGNEE NAME";
			houseBill[CusSCAHouseSchema.CA_ConsigneeAddress1] = "ADDRESS 1";
			houseBill[CusSCAHouseSchema.CA_ConsigneeAddress2] = "ADDRESS 2";
			houseBill[CusSCAHouseSchema.CA_ConsigneeSuburb] = "CITY";
			houseBill[CusSCAHouseSchema.CA_ConsigneeState] = "STATE";
			houseBill[CusSCAHouseSchema.CA_ConsigneePostcode] = "PCOST";
			houseBill[CusSCAHouseSchema.CA_RN_NKConsigneeCountryCode] = "AU";
			houseBill[CusSCAHouseSchema.CA_ConsigneeContactName] = "CONTACT";
			houseBill[CusSCAHouseSchema.CA_ConsigneePhone] = "PHONE";
			houseBill[CusSCAHouseSchema.CA_ConsigneeFax] = "FAX";

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var dataObject = OrganizationDataObjectWriter.GetDataObject(Factory, writeManager, houseBill, nameof(DocAddressType.ConsigneeDocumentaryAddress), CusSCAHouseSchema.CA_ConsigneeName,
				CusSCAHouseSchema.CA_ConsigneeAddress1, CusSCAHouseSchema.CA_ConsigneeAddress2, CusSCAHouseSchema.CA_ConsigneeSuburb, CusSCAHouseSchema.CA_ConsigneeState,
				CusSCAHouseSchema.CA_ConsigneePostcode, CusSCAHouseSchema.CA_RN_NKConsigneeCountryCode, CusSCAHouseSchema.CA_ConsigneePhone, CusSCAHouseSchema.CA_ConsigneeFax,
				CusSCAHouseSchema.CA_ConsigneeContactName);

			AssertEquals("CONSIGNEE NAME", dataObject.CompanyName);
			AssertEquals("ADDRESS 1", dataObject.Address1);
			AssertEquals("ADDRESS 2", dataObject.Address2);
			AssertEquals("CITY", dataObject.City);
			AssertEquals("STATE", (string)dataObject.State);
			AssertEquals("PCOST", dataObject.Postcode);
			AssertEquals("AU", dataObject.Country.Code);
			AssertEquals("CONTACT", dataObject.Contact);
			AssertEquals("PHONE", dataObject.Phone);
			AssertEquals("FAX", dataObject.Fax);
		}

		public void TestPortComesFromAddressNotFromOrganisationMainUNLOCO()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "GO THE MIGHTY EELS INC.";
			organisation.OH_RL_NKClosestPort = "AUPRM";

			var address1 = organisation.MainAddress;
			address1.OA_Address1 = "123 PARRAMATTA WAY";
			address1.OA_RL_NKRelatedPortCode = "AUPRM";

			var address2 = organisation.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			address2.OA_Address1 = "456 SOMEWHERE ELSE";
			address2.OA_RL_NKRelatedPortCode = "AUMAS";

			var writeManager = new DataWritingManager(new ActionInfo(null, address2));
			var addressData = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ConsignorPickupDeliveryAddress)).GetDataObject(address2);

			AssertEquals("addressData.Port.Code", "AUMAS", addressData.Port.Code);
		}

		public void TestCountryComesFromAddressNotFromOrganisationCountry()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "NZAKL";

			var address1 = organisation.MainAddress;
			address1.OA_RL_NKRelatedPortCode = "NZAKL";

			var address2 = organisation.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			address2.OA_RL_NKRelatedPortCode = "USSFO";

			AssertEquals("Precondition: organisation.CountryCode", "NZ", organisation.CountryCode);
			AssertEquals("Precondition: address1.CountryCode", "NZ", address1.Country.Code);
			AssertEquals("Precondition: address2.CountryCode", "US", address2.Country.Code);

			var writeManager = new DataWritingManager(new ActionInfo(null, address2));
			var addressData = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ConsignorPickupDeliveryAddress)).GetDataObject(address2);

			AssertEquals("addressData.Country.Code", "US", addressData.Country.Code);
		}

		public void TestNullCallsDoNotThrowExceptions()
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			AssertEquals("null OrgAddress", null, new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Consolidator)).GetDataObject(null));
			AssertEquals("null OrgHeader", null, new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Consolidator)).GetDataObject(Factory.New<OrgAddress>()));
		}

		public void TestCompanyNameIsTakenFromTheOrgAddressLevelOverrideWherePresent()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "A COMPANY NAME WITH LENGTH GREATER THAN 50 CHARACTERS BUT LESS THAN 100";
			orgHeader.OH_RL_NKClosestPort = "AUPAU";

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "123 LONELY LANE";
			orgAddress.OA_City = "PORT ARTHUR";
			orgAddress.OA_PostCode = "7182";
			orgAddress.OA_RL_NKRelatedPortCode = "AUPAU";
			orgAddress.OA_State = "TASMANIA";

			orgHeader.OH_Code = "DRODEAPAU";

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var organization = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Consolidator)).GetDataObject(orgAddress);
			AssertEquals("organization.CompanyName without override", "A COMPANY NAME WITH LENGTH GREATER THAN 50 CHARACTERS BUT LESS THAN 100", organization.CompanyName);

			orgAddress.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME WITH LENGTH GREATER THAN 50 CHARACTERS BUT LESS THAN 100";

			organization = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Consolidator)).GetDataObject(orgAddress);
			AssertEquals("organization.CompanyName with override", "OVERRIDEN COMPANY NAME WITH LENGTH GREATER THAN 50 CHARACTERS BUT LESS THAN 100", organization.CompanyName);
		}

		public void TestLocalAddress()
		{
			var organization = Factory.New<OrgHeader>();
			var address = organization.Addresses.AddNew();
			address.TranslatedAddresses.AddNew().Address1 = "Translated address 1";
			address.TranslatedAddresses.AddNew().Address1 = "Translated address 2";

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var addressDataObject = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.Consolidator)).GetDataObject(address);

			AssertEquals(2, addressDataObject.LocalAddressCollection.Count);
			Assert(addressDataObject.LocalAddressCollection.Any(translatedaddress => translatedaddress.Address1.ToString() == "Translated address 1"));
			Assert(addressDataObject.LocalAddressCollection.Any(translatedaddress => translatedaddress.Address1.ToString() == "Translated address 2"));
		}

		public void TestGetDataObjectWithStringAddressType_DoNotDefaultContactInfo()
		{
			AssertGetDataObjectWithStringAddressType(false);
		}
		void AssertGetDataObjectWithStringAddressType(bool expectToDefaultContactInfo)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "DROP DEAD FRED P/L";
			orgHeader.OH_RL_NKClosestPort = "AUPAU";
			orgHeader.OH_Code = "DRODEAPAU";

			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_Address1 = "123 MAIN LANE";

			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "123 LONELY LANE";
			orgAddress.OA_City = "PORT ARTHUR";
			orgAddress.OA_PostCode = "7182";
			orgAddress.OA_RL_NKRelatedPortCode = "AUPAU";
			orgAddress.OA_State = "TASMANIA";

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "JACK";
			var document1 = contact1.Documents.AddNew();
			document1.OD_DocumentGroup = ContactType.Consignee.Code;

			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "BILL";
			var document2 = contact2.Documents.AddNew();
			document2.OD_DocumentGroup = ContactType.All.Code;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

			var organization = new OrganizationDataObjectWriter(writeManager, "BOB").GetDataObject(orgHeader.MainAddress);
			AssertEquals("organization.Address1", "123 MAIN LANE", organization.Address1);
			AssertEquals("organization.Contact", expectToDefaultContactInfo ? "JACK" : null, organization.Contact);

			organization = new OrganizationDataObjectWriter(writeManager, "BOB").GetDataObject(orgHeader.MainAddress);
			AssertEquals("organization.Address1", "123 MAIN LANE", organization.Address1);
			AssertEquals("organization.Contact", expectToDefaultContactInfo ? "BILL" : null, organization.Contact);

			organization = new OrganizationDataObjectWriter(writeManager, "BOB").GetDataObject(orgAddress);
			AssertEquals("organization.Address1", "123 LONELY LANE", organization.Address1);
			AssertEquals("organization.Contact", expectToDefaultContactInfo ? "JACK" : null, organization.Contact);

			organization = new OrganizationDataObjectWriter(writeManager, "BOB", contact1).GetDataObject(orgAddress);
			AssertEquals("organization.Address1", "123 LONELY LANE", organization.Address1);
			AssertEquals("organization.Contact (contact was explicitly passed in to the writer)", "JACK", organization.Contact);
		}

		public void TestOrganizationsShouldNotExportOrgCusCodesLinkedToOtherAddresses()
		{
			var organizationBO = Factory.New<OrgHeader>();
			organizationBO.OH_FullName = "NZ COMPANY";
			organizationBO.OH_RL_NKClosestPort = "NZAKL";
			organizationBO.OH_Code = "NZCOMPAKL";

			var mainAddressBO = organizationBO.MainAddress;
			mainAddressBO.OA_Address1 = "123 Main Address Road";
			mainAddressBO.OA_Code = "MAINADDRESS";
			mainAddressBO.OA_City = "Auckland";
			mainAddressBO.OA_PostCode = "1234";

			var otherAddressBO = organizationBO.Addresses.AddNew();
			otherAddressBO.OA_Address1 = "456 Other Address Street";
			otherAddressBO.OA_City = "Christchurch";
			otherAddressBO.OA_PostCode = "4321";

			var nonATFCode = organizationBO.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "CLIENT_ID");
			nonATFCode.OK_RN_NKCodeCountry = "NZ";

			var mainATFCode = organizationBO.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "MAIN_ATF");
			mainATFCode.OK_RN_NKCodeCountry = "NZ";
			mainATFCode.OK_OA_PremisesAddress = mainAddressBO.PK;

			var otherATFCode = organizationBO.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "OTHER_ATF");
			otherATFCode.OK_RN_NKCodeCountry = "NZ";
			otherATFCode.OK_OA_PremisesAddress = otherAddressBO.PK;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var organizationData = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.ContainerLegPickupAddress)).GetDataObject(organizationBO.MainAddress);

			var registrationNumbers = organizationData.RegistrationNumberCollection;
			AssertNotNull("Precondition: organizationData.RegistrationNumberCollection", registrationNumbers);
			CombineAssertions(delegate
			{
				AssertEquals("organizationData.Port", "NZAKL - Auckland", organizationData.Port.Contents());
				AssertEquals("organizationData.AddressShortCode", "MAINADDRESS", organizationData.AddressShortCode);
				AssertEquals("Should only be 2 registration numbers, 'OTHER_ATF' should not be there.", 2, registrationNumbers.Count);
				AssertEquals("registrationNumbers[0].Value", "CLIENT_ID", registrationNumbers[0].Value);
				AssertEquals("registrationNumbers[1].Value", "MAIN_ATF", registrationNumbers[1].Value);
			});
		}

		public void TestGetActivityOrganizationAddress()
		{
			var temp = Factory.New<DummyBusinessObject>();
			OrgAddress address = null;
			var dataWritingManager = new DataWritingManager(new ActionInfo(null, temp));
			var addressType = "None";
			var organizationAddress = OrganizationAddressHelper.GetAddressDataObject(address, dataWritingManager, addressType);
			AssertNull(organizationAddress);
		}

		public void TestPopulateOrgAddressState()
		{
			var organizationBO = Factory.New<OrgHeader>();
			organizationBO.OH_FullName = "THE MAGIC SAND FOOD COMPANY";
			organizationBO.OH_RL_NKClosestPort = "CNSNZ";
			var addressBO = organizationBO.MainAddress;
			addressBO.City = "Shenzhen";
			addressBO.StateCode = "44"; // Guangdong

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var writer = new OrganizationDataObjectWriter(writeManager, "BOB");

			var addressData = writer.GetDataObject(addressBO);
			AssertEquals("44", addressData.State.ToString());
			AssertEquals("Guangdong", addressData.State.Description);
		}

		public void TestPopulateGeoLocation()
		{
			var organizationBO = Factory.New<OrgHeader>();
			var addressBO = organizationBO.MainAddress;
			addressBO.GeoLocation = ZGeography.CreatePoint(1.23, 4.56);

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var writer = new OrganizationDataObjectWriter(writeManager, "ANYTHING");

			writer.PopulateGeoLocation = true;

			var addressData = writer.GetDataObject(addressBO);
			AssertEquals(new ZDecimal(1.23), addressData.GeoLocation.Longitude);
			AssertEquals(new ZDecimal(4.56), addressData.GeoLocation.Latitude);

			writer.PopulateGeoLocation = false;

			addressData = writer.GetDataObject(addressBO);
			AssertNull("addressData.GeoLocation", addressData.GeoLocation);
		}

		public void TestPopulateValidationStatus()
		{
			var organizationBO = Factory.New<OrgHeader>();
			var addressBO = organizationBO.MainAddress;
			addressBO.ValidationStatus = "VAD";

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var writer = new OrganizationDataObjectWriter(writeManager, "ANYTHING");

			writer.PopulateValidationStatus = true;

			var addressData = writer.GetDataObject(addressBO);
			AssertEquals("VAD", addressData.ValidationStatus.Code);
			AssertEquals("Verified", addressData.ValidationStatus.Description);

			writer.PopulateValidationStatus = false;

			addressData = writer.GetDataObject(addressBO);
			AssertNull("addressData.ValidationStatus", addressData.ValidationStatus);
		}

		public void TestOrganizationCategory()
		{
			var organizationBO = Factory.New<OrgHeader>();
			organizationBO.OH_Category = "BUS";
			var addressBO = organizationBO.MainAddress;

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var writer = new OrganizationDataObjectWriter(writeManager, "TestAddress");

			var addressData = writer.GetDataObject(addressBO);
			AssertEquals("BUS", addressData.OrganizationCategory);

			organizationBO.OH_Category = "GOV";
			addressData = writer.GetDataObject(addressBO);
			AssertEquals("GOV", addressData.OrganizationCategory);
		}
	}
}
