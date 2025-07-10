using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FWSLicense))]
	public class FWSLicenseTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<FWSLicense>
	{
		public void TestIFWSLicenseMembers()
		{
			var license = Header.Licenses.AddNew();
			IFWSLicense iLicense = license;
			AssertEquals("iLicense.Number", ZString.Empty, iLicense.Number);
			AssertEquals("iLicense.Type", ZString.Empty, iLicense.Type);
			license.US_Type = FWSLicenseTypeList.Codes.ForeignWildlifeExportDocument;
			license.US_Number = "BTB212";
			AssertEquals("iLicense.Number", "BTB212", iLicense.Number);
			AssertEquals("iLicense.Type", FWSLicenseTypeList.Codes.ForeignWildlifeExportDocument, iLicense.Type);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fwsHeader = invoiceLine.FWSHeaders.AddNew();
			var license = fwsHeader.Licenses.AddNew();
			license.US_Number = "ABC";
			return license;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Header.Licenses.AddNew();
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		FWSHeader Header
		{
			get
			{
				if (fwsHeader == null)
				{
					fwsHeader = InvoiceLine.FWSHeaders.AddNew();
				}
				return fwsHeader;
			}
		}
		FWSHeader fwsHeader;

		#endregion
	}
}
