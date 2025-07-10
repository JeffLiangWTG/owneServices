using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SriLankaComplianceInfo))]
	sealed class SriLankaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.SriLanka;

		#region IComplianceSubTypeCodeProvider

		protected override string[] ExpectedComplianceSubTypes => new string[] { "TCR", "TXI", "STX", "STC", "NTI", "NCR" };

		protected override string[] ExpectedReceivablesComplianceSubTypes => new string[] { "TCR", "TXI", "STX", "STC", "NTI", "NCR" };

		protected override string[] ExpectedPayablesComplianceSubTypes => new string[] { "TCR", "TXI", "STX", "STC", "NTI", "NCR" };

		protected override string[] ExpectedReceivablesInvoiceComplianceSubTypes => new string[] { "TCR", "TXI", "STX", "STC", "NTI", "NCR" };

		protected override string[] ExpectedPayablesInvoiceComplianceSubTypes => new string[] { "TCR", "TXI", "STX", "STC", "NTI", "NCR" };

		protected override string[] ExpectedReceivablesCreditNoteComplianceSubTypes => new string[] { "TCR", "TXI", "STX", "STC", "NTI", "NCR" };

		protected override string[] ExpectedPayablesCreditNoteComplianceSubTypes => new string[] { "TCR", "TXI", "STX", "STC", "NTI", "NCR" };

		protected override string ExpectedComplianceSubTypeCodeForDescriptionTest => "STX";

		protected override string ExpectedComplianceSubTypeDescription => "Suspended Tax Invoice";

		protected override string ExpectedComplianceSubTypeLocalDescription => "Suspended Tax Invoice";

		#endregion

		protected override string ExpectedComplianceRules => @"LK,TXI,AR,INV,TXS,ALL,OTO,,,ALL,1,TXI, TCR, STX, STC,,,,,,
LK,TXI,AR,INV,TXS,ALL,ARO,,TXI,ALL,1,TXI, TCR, STX, STC,,,,,,
LK,TXI,AR,INV,TXS,ALL,ARO,,TCR,ALL,1,TXI, TCR, STX, STC,,,,,,
LK,TCR,AR,CRD,TXS,ALL,OTO,,,ALL,1,TXI, TCR, STX, STC,,,,,,
LK,TCR,AR,CRD,TXS,ALL,ARO,,TXI,ALL,1,TXI, TCR, STX, STC,,,,,,
LK,STX,AR,INV,SUS,ALL,OTO,,,SVT,1,TXI, TCR, STX, STC,,,,,,
LK,STX,AR,INV,TID,ALL,ARO,,STX,SVT,1,TXI, TCR, STX, STC,,,,,,
LK,STX,AR,INV,TID,ALL,ARO,,STC,SVT,1,TXI, TCR, STX, STC,,,,,,
LK,STC,AR,CRD,SUS,ALL,OTO,,,SVT,1,TXI, TCR, STX, STC,,,,,,
LK,STC,AR,CRD,TID,ALL,ARO,,STX,SVT,1,TXI, TCR, STX, STC,,,,,,
LK,NTI,AR,INV,TXS,ALL,ALL,,,NON,1,TXI, TCR, STX, STC,,,,,,
LK,NTI,AR,INV,SUS,ALL,ALL,,,NON,1,TXI, TCR, STX, STC,,,,,,
LK,NCR,AR,CRD,TXS,ALL,ALL,,,NON,1,TXI, TCR, STX, STC,,,,,,
LK,NCR,AR,CRD,SUS,ALL,ALL,,,NON,1,TXI, TCR, STX, STC,,,,,,
LK,NTI,AR,INV,EXL,ALL,OTO,,,ALL,1,TXI, TCR, STX, STC,,,,,,
LK,NCR,AR,CRD,EXL,ALL,OTO,,,ALL,1,TXI, TCR, STX, STC,,,,,,
LK,NTI,AR,INV,EXL,ALL,ARO,,NTI,ALL,1,TXI, TCR, STX, STC,,,,,,
LK,NCR,AR,CRD,EXL,ALL,ARO,,NTI,ALL,1,TXI, TCR, STX, STC,,,,,,
LK,NTI,AR,INV,STI,ALL,ALL,,,ALL,1,TXI, TCR, STX, STC,,,,NOTREPORT,,
LK,NCR,AR,CRD,STI,ALL,ALL,,,ALL,1,TXI, TCR, STX, STC,,,,NOTREPORT,,";

		public void TestGetTaxRegistrationTypeList()
		{
			var list = CountryComplianceFactory.GetIComplianceSubTypeAdditionalTaxRegistrationTypeListProvider(CountryCode)?.GetTaxRegistrationTypeList();
			AssertEquals("MexicoComplianceInfo.GetTaxRegistrationTypeList", 3, list.Count);
			Assert(list.ContainsCode("ALL"));
			Assert(list.ContainsCode("SVT"));
			Assert(list.ContainsCode("NON"));
		}
	}
}
