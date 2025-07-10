using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using NUnit.Framework;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC043CMessageInterpreter))]
sealed class CC043CMessageInterpreterTest : MessageInterpreterTest<IIE043>
{
	public void TestInterpret()
	{
		const string mrn = "MRN";
		const string declarationType = "DeclarationType";
		const string customsOfficeOfDeparture = "CustomsOfficeOfDeparture";
		const string continueUnloadingNotes = "ContinueUnloadingNotes";
		const decimal grossMass = 10m;
		const int TotalNumberOfHouse = 2;
		const int TotalNumberOfItems = 2;
		const string traderAtDestination = "TraderAtDestination";
		const string holderOfTheTransitProcedureEori = "HolderOfTheTransitProcedureEori";
		const string tirHolderIdentificationNumber = "TIRHolderIdentificationNumber";
		const string name = "Name";
		const string streetAndNumber = "StreetAndNumber";
		const string postcode = "Postcode";
		const string city = "City";
		const string country = "Country";

		var declarationAcceptanceDate = new DateTime(1994, 1, 1);
		const string expectedInterpretation = GlobalHtmlStyle +
$"<h2>IE043 - Unloading Permission</h2>" +
"<hr />" +
"<table>" +
	"<tbody>" +
		$"<tr><th>MRN</th><td>{mrn}</td></tr>" +
		$"<tr><th>Declaration Type</th><td>{declarationType}</td></tr>" +
		$"<tr><th>Acceptance Date</th><td>01-Jan-94 00:00:00</td></tr>" +
		$"<tr><th>Customs Office Of Departure</th><td>{customsOfficeOfDeparture}</td></tr>" +
		$"<tr><th>Continue Unloading Notes</th><td>{continueUnloadingNotes}</td></tr>" +
		$"<tr><th>Total Gross Weight</th><td>10</td></tr>" +
		$"<tr><th>Total Number Of House</th><td>2</td></tr>" +
		$"<tr><th>Total Number Of Items</th><td>2</td></tr>" +
	"</tbody>" +
"</table>" +
"<hr />" +
"<table>" +
	$"<caption><h3>Trader At Destination</h3></caption>" +
	"<tbody>" +
		$"<tr><th>EORI</th><td>{traderAtDestination}</td></tr>" +
	"</tbody>" +
"</table>" +
"<hr />" +
"<table>" +
	$"<caption><h3>Holder of the Transit Procedure</h3></caption>" +
	"<tbody>" +
		$"<tr><th>EORI</th><td>{holderOfTheTransitProcedureEori}</td></tr>" +
		$"<tr><th>TIR Holder Identification Number</th><td>{tirHolderIdentificationNumber}</td></tr>" +
		$"<tr><th>Name</th><td>{name}</td></tr>" +
		$"<tr><th>Street &amp; Address</th><td>{streetAndNumber}</td></tr>" +
		$"<tr><th>Postcode</th><td>{postcode}</td></tr>" +
		$"<tr><th>City</th><td>{city}</td></tr>" +
		$"<tr><th>Country</th><td>{country}</td></tr>" +
		"</tbody>" +
		"</table>";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Departure;
		var interpreter = new CC043CMessageInterpreter(nctsHeader.MovementHeader);
		var transitOperation = new Mock<ICC043CTransitOperation>();
		var holderOfTheTransitProcedure = new Mock<IHolderOfTheTransitProcedure>();
		var address = new Mock<IAddress>();

		transitOperation.Setup(x => x.DeclarationType).Returns(declarationType);
		transitOperation.Setup(x => x.DeclarationAcceptanceDate).Returns(declarationAcceptanceDate);

		address.Setup(a => a.StreetAndNumber).Returns(streetAndNumber);
		address.Setup(a => a.City).Returns(city);
		address.Setup(a => a.PostCode).Returns(postcode);
		address.Setup(a => a.CountryCode).Returns(country);
		holderOfTheTransitProcedure.Setup(x => x.Address).Returns(address.Object);
		holderOfTheTransitProcedure.Setup(x => x.IdentificationNumber).Returns(holderOfTheTransitProcedureEori);
		holderOfTheTransitProcedure.Setup(x => x.TIRHolderIdentificationNumber).Returns(tirHolderIdentificationNumber);
		holderOfTheTransitProcedure.Setup(x => x.Name).Returns(name);

		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.CountrySpecificDataPLCustomsOfficeOfDeparture).Returns(customsOfficeOfDeparture);
		dataProviderMock.Setup(x => x.GrossMassValue).Returns(grossMass);
		dataProviderMock.Setup(x => x.TotalCountOfHouseConsignment).Returns(TotalNumberOfHouse);
		dataProviderMock.Setup(x => x.TotalCountOfConsignmentItem).Returns(TotalNumberOfItems);
		dataProviderMock.Setup(x => x.TraderAtDestination).Returns(traderAtDestination);
		dataProviderMock.Setup(x => x.ContinueUnloading).Returns(continueUnloadingNotes);

		dataProviderMock.Setup(x => x.TransitOperation).Returns(transitOperation.Object);
		dataProviderMock.Setup(x => x.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedure.Object);

		AssertInterpret(interpreter, dataProviderMock.Object, expectedInterpretation);
	}
}
