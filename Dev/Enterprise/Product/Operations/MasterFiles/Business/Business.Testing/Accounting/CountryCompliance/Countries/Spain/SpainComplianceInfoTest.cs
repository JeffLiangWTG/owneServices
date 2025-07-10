using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SpainComplianceInfo))]
	sealed class SpainComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Spain;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TXI", "TCR", "TCD", "XCL" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TXI", "TCR", "TCD", "XCL" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TXI", "TCR", "TCD", "XCL" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TXI", "TCR", "TCD", "XCL" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TXI", "TCR", "TCD", "XCL" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TXI", "TCR", "TCD", "XCL" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TXI", "TCR", "TCD", "XCL" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TXI";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Factura";

		protected override string ExpectedComplianceRules => @"ES,TXI,AR,INV,TID,ALL,OTO,,,,1,Tax Documents and Excluded Supply,,,,,,
ES,TCR,AR,CRD,TID,ALL,ARO,,TXI,,1,Tax Documents and Excluded Supply,,,,,,
ES,TCD,AR,INV,TID,ALL,ARO,,TXI,,1,Tax Documents and Excluded Supply,,,,,,
ES,XCL,AR,INV,EXL,ALL,OTO,,,,1,Tax Documents and Excluded Supply,,,,,,
ES,XCL,AR,INV,EXL,ALL,ARO,,XCL,,1,Tax Documents and Excluded Supply,,,,,,
ES,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,1,Tax Documents and Excluded Supply,,,,,,
ES,XCL,AR,CRD,EXL,ALL,OTO,,,,1,Tax Documents and Excluded Supply,,,,,,
";

		protected override bool ComplianceDateDependOfIsProductionSystem => true;

		protected override ZDate ExpectedEInvoicingComplianceDate => IsProductionSystem ? new ZDate(2022, 7, 1) : new ZDate(2022, 4, 1);

		protected override ZDate ExpectedEInvoicingComplianceDateForPayables => ExpectedEInvoicingComplianceDate;

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Spain;

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue_Spain()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(SpainComplianceInfo.RuleSetCodes.TaxDocumentsAndExcludedSupply, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(1, ruleSet.Count);
			Assert(ruleSet.ContainsCode(SpainComplianceInfo.RuleSetCodes.TaxDocumentsAndExcludedSupply));
		}

		public override void TestEInvoiceEliglibleComplianceSubTypes()
		{
			if (!typeof(IComplianceInfoElectronicInvoicingEligibleSubType).IsAssignableFrom(TestsInstancesOfType))
			{
				Assert("Test is not applicable", true);
				return;
			}
			CombineAssertions("Preconditions", () =>
			{
				Assert("ExpectedComplianceSubTypes.Length == 0", ExpectedEInvoiceEligibleComplianceSubType.Length == 0);
			});
			AssertContainsExactElementsInAnyOrder(ExpectedEInvoiceEligibleComplianceSubType, CountryComplianceFactory.GetICountryEligibleComplianceSubTypeForEInvoice(CountryCode).GetEligibleComplianceSubTypeListForEInvoicing());
		}

		protected override IEnumerable<CodeDescriptionBoolRelatedItem> ExpectedTaxMessageGroups =>
			new CodeDescriptionBoolRelatedItem[]
			{
				new CodeDescriptionBoolRelatedItem() { Code = "A", Description = (NoResString)"Subject to IVA - Not Exempt", Bool = true, RelatedItemCode = "01S1" },
				new CodeDescriptionBoolRelatedItem() { Code = "B", Description = (NoResString)"Reverse Charge IVA (except Intracommunity purchases)", Bool = true, RelatedItemCode = "01S2" },
				new CodeDescriptionBoolRelatedItem() { Code = "C", Description = (NoResString)"Subject to IGIC/IPSI", Bool = true, RelatedItemCode = "08N2" },
				new CodeDescriptionBoolRelatedItem() { Code = "D", Description = (NoResString)"AR Only - Exempt - Export E1 (Art 20)", Bool = true, RelatedItemCode = "02E1" },
				new CodeDescriptionBoolRelatedItem() { Code = "E", Description = (NoResString)"AR Only - Exempt - Export E2 (Art 21)", Bool = true, RelatedItemCode = "02E2" },
				new CodeDescriptionBoolRelatedItem() { Code = "F", Description = (NoResString)"AR Only - Exempt - Export E3 (Art 22)", Bool = true, RelatedItemCode = "02E3" },
				new CodeDescriptionBoolRelatedItem() { Code = "G", Description = (NoResString)"AR Only - Exempt - E4 (Art 24)", Bool = true, RelatedItemCode = "01E4" },
				new CodeDescriptionBoolRelatedItem() { Code = "H", Description = (NoResString)"AR Only - Exempt - Other E6", Bool = true, RelatedItemCode = "01E6" },
				new CodeDescriptionBoolRelatedItem() { Code = "I", Description = (NoResString)"AR Only - Not Subject to IVA - Art 7 and 14", Bool = true, RelatedItemCode = "01N1" },
				new CodeDescriptionBoolRelatedItem() { Code = "J", Description = (NoResString)"AR Only - Not Subject to IVA - Location Rules", Bool = true, RelatedItemCode = "01N2" },
				new CodeDescriptionBoolRelatedItem() { Code = "K", Description = (NoResString)"AR Only - Collection on behalf of third parties", Bool = true, RelatedItemCode = "10N1" },
				new CodeDescriptionBoolRelatedItem() { Code = "L", Description = (NoResString)"AP Only - Intracommunity Purchases", Bool = true, RelatedItemCode = "09S1" },
				new CodeDescriptionBoolRelatedItem() { Code = "M", Description = (NoResString)"AP Only - Leasing operations of business premises", Bool = true, RelatedItemCode = "12S1" },
				new CodeDescriptionBoolRelatedItem() { Code = "X", Description = (NoResString)"Exclude from reporting", Bool = true, RelatedItemCode = "XX" },
			};
	}
}
