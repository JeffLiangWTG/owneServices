using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC140CMessageInterpreter))]
sealed class CC140CMessageInterpreterTest : MessageInterpreterTest<IIE140>
{
	public void TestInterpret() => CombineAssertions(() =>
	{
		const string testMrn = "TST_MRN";
		const string customsOfficeOfDeparture = $"{CountryCodes.Poland}2233";
		const string customsOfficeOfEnquiryAtDeparture = $"{CountryCodes.Poland}5555";

		Factory.CreateCustomOfficesForTest((code: customsOfficeOfDeparture, description: "Polish office departure"));
		Factory.CreateCustomOfficesForTest((code: customsOfficeOfEnquiryAtDeparture, description: "Polish office enquiry"));
		Factory.Save();
		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		dataProviderMock.Setup(m => m.PreparationDateAndTime).Returns("2024-01-01T00:01:02");
		dataProviderMock.Setup(x => x.CustomsOfficeOfDeparture).Returns(customsOfficeOfDeparture);
		dataProviderMock.Setup(x => x.CustomsOfficeOfEnquiryAtDeparture).Returns(customsOfficeOfEnquiryAtDeparture);

		var transitOperation = Mock.Of<ICC140CTransitOperation>(a =>
			a.LimitForResponseDate == new System.DateTime(2024, 01, 02, 03, 04, 05) &&
			a.RequestOnNonArrivedMovementDate == new System.DateTime(2024, 06, 07, 08, 09, 10));
		var addressMock = Mock.Of<IAddress>(a =>
			a.StreetAndNumber == "Test holder address" &&
			a.PostCode == "12-345" &&
			a.City == "Warsaw" &&
			a.CountryCode == "Poland"
		);
		var holderOfTheTransitProcedureMock = Mock.Of<IHolderOfTheTransitProcedureWithContactInfo>(x =>
			x.IdentificationNumber == "EORI_456" &&
			x.TIRHolderIdentificationNumber == "TIR_123" &&
			x.Name == "Test holder 1" &&
			x.Address == addressMock);

		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock);
		dataProviderMock.Setup(m => m.TransitOperation).Returns(transitOperation);

		const string expectedInterpretation =
"<style>table, th, td { border: 1px solid black; border-collapse: collapse; } " +
"th, td { padding: 5px; text-align: left; }" +
"</style>" +
"<h2>IE140 - Consignment Under Search Procedure</h2><hr />" +
"<table class=\"no-border bold-font\">" +
	"<tbody>" +
		"<tr><th>MRN</th><td>TST_MRN</td></tr>" +
		"<tr><th>Message Sent On</th><td>2024-01-01T00:01:02</td></tr>" +
		"<tr><th>Non-Arrived Movement Date</th><td>07-Jun-24 08:09:10</td></tr>" +
		"<tr><th>Customs Office Of Departure</th><td>PL2233 - Polish office departure</td></tr>" +
		"<tr><th>Customs Office of Enquiry at Departure</th><td>PL5555 - Polish office enquiry</td></tr>" +
		"<tr><th>Limit For Response Date</th><td>02-Jan-24 03:04:05</td></tr>" +
	"</tbody>" +
"</table><hr />" +
"<p>" +
"<h3>Holder of the Transit Procedure</h3>" +
	"EORI: EORI_456<br />" +
	"TIR Holder Identification Number: TIR_123<br />" +
	"Name: Test holder 1<br />" +
	"Street &amp; Address: Test holder address<br />" +
	"Postcode: 12-345<br />" +
	"City: Warsaw<br />" +
	"Country: Poland" +
"</p>" +
"<hr />";

		var interpretation = new CC140CMessageInterpreter(nctsHeader.MovementHeader).Interpret(dataProviderMock.Object);
		AssertEquals("Interpretation", expectedInterpretation, interpretation);
	});

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
	}

	NctsHeader nctsHeader;
}
