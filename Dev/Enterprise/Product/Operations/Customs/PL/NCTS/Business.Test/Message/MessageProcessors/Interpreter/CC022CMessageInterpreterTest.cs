using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.PL.Business.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC022CMessageInterpreter))]
sealed class CC022CMessageInterpreterTest : MessageInterpreterTest<IIE022>
{
	public void TestInterpret() => CombineAssertions(() =>
	{
		const string testMrn = "TST_MRN";
		const string customsOfficeOfDeparture = $"{CountryCodes.Poland}2233";

		Factory.CreateCustomOfficesForTest((code: customsOfficeOfDeparture, description: "Polish office"));
		Factory.Save();
		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		dataProviderMock.Setup(m => m.PreparationDateAndTime).Returns("2024-01-01T00:01:02");
		dataProviderMock.Setup(m => m.AmendmentNotificationDateAndTime).Returns("2024-02-02T00:01:02");
		dataProviderMock.Setup(x => x.CustomsOfficeOfDeparture).Returns(customsOfficeOfDeparture);

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

		var functionalError = Mock.Of<IFunctionalError>(x =>
			x.ErrorPointer == "x" &&
			x.ErrorReason == "reason" &&
			x.ErrorCode == 999 &&
			x.OriginalAttributeValue == "value"
		);

		dataProviderMock.Setup(m => m.FunctionalErrors).Returns(new[] { functionalError });
		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock);

		const string expectedInterpretation =
"<style>table, th, td { border: 1px solid black; border-collapse: collapse; } " +
"th, td { padding: 5px; text-align: left; }" +
"</style>" +
"<h2>IE022 - Amendment Requested</h2>" +
"<hr />" +
"<table class=\"no-border bold-font\">" +
"<tbody>" +
"<tr><th>MRN</th><td>: TST_MRN</td></tr>" +
"<tr><th>Message Sent On</th><td>: 2024-01-01T00:01:02</td></tr>" +
"<tr><th>Amendment Notification Date</th><td>: 2024-02-02T00:01:02</td></tr>" +
"<tr><th>Customs Office Of Departure</th><td>: PL2233 - Polish office</td></tr>" +
"</tbody>" +
"</table>" +
"<hr />" +
"<table>" +
"<caption><h3>Functional Error</h3></caption>" +
"<tbody>" +
"<tr><th></th><th>Code</th><th>Reason</th><th>Error Pointer</th><th>Error Reference Field</th></tr>" +
"<tr><td>0</td><td>999</td><td>reason</td><td>x</td><td>value</td></tr>" +
"</tbody>" +
"</table>" +
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

		var interpretation = new CC022CMessageInterpreter(nctsHeader.MovementHeader).Interpret(dataProviderMock.Object);
		AssertEquals("Interpretation", expectedInterpretation, interpretation);
	});

	public void TestFunctionalError() => CombineAssertions(() =>
	{
		const int testErrorCode1 = 13;
		const int testErrorCode2 = 14;
		const string testErrorReason = "C0060";
		const string testErrorPointer = "/CC015C/Consignment/HouseConsignment[1]/ConsignmentItem[2]/Packaging[1]/numberOfPackages";
		const string expectedDescription1 = "<tr><td>0</td><td>13</td><td>C0060<br />Condition Violation (Missing)</td><td>/CC015C/Consignment/HouseConsignment[1]/ConsignmentItem[2]/Packaging[1]/numberOfPackages</td><td></td></tr>";
		const string expectedDescription2 = "<tr><td>1</td><td>14</td><td>C0060<br />Rule violation</td><td>/CC015C/Consignment/HouseConsignment[1]/ConsignmentItem[2]/Packaging[1]/numberOfPackages</td><td></td></tr>";

		var functionalErrorMock1 = new Mock<IFunctionalError>();
		functionalErrorMock1.Setup(x => x.ErrorCode).Returns(testErrorCode1);
		functionalErrorMock1.Setup(x => x.ErrorReason).Returns(testErrorReason);
		functionalErrorMock1.Setup(x => x.ErrorPointer).Returns(testErrorPointer);
		var functionalErrorMock2 = new Mock<IFunctionalError>();
		functionalErrorMock2.Setup(x => x.ErrorCode).Returns(testErrorCode2);
		functionalErrorMock2.Setup(x => x.ErrorReason).Returns(testErrorReason);
		functionalErrorMock2.Setup(x => x.ErrorPointer).Returns(testErrorPointer);
		dataProviderMock.SetupGet(x => x.FunctionalErrors).Returns([functionalErrorMock1.Object, functionalErrorMock2.Object]);

		const string codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL180;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var countryCodeEUN = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
		var eunGrouping = helper.CreateNewOrGetExistingDataGrouping(countryCodeEUN);
		var plGrouping = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland, parent: eunGrouping);
		helper.CreateNewOrGetExistingCusCodeType(codeType, $"Test code type {codeType}", countryCodeEUN);
		helper.CreateNewOrGetExistingCusCodeList(plGrouping.ZZZ_DataGrouping, codeType, testErrorCode1.ToString(), "Condition Violation (Missing)", new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));
		helper.CreateNewOrGetExistingCusCodeType(codeType, $"Test code type {codeType}", CountryCodes.Poland);
		helper.CreateNewOrGetExistingCusCodeList(plGrouping.ZZZ_DataGrouping, codeType, testErrorCode2.ToString(), "Rule violation", new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));
		Factory.Save();

		var result = new CC022CMessageInterpreter(nctsHeader.MovementHeader).Interpret(dataProviderMock.Object);
		AssertEquals("Functional Error 1", true, result.Contains(expectedDescription1));
		AssertEquals("Functional Error 2", true, result.Contains(expectedDescription2));
	});

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
	}

	NctsHeader nctsHeader;
}

