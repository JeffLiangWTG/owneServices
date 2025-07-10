using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC051CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC051CMessageInterpreter();
		var mockCC051C = new Mock<ICC051CDataProvider>();

		mockCC051C.Setup(x => x.NoReleaseMotivationCode).Returns("G1");
		mockCC051C.Setup(x => x.NoReleaseMotivationText).Returns("Guarantee unknown");
		var result = interpreter.Interpret(mockCC051C.Object);
		AssertEquals("Declaration is NOT RELEASED FOR TRANSIT AT DEPARTURE.</br>Motivation code: G1 Guarantee not valid</br>Guarantee unknown", result);
	}
}
