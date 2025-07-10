using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC051CMessageInterpreter))]
sealed class CC051CMessageInterpreterTest : MessageInterpreterTest<IIE051>
{
	public void TestInterpret() => CombineAssertions(() =>
	{
		const string testLrn = "TST_LRN";
		const string testLRNSubmissionDate = "20/01/2002";
		const string testMrn = "TST_MRN";
		const string testSentOnDate = "20/01/2002";
		const string testOfficeDescription = "Test Office Of Departure";
		const string testOfficeDestination = "Test Office Of Destination";
		const string testDeclarationType = "testType";
		const int testNumberOfPackages = 5;
		const int testTotalGoodsItems = 100;
		const decimal testGrossWeight = 20.5m;
		const string testReleaseRejectionCode = "B1";
		const string testAdditionalRejectionRemark = "Reason";
		const string representativeId = "EORI123";

		const string testEori = "EORI_456";
		const string testTirNumber = "TIR_123";
		const string testHolderName = "Test holder 1";
		const string testStreetAndAddress = "Test holder address";
		const string testPostCode = "12-345";
		const string testCity = "Warsaw";
		const string testCountry = "Poland";

		var dataProviderMock = Mock.Of<IIE051>(x =>
			x.MessageType == "CC051C" &&
			x.MRN == testMrn &&
			x.LRN == testLrn &&
			x.PreparationDateAndTime == testSentOnDate &&
			x.CustomsOfficeOfDeparture == testOfficeDescription &&
			x.CountrySpecificDataPL == Mock.Of<IIE051CountrySpecificDataPL>(c =>
				c.CustomsOfficeOfDestination == testOfficeDestination &&
				c.DeclarationType == testDeclarationType &&
				c.NumberOfPackages == testNumberOfPackages &&
				c.TotalNumberOfConsignmentItems == testTotalGoodsItems &&
				c.TotalGrossMass == testGrossWeight) &&
			x.TransitOperation == Mock.Of<ICC051CTransitOperation>(o =>
				o.DeclarationSubmissionDateAndTime == testLRNSubmissionDate &&
				o.NoReleaseMotivationCode == testReleaseRejectionCode &&
				o.NoReleaseMotivationText == testAdditionalRejectionRemark) &&
			x.Representative ==  Mock.Of<ICC051CRepresentative>(r =>
				r.IdentificationNumber == representativeId) &&
			x.HolderOfTheTransitProcedure == Mock.Of<IHolderOfTheTransitProcedureWithContactInfo>(h =>
				h.IdentificationNumber == testEori &&
				h.TIRHolderIdentificationNumber == testTirNumber &&
				h.Name == testHolderName &&
				h.Address == Mock.Of<IAddress>(a =>
					a.StreetAndNumber == testStreetAndAddress &&
					a.PostCode == testPostCode &&
					a.City == testCity &&
					a.CountryCode == testCountry)));

		var expectedInterpretation = ExtendedGlobalHtmlStyle + """
			<h2>IE051 - Not Released For Transit</h2><hr />
			<table class="no-border bold-font">
				<tbody>
					<tr><th>LRN</th><td>: TST_LRN</td></tr>
					<tr><th>LRN Submission Date</th><td>: 20/01/2002</td></tr>
					<tr><th>MRN</th><td>: TST_MRN</td></tr>
					<tr><th>Message Sent On</th><td>: 20/01/2002</td></tr>
					<tr><th>Customs Office Of Departure</th><td>: Test Office Of Departure</td></tr>
					<tr><th>Customs Office Of Destination</th><td>: Test Office Of Destination</td></tr>
					<tr><th>Declaration Type</th><td>: testType</td></tr>
					<tr><th>Number Of Packages</th><td>: 5</td></tr>
					<tr><th>Total Goods Items</th><td>: 100</td></tr>
					<tr><th>Gross Weight</th><td>: 20.5</td></tr>
					<tr><th>Release Rejection Code</th><td>: B1</td></tr>
					<tr><th>Additional Release Remark</th><td>: Reason</td></tr>
				</tbody>
			</table><hr />

			<p>
			<h3>Representative</h3>
			EORI: EORI123</p>
			<hr />

			<p>
			<h3>Holder of the Transit Procedure</h3>
			EORI: EORI_456<br />
			TIR Holder Identification Number: TIR_123<br />
			Name: Test holder 1<br />
			Street &amp; Address: Test holder address<br />
			Postcode: 12-345<br />
			City: Warsaw<br />
			Country: Poland</p>
			<hr />
			""".ToSingleLineHtml();

		var interpretation = CreateInterpreterAndInterpret(dataProviderMock).ToSingleLineHtml();
		AssertEquals("Interpretation", expectedInterpretation, interpretation);
	});

