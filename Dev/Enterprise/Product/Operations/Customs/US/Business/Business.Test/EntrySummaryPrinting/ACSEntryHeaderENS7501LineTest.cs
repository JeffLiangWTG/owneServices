using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting.Testing
{
	[TestedType(typeof(ACSEntryHeaderENS7501Line))]
	sealed class ACSEntryHeaderENS7501LineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestADDSpecificRate()
		{
			var caseRecord = Factory.New<USCACCase>();
			caseRecord.U5_CaseNumber = "A342089";
			caseRecord.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var caseRate = caseRecord.CaseRates.AddNew();
			caseRate.U6_AdValoremRate = 0.123m;
			caseRate.U6_SpecificRate = 0.02m;
			caseRate.U6_EffectiveDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "ABC";
			declaration.US_EnableENS = true;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 123620m;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A342089";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var printBO = new ACSEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("12.30%", printBO.ADDRate);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new ACSEntryHeaderENS7501Line(Factory.New<CusEntryLine>(), false, false);
		}
	}
}
