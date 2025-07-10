using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISIdentityNumberRange))]
	public class APHISIdentityNumberRangeTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<APHISIdentityNumberRange>
	{
		public void TestINumberRangeMembers()
		{
			var range = Identity.NumberRanges.AddNew();
			range.US_StartNumber = "N01";
			range.US_EndNumber = "N10";
			INumberRange iRange = range;
			AssertEquals("StartNumber", "N01", iRange.StartNumber);
			AssertEquals("EndNumber", "N10", iRange.EndNumber);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var header = invoiceLine.APHISHeaders.AddNew();
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var product = header.Products.AddNew();
			var identity = product.Identities.AddNew();
			var range = identity.NumberRanges.AddNew();
			range.US_StartNumber = "N1";
			return range;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Identity.NumberRanges.AddNew();
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

		#endregion
	}
}
