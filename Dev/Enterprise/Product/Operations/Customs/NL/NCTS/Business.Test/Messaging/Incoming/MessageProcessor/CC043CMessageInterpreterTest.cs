using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC043CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var systemDateTime = ZDateTime.Now;
		CombineAssertions(() =>
		{
			AssertInterpret(null, "Started", systemDateTime);
			AssertInterpret(0, "Started", systemDateTime);
			AssertInterpret(1, "Continue", systemDateTime);
		});
	}

	void AssertInterpret(int? continueUnloading, string status, ZDateTime systemDateTime)
	{
		var interpreter = new CC043CMessageInterpreter();
		var mockCC043C = new Mock<ICC043CDataProvider>();
		mockCC043C.Setup(x => x.CTLControlContinueUnloading).Returns(continueUnloading);
		var result = interpreter.Interpret(mockCC043C.Object);
		AssertEquals($"{continueUnloading}" ,$"Unloading Permission: {status}.</br>Status granted on: {systemDateTime:dd/MM/yyyy hh:mm:ss}.", result);
	}
}
