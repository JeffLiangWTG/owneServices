using System;
using System.Globalization;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC928CMessageInterpreter))]
sealed class CC928CMessageInterpreterTest : MessageInterpreterTest<IIE928>
{
	public void TestInterpret() => CombineAssertions(() =>
	{
		const string testLrn = "TST_LRN";
		var testPreparationDate = new DateTime(1994, 1, 1).ToString(CultureInfo.InvariantCulture);
		const string testOfficeDescription = "Test Office Of Departure";
		const string testEori = "Test EORI";
		const string testTirNumber = "Test TIR number";
		const string testHolderName = "Test_Holder_Name";
		const string testStreetAndAddress = "Test Address1";
		const string testPostCode = "01-101";
		const string testCity = "Test City1";

		dataProviderMock.Setup(m => m.LRN).Returns(testLrn);
		dataProviderMock.Setup(m => m.PreparationDateAndTime).Returns(testPreparationDate);
		dataProviderMock.Setup(m => m.CustomsOfficeOfDeparture).Returns(testOfficeDescription);
		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);
		holderOfTheTransitProcedureMock.Setup(x => x.IdentificationNumber).Returns(testEori);
		holderOfTheTransitProcedureMock.Setup(x => x.TIRHolderIdentificationNumber).Returns(testTirNumber);
		holderOfTheTransitProcedureMock.Setup(x => x.Name).Returns(testHolderName);
		var addressMock = new Mock<IAddress>();
		holderOfTheTransitProcedureMock.Setup(x => x.Address).Returns(addressMock.Object);
		addressMock.Setup(x => x.StreetAndNumber).Returns(testStreetAndAddress);
		addressMock.Setup(x => x.PostCode).Returns(testPostCode);
		addressMock.Setup(x => x.City).Returns(testCity);
		addressMock.Setup(x => x.CountryCode).Returns(CountryCodes.Poland);

		var expectedInterpretation =
ExtendedGlobalHtmlStyle +
"<h2>IE928 - Positive Acknowledgment</h2>" +
"<hr />" +
"<table class=\"no-border bold-font\">" +
"<tbody>" +
"<tr><th>LRN</th><td>: TST_LRN</td></tr>" +
"<tr><th>Message Sent On</th><td>: 01/01/1994 00:00:00</td></tr>" +
"<tr><th>Customs Office Of Departure</th><td>: Test Office Of Departure</td></tr>" +
"</tbody>" +
"</table>" +
"<hr />" +
"<p>" +
"<h3>Holder of the Transit Procedure</h3>EORI: Test EORI<br />" +
"TIR Holder Identification Number: Test TIR number<br />" +
"Name: Test_Holder_Name<br />" +
"Street &amp; Address: Test Address1<br />" +
"Postcode: 01-101<br />" +
"City: Test City1<br />" +
"Country: PL" +
"</p>" +
"<hr />";
		AssertEquals(expectedInterpretation, interpreter.Interpret(dataProviderMock.Object));
	});

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		interpreter = new CC928CMessageInterpreter(nctsHeader.MovementHeader);
	}

	NctsHeader nctsHeader;
	CC928CMessageInterpreter interpreter;
}
