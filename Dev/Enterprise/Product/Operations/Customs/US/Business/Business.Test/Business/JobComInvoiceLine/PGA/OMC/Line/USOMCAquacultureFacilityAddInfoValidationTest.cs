using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USOMCAquacultureFacilityAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOMCValidateCharactorsForAddressDescription()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var party1 = Factory.New<OrgHeader>();
			var orgAddress1 = party1.MainAddress;
			orgAddress1.OA_City = "KYIV";
			orgAddress1.OA_Address1 = "éééÄöß";
			orgAddress1.OA_Address2 = "Address2Äöß";
			orgAddress1.OA_Code = "öß";

			OrgHeader party2 = Factory.New<OrgHeader>();
			OrgAddress orgAddress2 = party2.MainAddress;
			orgAddress2.OA_City = "KYIV";
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "address2";
			orgAddress2.OA_Code = "code";

			var aquacultureFacility = Header.AquacultureFacilities.AddNew();

			aquacultureFacility.US_OA_AquacultureFacility = orgAddress1.PK;
			AssertHasWarning(aquacultureFacility.US_OA_AquacultureFacilityInfo, addressDescriptionWarning);
			AssertHasWarning(aquacultureFacility.US_OA_AquacultureFacilityInfo, addressCodeWarning);
			aquacultureFacility.US_OA_AquacultureFacility = orgAddress2.PK;
			AssertNoWarning(aquacultureFacility.US_OA_AquacultureFacilityInfo, addressDescriptionWarning);
			AssertNoWarning(aquacultureFacility.US_OA_AquacultureFacilityInfo, addressCodeWarning);
		}

		public void TestCheckUS_OA_AquacultureFacility()
		{
			var org = Factory.New<OrgHeader>();
			var address1 = org.Addresses.AddNew();
			var aquacultureFacility1 = Header.AquacultureFacilities.AddNew();
			aquacultureFacility1.US_OA_AquacultureFacility = address1.PK;
			AssertNoMessageError(aquacultureFacility1.US_OA_AquacultureFacilityInfo, USOMCAquacultureFacilityAddInfoValidation.AquacultureFacilityRepeat);
			var aquacultureFacility2 = Header.AquacultureFacilities.AddNew();
			aquacultureFacility2.US_OA_AquacultureFacility = address1.PK;
			AssertHasMessageError(aquacultureFacility2.US_OA_AquacultureFacilityInfo, USOMCAquacultureFacilityAddInfoValidation.AquacultureFacilityRepeat);
		}

		OMCHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableCRL = true;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					header = invoiceLine.OMCHeaders.AddNew();
				}
				return header;
			}
		}
		OMCHeader header;
	}
}
