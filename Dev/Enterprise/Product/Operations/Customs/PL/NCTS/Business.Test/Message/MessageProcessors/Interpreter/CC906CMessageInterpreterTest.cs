using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Moq;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC906CMessageInterpreterTest : MessageInterpreterTest<IIE906>
{
	public void TestInterpretCC906CMessageWithoutFunctionalErrors()
	{
		const string expectedResult = GlobalHtmlStyle +
"<table>" +
	"<caption><h3>IE906 - Functional Error</h3></caption>" +
	"<tbody>" +
		"<tr><th>LRN</th><td>LRN</td></tr>" +
		"<tr><th>MRN</th><td>MRN</td></tr>" +
		"<tr><th>Message Sent On</th><td>11</td></tr>" +
	"</tbody>" +
"</table>";

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var dataProviderMock = new Mock<IIE906>();

		dataProviderMock.SetupGet(x => x.MRN).Returns("MRN");
		dataProviderMock.SetupGet(x => x.LRN).Returns("LRN");
		dataProviderMock.SetupGet(x => x.PreparationDateAndTime).Returns("11");
		var interpreter = new CC906CMessageInterpreter(header.MovementHeader);

		AssertInterpret(interpreter, dataProviderMock.Object, expectedResult);
	}

	public void TestInterpretCC906CMessageWithSingleFunctionalErrors()
	{
		const string expectedResult = GlobalHtmlStyle +
"<table>" +
	"<caption><h3>IE906 - Functional Error</h3></caption>" +
	"<tbody>" +
		"<tr><th>Message Sent On</th><td>11</td></tr>" +
	"</tbody>" +
"</table>" +
"<table>" +
	"<caption><h3>Functional Error</h3></caption>" +
	"<tbody>" +
		"<tr><th></th>" + "<th>Code</th>" + "<th>Reason</th>" + "<th>Error Pointer</th>" + "<th>Error Reference Field</th></tr>" +
		"<tr><td>0</td>" + "<td>1000</td>" + "<td>E1<br />Rule violation</td>" + "<td>/IE906PL/CC906C/@PhaseID</td>" + "<td>Sample1 For Original Attribute</td></tr>" +
	"</tbody>" +
"</table>";

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var dataProviderMock = new Mock<IIE906>();
		dataProviderMock.SetupGet(x => x.PreparationDateAndTime).Returns("11");

		var iFunctionalErrorContentMock = new Mock<IFunctionalError>();
		iFunctionalErrorContentMock.SetupGet(x => x.ErrorCode).Returns(1000);
		iFunctionalErrorContentMock.SetupGet(x => x.ErrorReason).Returns("E1");
		iFunctionalErrorContentMock.SetupGet(x => x.ErrorPointer).Returns("/IE906PL/CC906C/@PhaseID");
		iFunctionalErrorContentMock.SetupGet(x => x.OriginalAttributeValue).Returns("Sample1 For Original Attribute");
		dataProviderMock.SetupGet(x => x.FunctionalErrors).Returns(new[] { iFunctionalErrorContentMock.Object });
		var interpreter = new CC906CMessageInterpreter(header.MovementHeader);

		AssertInterpret(interpreter, dataProviderMock.Object, expectedResult);
	}

	public void TestInterpretCC906CMessageWithMultipleFunctionalErrors()
	{
		const string expectedResult = GlobalHtmlStyle +
"<table>" +
	"<caption><h3>IE906 - Functional Error</h3></caption>" +
	"<tbody>" +
		"<tr><th>Message Sent On</th><td>11</td></tr>" +
	"</tbody>" +
"</table>" +
"<table>" +
	"<caption><h3>Functional Error</h3></caption>" +
	"<tbody>" +
		"<tr><th></th>" + "<th>Code</th>" + "<th>Reason</th>" + "<th>Error Pointer</th>" + "<th>Error Reference Field</th></tr>" +
		"<tr><td>0</td>" + "<td>1000</td>" + "<td>E1<br />Rule violation</td>" + "<td>/IE906PL/CC906C/@PhaseID</td>" + "<td>Sample1 For Original Attribute</td></tr>" +
		"<tr><td>1</td>" + "<td>2000</td>" + "<td>E2<br />Message out of sequence</td>" + "<td>/IE906PL/CC906C/@PhaseID</td>" + "<td>Sample2 For Original Attribute</td></tr>" +
	"</tbody>" +
"</table>";

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var dataProviderMock = new Mock<IIE906>();
		dataProviderMock.SetupGet(x => x.PreparationDateAndTime).Returns("11");

		var iFunctionalErrorContentMock1 = new Mock<IFunctionalError>();
		iFunctionalErrorContentMock1.SetupGet(x => x.ErrorCode).Returns(1000);
		iFunctionalErrorContentMock1.SetupGet(x => x.ErrorReason).Returns("E1");
		iFunctionalErrorContentMock1.SetupGet(x => x.ErrorPointer).Returns("/IE906PL/CC906C/@PhaseID");
		iFunctionalErrorContentMock1.SetupGet(x => x.OriginalAttributeValue).Returns("Sample1 For Original Attribute");

		var iFunctionalErrorContentMock2 = new Mock<IFunctionalError>();
		iFunctionalErrorContentMock2.SetupGet(x => x.ErrorCode).Returns(2000);
		iFunctionalErrorContentMock2.SetupGet(x => x.ErrorReason).Returns("E2");
		iFunctionalErrorContentMock2.SetupGet(x => x.ErrorPointer).Returns("/IE906PL/CC906C/@PhaseID");
		iFunctionalErrorContentMock2.SetupGet(x => x.OriginalAttributeValue).Returns("Sample2 For Original Attribute");
		dataProviderMock.SetupGet(x => x.FunctionalErrors).Returns([iFunctionalErrorContentMock1.Object, iFunctionalErrorContentMock2.Object]);
		var interpreter = new CC906CMessageInterpreter(header.MovementHeader);

		AssertInterpret(interpreter, dataProviderMock.Object, expectedResult);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, parent: euGrouping);
		helper.CreateNewOrGetExistingCusCodeType("CL180", "CL180 Desc.");

		var yesterday = ZDateTime.Today.AddDays(-1);
		var tomorrow = ZDateTime.Today.AddDays(1);
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL180", "1000", "Rule violation", yesterday, tomorrow);
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL180", "2000", "Message out of sequence", yesterday, tomorrow);
		Factory.Save();
	}
}
