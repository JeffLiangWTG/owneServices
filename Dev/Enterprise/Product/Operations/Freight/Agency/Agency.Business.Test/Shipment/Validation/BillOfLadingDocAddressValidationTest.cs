using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	internal class BillOfLadingDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestConsigneeAndNotifyPartiesAllowToOrder()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AssertAllowToOrder(shipment.ConsigneeDocumentaryAddress);
			AssertAllowToOrder(shipment.NotifyPartyDocumentaryAddress);
			AssertAllowToOrder(shipment.NotifyParty2DocumentaryAddress);
			AssertAllowToOrder(shipment.NotifyParty3DocumentaryAddress);
		}

		public void TestConsignorDoesNotAllowToOrder()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AssertDenyToOrder(shipment.ConsignorDocumentaryAddress);
		}

		public void TestMandatoryConsignorValidation()
		{
			string errorMessage = "Please enter a consignor";
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasError(shipment.ConsignorDocumentaryAddress.OrganisationPKInfo, errorMessage);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
			shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoError(shipment.ConsignorDocumentaryAddress.OrganisationPKInfo, errorMessage);
		}

		public void TestMandatoryConsigneeValidation()
		{
			string errorMessage = "Please enter a consignee";
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasError(shipment.ConsigneeDocumentaryAddress.OrganisationPKInfo, errorMessage);
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org.PK;
			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoError(shipment.ConsigneeDocumentaryAddress.OrganisationPKInfo, errorMessage);
		}

		public void TestMandatoryBookedByValidation()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.BookingPartyDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoErrors(shipment.BookingPartyDocumentaryAddress.OrganisationPKInfo);
			shipment.BookingPartyDocumentaryAddress.OrganisationPK = org.PK;
			shipment.BookingPartyDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoErrors(shipment.BookingPartyDocumentaryAddress.OrganisationPKInfo);
		}

		#region Implementation
		void AssertAllowToOrder(JobDocAddress address)
		{
			string desc = address.AddressDescription;
			address.E2_AddressOverride = true;
			address.E2_CompanyName = "Crap";
			address.E2_Address1 = ZString.Empty;
			address.E2_Address2 = ZString.Empty;
			address.E2_Postcode = ZString.Empty;
			address.E2_City = ZString.Empty;
			address.E2_State = ZString.Empty;
			address.E2_RN_NKCountryCode = ZString.Empty;
			address.E2_Contact = ZString.Empty;
			address.E2_Phone = ZString.Empty;
			address.E2_Fax = ZString.Empty;
			address.E2_Email = ZString.Empty;
			address.Validation.ValidateAll();
			AssertNoNotifications(desc, address);
			address.E2_CompanyName = ZString.Empty;
			AssertHasError(desc + ": company name", address.E2_CompanyNameInfo, "Please enter a Company Name, or remove the override for this Address.");
		}

		void AssertDenyToOrder(JobDocAddress address)
		{
			string desc = address.AddressDescription;
			address.E2_AddressOverride = true;
			address.E2_CompanyName = ZString.Empty;
			address.E2_Address1 = ZString.Empty;
			address.E2_Address2 = ZString.Empty;
			address.E2_Postcode = ZString.Empty;
			address.E2_City = ZString.Empty;
			address.E2_State = ZString.Empty;
			address.E2_RN_NKCountryCode = ZString.Empty;
			address.E2_Contact = ZString.Empty;
			address.E2_Phone = ZString.Empty;
			address.E2_Fax = ZString.Empty;
			address.E2_Email = ZString.Empty;
			address.Validation.ValidateAll();
			AssertHasError(desc + ": company name", address.E2_CompanyNameInfo, "Please enter a Company Name, or remove the override for this Address.");
			AssertHasError(desc + ": address 1", address.E2_Address1Info, "Please enter a " + desc + ": Address Line 1.");
			AssertHasError(desc + ": city", address.E2_CityInfo, "Please enter a " + desc + ": City.");
		}
		#endregion
	}
}
