using System;
using System.Collections.Generic;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC060CMessageInterpreter))]
sealed class CC060CMessageInterpreterTest : MessageInterpreterTest<IIE060>
{
	public void TestInterpret() => CombineAssertions(() =>
	{
		const string testLrn = "TST_LRN";
		const string testMrn = "TST_MRN";
		const string testSentOnDate = "20/01/2002";
		const string testOfficeDescription = "Test Office Of Departure";
		const string testNotificationDateTime = "20/01/2002 15:30:00";
		const string notificationType = "5";
		const string typeOfControl = "T1";
		const string typeOfControlText = "Test Text";
		const string documentType = "doc. type 1";
		const string documentDescription = "doc. description 1";
		const string representativeId = "EORI123";
		const string representativeStatus = "1";

		dataProviderMock.Setup(m => m.LRN).Returns(testLrn);
		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		dataProviderMock.Setup(m => m.PreparationDateAndTime).Returns(testSentOnDate);
		dataProviderMock.Setup(m => m.CustomsOfficeOfDeparture).Returns(testOfficeDescription);
		mockTransitOperation.Setup(m => m.ControlNotificationDateAndTime).Returns(testNotificationDateTime);
		mockTransitOperation.Setup(m => m.NotificationType).Returns(notificationType);

		var typeOfControlMock = new Mock<ITypeOfControls>();
		dataProviderMock.Setup(m => m.TypeOfControls).Returns(new List<ITypeOfControls>() { typeOfControlMock.Object });
		typeOfControlMock.Setup(m => m.Type).Returns(typeOfControl);
		typeOfControlMock.Setup(m => m.Text).Returns(typeOfControlText);

		var requestedDocumentMock = new Mock<IRequestedDocument>();
		dataProviderMock.Setup(m => m.RequestedDocuments).Returns(new List<IRequestedDocument>() { requestedDocumentMock.Object });
		requestedDocumentMock.Setup(m => m.DocumentType).Returns(documentType);
		requestedDocumentMock.Setup(m => m.Description).Returns(documentDescription);

		var representativeMock = new Mock<IRepresentative>();
		dataProviderMock.Setup(m => m.Representative).Returns(representativeMock.Object);
		representativeMock.Setup(m => m.IdentificationNumber).Returns(representativeId);
		representativeMock.Setup(m => m.Status).Returns(representativeStatus);

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

		"<h2>IE060 - Control Decision</h2><hr />" +
		"<table class=\"no-border bold-font\">" +
		"<tbody>" +
		"<tr><th>LRN</th><td>: TST_LRN</td></tr>" +
		"<tr><th>MRN</th><td>: TST_MRN</td></tr>" +
		"<tr><th>Message Sent On</th><td>: 20/01/2002</td></tr>" +
		"<tr><th>Customs Office Of Departure</th><td>: Test Office Of Departure</td></tr>" +
		"<tr><th>Control Notification Date &amp; Time</th><td>: 20/01/2002 15:30:00</td></tr>" +
		"<tr><th>Notification Type</th><td>: </td></tr>" +
		"</tbody>" +
		"</table><hr />" +

		"<table class=\"fixed-table\">" +
		"<caption><h3>Type of Control</h3></caption>" +
		"<tbody>" +
		"<tr><th>Code</th><td>Description</td></tr>" +
		"<tr><th>T1</th><td><br />Test Text</td></tr>" +
		"</tbody>" +
		"</table><hr />" +

		"<table class=\"fixed-table\">" +
		"<caption><h3>Requested Documents</h3></caption>" +
		"<tbody>" +
		"<tr><th>Document Type</th><td>Description</td></tr>" +
		"<tr><th>doc. type 1</th><td>doc. description 1</td></tr>" +
		"</tbody>" +
		"</table><hr />" +

		"<p>" +
		"<h3>Representative</h3>" +
		"EORI: EORI123<br />" +
		"Status: 1</p>" +
		"<hr />" +

		"<p>" +
		"<h3>Holder of the Transit Procedure</h3>" +
		"EORI: EORI_456<br />" +
		"TIR Holder Identification Number: TIR_123<br />" +
		"Name: Test holder 1<br />" +
		"Street &amp; Address: Test holder address<br />" +
		"Postcode: 12-345<br />" +
		"City: Warsaw<br />" +
		"Country: Poland</p>" +
		"<hr />";

		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Interpretation", expectedInterpretation, interpretation);
	});

