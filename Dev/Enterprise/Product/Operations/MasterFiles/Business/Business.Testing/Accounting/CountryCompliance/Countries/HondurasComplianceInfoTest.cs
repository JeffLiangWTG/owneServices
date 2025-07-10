using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(HondurasComplianceInfo))]
	sealed class HondurasComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Honduras;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TXI", "TCD", "TCR", "XCL" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TXI";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Factura";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;
	}
}
