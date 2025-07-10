using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC028CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC028CMessageInterpreter();
		var mockCC028C = new Mock<ICC028CDataProvider>();
		mockCC028C.Setup(m => m.DeclarationAcceptanceDate).Returns(new DateTime(2022, 4, 1, 12, 34, 56));
		mockCC028C.Setup(m => m.CorrelationIdentifier).Returns("123");
		var result = interpreter.Interpret(mockCC028C.Object);
		AssertEquals("New declaration status: Declaration MRN Allocated</br>Status granted on: 1/04/2022 12:34:56 PM</br>Correlation id: 123", result);
	}
}
