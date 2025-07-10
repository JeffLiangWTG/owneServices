using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business.Test
{
	internal class ProcessCompanyLinkRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPCR_GC_Company()
		{
			var duplicateRow = Factory.New<ProcessCompanyLinkRule>();
			var row = Factory.New<ProcessCompanyLinkRule>();
			row.PCR_Type = duplicateRow.PCR_Type = "SHP";
			row.PCR_GC_Company = duplicateRow.PCR_GC_Company = GlbCompany.CurrentCompany.PK;
			row.PCR_Macro = duplicateRow.PCR_Macro = "1";
			Factory.Save();
			row.Validation.ValidatePCR_GC_Company();
			AssertHasWarning(row.PCR_GC_CompanyInfo, "This is one of 2 rules with the same Type and Company.");
		}

		public void TestPCR_Macro()
		{
			var row = Factory.New<ProcessCompanyLinkRule>();
			row.Validation.ValidatePCR_Macro();
			AssertHasError(row.PCR_MacroInfo, "Please enter a Rule.");
		}

		public void TestPCR_Type()
		{
			var row = Factory.New<ProcessCompanyLinkRule>();
			row.Validation.ValidatePCR_Type();
			AssertHasError(row.PCR_TypeInfo, "Please enter a Workflow Type.");
			row.PCR_Type = "XXY";
			AssertHasError(row.PCR_TypeInfo, "Enter a valid Workflow Type.");
		}
	}
}
