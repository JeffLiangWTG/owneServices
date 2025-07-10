using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class APHISIdentityLookupsTest : TestCaseWithFactory
	{
		public void TestCY_CodeList()
		{
			var list = Identity.Lookups.CY_CodeList;
			AssertEquals("CY_CodeList", APHISItemIdentityNumberQualifierList.GetListWithoutBouquet(Factory), list);
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

		APHISHeader Header
		{
			get
			{
				if (aphisHeader == null)
				{
					aphisHeader = InvoiceLine.APHISHeaders.AddNew();
					aphisHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
				}
				return aphisHeader;
			}
		}
		APHISHeader aphisHeader;

		APHISProduct Product
		{
			get { return product ?? (product = Header.Products.AddNew()); }
		}
		APHISProduct product;

		APHISIdentity Identity
		{
			get { return identity ?? (identity = Product.Identities.AddNew()); }
		}
		APHISIdentity identity;
	}
}
