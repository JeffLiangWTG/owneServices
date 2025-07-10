using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Moq;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC917CMessageInterpreterTest : MessageInterpreterTest<IIE917>
{
	public void TestInterpretCC917CMessageWithoutXmlError()
	{
		const string expectedResult = GlobalHtmlStyle +
"<table>" +
	"<caption><h3>IE917 - Syntax error</h3></caption>" +
	"<tbody>" +
		"<tr><th>LRN</th><td>LRN</td></tr>" +
		"<tr><th>MRN</th><td>MRN</td></tr>" +
	"</tbody>" +
"</table>";

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var dataProviderMock = new Mock<IIE917>();

		dataProviderMock.SetupGet(x => x.MRN).Returns("MRN");
		dataProviderMock.SetupGet(x => x.LRN).Returns("LRN");

		var interpreter = new CC917CMessageInterpreter(header.MovementHeader);

		AssertInterpret(interpreter, dataProviderMock.Object, expectedResult);
	}

	public void TestInterpretCC917CMessageWithoutXmlError_Arrival()
	{
		const string expectedResult = GlobalHtmlStyle +
"<table>" +
	"<caption><h3>IE917 - Syntax error</h3></caption>" +
	"<tbody>" +
		"<tr><th>MRN</th><td>MRN</td></tr>" +
	"</tbody>" +
"</table>";

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		var dataProviderMock = new Mock<IIE917>();

		dataProviderMock.SetupGet(x => x.MRN).Returns("MRN");
		dataProviderMock.SetupGet(x => x.LRN).Returns("LRN");

		var interpreter = new CC917CMessageInterpreter(header.ArrivalMovementHeader);

		AssertInterpret(interpreter, dataProviderMock.Object, expectedResult);
	}

	public void TestInterpretCC917CMessageWithSingleXmlError()
	{
		const string expectedResult = GlobalHtmlStyle +
"<table>" +
	"<caption><h3>IE917 - Syntax error</h3></caption>" +
	"<tbody>" +
		"<tr><th>LRN</th><td>LRN</td></tr>" +
		"<tr><th>MRN</th><td>MRN</td></tr>" +
	"</tbody>" +
"</table>" +
"<table>" +
	"<caption><h3>XML Error</h3></caption>" +
	"<tbody>" +
		"<tr><th></th>"		+ "<th>Line No.</th>"	+ "<th>Column Reference</th>"	+ "<th>Pointer</th>"					+ "<th>Code</th>"						+ "<th>Additional Text</th>"		+ "<th>Submitted Value</th></tr>" +
		"<tr><td>0</td>"	+ "<td>0</td>"			+ "<td>0</td>"					+ "<td>/IE015PL/CC015C/@PhaseID</td>"	+ "<td>12-Incorrect enumeration</td>"	+ "<td>Incorrect Enumeration</td>"	+ "<td>Test For Original Attribute</td></tr>" +
	"</tbody>" +
"</table>";

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var dataProviderMock = new Mock<IIE917>();

		dataProviderMock.SetupGet(x => x.MRN).Returns("MRN");
		dataProviderMock.SetupGet(x => x.LRN).Returns("LRN");

		var iXMLErrorContentMock = new Mock<IXMLError>();
		iXMLErrorContentMock.SetupGet(x => x.ErrorLineNumber).Returns("0");
		iXMLErrorContentMock.SetupGet(x => x.ErrorColumnNumber).Returns("0");
		iXMLErrorContentMock.SetupGet(x => x.ErrorPointer).Returns("/IE015PL/CC015C/@PhaseID");
		iXMLErrorContentMock.SetupGet(x => x.ErrorCode).Returns("12");
		iXMLErrorContentMock.SetupGet(x => x.ErrorText).Returns("Incorrect Enumeration");
		iXMLErrorContentMock.SetupGet(x => x.OriginalAttributeValue).Returns("Test For Original Attribute");
		dataProviderMock.SetupGet(x => x.XMLErrors).Returns([iXMLErrorContentMock.Object]);
		var interpreter = new CC917CMessageInterpreter(header.MovementHeader);

		AssertInterpret(interpreter, dataProviderMock.Object, expectedResult);
	}

	public void TestInterpretCC917CMessageWithMultipleXmlErrors()
	{
		const string expectedResult = GlobalHtmlStyle +
"<table>" +
	"<caption><h3>IE917 - Syntax error</h3></caption>" +
	"<tbody>" +
		"<tr><th>LRN</th><td>LRN</td></tr>" +
		"<tr><th>MRN</th><td>MRN</td></tr>" +
	"</tbody>" +
"</table>" +
"<table>" +
	"<caption><h3>XML Error</h3></caption>" +
	"<tbody>" +
		"<tr><th></th>"		+ "<th>Line No.</th>"	+ "<th>Column Reference</th>"	+ "<th>Pointer</th>"					+ "<th>Code</th>"						+ "<th>Additional Text</th>"		+ "<th>Submitted Value</th></tr>" +
		"<tr><td>0</td>"	+ "<td>0</td>"			+ "<td>0</td>"					+ "<td>/IE015PL/CC015C/@PhaseID</td>"	+ "<td>12-Incorrect enumeration</td>"	+ "<td>Error Text For Sample1</td>"	+ "<td>Sample1 For Original Attribute</td></tr>" +
		"<tr><td>1</td>"	+ "<td>1</td>"			+ "<td>1</td>"					+ "<td>/IE015PL/CC015C/@PhaseID</td>"	+ "<td>13-Missing</td>"					+ "<td>Error Text For Sample2</td>"	+ "<td>Sample2 For Original Attribute</td></tr>" +
	"</tbody>" +
"</table>";

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var dataProviderMock = new Mock<IIE917>();

		dataProviderMock.SetupGet(x => x.MRN).Returns("MRN");
		dataProviderMock.SetupGet(x => x.LRN).Returns("LRN");

		var iXMLErrorContentMock1 = new Mock<IXMLError>();
		iXMLErrorContentMock1.SetupGet(x => x.ErrorLineNumber).Returns("0");
		iXMLErrorContentMock1.SetupGet(x => x.ErrorColumnNumber).Returns("0");
		iXMLErrorContentMock1.SetupGet(x => x.ErrorPointer).Returns("/IE015PL/CC015C/@PhaseID");
		iXMLErrorContentMock1.SetupGet(x => x.ErrorCode).Returns("12");
		iXMLErrorContentMock1.SetupGet(x => x.ErrorText).Returns("Error Text For Sample1");
		iXMLErrorContentMock1.SetupGet(x => x.OriginalAttributeValue).Returns("Sample1 For Original Attribute");

		var iXMLErrorContentMock2 = new Mock<IXMLError>();
		iXMLErrorContentMock2.SetupGet(x => x.ErrorLineNumber).Returns("1");
		iXMLErrorContentMock2.SetupGet(x => x.ErrorColumnNumber).Returns("1");
		iXMLErrorContentMock2.SetupGet(x => x.ErrorPointer).Returns("/IE015PL/CC015C/@PhaseID");
		iXMLErrorContentMock2.SetupGet(x => x.ErrorCode).Returns("13");
		iXMLErrorContentMock2.SetupGet(x => x.ErrorText).Returns("Error Text For Sample2");
		iXMLErrorContentMock2.SetupGet(x => x.OriginalAttributeValue).Returns("Sample2 For Original Attribute");
		dataProviderMock.SetupGet(x => x.XMLErrors).Returns([iXMLErrorContentMock1.Object, iXMLErrorContentMock2.Object]);
		var interpreter = new CC917CMessageInterpreter(header.MovementHeader);

		AssertInterpret(interpreter, dataProviderMock.Object, expectedResult);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, parent: euGrouping);
		helper.CreateNewOrGetExistingCusCodeType("CL030", "CL030 Desc.");

		var yesterday = ZDateTime.Today.AddDays(-1);
		var tomorrow = ZDateTime.Today.AddDays(1);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL030", "12", "Incorrect enumeration", yesterday, tomorrow);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL030", "13", "Missing", yesterday, tomorrow);
		Factory.Save();
	}
}
