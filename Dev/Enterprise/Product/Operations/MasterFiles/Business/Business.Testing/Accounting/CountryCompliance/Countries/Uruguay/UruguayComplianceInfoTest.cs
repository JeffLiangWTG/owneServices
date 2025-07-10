using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing.Uruguay
{
	[TestedType(typeof(UruguayComplianceInfo))]
	sealed class UruguayComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Uruguay;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TXI", "TKT", "TCD", "TKD", "TCR", "TKC", "YXI", "YKT", "YCD", "YKD", "YCR", "YKR", "XCL" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TXI", "TKT", "TCD", "TKD", "TCR", "TKC", "YXI", "YKT", "YCD", "YKD", "YCR", "YKR", "XCL" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "YXI", "YCD", "YCR", "XCL" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TXI", "TKT", "TCD", "TKD", "YXI", "YKT", "YCD", "YKD", "XCL" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TXI", "TCD", "YXI", "YCD", "XCL" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TCR", "TKC", "YKR", "YCR", "XCL" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TCR", "YCR", "XCL" };

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => new string[] { "TXI", "TKT", "TCD", "TKD", "TCR", "TKC", "YXI", "YKT", "YCD", "YKD", "YCR", "YKR" };

		protected override bool ComplianceDateDependOfIsProductionSystem => true;

		protected override ZDate ExpectedEInvoicingComplianceDate => IsProductionSystem ? new ZDate(2022, 8, 1) : new ZDate(2022, 7, 1);

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new Dictionary<string, LedgerOfUse> {
		{ "TKT", LedgerOfUse.AR }, { "TKD", LedgerOfUse.AR }, { "TKC", LedgerOfUse.AR }, { "YKT", LedgerOfUse.AR },
		{ "YKD", LedgerOfUse.AR }, { "YKR", LedgerOfUse.AR } };

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse> {
			{ "YCD", TransactionTypeOfUse.INV }, { "YCR", TransactionTypeOfUse.CRD }, { "YKR", TransactionTypeOfUse.CRD }, { "YKD", TransactionTypeOfUse.INV },
			{ "YKT", TransactionTypeOfUse.INV }, { "YXI", TransactionTypeOfUse.INV }, { "TXI", TransactionTypeOfUse.INV }, { "TKT", TransactionTypeOfUse.INV },
			{ "TCR", TransactionTypeOfUse.CRD }, { "TKC", TransactionTypeOfUse.CRD }, { "TCD", TransactionTypeOfUse.INV }, { "TKD", TransactionTypeOfUse.INV }, { "XCL", TransactionTypeOfUse.ALL }
		};

		protected override IReadOnlyDictionary<string, string> ExpectedComplianceDocumentNumberAllocationOverride
			=> new Dictionary<string, string>
			{
						{ "YXI", "PST" },
						{ "YCD", "PST" },
						{ "YCR", "PST" },
						{ "YKT", "PST" },
						{ "YKD", "PST" },
						{ "YKR", "PST" },
						{ "XCL", "PST" }
			};

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TCD";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Debit Note";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Nota de D\u00e9bito de e-Factura";

		protected override string ExpectedComplianceRules => @"UY,TXI,AR,INV,TID,ALL,OTO,,,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,UY,,,,
