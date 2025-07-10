using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISRoutingCollection))]
	public class APHISRoutingCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddOriginalLocationElementIfRequired()
		{
			InvoiceLine.US_UC_NKCountryOfOrigin = "US";
			var header = Header;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			Assert(header.Routings.Count == 1);
			var routing = header.Routings[0];
			AssertEquals(RoutingTypeList.Codes.OriginalLocation, routing.US_Type);
			AssertEquals("US", routing.US_Country);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APHISRoutingCollection(Header);
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

		#endregion
	}
}
