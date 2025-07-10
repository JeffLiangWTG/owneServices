using System.Collections.Generic;
using Enterprise.Integration.Compliance;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(EcuadorComplianceInfo))]
	sealed class EcuadorComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Ecuador;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TCD", "TCR", "TXI", "TXV", "XCL" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TCD", "TCR", "TXI", "XCL" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TCD", "TCR", "TXI", "TXV", "XCL" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TCD", "TCR", "TXI", "XCL" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TCD", "TCR", "TXI", "TXV", "XCL" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TCD", "TCR", "TXI", "XCL" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TCD", "TCR", "TXI", "TXV", "XCL" };

		protected override Dictionary<string, LedgerOfUse> ExpectedComplianceLedgerOfUse => new Dictionary<string, LedgerOfUse> { { "TXV", LedgerOfUse.AP } };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TCD";

		protected override string ExpectedComplianceSubTypeDescription => "Debit Note";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Nota de D\u00e9bito";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;
	}
}