	public void TestLineDescription()
	{
		const string expectedDescription = "<h2>IE051 - Not Released For Transit</h2>";
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Description", true, interpretation.Contains(expectedDescription));
	}

	public void TestLrn()
	{
		const string testLrn = "TST_LRN";

		dataProviderMock.Setup(m => m.LRN).Returns(testLrn);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("LRN", interpretation, "LRN", ": " + testLrn);
	}

	public void TestLRNSubmissionDate()
	{
		const string testLRNSubmissionDate = "20/01/2002";

		mockTransitOperation.Setup(m => m.DeclarationSubmissionDateAndTime).Returns(testLRNSubmissionDate);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("LRN Submission Date", interpretation, "LRN Submission Date", ": " + testLRNSubmissionDate);
	}

	public void TestMrn()
	{
		const string testMrn = "TST_MRN";

		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("MRN", interpretation, "MRN", ": " + testMrn);
	}

	public void TestMessageSentOn()
	{
		const string testSentOnDate = "20/01/2002";

		dataProviderMock.Setup(m => m.PreparationDateAndTime).Returns(testSentOnDate);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("Message Sent On", interpretation, "Message Sent On", ": " + testSentOnDate);
	}

	public void TestCustomsOfficeOfDeparture() => CombineAssertions(() =>
	{
		const string testOfficeCode = $"{CountryCodes.Poland}2233";
		const string testOfficeCodeWithMissingGrouping = "DE2233";
		const string testOfficeDescription = "Test Office Of Departure";

		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, $"Test code type {codeType}", CountryCodes.Poland);
		helper.CreateCusCodeList(CountryCodes.Poland, codeType, testOfficeCode, testOfficeDescription, new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));
		Factory.Save();

		dataProviderMock.Setup(m => m.CustomsOfficeOfDeparture).Returns(testOfficeCode);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("Office Of Departure includes description if office code has valid grouping", interpretation, "Customs Office Of Departure", $": {testOfficeCode} - {testOfficeDescription}");

