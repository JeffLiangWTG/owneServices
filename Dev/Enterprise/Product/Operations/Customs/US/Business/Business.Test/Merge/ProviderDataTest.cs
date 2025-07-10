using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconDerivedDutyCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2019, 04, 28)]
		public void TestDerivedComputationForRecon()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var reconDec = new ReconDeclaration(declaration);
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var originalEntry = reconDec.OriginalEntries.AddNew();

			originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;

			var invoice = originalEntry.Invoice;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_CH_ReconEntry = originalEntry.CH_PK;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8215.20.0000";
			invoiceLine1.JI_LinePrice = 1512m;
			invoiceLine1.JI_CustomsQuantity = 689m;
			AssertEquals("PreCondition:Derived Computation", ComputationCodeList.Codes.Derived, invoiceLine1.ImportTariff.UE_DutyComputationCode);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_Tariff = "8211.92.9060";
			invoiceLine2.JI_LinePrice = 0m;
			invoiceLine2.JI_CustomsQuantity = 336m;
			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals("0.4c/NO + 6.1%, 1512*.061 + 336*.004", 93.58m, invoiceLine1.US_Duty);
			AssertEquals(0m, invoiceLine2.US_Duty);

			invoiceLine1.JI_LinePrice = 16000m;
			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals("0.4c/NO + 6.1%, 16000*.061 + 336*.004", 977.34m, invoiceLine1.US_Duty);
			AssertEquals(0m, invoiceLine2.US_Duty);
		}
	}
}
