using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC057CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC057CMessageInterpreter();

		var mockCC057C = new Mock<ICC057CDataProvider>();
		mockCC057C.Setup(x => x.BusinessRejectionType).Returns(NLNctsOutgoingMessageTypes.Codes.CC007C);
		mockCC057C.Setup(x => x.RejectionCode).Returns(EU.NCTS.Business.RejectionCodes.Codes.Code4);
		mockCC057C.Setup(x => x.RejectionDateAndTime).Returns(new DateTime(2022, 04, 01, 12, 34, 56));
		mockCC057C.Setup(x => x.RejectionReason).Returns("Invalid arrival date");

		var mockFunctionalError1 = new Mock<INCTSFunctionalError>();
		mockFunctionalError1.Setup(x => x.SequenceNumeric).Returns(1);
		mockFunctionalError1.Setup(x => x.ErrorCode).Returns("12");
		mockFunctionalError1.Setup(x => x.ErrorPointer).Returns("cc015c.DepartureTransportMeans(1).typeOfIdentification");
		mockFunctionalError1.Setup(x => x.ErrorReason).Returns("Type of transport does not exist");
		mockFunctionalError1.Setup(x => x.OriginalAttributeValue).Returns("value1");

		var mockFunctionalError2 = new Mock<INCTSFunctionalError>();
		mockFunctionalError2.Setup(x => x.SequenceNumeric).Returns(2);
		mockFunctionalError2.Setup(x => x.ErrorCode).Returns("12");
		mockFunctionalError2.Setup(x => x.ErrorPointer).Returns("cc015c.DepartureTransportMeans(2).typeOfIdentification");
		mockFunctionalError2.Setup(x => x.ErrorReason).Returns("Type of transport does not exist");
		mockFunctionalError2.Setup(x => x.OriginalAttributeValue).Returns("value2");

		var list = new Collection<INCTSFunctionalError> { mockFunctionalError1.Object, mockFunctionalError2.Object };
		mockCC057C.Setup(x => x.FunctionalErrors).Returns(new ReadOnlyCollection<INCTSFunctionalError>(list));

		var result = interpreter.Interpret(mockCC057C.Object);
		AssertEquals("Declaration received an error for type 007 (Arrival notification rejection) on 01/04/2022 12:34:56</br>Reason: 4 (Other reasons) Invalid arrival date</br>Functional error code: 12 (Code list violation (incorrect enumeration))</br>Reason: Type of transport does not exist</br>Attribute: cc015c.DepartureTransportMeans(1).typeOfIdentification</br>Element in declaration contains now the value: value1</br></br>Functional error code: 12 (Code list violation (incorrect enumeration))</br>Reason: Type of transport does not exist</br>Attribute: cc015c.DepartureTransportMeans(2).typeOfIdentification</br>Element in declaration contains now the value: value2</br>", result);
	}
}
