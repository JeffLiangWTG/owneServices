using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineTaxLookups))]
	sealed class JobComInvoiceLineTaxLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTariffTypeList()
		{
			var invoiceLine = (JobComInvoiceLine)Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "SS");
			universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "TEST");
			universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			var testList = invoiceLineTax.Lookups.TypeList;
			AssertEquals("SS,TEST", string.Join(",", testList.OfType<RefCusTariffType>().Select(x => x.ZZI_TariffType).OrderBy(x => x)));
		}

		public void TestMOPList()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceLine = jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			var lookups = invoiceLineTax.Lookups;
			CombineAssertions("ROR duty treatment", () =>
			{
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._38;
				invoiceLineTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.AT;
				AssertEquals("AT of MOPList", "CAS, DEF", lookups.MOPList.CodesAsString);
				invoiceLineTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.CT;
				AssertEquals("CT of MOPList", "CAS, DEF, ROR", lookups.MOPList.CodesAsString);
				invoiceLineTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.SS;
				AssertEquals("SS of MOPList", "CAS, DEF, ROR", lookups.MOPList.CodesAsString);
				invoiceLineTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.TT;
				AssertEquals("TT of MOPList", "CAS, DEF", lookups.MOPList.CodesAsString);
			});

			CombineAssertions("NonROR duty treatment", () =>
			{
				invoiceLine.JI_Procedure = Constants.ProcedureCodes._37;
				invoiceLineTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.AT;
				AssertEquals("AT of MOPList", "CAS, DEF", lookups.MOPList.CodesAsString);
				invoiceLineTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.CT;
				AssertEquals("CT of MOPList", "CAS, DEF", lookups.MOPList.CodesAsString);
				invoiceLineTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.SS;
				AssertEquals("SS of MOPList", "CAS, DEF", lookups.MOPList.CodesAsString);
				invoiceLineTax.JLT_Type = Constants.UniversalReferenceConstants.RefCusRateTypes.TT;
				AssertEquals("TT of MOPList", "CAS, DEF", lookups.MOPList.CodesAsString);
			});
		}
	}
}
