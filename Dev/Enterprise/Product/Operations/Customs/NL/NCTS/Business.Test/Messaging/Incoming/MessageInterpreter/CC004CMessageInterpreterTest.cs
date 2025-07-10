using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC004CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var acceptanceDateTime = DateTime.Now;
		var submissionDateTime = DateTime.Now;
		var correlationId = "test-id";

		var expectedResult = $"New declaration status: Amendment acceptance</br>Status granted on {acceptanceDateTime.ToString("dd-MMM-y HH:mm:ss")}</br>" +
			$"Amendment submission date and time: {submissionDateTime.ToString("dd-MMM-y HH:mm:ss")}</br>" +
			$"Correlation id: {correlationId}</br>";

		var mockProvider = new Mock<ICC004CDataProvider>();
		mockProvider.Setup(x => x.AmendmentAcceptanceDateTime).Returns(acceptanceDateTime);
		mockProvider.Setup(x => x.AmendmentSubmissionDateTime).Returns(submissionDateTime);
		mockProvider.Setup(x => x.CorrelationIdentifier).Returns(correlationId);

		AssertEquals(expectedResult, new CC004CMessageInterpreter().Interpret(mockProvider.Object));
	}
}
