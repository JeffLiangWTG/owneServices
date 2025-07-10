using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USFDAAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckFDAValidateCharactorsForAddressDescription()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var party1 = Factory.New<OrgHeader>();
			var orgAddress1 = party1.MainAddress;
			orgAddress1.OA_City = "KYIV";
			orgAddress1.OA_Address1 = "éééÄöß";
			orgAddress1.OA_Address2 = "Address2Äöß";
			orgAddress1.OA_Code = "öß";

			var party2 = Factory.New<OrgHeader>();
			var orgAddress2 = party2.MainAddress;
			orgAddress2.OA_City = "KYIV";
			orgAddress2.OA_Address1 = "address1";
			orgAddress2.OA_Address2 = "address2";
			orgAddress2.OA_Code = "code";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var fda = invoiceLine.FDAs.AddNew();

			fda.US_OA_FDAFEI = orgAddress1.PK;
			AssertHasWarning(fda.US_OA_FDAFEIInfo, addressDescriptionWarning);
			AssertHasWarning(fda.US_OA_FDAFEIInfo, addressCodeWarning);
			fda.US_OA_FDAFEI = orgAddress2.PK;
			AssertNoWarning(fda.US_OA_FDAFEIInfo, addressDescriptionWarning);
			AssertNoWarning(fda.US_OA_FDAFEIInfo, addressCodeWarning);

			fda.US_FDAManufacturerAddress = orgAddress1.PK;
			AssertHasWarning(fda.US_FDAManufacturerAddressInfo, addressDescriptionWarning);
			AssertHasWarning(fda.US_FDAManufacturerAddressInfo, addressCodeWarning);
			fda.US_FDAManufacturerAddress = orgAddress2.PK;
			AssertNoWarning(fda.US_FDAManufacturerAddressInfo, addressDescriptionWarning);
			AssertNoWarning(fda.US_FDAManufacturerAddressInfo, addressCodeWarning);

			fda.US_FDAShipperAddress = orgAddress1.PK;
			AssertHasWarning(fda.US_FDAShipperAddressInfo, addressDescriptionWarning);
			AssertHasWarning(fda.US_FDAShipperAddressInfo, addressCodeWarning);
			fda.US_FDAShipperAddress = orgAddress2.PK;
			AssertNoWarning(fda.US_FDAShipperAddressInfo, addressDescriptionWarning);
			AssertNoWarning(fda.US_FDAShipperAddressInfo, addressCodeWarning);
		}
	}
}
