using System.Collections.Generic;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class CC917CMessageInterpreterTest : TestCaseWithFactory
{
	public void TestInterpret()
	{
		var interpreter = new CC917CMessageInterpreter();
		var mockCC917C = new Mock<ICC917CDataProvider>();

		var xmlErrors = new List<INCTSFunctionalError>
	{
		Mock.Of<INCTSFunctionalError>(x =>
			x.SequenceNumeric == 1 &&
			x.ErrorColumnNumber == 2 &&
			x.ErrorPointer == "CustomsOfficeOfDestination" &&
			x.ErrorCode == "53" &&
			x.ErrorReason == "Invalid character > detected." &&
			x.OriginalAttributeValue == "NL>01010001"),
	};

		mockCC917C.Setup(x => x.LRN).Returns("LRN123");
		mockCC917C.Setup(x => x.MRN).Returns("22NL000000000012J1");
		mockCC917C.Setup(x => x.XmlErrors).Returns(xmlErrors);
		var result = interpreter.Interpret(mockCC917C.Object);
		AssertEquals("New transaction status: XML error. Xml gives xsd errors.</br></br>Error line number: 1</br>Error column number: 2</br>Error pointer: CustomsOfficeOfDestination</br>Error code: 53 Invalid character(s)</br>Error text: Invalid character > detected.</br>Original attribute value: NL>01010001</br>", result);
	}
}
