using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class USAPHISSourceAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSourceTypes()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			var list = Source.AddInfoLookups.SourceTypes;
			AssertEquals("Should be cached", SourceTypeCodesList.GetListForAPHIS(Factory, APHISProgramCodeList.Codes.AVS), list);
		}

		public void TestCountries()
		{
			AssertEquals("Countries", typeof(RefCountryCollection), Source.AddInfoLookups.Countries.GetType());
		}

		public void TestProcessingTypes()
		{
			Header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			Header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			var list = Source.AddInfoLookups.ProcessingTypes;
			AssertEquals("Should be cached", APHISProcessingTypeCodeList.GetListFor(Factory, APHISProgramCodeList.Codes.AVS, APHISCategoryTypeCodeList.Codes.LiveAnimals), list);
		}

		#region Implementation

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

		APHISSource Source
		{
			get { return source ?? (source = Header.Sources.AddNew()); }
		}
		APHISSource source;
		#endregion
	}
}
