using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(TTBCOLAAndCertificate))]
	public class TTBCOLAAndCertificateTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<TTBCOLAAndCertificate>
	{
		public void TestITTBCOLAAndCertificateMembers()
		{
			var colaAndCertificate = TTBLine.COLAAndCertificates.AddNew();
			colaAndCertificate.US_COLA = "2342342";
			colaAndCertificate.US_ForeignCertificateCountry = Core.Constants.CountryCodes.NewZealand;
			colaAndCertificate.US_COLAExemptionCode = TTBExemptionCodeList.Codes.TTBEX7;

			ITTBCOLAAndCertificate line = colaAndCertificate;
			AssertEquals("COLA", "2342342", line.COLA);
			AssertEquals("ForeignCertificateCountry", Core.Constants.CountryCodes.NewZealand, line.ForeignCertificateCountry);
			AssertEquals("COLAExemptionCode", TTBExemptionCodeList.Codes.TTBEX7, line.ExemptionCode);
		}

		public void TestForeignCertificate()
		{
			TTBLine.US_ProcessingCode = TTBProgramCodeList.Codes.Wine;
			var colaAndCertificate = TTBLine.COLAAndCertificates.AddNew();
			colaAndCertificate.US_COLA = "asd";
			AssertEquals("HasForeignCertificate", false, colaAndCertificate.HasForeignCertificate);
			colaAndCertificate.US_ForeignCertificateCountry = Core.Constants.CountryCodes.NewZealand;
			AssertEquals("HasForeignCertificate", true, colaAndCertificate.HasForeignCertificate);
			AssertEquals("US_ForeignCertificateCountry", Core.Constants.CountryCodes.NewZealand, colaAndCertificate.US_ForeignCertificateCountry);
			colaAndCertificate.HasForeignCertificate = false;
			AssertEquals("US_ForeignCertificateCountry", "", colaAndCertificate.US_ForeignCertificateCountry);
			AssertEquals("HasForeignCertificate", false, colaAndCertificate.HasForeignCertificate);
			colaAndCertificate.HasForeignCertificate = true;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var colaAndCertificateInDiffFactory = newFactory.Load<TTBCOLAAndCertificate>(colaAndCertificate.PK);
			AssertEquals("HasForeignCertificate was not saved as true because US_ForeignCertificateCountry is empty", false, colaAndCertificateInDiffFactory.HasForeignCertificate);
			AssertEquals("US_ForeignCertificateCountry", "", colaAndCertificateInDiffFactory.US_ForeignCertificateCountry);

			colaAndCertificate.US_ForeignCertificateCountry = Core.Constants.CountryCodes.NewZealand;
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			colaAndCertificateInDiffFactory = newFactory.Load<TTBCOLAAndCertificate>(colaAndCertificate.PK);
			AssertEquals("HasForeignCertificate was saved as true because US_ForeignCertificateCountry is not empty", true, colaAndCertificateInDiffFactory.HasForeignCertificate);
			AssertEquals("US_ForeignCertificateCountry", Core.Constants.CountryCodes.NewZealand, colaAndCertificateInDiffFactory.US_ForeignCertificateCountry);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			var cola = ttbLine.COLAAndCertificates.AddNew();
			cola.US_COLA = "21342";
			return cola;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return TTBLine.COLAAndCertificates.AddNew();
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
					declaration.US_EnableCRL = true;
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

		TTBLine TTBLine
		{
			get { return ttbLine ?? (ttbLine = InvoiceLine.TTBLines.AddNew()); }
		}
		TTBLine ttbLine;
		#endregion
	}
}
