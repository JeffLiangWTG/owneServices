using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(PolandComplianceInfo))]
	sealed class PolandComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Poland;

		protected override string[] ExpectedComplianceSubTypes => new string[] { "DCR", "DSB", "TCD", "TXI" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "DCR", "DSB", "TCD", "TXI" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "DCR", "DSB", "TCD", "TXI" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "DCR", "DSB", "TCD", "TXI" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "DCR", "DSB", "TCD", "TXI" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "DCR", "DSB", "TCD", "TXI" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "DCR", "DSB", "TCD", "TXI" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "TXI";

		protected override string ExpectedComplianceSubTypeDescription => "Tax Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "FAKTURA VAT";

		protected override string ExpectedComplianceRules => @"PL,TXI,AR,INV,TID,NDB,OTO,,,,1,Default Poland Rule Set,,,,,,
PL,TXI,AR,INV,AMT,DSB,OTO,,,,1,Default Poland Rule Set,,,,,,
PL,TCD,AR,INV,TID,NDB,ARO,,,,1,Default Poland Rule Set,,,,,,
PL,TCD,AR,CRD,TID,NDB,ARO,,,,1,Default Poland Rule Set,,,,,,
PL,TCD,AR,INV,TXA,DSB,ARO,,,,1,Default Poland Rule Set,,,,,,
PL,TCD,AR,CRD,TXA,DSB,ARO,,,,1,Default Poland Rule Set,,,,,,
PL,DSB,AR,INV,TXX,DSB,ALL,,,,1,Default Poland Rule Set,,,,,,
PL,DCR,AR,CRD,TXX,DSB,ALL,,,,1,Default Poland Rule Set,,,,,,";

		protected override string ExpectedGovernmentAllocatedNumberColumnName => AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number;

		protected override string ExpectedAccTransactionHeaderAuthorisationRecordType => AccTransactionHeaderAuthorisationRecordTypes.Poland;
	}
}
