using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC045CMessageInterpreter))]
sealed class CC045CMessageInterpreterTest : MessageInterpreterTest<IIE045>
{
	public void TestInterpret() => CombineAssertions(() =>
	{
		const string testMrn = "TST_MRN";
		const string customsOfficeOfDeparture = $"{CountryCodes.Poland}2233";
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		Factory.CreateCustomOfficesForTest((code: customsOfficeOfDeparture, description: "Polish office departure"));
		Factory.Save();
		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		dataProviderMock.Setup(m => m.WriteOffDate).Returns(new DateTime(2024, 01, 02, 03, 04, 05));
		dataProviderMock.Setup(x => x.CustomsOfficeOfDeparture).Returns(customsOfficeOfDeparture);

		var addressMock = Mock.Of<IAddress>(a =>
			a.StreetAndNumber == "Test holder address" &&
			a.PostCode == "12-345" &&
			a.City == "Warsaw" &&
			a.CountryCode == "Poland"
		);
		var guarantorMock = Mock.Of<IGuarantor>(x =>
			x.IdentificationNumber == "EORI_456" &&
			x.Name == "Test Guarantor" &&
			x.Address == addressMock);
		var holderOfTheTransitProcedureMock = Mock.Of<IHolderOfTheTransitProcedureWithContactInfo>(x =>
			x.IdentificationNumber == "EORI_456" &&
			x.TIRHolderIdentificationNumber == "TIR_123" &&
			x.Name == "Test holder 1" &&
			x.Address == addressMock);

		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock);
		dataProviderMock.Setup(m => m.Guarantor).Returns(guarantorMock);

		const string expectedInterpretation =
"<style>table, th, td { border: 1px solid black; border-collapse: collapse; } " +
"th, td { padding: 5px; text-align: left; }" +
"</style>" +
		"<h2>IE045 - Write-Off</h2><hr />" +
		"<table>" +
			"<tbody>" +
				"<tr><th>MRN</th><td>TST_MRN</td></tr>" +
				"<tr><th>Write Off Date</th><td>02-Jan-24 03:04:05</td></tr>" +
				"<tr><th>Customs Office Of Departure</th><td>PL2233 - Polish office departure</td></tr>" +
			"</tbody>" +
		"</table><hr />" +
		"<table>" +
		"<caption><h3>Guarantor</h3></caption>" +
			"<tbody>" +
				"<tr><th>EORI</th><td>EORI_456</td></tr>" +
				"<tr><th>Name</th><td>Test Guarantor</td></tr>" +
				"<tr><th>Street &amp; Address</th><td>Test holder address</td></tr>" +
				"<tr><th>Postcode</th><td>12-345</td></tr>" +
				"<tr><th>City</th><td>Warsaw</td></tr>" +
				"<tr><th>Country</th><td>Poland</td></tr>" +
			"</tbody>" +
		"</table>" +
		"<hr />" +
		"<table>" +
		"<caption><h3>Holder of the Transit Procedure</h3></caption>" +
			"<tbody>" +
				"<tr><th>EORI</th><td>EORI_456</td></tr>" +
				"<tr><th>TIR Holder Identification Number</th><td>TIR_123</td></tr>" +
				"<tr><th>Name</th><td>Test holder 1</td></tr>" +
				"<tr><th>Street &amp; Address</th><td>Test holder address</td></tr>" +
				"<tr><th>Postcode</th><td>12-345</td></tr>" +
				"<tr><th>City</th><td>Warsaw</td></tr>" +
				"<tr><th>Country</th><td>Poland</td></tr>" +
			"</tbody>" +
		"</table>" +
		"<hr />";

		var interpretation = new CC045CMessageInterpreter(nctsHeader.MovementHeader).Interpret(dataProviderMock.Object);
		AssertEquals("Interpretation", expectedInterpretation, interpretation);
	});
}
