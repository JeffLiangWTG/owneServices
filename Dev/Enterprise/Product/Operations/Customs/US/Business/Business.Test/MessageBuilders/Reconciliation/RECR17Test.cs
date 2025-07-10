using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Moq;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class RECR17Test : ReconciliationMessageBuilderAbstractTest<IReconciliation>
	{
		public void TestPopulate()
		{
			var mock = new Mock<IReconciliation>();
			mock.Setup(m => m.PaymentTypeIndicator).Returns(PaymentTypeList.Codes.IndividualBasis);
			mock.Setup(m => m.PreliminaryStatementPrintDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.ClientBranchDesignation).Returns("CB");
			mock.Setup(m => m.DutyPaymentAmount).Returns(2.2m);
			mock.Setup(m => m.TaxPaymentAmount).Returns(3.3m);
			mock.Setup(m => m.FeePaymentAmount).Returns(4.4m);
			mock.Setup(m => m.InterestPaymentAmount).Returns(5.5m);

			var r17 = RECR17Populator.Populate(mock.Object);

			AssertEquals("1", r17.PaymentTypeIndicator);
			AssertEquals(ZDate.BrettsBirthday, r17.PreliminaryStatementPrintDate);
			AssertEquals("CB", r17.ClientBranchDesignation);

			AssertEquals(2.2m, r17.DutyPaymentAmount);
			AssertEquals(3.3m, r17.TaxPaymentAmount);
			AssertEquals(4.4m, r17.FeePaymentAmount);
			AssertEquals(5.5m, r17.InterestPaymentAmount);
		}

		public void TestPopulateWithNegative()
		{
			var mock = new Mock<IReconciliation>();
			mock.Setup(m => m.PaymentTypeIndicator).Returns(PaymentTypeList.Codes.IndividualBasis);
			mock.Setup(m => m.PreliminaryStatementPrintDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.ClientBranchDesignation).Returns("CB");
			mock.Setup(m => m.DutyPaymentAmount).Returns(-2.2m);
			mock.Setup(m => m.TaxPaymentAmount).Returns(-3.3m);
			mock.Setup(m => m.FeePaymentAmount).Returns(-4.4m);
			mock.Setup(m => m.InterestPaymentAmount).Returns(-5.5m);

			var r17 = RECR17Populator.Populate(mock.Object);
			AssertEquals(0m, r17.DutyPaymentAmount);
			AssertEquals(0m, r17.TaxPaymentAmount);
			AssertEquals(0m, r17.FeePaymentAmount);
			AssertEquals(0m, r17.InterestPaymentAmount);
		}
	}
}
