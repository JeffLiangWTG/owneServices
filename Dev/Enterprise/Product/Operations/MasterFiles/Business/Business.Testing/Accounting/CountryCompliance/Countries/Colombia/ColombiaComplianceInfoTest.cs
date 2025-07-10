using System.Collections.Generic;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance.Countries.Colombia;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(ColombiaComplianceInfo))]
	sealed class ColombiaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.Colombia;

		#region IComplianceSubTypeCodeProvider

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TCR", "TXI", "TDR", "DSO" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TCR", "TXI", "TDR" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TCR", "TXI", "TDR", "DSO" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TXI", "TDR" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TXI", "TDR", "DSO" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TCR" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TCR" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TCR";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Credit Note";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Nota De Credito";

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new Dictionary<string, LedgerOfUse> { { "DSO", LedgerOfUse.AP } };

		protected override IReadOnlyDictionary<string, TransactionTypeOfUse> ExpectedComplianceTransactionTypeOfUse => new Dictionary<string, TransactionTypeOfUse> {
		{ "TCR", TransactionTypeOfUse.CRD }, { "TXI", TransactionTypeOfUse.INV }, { "TDR", TransactionTypeOfUse.INV }, { "DSO", TransactionTypeOfUse.INV } };

		#endregion

		protected override string ExpectedComplianceRules => @"CO,TXI,AR,INV,TID,ALL,OTO,,,,,,,,,,,
CO,TCR,AR,CRD,TID,ALL,OTO,,,,,,,,,,,
CO,TCR,AR,CRD,TID,ALL,ARO,,TXI,,,,,,,,,
CO,TDR,AR,INV,TID,ALL,ARO,,TXI,,,,,,,,,
";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		protected override string ExpectedDefaultValueForComplianceDocumentNumberAllocation_ReceivablesRegistry_EInvoicingEnabled
			=> AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

		#region ICountryComplianceElectronicInvoiceEligibleSubType

		protected override string[] ExpectedEInvoiceEligibleComplianceSubType => new string[] { "TXI", "TDR", "TCR" };

		#endregion

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Colombia;

		protected override string ExpectedGovernmentAllocatedNumberColumnName => AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number;

		#region ExtraTaxRate

		public override void TestHasExtraTaxInfo()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(true, complianceInfo.HasExtraTaxInfo());
		}

		public override void TestGetExtraTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("RET Amt", complianceInfo.GetExtraTaxOSAmountCaption().ShortCaption);
			AssertEquals("RET Amount", complianceInfo.GetExtraTaxOSAmountCaption().Caption);
		}

		public override void TestGetExtraTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("RET Local", complianceInfo.GetExtraTaxLocalAmountCaption().Caption);
		}

		public override void TestGetTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("IVA Amt", complianceInfo.GetTaxOSAmountCaption().ShortCaption);
			AssertEquals("IVA Amount", complianceInfo.GetTaxOSAmountCaption().Caption);
		}

		public override void TestGetTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("IVA Local", complianceInfo.GetTaxLocalAmountCaption().Caption);
		}

		#endregion

		#region IComplianceSequencesValidationProvider

		public void TestIsPrintingAuthorizationNumberLengthValid()
		{
			var complianceSequenceValidatorProvider = GetCountryComplianceInfo as IComplianceSequenceValidationProvider;

			AssertEquals(expected: true, complianceSequenceValidatorProvider.IsPrintingAuthorizationNumberLengthValid("anyValue"));
		}

		public void TestIsPrintingAuthorizationNumberFormatValid()
		{
			var complianceSequenceValidatorProvider = GetCountryComplianceInfo as IComplianceSequenceValidationProvider;

			AssertEquals(expected: true, complianceSequenceValidatorProvider.IsPrintingAuthorizationNumberFormatValid("anyValue"));
		}

		public void TestGetAccComplianceSequenceValidation()
		{
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			var complianceSequenceValidatorProvider = GetCountryComplianceInfo as IComplianceSequenceValidationProvider;
			var complianceSequenceValidation = complianceSequenceValidatorProvider.GetAccComplianceSequenceValidation(complianceSequence);

			AssertType(typeof(AccComplianceSequenceColombiaValidation), complianceSequenceValidation);
		}

		#endregion IComplianceSequencesValidationProvider
	}
}
