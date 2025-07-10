using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC928CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC928CMessageInterpreter();
		var mockCC928C = new Mock<ICC928CDataProvider>();
		mockCC928C.Setup(m => m.CorrelationIdentifier).Returns("123");
		var result = interpreter.Interpret(mockCC928C.Object);
		AssertEquals("New declaration status: 'Declaration accepted'</br>Correlation id: 123", result);
	}
}
