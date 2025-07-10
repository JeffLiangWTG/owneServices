using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Moq;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class AutomatedClearinghouseMessageBuilderTestCase : TestCaseWithFactory
	{
		public void TestGenerate()
		{
			var paymentMock = new Mock<IPaymentAuthorisation>();
			paymentMock.Setup(m => m.PayersUnitNumber).Returns("101009");
			paymentMock.Setup(m => m.StatementFiler).Returns("JSD");
			paymentMock.Setup(m => m.StatementBillNumber).Returns("43131398549");
			paymentMock.Setup(m => m.PaymentAmount).Returns(435.23m);
			paymentMock.Setup(m => m.ProcessingPortCode).Returns("2904");
			paymentMock.Setup(m => m.NegationCode).Returns(" ");
			paymentMock.Setup(m => m.NegationDate).Returns(new ZDate(ZDateTime.Empty));

			IPaymentAuthorisation payment = paymentMock.Object;
			var builder = new AutomatedClearinghouseMessageBuilder(Factory);
			var message = builder.Generate<PDSPT>(ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation, payment, "01");
			AssertEquals("B  2904JSDRM                                               <<MSGNO PLACEHOLDER>>PT10100901JSD431313985490000043523                                              Y  2904JSDRM", message.EM_MessageText);
			paymentMock.VerifyAll();

			AssertEquals(EM_MessageSubTypeList.Codes.AutomatedClearinghouse, message.EM_MessageSubType);
		}
	}
}
