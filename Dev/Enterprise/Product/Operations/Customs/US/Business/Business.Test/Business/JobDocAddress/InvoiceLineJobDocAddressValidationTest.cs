using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InvoiceLineJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckE2_Address1()
		{
			string addressDescriptionWarning = "Address 1 : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			invoiceLine.ExporterOrDestroyer.E2_AddressOverride = true;
			invoiceLine.ExporterOrDestroyer.E2_Address1 = "测试地址";
			AssertHasWarning(invoiceLine.ExporterOrDestroyer.E2_Address1Info, addressDescriptionWarning);
		}

		public void TestCheckCheckE2_OA_Address()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			var party = Factory.New<OrgHeader>();
			var address = party.Addresses.AddNew();
			address.OA_City = "KYIV";
			address.OA_Address1 = "éééÄöß";
			address.OA_Address2 = "Address2Äöß";
			address.OA_Code = "öß";
			invoiceLine.ExporterOrDestroyer.E2_OA_Address = address.PK;
			AssertHasWarning(invoiceLine.ExporterOrDestroyer.E2_OA_AddressInfo, addressCodeWarning);
			AssertHasWarning(invoiceLine.ExporterOrDestroyer.E2_OA_AddressInfo, addressDescriptionWarning);
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}
	}
}
