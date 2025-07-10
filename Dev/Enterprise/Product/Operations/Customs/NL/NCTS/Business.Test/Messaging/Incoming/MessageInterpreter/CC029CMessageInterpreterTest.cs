using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC029CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC029CMessageInterpreter();
		var mockCC029C = new Mock<ICC029CDataProvider>();
		mockCC029C.Setup(m => m.ReleaseDate).Returns(new DateTime(1994, 2, 1));
		mockCC029C.Setup(m => m.DeclarationAcceptanceDate).Returns(new DateTime(1994, 2, 2));
		mockCC029C.Setup(m => m.ControlResult.Code).Returns("A1");
		var result = interpreter.Interpret(mockCC029C.Object);
		AssertEquals("New detailed status: Goods Released for Transit at Departure.</br>Status granted on 01/02/1994</br>Acceptance Date 02/02/1994</br>Control Result: A1 Satisfactory</br>Controlled by: </br>Text: </br>Date: 01/01/0001", result);
	}
}                     
