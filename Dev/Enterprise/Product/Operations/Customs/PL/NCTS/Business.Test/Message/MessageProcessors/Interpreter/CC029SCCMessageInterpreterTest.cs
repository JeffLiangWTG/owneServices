using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC029SCCMessageInterpreter))]
sealed class CC029SCCMessageInterpreterTest : MessageInterpreterTest<IIE029SC>
{
	public void TestInterpret() => CombineAssertions(() =>
	{
		const string testLrn = "TST_LRN";
		const string testMrn = "TST_MRN";
		const string testDeclarationType = "DeclarationType";
		const string testCustomsOfficeOfDeparture = "CustomsOfficeOfDeparture";
		const string testCustomsOfficeOfDestination = "CustomsOfficeOfDestination";
		const decimal testGrossMass = 12m;
		const string testAcceptanceDate = "20/01/2002";
		var testReleaseDate = new DateTime(2024, 2, 12);

		dataProviderMock.Setup(m => m.LRN).Returns(testLrn);
		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		dataProviderMock.Setup(m => m.DeclarationType).Returns(testDeclarationType);
		dataProviderMock.Setup(m => m.AcceptanceDate).Returns(testAcceptanceDate);
		dataProviderMock.Setup(m => m.ReleaseDate).Returns(testReleaseDate);
		dataProviderMock.Setup(m => m.CustomsOfficeOfDeparture).Returns(testCustomsOfficeOfDeparture);
		dataProviderMock.Setup(m => m.CustomsOfficeOfDestination).Returns(testCustomsOfficeOfDestination);
		dataProviderMock.Setup(m => m.GrossMass).Returns(testGrossMass);

		const string expectedInterpretation = @"
<style>table, th, td { border: 1px solid black; border-collapse: collapse; } th, td { padding: 5px; text-align: left; }</style>

<h2>IE029SC - Released For Transit</h2>
<hr />

<table><tbody>
	<tr><th>LRN</th><td>TST_LRN</td></tr>
	<tr><th>MRN</th><td>TST_MRN</td></tr>
	<tr><th>Message Sent On</th><td>20/01/2002</td></tr>
	<tr><th>Customs Office Of Departure</th><td>CustomsOfficeOfDeparture</td></tr>
	<tr><th>Customs Office Of Destination</th><td>CustomsOfficeOfDestination</td></tr>
	<tr><th>Declaration Type</th><td>DeclarationType</td></tr>
	<tr><th>Release Date</th><td>12-Feb-24 00:00:00</td></tr>
	<tr><th>Gross Weight</th><td>12</td></tr>
</tbody></table>";

		AssertEquals(expectedInterpretation.ToSingleLineHtml(), interpreter.Interpret(dataProviderMock.Object));
	});

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		interpreter = new CC029SCCMessageInterpreter(nctsHeader.MovementHeader);
	}

	NctsHeader nctsHeader;
	CC029SCCMessageInterpreter interpreter;
}
