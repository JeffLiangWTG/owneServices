using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC055CMessageInterpreterTest : MessageInterpreterTest<IIE055>
{
	public void TestInterpret() => CombineAssertions(() =>
	{
		const string testLrn = "TST_LRN";
		const string testMrn = "TST_MRN";
		const string testSentOnDate = "20/01/2002";
		const string testOfficeDescription = "Test Office Of Departure";
		var testDeclarationAcceptanceDate = new DateTime(2024, 01, 01, 12, 03, 55);

		nctsHeader.MovementHeader.BM_PaperlessInbondNum = testLrn;
		dataProviderMock.Setup(m => m.PreparationDateAndTime).Returns(testSentOnDate);
		dataProviderMock.Setup(m => m.CustomsOfficeOfDeparture).Returns(testOfficeDescription);
		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		mockTransitOperation.Setup(m => m.DeclarationAcceptanceDate).Returns(testDeclarationAcceptanceDate);

		var invalidGuaranteeReason1 = new Mock<ICC055CInvalidGuaranteeReason>();
		invalidGuaranteeReason1.Setup(x => x.Code).Returns("A");
		invalidGuaranteeReason1.Setup(x => x.Text).Returns("Text for A");

		var invalidGuaranteeReason2 = new Mock<ICC055CInvalidGuaranteeReason>();
		invalidGuaranteeReason2.Setup(x => x.Code).Returns("B");

		var invalidGuaranteeReason3 = new Mock<ICC055CInvalidGuaranteeReason>();
		invalidGuaranteeReason3.Setup(x => x.Text).Returns("QWERTY");

		var invalidGuaranteeReason4 = new Mock<ICC055CInvalidGuaranteeReason>();

		const string testGRN1 = "GRN1";
		var guaranteeReference1 = new Mock<ICC055CGuaranteeReference>();
		guaranteeReference1.Setup(x => x.GRN).Returns(testGRN1);
		guaranteeReference1.Setup(x => x.InvalidGuaranteeReasons).Returns(new[] {
			invalidGuaranteeReason1.Object, invalidGuaranteeReason2.Object,
			invalidGuaranteeReason3.Object, invalidGuaranteeReason4.Object });

		const string testGRN2 = "GRN2";
		var guaranteeReference2 = new Mock<ICC055CGuaranteeReference>();
		guaranteeReference1.Setup(x => x.GRN).Returns(testGRN2);

		var guaranteeReference3 = new Mock<ICC055CGuaranteeReference>();

		dataProviderMock.Setup(m => m.GuaranteeReferences).Returns(new[] {
			guaranteeReference1.Object, guaranteeReference2.Object, guaranteeReference3.Object });

		const string testEori = "EORI_456";
		const string testTirNumber = "TIR_123";
		const string testHolderName = "Test holder 1";
		const string testStreetAndAddress = "Test holder address";
		const string testPostCode = "12-345";
		const string testCity = "Warsaw";
		const string testCountry = "Poland";
		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedureWithContactInfo>();
		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);
		holderOfTheTransitProcedureMock.Setup(x => x.IdentificationNumber).Returns(testEori);
		holderOfTheTransitProcedureMock.Setup(x => x.TIRHolderIdentificationNumber).Returns(testTirNumber);
		holderOfTheTransitProcedureMock.Setup(x => x.Name).Returns(testHolderName);
		var addressMock = new Mock<IAddress>();
		holderOfTheTransitProcedureMock.Setup(x => x.Address).Returns(addressMock.Object);
		addressMock.Setup(x => x.StreetAndNumber).Returns(testStreetAndAddress);
		addressMock.Setup(x => x.PostCode).Returns(testPostCode);
		addressMock.Setup(x => x.City).Returns(testCity);
		addressMock.Setup(x => x.CountryCode).Returns(testCountry);

		const string expectedInterpretation =
ExtendedGlobalHtmlStyle +
"<h2>IE055 - Guarantee Invalid</h2>" +
"<hr />" +
"<table class=\"no-border bold-font\">" +
"<tbody>" +
"<tr><th>LRN</th><td>TST_LRN</td></tr>" +
"<tr><th>MRN</th><td>TST_MRN</td></tr>" +
"<tr><th>Message Sent On</th><td>20/01/2002</td></tr>" +
"<tr><th>Acceptance Date</th><td>01-Jan-24 12:03:55</td></tr>" +
"<tr><th>Customs Office Of Departure</th><td>Test Office Of Departure</td></tr>" +
"</tbody>" +
"</table>" +
"<hr />" +
"<table class=\"fixed-table\">" +
"<caption><h3>Guarantee References</h3></caption>" +
"<tbody>" +
"<tr><th>Guarantee Reference Number</th><td>Reason For Invalidation</td></tr>" +
"<tr><th>GRN2</th><td>A : Text for A<br />B : <br /> : QWERTY<br /> : </td></tr>" +
"<tr><th></th><td></td></tr>" +
"<tr><th></th><td></td></tr>" +
"</tbody>" +
"</table>" +
"<hr />" +
"<p>" +
"<h3>Holder of the Transit Procedure</h3>EORI: EORI_456" +
"<br />TIR Holder Identification Number: TIR_123" +
"<br />Name: Test holder 1" +
"<br />Street &amp; Address: Test holder address" +
"<br />Postcode: 12-345" +
"<br />City: Warsaw" +
"<br />Country: Poland" +
"</p>" +
"<hr />";

		var interpretation = new CC055CMessageInterpreter(nctsHeader.MovementHeader).Interpret(dataProviderMock.Object);
		AssertEquals("Interpretation", expectedInterpretation, interpretation);
	});

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Departure;
		mockTransitOperation = new Mock<ICC055CTransitOperation>();
		dataProviderMock.Setup(m => m.TransitOperation).Returns(mockTransitOperation.Object);
	}

	NctsHeader nctsHeader;
	Mock<ICC055CTransitOperation> mockTransitOperation;
}
