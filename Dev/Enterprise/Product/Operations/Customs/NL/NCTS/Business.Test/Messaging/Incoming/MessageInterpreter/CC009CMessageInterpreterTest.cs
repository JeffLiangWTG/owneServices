using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC009CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC009CMessageInterpreter();
		var decisionDateTime = DateTime.Now.AddDays(-5);
		var requestDateTime = DateTime.Now.AddDays(-10);
		const string justificationdescription = "JustificationDescription";
		var correlationId = ZGuid.NewZGuid().ToString();

		var mockCC009C = new Mock<ICC009CDataProvider>();
		mockCC009C.Setup(m => m.Invalidation).Returns(Mock.Of<INCTSInvalidationProvider>(i =>
			i.Decision == ZBool.True &&
			i.DecisionDateAndTimeValue == decisionDateTime &&
			i.RequestDateAndTimeValue == requestDateTime &&
			i.InitiatedByCustoms == ZBool.True &&
			i.Justification == justificationdescription));
		mockCC009C.Setup(m => m.CorrelationIdentifier).Returns(correlationId);

		var result = interpreter.Interpret(mockCC009C.Object);

		AssertEquals(
			"New declaration status: Cancellation accepted</br>" +
			$"Status granted on: {decisionDateTime:dd/MM/yyyy hh:mm:ss}</br>" +
			$"Request date and time to invalidate/cancel: {requestDateTime:dd/MM/yyyy hh:mm:ss}</br>" +
			"Initiated by customs: yes</br>" +
			$"Justification: {justificationdescription}</br>" +
			$"Correlation id: {correlationId}", result);
	}

	public void TestInterpret_WhenDecisionIsFalse()
	{
		var interpreter = new CC009CMessageInterpreter();
		var mockCC009C = new Mock<ICC009CDataProvider>();
		mockCC009C.Setup(m => m.Invalidation).Returns(Mock.Of<INCTSInvalidationProvider>(i => i.Decision == ZBool.False));

		var result = interpreter.Interpret(mockCC009C.Object);

		AssertContains("New declaration status: Cancellation refused", result);
	}

	public void TestInterpretWhenInitiatedByCustomsIsFalse()
	{
		var interpreter = new CC009CMessageInterpreter();
		var mockCC009C = new Mock<ICC009CDataProvider>();
		mockCC009C.Setup(m => m.Invalidation).Returns(Mock.Of<INCTSInvalidationProvider>(i => i.InitiatedByCustoms == ZBool.False));

		var result = interpreter.Interpret(mockCC009C.Object);

		AssertContains("Initiated by customs: no", result);
	}
}
