using System;
using System.Globalization;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC056CMessageInterpreter))]
sealed class CC056CMessageInterpreterTest : MessageInterpreterTest<IIE056>
{
	public void TestInterpret() => CombineAssertions(() =>
	{
		const string testLrn = "TST_LRN";
		const string testMrn = "TST_MRN";
		var testPreparationDate = new DateTime(1994, 1, 1).ToString(CultureInfo.InvariantCulture);
		const string testOfficeDescription = "Test Office Of Departure";

		const string testIdentificationNumber = "4";
		const string testStatus = "2";

		const int testErrorCode = 13;
		const string testErrorCodeDescription = "Condition Violation (Missing)";
		const string testErrorReason = "C0060";
		const string testErrorPointer = "/CC015C/Consignment/HouseConsignment[1]/ConsignmentItem[2]/Packaging[1]/numberOfPackages";
		const string expectedErrorLine = "<tr><td>0</td><td>13</td><td>C0060<br />Condition Violation (Missing)</td><td>/CC015C/Consignment/HouseConsignment[1]/ConsignmentItem[2]/Packaging[1]/numberOfPackages</td><td></td></tr>";

		const string testEori = "Test EORI";
		const string testTirNumber = "Test TIR number";
		const string testHolderName = "Test_Holder_Name";
		const string testStreetAndAddress = "Test Address1";
		const string testPostCode = "01-101";
		const string testCity = "Test City1";

		dataProviderMock.Setup(m => m.LRN).Returns(testLrn);
		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		dataProviderMock.Setup(m => m.PreparationDateAndTime).Returns(testPreparationDate);
		dataProviderMock.Setup(m => m.CustomsOfficeOfDeparture).Returns(testOfficeDescription);

		dataProviderMock.Setup(m => m.Representative.IdentificationNumber).Returns(testIdentificationNumber);
		dataProviderMock.Setup(m => m.Representative.Status).Returns(testStatus);

		FunctionalErrorInterpreterTest.CreateCodeWithDescriptionForCL180(Factory, testErrorCode, testErrorCodeDescription);

		dataProviderMock.Setup(m => m.FunctionalErrors).Returns(new[]
		{
			Mock.Of<IFunctionalError>(x =>
				x.ErrorCode == testErrorCode &&
				x.ErrorReason == testErrorReason &&
				x.ErrorPointer == testErrorPointer),
		});

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

		AssertHasLine("LRN is added from provider's Transit Operation", testLrn);
		AssertHasLine("MRN is added from provider's Transit Operation", testMrn);
		AssertHasLine("Message Sent Date is added from provider's Preparation Date", testPreparationDate);
		AssertHasLine("Customs Office Code is added from provider's Customs Office Code", testOfficeDescription);

		AssertHasLine("Representative IdentificationNumber is added from provider's Representative IdentificationNumber", testIdentificationNumber);
		AssertHasLine("Representative Status Reason is added from provider's Representative Status", "2 : Representative - direct representation (within the meaning of Article 18(1) of the Code)");

		AssertHasLine("Functional error is added from provider's Functional Errors", expectedErrorLine);

		AssertHasLine("EORI is added from provider's EORI", testEori);
		AssertHasLine("TIR Number is added from provider's TIR Number", testTirNumber);
		AssertHasLine("Name is added from provider's Name", testHolderName);
		AssertHasLine("Street and Address is added from provider's Street and Address", testStreetAndAddress);
		AssertHasLine("Post Code is added from provider's Post Code", testPostCode);
		AssertHasLine("City is added from provider's City", testCity);
		AssertHasLine("Country is added from provider's Country", CountryCodes.Poland);
	});

	public void TestLineDescription() => CombineAssertions(() =>
	{
		const string expectedDescription = "<h2>IE056 - Functional Error</h2>";
		AssertHasLine("Description", expectedDescription);
	});

	public void TestTransitOperation() => CombineAssertions(() =>
	{
		const string testBusinessRejectionType = "015";
		const string testRejectionDateAndTime = "2023-07-26T00:12:17.072339";
		const string testRejectionCode = "4";
		const string testRejectionReason = "Other Reasons";

		const string codeType560 = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL560;
		const string codeType226 = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL226;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		var plGrouping = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland, parent: eunGrouping);
		helper.CreateNewOrGetExistingCusCodeType(codeType560, $"Test code type {codeType560}", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusCodeList(plGrouping.ZZZ_DataGrouping, codeType560, testBusinessRejectionType, "Departure Declaration", new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));
		helper.CreateNewOrGetExistingCusCodeType(codeType226, $"Test code type {codeType226}", CountryCodes.Poland);
		helper.CreateCusCodeList(plGrouping.ZZZ_DataGrouping, codeType226, testRejectionCode, "Other Reasons", new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));

