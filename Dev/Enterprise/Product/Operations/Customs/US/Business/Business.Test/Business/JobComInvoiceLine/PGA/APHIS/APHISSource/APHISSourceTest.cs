using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISSource))]
	public class APHISSourceTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<APHISSource>
	{
		public void TestISourceDetailMembers()
		{
			var source = Header.Sources.AddNew();
			source.US_CountryCode = "AU";
			source.US_GeographicLocation = "HERE";
			source.US_ProcessingDescription = "PRO DESC";
			source.US_ProcessingStartDate = new ZDateTime(2015, 6, 1);
			source.US_ProcessingEndDate = new ZDateTime(2015, 7, 1);
			source.US_ProcessingTypeCode = "P1";
			source.US_SourceTypeCode = "S1";
			ISource iSource = source;
			AssertEquals("CountryCode", "AU", iSource.CountryCode);
			AssertEquals("GeographicLocation", "HERE", iSource.GeographicLocation);
			AssertEquals("ProcessingDescription", "PRO DESC", iSource.ProcessingDescription);
			AssertEquals("ProcessingStartDate", new ZDate(2015, 6, 1), iSource.ProcessingStartDate);
			AssertEquals("ProcessingEndDate", new ZDate(2015, 7, 1), iSource.ProcessingEndDate);
			AssertEquals("ProcessingTypeCode", "P1", iSource.ProcessingTypeCode);
			AssertEquals("SourceTypeCode", "S1", iSource.SourceTypeCode);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var aphisHeader = invoiceLine.APHISHeaders.AddNew();
			var source = aphisHeader.Sources.AddNew();
			source.US_CountryCode = "1";
			return source;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Header.Sources.AddNew();
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
