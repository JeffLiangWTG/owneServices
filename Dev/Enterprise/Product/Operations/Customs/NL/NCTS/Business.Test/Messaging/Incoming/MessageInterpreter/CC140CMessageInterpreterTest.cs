using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC140CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC140CMessageInterpreter();

		var mockCC140C = new Mock<ICC140CDataProvider>();
		mockCC140C.Setup(m => m.RequestOnNonArrivedMovementDate).Returns(new DateTime(2022, 4, 1));
		mockCC140C.Setup(m => m.LimitForResponseDate).Returns(new DateTime(2022, 4, 2));
		var result = interpreter.Interpret(mockCC140C.Object);
		AssertEquals("New Customs Status: 'Request on Non-Arrived Movement'</br>Status granted on 01/04/2022</br>Response on the Request for info on Non-Arrived Movement, must be sent to customs before 02/04/2022", result);
	}
}
