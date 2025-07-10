
using System.Collections.Generic;
using Enterprise.Integration.Compliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(PanamaComplianceInfo))]
	sealed class PanamaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Panama;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TXI", "TCR", "TCD", "XCL" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TXI", "TCR", "TCD", "XCL" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TXI", "TCR", "TCD", "XCL" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TXI", "TCD", "XCL" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TXI", "TCD", "XCL" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TCR", "XCL" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TCR", "XCL" };

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse>
		{
			{ "TXI", TransactionTypeOfUse.INV },
			{ "TCR", TransactionTypeOfUse.CRD },
			{ "TCD", TransactionTypeOfUse.INV },
			{ "XCL", TransactionTypeOfUse.ALL },
		};

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TCR";

		protected override string ExpectedComplianceSubTypeDescription => "Credit Note";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Nota de Cr\u00e9dito";

		#region ICountryComplianceElectronicInvoiceEligibleSubType

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => new string[] { "TXI", "TCR", "TCD", "XCL" };

		#endregion

		protected override IEnumerable<CodeDescriptionBoolRelatedItem> ExpectedTaxMessageGroups => new CodeDescriptionBoolRelatedItem[]
{
			new CodeDescriptionBoolRelatedItem() { Code = "I0", Description = (NoResString)"ITBMS Exento", Bool = true, RelatedItemCode = "00" },
			new CodeDescriptionBoolRelatedItem() { Code = "I7", Description = (NoResString)"ITBMS 7%", Bool = true, RelatedItemCode = "01" },
			new CodeDescriptionBoolRelatedItem() { Code = "I10", Description = (NoResString)"ITBMS 10%", Bool = true, RelatedItemCode = "02" },
			new CodeDescriptionBoolRelatedItem() { Code = "I15", Description = (NoResString)"ITBMS 15%", Bool = false, RelatedItemCode = "03" },
};

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue_Panama()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(PanamaComplianceInfo.RuleSetCodes.TXITCRTCDAndXCL, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(1, ruleSet.Count);
			Assert(ruleSet.ContainsCode(PanamaComplianceInfo.RuleSetCodes.TXITCRTCDAndXCL));
		}

		protected override string ExpectedComplianceRules => @"
PA,TXI,AR,INV,TID,ALL,OTO,,,,1,TXI, TCR, TCD & XCL,,,,,,
PA,TCR,AR,CRD,TID,ALL,OTO,,,,1,TXI, TCR, TCD & XCL,,,,,,
PA,TCR,AR,CRD,TID,ALL,ARO,,TXI,,1,TXI, TCR, TCD & XCL,,,,,,
PA,TCR,AR,CRD,TID,ALL,ARO,,TCD,,1,TXI, TCR, TCD & XCL,,,,,,
PA,TCD,AR,INV,TID,ALL,ARO,,TXI,,1,TXI, TCR, TCD & XCL,,,,,,
PA,TCD,AR,INV,TID,ALL,ARO,,TCR,,1,TXI, TCR, TCD & XCL,,,,,,
PA,XCL,AR,INV,EXL,ALL,OTO,,,,1,TXI, TCR, TCD & XCL,,,,,,
PA,XCL,AR,INV,EXL,ALL,ARO,,XCL,,1,TXI, TCR, TCD & XCL,,,,,,
PA,XCL,AR,CRD,EXL,ALL,OTO,,,,1,TXI, TCR, TCD & XCL,,,,,,
PA,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,1,TXI, TCR, TCD & XCL,,,,,,
PA,XCL,AP,INV,EXL,ALL,ALL,,,,1,TXI, TCR, TCD & XCL,,,,,,
PA,XCL,AP,CRD,EXL,ALL,ALL,,,,1,TXI, TCR, TCD & XCL,,,,,,";

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Panama;

		protected override string ExpectedGovernmentAllocatedNumberColumnName => AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number;

		#region IComplianceRegistryDefaultProvider

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingEnabled => AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

		#endregion
	}
}
