using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	[TestedType(typeof(PeruComplianceInfo))]
	sealed class PeruComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Peru;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "CAE", "CMA", "DSB", "HON", "MQR", "NCD", "NCR", "NXI", "OTR", "SSP", "TBC", "TBO", "TCD", "TCR", "TXI" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "CAE", "CMA", "DSB", "HON", "MQR", "NCD", "NCR", "NXI", "OTR", "SSP", "TBC", "TBO", "TCD", "TCR", "TXI" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "CAE", "CMA", "DSB", "HON", "MQR", "NCD", "NCR", "NXI", "OTR", "SSP", "TBC", "TBO", "TCD", "TCR", "TXI" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "CAE", "CMA", "DSB", "HON", "MQR", "NCD", "NCR", "NXI", "OTR", "SSP", "TBC", "TBO", "TCD", "TCR", "TXI" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "CAE", "CMA", "DSB", "HON", "MQR", "NCD", "NCR", "NXI", "OTR", "SSP", "TBC", "TBO", "TCD", "TCR", "TXI" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "CAE", "CMA", "DSB", "HON", "MQR", "NCD", "NCR", "NXI", "OTR", "SSP", "TBC", "TBO", "TCD", "TCR", "TXI" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "CAE", "CMA", "DSB", "HON", "MQR", "NCD", "NCR", "NXI", "OTR", "SSP", "TBC", "TBO", "TCD", "TCR", "TXI" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "CAE";

		protected override string ExpectedComplianceSubTypeDescription => "AIR TRANSPORT INVOICE";

		protected override string ExpectedComplianceSubTypeLocalDescription => "CARTA DE PORTE AERO NACIONAL";
	}
}
