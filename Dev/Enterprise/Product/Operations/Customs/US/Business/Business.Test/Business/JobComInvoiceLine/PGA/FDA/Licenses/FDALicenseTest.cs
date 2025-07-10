using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FDALicense))]
	public class FDALicenseTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<FDALicense>
	{
		public void TestIFDALicenseMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			var license = fda.Licenses.AddNew();

			license.US_StateCode = "QLD";
			license.US_StateDescription = "STATE";
			license.US_CountryCode = "AU";
			license.US_Number = "012450";
			IFDALicense iLicense = license;
			AssertEquals("Issuer", "POV", iLicense.Issuer);
			AssertEquals("CountryCode", "AU", iLicense.CountryCode);
			AssertEquals("Number", "012450", iLicense.Number);
			AssertEquals("StateCode", "QLD", iLicense.StateCode);
			AssertEquals("StateDescription", "STATE", iLicense.StateDescription);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			var license = fda.Licenses.AddNew();
			license.US_StateCode = "MX";
			return license;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			return fda.Licenses.AddNew();
		}
	}
}
