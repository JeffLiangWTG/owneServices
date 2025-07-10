using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(APHISSourceCollection))]
	public class APHISSourceCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddCountryOfSpeciesOriginElementIfRequired()
		{
			InvoiceLine.US_UC_NKCountryOfOrigin = "US";
			Header.US_ProgramType = APHISProgramCodeList.Codes.AAC;
			var sources = Header.Sources;
			Assert(sources.Count == 1);
			var source = sources[0];
			AssertEquals(SourceTypeCodesList.Codes.CountryOfSpeciesOrigin, source.US_SourceTypeCode);
			AssertEquals("US", source.US_CountryCode);
		}

		public void TestSuppressAddNewSourceLineDuringDataImport()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AAC;
			AssertEquals(1, Header.Sources.Count);
			AssertEquals("APHIS source is added when import is not in progress", SourceTypeCodesList.Codes.CountryOfSpeciesOrigin, Header.Sources[0].US_SourceTypeCode);

			Header.US_ProgramType = "";
			Header.Sources.RemoveAndDeleteAll();

			using (DataImportIndicatorService.StartDataImport(Factory))
			{
				Header.US_ProgramType = APHISProgramCodeList.Codes.AAC;
				AssertEquals("APHIS Source is not added when import is in progress", 0, Header.Sources.Count);
			}
		}

		public void TestHasSourceType()
		{
			InvoiceLine.US_UC_NKCountryOfOrigin = "US";
			Header.US_ProgramType = APHISProgramCodeList.Codes.AAC;
			Header.Sources.RemoveAndDeleteAll();
			var source1 = Header.Sources.AddNew();
			source1.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfManipulation;
			AssertEquals(false, Header.Sources.HasSourceType39or267);
			var source2 = Header.Sources.AddNew();
			source2.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfProduction;
			AssertEquals(true, Header.Sources.HasSourceType39or267);
			AssertEquals(false, Header.Sources.HasSourceType262HRV267);
			source2.US_SourceTypeCode = SourceTypeCodesList.Codes.PlaceOfGrowth;
			AssertEquals(true, Header.Sources.HasSourceType262HRV267);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APHISSourceCollection(Header);
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
