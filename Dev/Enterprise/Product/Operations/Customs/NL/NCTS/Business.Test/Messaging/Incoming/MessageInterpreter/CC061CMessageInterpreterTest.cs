using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC061CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC061CMessageInterpreter();
		var mockCC061C = new Mock<ICC061CDataProvider>();
		var controlDateAndTime = new DateTime(2024, 8, 9, 14, 09, 32);
		mockCC061C.Setup(m => m.ControlNotificationDateAndTime).Returns(controlDateAndTime);
		var result = interpreter.Interpret(mockCC061C.Object);
		AssertEquals("New Customs Status: 'Decision to Control Notification'</br>Status granted on: 09/08/2024 14:09:32</br>Date and time of Control: 09/08/2024 14:09:32</br>Physical Control by Customs", result);
	}
}