		Factory.Save();

		const string ExpectedBusinessRejectionType = "<td>015 = Departure Declaration</td>";
		const string ExpectedRejectionReason = "<td>4 = Other Reasons</td>";

		dataProviderMock.Setup(m => m.TransitOperation.BusinessRejectionType).Returns(testBusinessRejectionType);
		dataProviderMock.Setup(m => m.TransitOperation.RejectionDateAndTime).Returns(testRejectionDateAndTime);
		dataProviderMock.Setup(m => m.TransitOperation.RejectionCode).Returns(testRejectionCode);
		dataProviderMock.Setup(m => m.TransitOperation.RejectionReason).Returns(testRejectionReason);

		AssertHasLine("Rejection Type is added from provider's Rejection Type", ExpectedBusinessRejectionType);
		AssertHasLine("Rejection Date is added from provider's Rejection Date", testRejectionDateAndTime);
		AssertHasLine("Rejection Code is added from provider's Rejection Code", testRejectionCode);
		AssertHasLine("Rejection Reason is added from provider's Rejection Reason", ExpectedRejectionReason);
	});

	public void TestLineMessageSentOn() => CombineAssertions(() =>
	{
		var testPreparationDate = new DateTime(1994, 1, 1).ToString(CultureInfo.InvariantCulture);
		var expectedPreparationDateLine = $"<tr><th>Message Sent On</th><td>{testPreparationDate}</td></tr>";

		dataProviderMock.Setup(x => x.PreparationDateAndTime).Returns(testPreparationDate);
		AssertHasLine("Preparation Date and Time", expectedPreparationDateLine);
	});

	public void TestLineCustomsOfficeOfDeparture() => CombineAssertions(() =>
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
		var expectedOfficeLine = $"<tr><th>Customs Office Of Departure</th><td>{testOfficeCode} - {testOfficeDescription}</td></tr>";
		AssertHasLine("Office Of Departure includes description if office code has valid grouping", expectedOfficeLine);

		dataProviderMock.Setup(m => m.CustomsOfficeOfDeparture).Returns(testOfficeCodeWithMissingGrouping);
		expectedOfficeLine = $"<tr><th>Customs Office Of Departure</th><td>{testOfficeCodeWithMissingGrouping}</td></tr>";
		AssertHasLine("Office Of Departure doesn't include description if office code has unexisting grouping", expectedOfficeLine);
	});

	public void TestLineLRN() => CombineAssertions(() =>
	{
		const string testLrn = "TST_LRN";
		const string expectedLrn = $"<tr><th>LRN</th><td>{testLrn}</td></tr>";

		dataProviderMock.Setup(m => m.LRN).Returns(testLrn);
		AssertHasLine("LRN", expectedLrn);
	});

	public void TestLineMRN() => CombineAssertions(() =>
	{
		const string testMrn = "TST_MRN";
		const string expectedMrn = $"<tr><th>MRN</th><td>{testMrn}</td></tr>";

		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		AssertHasLine("MRN", expectedMrn);
	});

	public void TestLineHolderOfTheTransitProcedure() => CombineAssertions(() =>
	{
		const string expectedLine = "<caption><h3>Holder of the Transit Procedure</h3></caption>";
		AssertNoLine("Holder of the Transit Procedure", expectedLine);
	});

	public void TestLineHolderEori() => CombineAssertions(() =>
	{
		const string testEori = "Test EORI";
		const string emptyEoriLine = "EORI: <br />";
		const string expectedLine = $"EORI: {testEori}";

		AssertNoLine("EORI line is not added if HolderOfTheTransitProcedure is null", emptyEoriLine);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);
		AssertNoLine("EORI line is not added if IdentificationNumber is null", emptyEoriLine);

		holderOfTheTransitProcedureMock.Setup(x => x.IdentificationNumber).Returns(string.Empty);
		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);
		AssertNoLine("EORI line is not added if IdentificationNumber is empty", emptyEoriLine);

		holderOfTheTransitProcedureMock.Setup(x => x.IdentificationNumber).Returns(testEori);
		AssertHasLine("EORI line is added", expectedLine);
	});

	public void TestLineHolderTirNumber() => CombineAssertions(() =>
	{
		const string testTirNumber = "Test TIR number";
		const string emptyTirNumberLine = "TIR Holder Identification Number: <br />";
		const string expectedLine = $"TIR Holder Identification Number: {testTirNumber}";

		AssertNoLine("TIR Number line is not added if HolderOfTheTransitProcedure is null", emptyTirNumberLine);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);
		AssertNoLine("TIR Number line is not added if TIRHolderIdentificationNumber is null", emptyTirNumberLine);

		holderOfTheTransitProcedureMock.Setup(x => x.TIRHolderIdentificationNumber).Returns(string.Empty);
		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);
		AssertNoLine("TIR Number line is not added if TIRHolderIdentificationNumber is empty", emptyTirNumberLine);

		holderOfTheTransitProcedureMock.Setup(x => x.TIRHolderIdentificationNumber).Returns(testTirNumber);
		AssertHasLine("TIR Number line is added", expectedLine);
	});

	public void TestGetName() => CombineAssertions(() =>
	{
		const string testHolderName = "Test_Holder_Name";
		const string expectedLine = $"Name: {testHolderName}";

		AssertNoLine("Name is empty if HolderOfTheTransitProcedure is null", testHolderName);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);
		AssertNoLine("Name is empty if HolderOfTheTransitProcedure.Name is null", testHolderName);

		holderOfTheTransitProcedureMock.Setup(x => x.Name).Returns(string.Empty);
		AssertNoLine("Name is empty if HolderOfTheTransitProcedure.Name is empty", testHolderName);

		holderOfTheTransitProcedureMock.Setup(x => x.Name).Returns(testHolderName);
		AssertHasLine("Name is not empty", expectedLine);
	});

	public void TestGetStreetAndNumber() => CombineAssertions(() =>
	{
		const string testAddress1 = "Test Address1";
		const string testAddress2 = "Test Address2";
		const string testStreet = "Test Street";

		AssertNoLine("Street & Address is empty if HolderOfTheTransitProcedure and Principal.Address.Postcode are null", testAddress1);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);
		AssertNoLine("Street & Address is empty if HolderOfTheTransitProcedure.Address and Principal.Address.Postcode are null", testAddress1);

		nctsHeader.Principal.E2_OA_Address = Factory.New<OrgAddress>().PK;
		nctsHeader.Principal.Address.Address1 = testAddress1;
		var expectedLine = testAddress1;
		AssertHasLine("Street & Address is added from Principal.Address.Postcode Address1", expectedLine);

		nctsHeader.Principal.Address.Address2 = testAddress2;
		expectedLine = testAddress1 + " " + testAddress2;
		AssertHasLine("Street & Address is added from Principal.Address.Postcode Address1 and Address2", expectedLine);

		nctsHeader.Principal.Address.Address1 = ZString.Empty;
		expectedLine = testAddress2;
		AssertHasLine("Street & Address is added from Principal.Address.Postcode Address2", expectedLine);

		var addressMock = new Mock<IAddress>();
		holderOfTheTransitProcedureMock.Setup(x => x.Address).Returns(addressMock.Object);
		AssertHasLine("Street & Address is added from Principal.Address.Postcode if provider's StreetAndNumber is null", expectedLine);

		addressMock.Setup(x => x.StreetAndNumber).Returns(string.Empty);
		AssertHasLine("Street & Address is added from Principal.Address.Postcode if provider's StreetAndNumber is empty", expectedLine);

		addressMock.Setup(x => x.StreetAndNumber).Returns(testStreet);
		expectedLine = $"Street &amp; Address: {testStreet}";
		AssertHasLine("Street & Address is added from provider's StreetAndNumber", expectedLine);
	});

	public void TestGetPostCode() => CombineAssertions(() =>
	{
		const string testPostCode1 = "01-101";
		const string testPostCode2 = "02-202";

		AssertNoLine("Post Code is empty if HolderOfTheTransitProcedure and Principal.Address.Postcode are null", testPostCode1);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);
		AssertNoLine("Post Code is empty if HolderOfTheTransitProcedure.Address and Principal.Address.Postcode are null", testPostCode1);

		nctsHeader.Principal.E2_OA_Address = Factory.New<OrgAddress>().PK;
		nctsHeader.Principal.Address.Postcode = testPostCode1;
		AssertHasLine("Post Code is added from Principal.Address.Postcode", testPostCode1);

		var addressMock = new Mock<IAddress>();
		holderOfTheTransitProcedureMock.Setup(x => x.Address).Returns(addressMock.Object);
		AssertHasLine("Post Code is added from Principal.Address.Postcode if provider's PostCode is null", testPostCode1);

		addressMock.Setup(x => x.PostCode).Returns(string.Empty);
		AssertHasLine("Post Code is added from Principal.Address.Postcode if provider's PostCode is empty", testPostCode1);

		addressMock.Setup(x => x.PostCode).Returns(testPostCode2);
		var expectedLine = $"Postcode: {testPostCode2}";
		AssertHasLine("Post Code is added from provider's PostCode", expectedLine);
	});

	public void TestGetCity() => CombineAssertions(() =>
	{
		const string testCity1 = "Test City1";
		const string testCity2 = "Test City2";

		AssertNoLine("City is empty if HolderOfTheTransitProcedure and Principal.Address.City are null", testCity1);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);
		AssertNoLine("City is empty if HolderOfTheTransitProcedure.Address and Principal.Address.City are null", testCity1);

		nctsHeader.Principal.E2_OA_Address = Factory.New<OrgAddress>().PK;
		nctsHeader.Principal.Address.City = testCity1;
		AssertHasLine("City is added from Principal.Address.City", testCity1);

		var addressMock = new Mock<IAddress>();
		holderOfTheTransitProcedureMock.Setup(x => x.Address).Returns(addressMock.Object);
		AssertHasLine("City is added from Principal.Address.City if provider's City is null", testCity1);

		addressMock.Setup(x => x.City).Returns(string.Empty);
		AssertHasLine("City is added from Principal.Address.City if provider's City is empty", testCity1);

		addressMock.Setup(x => x.City).Returns(testCity2);
		var expectedLine = "City: Test City2";
		AssertHasLine("City is added from provider's City", expectedLine);
	});

	public void TestGetCountry() => CombineAssertions(() =>
	{
		AssertNoLine("Country is empty if HolderOfTheTransitProcedure and Principal.Address.Country are null", CountryCodes.Australia);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		dataProviderMock.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);
		AssertNoLine("Country is empty if HolderOfTheTransitProcedure.Address and Principal.Address.Country are null", CountryCodes.Australia);

		nctsHeader.Principal.E2_OA_Address = Factory.New<OrgAddress>().PK;
		nctsHeader.Principal.Address.OA_RN_NKCountryCode = CountryCodes.Australia;
		AssertHasLine("Country is added from Principal.Address.Country", CountryCodes.Australia);

		var addressMock = new Mock<IAddress>();
		holderOfTheTransitProcedureMock.Setup(x => x.Address).Returns(addressMock.Object);
		AssertHasLine("Country is added from Principal.Address.Country if provider's Country is null", CountryCodes.Australia);

		addressMock.Setup(x => x.CountryCode).Returns(string.Empty);
		AssertHasLine("Country is added from Principal.Address.Country if provider's Country is empty", CountryCodes.Australia);

		addressMock.Setup(x => x.CountryCode).Returns(CountryCodes.Poland);
		var expectedLine = $"Country: {CountryCodes.Poland}";
		AssertHasLine("Country is added from provider's Country", expectedLine);
	});

	void AssertHasLine(string description, string expected)
	{
		var result = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals(description, true, result.Contains(expected));
	}

	void AssertNoLine(string description, string expected)
	{
		var result = interpreter.Interpret(dataProviderMock.Object);
		AssertEquals(description, false, result.Contains(expected));
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		mockTransitOperation = new Mock<ICC056CTransitOperation>();
		dataProviderMock.Setup(m => m.TransitOperation).Returns(mockTransitOperation.Object);
		interpreter = new CC056CMessageInterpreter(nctsHeader.MovementHeader);
	}

	NctsHeader nctsHeader;
	Mock<ICC056CTransitOperation> mockTransitOperation;
	CC056CMessageInterpreter interpreter;
}
