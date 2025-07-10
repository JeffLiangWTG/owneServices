using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
	{
		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration.AreMultipleEntryInstructionsAllowed);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals("Replace this with the correct currency code when implemented in a real country", GlbCompany.CurrentCompany.LocalCurrency.Code, GetJobDeclaration().LocalCurrencyCode);
		}

		public void TestHouseBillsCollectionIsOfRightType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertType<BillCollection<Bill, JobDeclaration>>(declaration.Bills);
		}

		public void TestLookupObjectIsCached()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			var firstLookup = declaration.Lookups;
			var secondLookup = declaration.Lookups;
			AssertEquals(secondLookup, firstLookup);
		}

		/***
		 * !!! Test to be removed in production code !!!
		 * Please remove the whole method TestTypeDecider from the production code once the following test case passes.
		 * It is only meant as an initial completeness check immdiately after the country project is set up.
		 ***/
		public void TestTypeDecider()
		{
			AssertType<JobDeclaration>("Update dbo.JobDeclaration to include a decider for this class", Factory.New<JobDeclaration>());
		}

		public void TestFilteredInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}
	}
}

