using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(StampDutyLedgerDocumentWrapper))]
	public class StampDutyLedgerDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestWrapper()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_PeriodStartDate = CargoWise.Types.ZDate.Today;
			statementHeader.B2_PeriodEndDate = CargoWise.Types.ZDate.Today.AddDays(1);
			statementHeader.B2_PrintDate = CargoWise.Types.ZDate.Today;

			var statementLine = statementHeader.StatementLines.AddNew();
			var statementLine2 = statementHeader.StatementLines.AddNew();

			var statementLineCharge = statementLine.Charges.AddNew();
			statementLineCharge.B4_ReferenceNumber = "TEST01";
			var statementLineCharge2 = statementLine.Charges.AddNew();
			statementLineCharge2.B4_ReferenceNumber = "TEST02";

			var wrapper = new StampDutyLedgerDocumentWrapper(statementHeader);

			CombineAssertions(() =>
			{
				AssertEquals("B2_PeriodStartDate", statementHeader.B2_PeriodStartDate.ToString("dd/MM/yyyy"), wrapper.PeriodStartDate);
				AssertEquals("B2_PeriodEndDate", statementHeader.B2_PeriodEndDate.ToString("dd/MM/yyyy"), wrapper.PeriodEndDate);
				AssertEquals("B2_PrintDate", statementHeader.B2_PrintDate.ToString("dd/MM/yyyy"), wrapper.PrintDate);
				AssertEquals("B4_ReferenceNumber", statementLineCharge.B4_ReferenceNumber, wrapper.Charges[0].ChargeReferenceNumber);
				AssertEquals("B4_ReferenceNumber", statementLineCharge2.B4_ReferenceNumber, wrapper.Charges[1].ChargeReferenceNumber);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			return new StampDutyLedgerDocumentWrapper(statementHeader);
		}
	}
}