		dataProviderMock.Setup(m => m.CustomsOfficeOfDeparture).Returns(testOfficeCodeWithMissingGrouping);
		interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("Office Of Departure doesn't include description if office code has nonexistent grouping", interpretation, "Customs Office Of Departure", ": " + testOfficeCodeWithMissingGrouping);
	});

	public void TestCustomsOfficeOfDestination() => CombineAssertions(() =>
	{
		const string testOfficeCode = $"{CountryCodes.Poland}2233";
		const string testOfficeCodeWithMissingGrouping = "DE2233";
		const string testOfficeDescription = "Test Office Of Destination";

		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, $"Test code type {codeType}", CountryCodes.Poland);
		helper.CreateCusCodeList(CountryCodes.Poland, codeType, testOfficeCode, testOfficeDescription, new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));
		Factory.Save();

		mockCountrySpecificDataPL.Setup(m => m.CustomsOfficeOfDestination).Returns(testOfficeCode);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("Office Of Destination includes description if office code has valid grouping", interpretation, "Customs Office Of Destination", $": {testOfficeCode} - {testOfficeDescription}");

		mockCountrySpecificDataPL.Setup(m => m.CustomsOfficeOfDestination).Returns(testOfficeCodeWithMissingGrouping);
		interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("Office Of Destination doesn't include description if office code has nonexistent grouping", interpretation, "Customs Office Of Destination", ": " + testOfficeCodeWithMissingGrouping);
	});

	public void TestDeclarationType()
	{
		const string testDeclarationType = "testType";

		mockCountrySpecificDataPL.Setup(m => m.DeclarationType).Returns(testDeclarationType);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("Declaration Type", interpretation, "Declaration Type", ": " + testDeclarationType);
	}

	public void TestNumberOfPackages()
	{
		const int testNumberOfPackages = 5;

		mockCountrySpecificDataPL.Setup(m => m.NumberOfPackages).Returns(testNumberOfPackages);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("Number Of Packages", interpretation, "Number Of Packages", ": " + testNumberOfPackages);
	}

	public void TestTotalGoodsItems()
	{
		const int testTotalGoodsItems = 100;

		mockCountrySpecificDataPL.Setup(m => m.TotalNumberOfConsignmentItems).Returns(testTotalGoodsItems);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("Total Goods Items", interpretation, "Total Goods Items", ": " + testTotalGoodsItems);
	}

	public void TestGrossWeight()
	{
		const decimal testGrossWeight = 20.5m;

		mockCountrySpecificDataPL.Setup(m => m.TotalGrossMass).Returns(testGrossWeight);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("Gross Weight", interpretation, "Gross Weight", ": " + testGrossWeight);
	}

	public void TestReleaseRejectionCode()
	{
		const string testReleaseRejectionCode = "B1";
		const string testDescription = "Unsuccessful Control Results";

		var today = ZDateTime.Today;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland);
		var codeType = helper.CreateCusCodeType(
			EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL211,
			"Test No Release Motivation");
		_ = helper.CreateNewOrGetExistingCusCodeList(
			grouping.ZZZ_DataGrouping,
			codeType.ZZK_CodeType,
			testReleaseRejectionCode,
			testDescription,
			ZDateTime.Today.AddDays(-2),
			ZDateTime.Today.AddDays(2));
		Factory.Save();

		mockTransitOperation.Setup(m => m.NoReleaseMotivationCode).Returns(testReleaseRejectionCode);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("Release Rejection Code", interpretation, "Release Rejection Code", ": " + testReleaseRejectionCode + " - " + testDescription);
	}

	public void TestAdditionalRejectionRemark()
	{
		const string testAdditionalRejectionRemark = "Reason";

		mockTransitOperation.Setup(m => m.NoReleaseMotivationText).Returns(testAdditionalRejectionRemark);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("Additional Release Remark", interpretation, "Additional Release Remark", ": " + testAdditionalRejectionRemark);
	}

	public void TestRepresentative() => CombineAssertions(() =>
	{
		dataProviderMock.Setup(m => m.Representative).Returns((ICC051CRepresentative)null);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Representative section is not presented if node is null.", false, interpretation.Contains("Representative"));

		var representativeMock = new Mock<ICC051CRepresentative>();

		dataProviderMock.Setup(m => m.Representative).Returns(representativeMock.Object);
		interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Representative section is not presented if child nodes are empty.", false, interpretation.Contains("Representative"));
		AssertEquals("EORI is not presented if it is not defined.", false, interpretation.Contains("EORI"));
		AssertEquals("Status is not presented if it is not defined.", false, interpretation.Contains("Status"));

		representativeMock.Setup(m => m.IdentificationNumber).Returns("TestNum.");
		interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Representative section is presented.", true, interpretation.Contains("Representative"));
		AssertEquals("EORI is presented if it is defined.", true, interpretation.Contains("EORI: TestNum."));
	});

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		mockCountrySpecificDataPL = new Mock<IIE051CountrySpecificDataPL>();
		mockTransitOperation = new Mock<ICC051CTransitOperation>();
		dataProviderMock.Setup(m => m.CountrySpecificDataPL).Returns(mockCountrySpecificDataPL.Object);
		dataProviderMock.Setup(m => m.TransitOperation).Returns(mockTransitOperation.Object);
		dataProviderMock.Setup(m => m.MessageType).Returns("CC051C");
	}

	NctsHeader nctsHeader;

	Mock<IIE051CountrySpecificDataPL> mockCountrySpecificDataPL;
	Mock<ICC051CTransitOperation> mockTransitOperation;

	void AssertHasRow(string description, string interpretation, string expectedHeader, string expectedData = null) =>
		AssertEquals(description, true, interpretation.Contains(
			$"<tr><th>{expectedHeader}</th><td>{expectedData}</td></tr>"));

	string CreateInterpreterAndInterpret(IIE051 dataProvider) => new CC051CMessageInterpreter(nctsHeader.MovementHeader).Interpret(dataProvider);
}
