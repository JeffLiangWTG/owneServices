using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	internal class AgencyBookingDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		#region TestMandatoryConsignorValidation
		public void TestMandatoryConsignorValidation()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignor = true;
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoErrors(shipment.ConsignorDocumentaryAddress.OrganisationPKInfo);
			shipment.ConsignorDocumentaryAddress.OrganisationPK = org.PK;
			shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoErrors(shipment.ConsignorDocumentaryAddress.OrganisationPKInfo);
		}

		#endregion
		#region TestMandatoryConsigneeValidation
		public void TestMandatoryConsigneeValidation()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignee = true;
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoErrors(shipment.ConsigneeDocumentaryAddress.OrganisationPKInfo);
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org.PK;
			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoErrors(shipment.ConsigneeDocumentaryAddress.OrganisationPKInfo);
		}

		#endregion
		#region TestMandatoryBookedByValidation
		public void TestMandatoryBookedByValidation()
		{
			string errorMessage = "Please enter a booking party";
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.BookingPartyDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasError(shipment.BookingPartyDocumentaryAddress.OrganisationPKInfo, errorMessage);
			shipment.BookingPartyDocumentaryAddress.OrganisationPK = org.PK;
			shipment.BookingPartyDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoError(shipment.BookingPartyDocumentaryAddress.OrganisationPKInfo, errorMessage);
		}
		#endregion
	}
}
