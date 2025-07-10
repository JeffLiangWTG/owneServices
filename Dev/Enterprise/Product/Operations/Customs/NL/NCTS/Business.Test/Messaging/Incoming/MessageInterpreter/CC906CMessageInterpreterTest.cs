using System.Collections.Generic;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.ctypes;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC906CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC906CMessageInterpreter();
		var mockCC906C = new Mock<ICC906CDataProvider>();
		var functionalErrors = new List<INCTSFunctionalError>
		{
			NCTSFunctionalErrorProvider.New(new FunctionalErrorType02
			{
				ErrorPointer = "EP1",
				ErrorCode = "12",
				ErrorReason = "bad type one",
				OriginalAttributeValue = "11",
			}),
			NCTSFunctionalErrorProvider.New(new FunctionalErrorType02
			{
				ErrorPointer = "EP2",
				ErrorCode = "15",
				ErrorReason = "bad type two",
				OriginalAttributeValue = "12",
			}),
		};

		mockCC906C.Setup(x => x.FunctionalErrors).Returns(functionalErrors);
		var result = interpreter.Interpret(mockCC906C.Object);
		AssertEquals("New transaction status: XML error. Xml gives xsd errors.</br></br>Error Pointer: EP1</br>Error Code: 12 Code list violation (incorrect enumeration)</br>Error Reason: bad type one</br>Original Attribute Value: 11</br></br>Error Pointer: EP2</br>Error Code: 15 Condition violation (not supported in this position)</br>Error Reason: bad type two</br>Original Attribute Value: 12</br>", result);
	}
}
