using System;
using System.Collections.Generic;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.ctypes;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC022CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var functionalErrors = new List<INCTSFunctionalError>
		{
			NCTSFunctionalErrorProvider.New(new FunctionalErrorType01
			{
				SequenceNumber = "1",
				ErrorPointer = "EP1",
				ErrorCode = "12",
				ErrorReason = "bad type one",
				OriginalAttributeValue = "11",
			}),
			NCTSFunctionalErrorProvider.New(new FunctionalErrorType01
			{
				SequenceNumber = "2",
				ErrorPointer = "EP2",
				ErrorCode = "15",
				ErrorReason = "bad type two",
				OriginalAttributeValue = "12",
			}),
		};

		var currentDateTime = DateTime.Now;

		var expectedResult = $"New declaration status: Amendment Requested</br>Declaration received a request to amend the declaration on {currentDateTime.ToString("dd-MMM-y HH:mm:ss")}</br></br>" +
			"1. Functional error code: 12</br>" +
			"Reason: bad type one</br>" +
			"Attribute: EP1</br>" +
			"Element in declaration contains now the value: 11</br></br>" +
			"2. Functional error code: 15</br>" +
			"Reason: bad type two</br>" +
			"Attribute: EP2</br>" +
			"Element in declaration contains now the value: 12</br>";

		var mockProvider = new Mock<ICC022CDataProvider>();
		mockProvider.Setup(x => x.AmendmentNotificationDateAndTime).Returns(currentDateTime);
		mockProvider.Setup(x => x.FunctionalErrors).Returns(functionalErrors);

		AssertEquals(expectedResult, new CC022CMessageInterpreter().Interpret(mockProvider.Object));
	}
}
