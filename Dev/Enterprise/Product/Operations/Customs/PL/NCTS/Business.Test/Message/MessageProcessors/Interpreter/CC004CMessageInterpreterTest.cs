using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC004CMessageInterpreter))]
sealed class CC004CMessageInterpreterTest : MessageInterpreterTest<IIE004>
{
	public void TestInterpret() => CombineAssertions(() =>
	{
		const string testLrn = "TST_LRN";
		const string testMrn = "TST_MRN";
		const string amendmentAcceptanceDateAndTime = "2024-01-01T12:00:00";
		const string amendmentSubmissionDateAndTime = "2024-02-03T01:02:03";
		const string customsOfficeOfDeparture = $"{CountryCodes.Poland}2233";

		Factory.CreateCustomOfficesForTest((code: customsOfficeOfDeparture, description: "Polish office"));
		Factory.Save();
		dataProviderMock.Setup(x => x.CustomsOfficeOfDeparture).Returns(customsOfficeOfDeparture);
		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		dataProviderMock.Setup(m => m.LRN).Returns(testLrn);
		mockTransitOperation.Setup(m => m.AmendmentAcceptanceDateAndTime).Returns(amendmentAcceptanceDateAndTime);
		mockTransitOperation.Setup(m => m.AmendmentSubmissionDateAndTime).Returns(amendmentSubmissionDateAndTime);

		const string testEori = "EORI_456";
		const string testTirNumber = "TIR_123";
		const string testHolderName = "Test holder 1";
		const string testStreetAndAddress = "Test holder address";
		const string testPostCode = "12-345";
		const string testCity = "Warsaw";
		const string testCountry = "Poland";
		var addressMock = Mock.Of<IAddress>(a =>
			a.StreetAndNumber == testStreetAndAddress &&
			a.PostCode == testPostCode &&
			a.City == testCity &&
			a.CountryCode == testCountry
		);
		var holderOfTheTransitProcedureMock = Mock.Of<IHolderOfTheTransitProcedureWithContactInfo>(x =>
			x.IdentificationNumber == testEori &&
			x.TIRHolderIdentificationNumber == testTirNumber &&
			x.Name == testHolderName &&
			x.Address == addressMock);

		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock);

		const string expectedInterpretation =
"<style>" +
"table, th, td { border: 1px solid black; border-collapse: collapse; } " +
"th, td { padding: 5px; text-align: left; }" +
"</style>" +
"<h2>IE004 - Amendment Accepted</h2>" +
"<hr />" +
"<table>" +
"<tbody>" +
"<tr><th>LRN</th><td>TST_LRN</td></tr>" +
"<tr><th>MRN</th><td>TST_MRN</td></tr>" +
"<tr><th>Amendment Submission Date</th><td>2024-02-03T01:02:03</td></tr>" +
"<tr><th>Amendment Acceptance Date</th><td>2024-01-01T12:00:00</td></tr>" +
"<tr><th>Customs Office Of Departure</th><td>PL2233 - Polish office</td></tr>" +
"</tbody>" +
"</table>" +
"<hr />" +
"<p><h3>Holder of the Transit Procedure</h3>EORI: EORI_456" +
"<br />TIR Holder Identification Number: TIR_123" +
"<br />Name: Test holder 1" +
"<br />Street &amp; Address: Test holder address" +
"<br />Postcode: 12-345" +
"<br />City: Warsaw" +
"<br />Country: Poland" +
"</p>" +
"<hr />";

		var interpretation = new CC004CMessageInterpreter(nctsHeader.MovementHeader).Interpret(dataProviderMock.Object);
		AssertEquals("Interpretation", expectedInterpretation, interpretation);
	});

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		mockTransitOperation = new Mock<ICC004CTransitOperation>();
		dataProviderMock.Setup(m => m.TransitOperation).Returns(mockTransitOperation.Object);
	}

	NctsHeader nctsHeader;
	Mock<ICC004CTransitOperation> mockTransitOperation;
}
