using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISIdentityNumberRangeCollection))]
	public class APHISIdentityNumberRangeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var numberRanges = Identity.NumberRanges;
			AssertEquals("AllowNew", false, numberRanges.AllowNew);
			Identity.UseMultipleNumbers = true;
			AssertEquals("AllowNew", true, numberRanges.AllowNew);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<APHISIdentityNumberRange>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APHISIdentityNumberRangeCollection(Identity);
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