	public void TestLineDescription()
	{
		const string expectedDescription = "<h2>IE060 - Control Decision</h2>";
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

	public void TestControlNotificationDateAndTime()
	{
		const string testNotificationDateTime = "20/01/2002 15:30:00";

		mockTransitOperation.Setup(m => m.ControlNotificationDateAndTime).Returns(testNotificationDateTime);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertHasRow("Control Notification Date & Time", interpretation, "Control Notification Date &amp; Time", ": " + testNotificationDateTime);
	}

	public void TestNotificationType() => CombineAssertions(() =>
	{
		foreach (var (testCode, expectedNotificationType) in new[] {
			("0", NCTS5NotificationTypes.Descriptions.DecisionToControlAndRequestedDocumentsIfNeeded),
			("1", NCTS5NotificationTypes.Descriptions.AdditionalDocumentsRequest),
			("2", NCTS5NotificationTypes.Descriptions.IntentionToControl),
			("wrong", string.Empty),
		})
		{
			mockTransitOperation.Setup(m => m.NotificationType).Returns(testCode);
			var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
			AssertHasRow($"Control Notification Date & Time (code {testCode}).", interpretation, "Notification Type", ": " + expectedNotificationType);
		}
	});

	public void TestTypeOfControl() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping("PL");
		helper.CreateNewOrGetExistingCusCodeType("CL716", "Test list CL716.");
		helper.CreateCusCodeList("PL", "CL716", "T1", "Type Of Control 1", ZDateTime.UtcNow.AddMonths(-2), ZDateTime.UtcNow.AddMonths(2));
		Factory.Save();

		dataProviderMock.Setup(m => m.TypeOfControls).Returns((IReadOnlyCollection<ITypeOfControls>)null);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Type Of Control section is not presented if node is null.", false, interpretation.Contains("Types of Control"));
		AssertNoRow("Table headers are not presented if node is null.", interpretation, "Code", "Description");

		dataProviderMock.Setup(m => m.TypeOfControls).Returns(new List<ITypeOfControls>());
		interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Type Of Control section is not presented if count is null.", false, interpretation.Contains("Types of Control"));
		AssertNoRow("Table headers are not presented if count is null.", interpretation, "Code", "Description");

		var typeOfControlMock = new Mock<ITypeOfControls>();
		typeOfControlMock.Setup(m => m.Type).Returns("T1");
		typeOfControlMock.Setup(m => m.Text).Returns("Test Text");

		dataProviderMock.Setup(m => m.TypeOfControls).Returns(new List<ITypeOfControls>() { typeOfControlMock.Object });
		interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Type Of Control section is presented.", true, interpretation.Contains("Type of Control"));
		AssertHasRow("Table headers are presented.", interpretation, "Code", "Description");
		AssertHasRow("Data is presented.", interpretation, "T1", "Type Of Control 1<br />Test Text");
	});

	public void TestRequestedDocuments() => CombineAssertions(() =>
	{
		dataProviderMock.Setup(m => m.RequestedDocuments).Returns((IReadOnlyCollection<IRequestedDocument>)null);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Requested Documents section is not presented if node is null.", false, interpretation.Contains("Requested Documents"));
		AssertNoRow("Table headers are not presented if node is null.", interpretation, "Document Type", "Description");

		dataProviderMock.Setup(m => m.RequestedDocuments).Returns(new List<IRequestedDocument>());
		interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Requested Documents section is not presented if count is null.", false, interpretation.Contains("Requested Documents"));
		AssertNoRow("Table headers are not presented if count is null.", interpretation, "Document Type", "Description");

		var requestedDocumentMock = new Mock<IRequestedDocument>();
		requestedDocumentMock.Setup(m => m.DocumentType).Returns("doc. type 1");
		requestedDocumentMock.Setup(m => m.Description).Returns("doc. description 1");

		dataProviderMock.Setup(m => m.RequestedDocuments).Returns(new List<IRequestedDocument>() { requestedDocumentMock.Object });
		interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Requested Documents section is presented.", true, interpretation.Contains("Requested Documents"));
		AssertHasRow("Table headers are presented.", interpretation, "Document Type", "Description");
		AssertHasRow("Data is presented.", interpretation, "doc. type 1", "doc. description 1");
	});

	public void TestRepresentative() => CombineAssertions(() =>
	{
		dataProviderMock.Setup(m => m.Representative).Returns((IRepresentative)null);
		var interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Representative section is not presented if node is null.", false, interpretation.Contains("Representative"));

		var representativeMock = new Mock<IRepresentative>();

		dataProviderMock.Setup(m => m.Representative).Returns(representativeMock.Object);
		interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Representative section is not presented if child nodes are empty.", false, interpretation.Contains("Representative"));
		AssertEquals("EORI is not presented if it is not defined.", false, interpretation.Contains("EORI"));
		AssertEquals("Status is not presented if it is not defined.", false, interpretation.Contains("Status"));

		representativeMock.Setup(m => m.IdentificationNumber).Returns("TestNum.");
		interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Representative section is presented.", true, interpretation.Contains("Representative"));
		AssertEquals("EORI is presented if it is defined.", true, interpretation.Contains("EORI: TestNum."));

		representativeMock.Setup(m => m.Status).Returns("1");
		interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Status is presented if it is defined.", true, interpretation.Contains("Status: 1"));

		representativeMock.Setup(m => m.Status).Returns("2");
		interpretation = CreateInterpreterAndInterpret(dataProviderMock.Object);
		AssertEquals("Status is presented and extended if it is defined as 2.", true, interpretation.Contains("Status: 2 : Representative - direct representation (within the meaning of Article 18(1) of the Code)"));
	});

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		mockTransitOperation = new Mock<ICC060CTransitOperation>();
		dataProviderMock.Setup(m => m.TransitOperation).Returns(mockTransitOperation.Object);
	}

	NctsHeader nctsHeader;

	Mock<ICC060CTransitOperation> mockTransitOperation;

	void AssertHasRow(string description, string interpretation, string expectedHeader, string expectedData = null) =>
		AssertEquals(description, true, interpretation.Contains(
			$"<tr><th>{expectedHeader}</th><td>{expectedData}</td></tr>"));

	void AssertNoRow(string description, string interpretation, string expectedHeader, string expectedData = null) =>
		AssertEquals(description, false, interpretation.Contains(
			$"<tr><th>{expectedHeader}</th><td>{expectedData}</td></tr>"));

	string CreateInterpreterAndInterpret(IIE060 dataProvider) => new CC060CMessageInterpreter(nctsHeader.MovementHeader).Interpret(dataProvider);
}
