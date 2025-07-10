using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(GuatemalaComplianceInfo))]
	sealed class GuatemalaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.Guatemala;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL", "XCR" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL", "XCR" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL", "XCR" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL", "XCR" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL", "XCR" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL", "XCR" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL", "XCR" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TCD";

		protected override string ExpectedComplianceSubTypeDescription => "Debit Note";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Nota de D\u00e9bito";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;
	}
}