UY,TCD,AR,INV,TID,ALL,ARO,,TCR,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,UY,,,,
UY,TCD,AR,INV,TID,ALL,ARO,,TXI,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,UY,,,,
UY,TCR,AR,CRD,TID,ALL,ARO,,TCD,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,UY,,,,
UY,TCR,AR,CRD,TID,ALL,ARO,,TXI,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,UY,,,,
UY,TCR,AR,CRD,TID,ALL,OTO,,,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,UY,,,,
UY,XCL,AR,INV,EXL,ALL,OTO,,,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,UY,,,,
UY,XCL,AR,INV,EXL,ALL,ARO,,XCL,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,UY,,,,
UY,XCL,AR,CRD,EXL,ALL,OTO,,,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,UY,,,,
UY,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,UY,,,,
UY,TKT,AR,INV,TID,ALL,OTO,,,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,TKD,AR,INV,TID,ALL,ARO,,TKT,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,TKD,AR,INV,TID,ALL,ARO,,TKC,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,TKC,AR,CRD,TID,ALL,OTO,,,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,TKC,AR,CRD,TID,ALL,ARO,,TKD,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,TKC,AR,CRD,TID,ALL,ARO,,TKT,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,XCL,AR,INV,EXL,ALL,OTO,,,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,XCL,AR,INV,EXL,ALL,ARO,,XCL,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,XCL,AR,CRD,EXL,ALL,OTO,,,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,TXI,AP,INV,TID,ALL,OTO,,,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,TCD,AP,INV,TID,ALL,ARO,,TXI,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,TCD,AP,INV,TID,ALL,ARO,,TCR,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,TCR,AP,CRD,TID,ALL,OTO,,,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,TCR,AP,CRD,TID,ALL,ARO,,TXI,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,TCR,AP,CRD,TID,ALL,ARO,,TCD,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,XCL,AP,INV,EXL,ALL,OTO,,,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,XCL,AP,INV,EXL,ALL,ARO,,XCL,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,XCL,AP,CRD,EXL,ALL,OTO,,,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,XCL,AP,CRD,EXL,ALL,ARO,,XCL,,1,CFE Comprobantes Fiscales Electrónicos (Fiscal Electronic Documents),,,,,,
UY,YXI,AR,INV,TID,ALL,OTO,,,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,UY,,,,
UY,YCD,AR,INV,TID,ALL,ARO,,YXI,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,UY,,,,
UY,YCD,AR,INV,TID,ALL,ARO,,YCR,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,UY,,,,
UY,YCR,AR,CRD,TID,ALL,OTO,,,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,UY,,,,
UY,YCR,AR,CRD,TID,ALL,ARO,,YXI,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,UY,,,,
UY,YCR,AR,CRD,TID,ALL,ARO,,YCD,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,UY,,,,
UY,XCL,AR,INV,EXL,ALL,OTO,,,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,UY,,,,
UY,XCL,AR,INV,EXL,ALL,ARO,,XCL,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,UY,,,,
UY,XCL,AR,CRD,EXL,ALL,OTO,,,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,UY,,,,
UY,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,UY,,,,
UY,YKT,AR,INV,TID,ALL,OTO,,,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,YKD,AR,INV,TID,ALL,ARO,,YKT,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,YKD,AR,INV,TID,ALL,ARO,,YKR,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,YKR,AR,CRD,TID,ALL,OTO,,,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,YKR,AR,CRD,TID,ALL,ARO,,YKT,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,YKR,AR,CRD,TID,ALL,ARO,,YKD,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,XCL,AR,INV,EXL,ALL,OTO,,,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,XCL,AR,INV,EXL,ALL,ARO,,XCL,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,XCL,AR,CRD,EXL,ALL,OTO,,,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,XCL,AR,CRD,EXL,ALL,ARO,,XCL,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,TXI,AP,INV,TID,ALL,OTO,,,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,TCD,AP,INV,TID,ALL,ARO,,TXI,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,TCD,AP,INV,TID,ALL,ARO,,TCR,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,TCR,AP,CRD,TID,ALL,OTO,,,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,TCR,AP,CRD,TID,ALL,ARO,,TXI,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,TCR,AP,CRD,TID,ALL,ARO,,TCD,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,XCL,AP,INV,EXL,ALL,OTO,,,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,XCL,AP,INV,EXL,ALL,ARO,,XCL,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,XCL,AP,CRD,EXL,ALL,OTO,,,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,
UY,XCL,AP,CRD,EXL,ALL,ARO,,XCL,,2,CFC Comprobantes Fiscales de Contingencia (Fiscal Contingency Documents),,,,,,";

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Uruguay;

		#region TestComplianceSubTypeRulesWithMultipleRuleSetProvider

		public void TestComplianceSubTypeAttributionRuleSetDefaultValue_Uruguay()
		{
			var ruleSetProvider = CountryComplianceFactory.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(CountryCode);

			AssertEquals(UruguayComplianceInfo.RuleSetCodes.TipoCFE, ruleSetProvider.GetDefaultRuleSet());

			var ruleSet = ruleSetProvider.GetRuleSet();
			AssertEquals(2, ruleSet.Count);
			Assert(ruleSet.ContainsCode(UruguayComplianceInfo.RuleSetCodes.TipoCFE));
			Assert(ruleSet.ContainsCode(UruguayComplianceInfo.RuleSetCodes.TipoCFC));
		}

		#endregion

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingEnabled
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate;

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingDisabled
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print;

		protected override IReadOnlyDictionary<(string code, bool isEInvoicingEnabled), string> GetExpectedResultsForValidateComplianceDocumentNumberAllocation_ReceivablesRegistry()
			=> new Dictionary<(string code, bool isEInvoicingEnabled), string>
			{
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  true), "Cannot change default value for country/region 'UY'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    true), "Cannot change default value for country/region 'UY'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   true), "Cannot change default value for country/region 'UY'." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, false), $"'GVT' is not valid for country/region 'UY' when registry '{AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Caption}' is not enabled." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  false), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    false), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   false), string.Empty },
			};

		protected override IReadOnlyDictionary<(string code, bool isEInvoicingEnabled), string> GetExpectedResultsForValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry()
			=> new Dictionary<(string code, bool isEInvoicingEnabled), string>
			{
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   true), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, false), $"'GVT' is not valid for country/region 'UY' when registry '{AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Caption}' is not enabled." },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual,                  false), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,                    false), string.Empty },
				{ (AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print,                   false), string.Empty },
			};
	}
}
