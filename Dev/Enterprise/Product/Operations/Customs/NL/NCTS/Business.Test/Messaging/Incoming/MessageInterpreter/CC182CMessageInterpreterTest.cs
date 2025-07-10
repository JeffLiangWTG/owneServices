using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC182CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC182CMessageInterpreter();

		var result = interpreter.Interpret(CC182CMessageProcessorTest.GetMessageDataProviderMock(true));
		AssertEquals("Incident Notification Forwarded</br>Status granted on 28/03/2024 at 12:25:45", result);
	}
}
