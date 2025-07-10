using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using NUnit.Framework;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC009CMessageInterpreter))]
sealed class CC009CMessageInterpreterTest : MessageInterpreterTest<IIE009>
{
	public void TestInterpret()
	{
		const string mrn = "MRN";
		const string lrn = "LRN";
		const string testSentOnDate = "20/01/2002";
		const string customsOfficeOfDeparture = "CustomsOfficeOfDeparture";
		const string invalidationRequestedDate = "22/01/2002";
		const string decisionDateAndTime = "2023-07-26T00:12:17.072339";
		const string justification = "justification1";
		const string holderOfTheTransitProcedureEori = "HolderOfTheTransitProcedureEori";
		const string tirHolderIdentificationNumber = "TIRHolderIdentificationNumber";
		const string name = "Name";
		const string streetAndAddress = "StreetAndAddress";
		const string postcode = "Postcode";
		const string city = "City";
		const string country = "Country";

		var declarationAcceptanceDate = new DateTime(1994, 1, 1);
		const string expectedInterpretation = GlobalHtmlStyle +
$"<h2>IE009 - Invalidation Response</h2>" +
"<hr />" +
"<table>" +
	"<tbody>" +
		$"<tr><th>LRN</th><td>LRN</td></tr>" +
		$"<tr><th>MRN</th><td>MRN</td></tr>" +
		$"<tr><th>Message Sent On</th><td>20/01/2002</td></tr>" +
		$"<tr><th>Customs Office Of Departure</th><td>CustomsOfficeOfDeparture</td></tr>" +
		$"<tr><th>Invalidation Requested Date</th><td>22/01/2002</td></tr>" +
		$"<tr><th>Decision</th><td>Invalidation Request Accepted</td></tr>" +
		$"<tr><th>Decision Date &amp; Time</th><td>2023-07-26T00:12:17.072339</td></tr>" +
		$"<tr><th>Justification</th><td>justification1</td></tr>" +
	"</tbody>" +
"</table>" +
"<hr />" +
"<table>" +
	$"<caption><h3>Holder of the Transit Procedure</h3></caption>" +
	"<tbody>" +
		$"<tr><th>EORI</th><td>HolderOfTheTransitProcedureEori</td></tr>" +
		$"<tr><th>TIR Holder Identification Number</th><td>TIRHolderIdentificationNumber</td></tr>" +
		$"<tr><th>Name</th><td>Name</td></tr>" +
		$"<tr><th>Street &amp; Address</th><td>StreetAndAddress</td></tr>" +
		$"<tr><th>Postcode</th><td>Postcode</td></tr>" +
		$"<tr><th>City</th><td>City</td></tr>" +
		$"<tr><th>Country</th><td>Country</td></tr>" +
	"</tbody>" +
"</table>";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Departure;
		var interpreter = new CC009CMessageInterpreter(nctsHeader.MovementHeader);
		var invalidation = new Mock<ICC009Invalidation>();
		var holderOfTheTransitProcedure = new Mock<IHolderOfTheTransitProcedureWithContactInfo>();
		var address = new Mock<IAddress>();

		invalidation.Setup(x => x.RequestDateAndTime).Returns(invalidationRequestedDate);
		invalidation.Setup(x => x.Decision).Returns(Decision.Item1);
		invalidation.Setup(x => x.DecisionDateAndTime).Returns(decisionDateAndTime);
		invalidation.Setup(x => x.Justification).Returns(justification);

		address.Setup(a => a.StreetAndNumber).Returns(streetAndAddress);
		address.Setup(a => a.City).Returns(city);
		address.Setup(a => a.PostCode).Returns(postcode);
		address.Setup(a => a.CountryCode).Returns(country);
		holderOfTheTransitProcedure.Setup(x => x.Address).Returns(address.Object);
		holderOfTheTransitProcedure.Setup(x => x.IdentificationNumber).Returns(holderOfTheTransitProcedureEori);
		holderOfTheTransitProcedure.Setup(x => x.TIRHolderIdentificationNumber).Returns(tirHolderIdentificationNumber);
		holderOfTheTransitProcedure.Setup(x => x.Name).Returns(name);

		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);
		dataProviderMock.Setup(x => x.PreparationDateAndTime).Returns(testSentOnDate);
		dataProviderMock.Setup(x => x.CustomsOfficeOfDepartureReferenceNumber).Returns(customsOfficeOfDeparture);

		dataProviderMock.Setup(x => x.Invalidation).Returns(invalidation.Object);
		dataProviderMock.Setup(x => x.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedure.Object);

		AssertInterpret(interpreter, dataProviderMock.Object, expectedInterpretation);
	}
}
