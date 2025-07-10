using System;
using System.Globalization;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CC028CMessageInterpreter))]
sealed class CC028CMessageInterpreterTest : MessageInterpreterTest<IIE028>
{
	public void TestInterpret()
	{
		const string testLrn = "TST_LRN";
		var testPreparationDate = new DateTime(1994, 1, 1).ToString(CultureInfo.InvariantCulture);
		const string testOfficeDescription = "Test Office Of Departure";
		const string testMrn = "TST_MRN";
		var testAcceptanceDate = new DateTime(1994, 1, 1);
		const string testEori = "Test EORI";
		const string testTirNumber = "Test TIR number";
		const string testHolderName = "Test_Holder_Name";
		const string testStreetAndAddress = "Test Address1";
		const string testPostCode = "01-101";
		const string testCity = "Test City1";

		dataProviderMock.Setup(m => m.LRN).Returns(testLrn);
		dataProviderMock.Setup(m => m.PreparationDateAndTime).Returns(testPreparationDate);
		dataProviderMock.Setup(m => m.CustomsOfficeOfDeparture).Returns(testOfficeDescription);
		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		dataProviderMock.Setup(m => m.DeclarationAcceptanceDate).Returns(testAcceptanceDate);
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

		AssertHasLine("LRN is added from provider's Country", testLrn);
		AssertHasLine("Message Sent Date is added from provider's Preparation Date", testPreparationDate);
		AssertHasLine("Customs Office Code is added from provider's Customs Office Code", testOfficeDescription);
		AssertHasLine("MRN is added from provider's Country", testMrn);
		AssertHasLine("Acceptance Date is added from provider's Acceptance Date", testAcceptanceDate.ToShortDateString());
		AssertHasLine("EORI is added from provider's EORI", testEori);
		AssertHasLine("TIR Number is added from provider's TIR Number", testTirNumber);
		AssertHasLine("Name is added from provider's Name", testHolderName);
		AssertHasLine("Street and Address is added from provider's Street and Address", testStreetAndAddress);
		AssertHasLine("Post Code is added from provider's Post Code", testPostCode);
		AssertHasLine("City is added from provider's City", testCity);
		AssertHasLine("Country is added from provider's Country", CountryCodes.Poland);
	}

	public void TestLineDescription()
	{
		const string expectedDescription = "<tr><th align=\"left\" width=\"700\">IE028 - MRN ALLOCATED</th></tr>";
		AssertHasLine("Description", expectedDescription);
	}

	public void TestLineMessageSentOn()
	{
		var testPreparationDate = new DateTime(1994, 1, 1).ToString(CultureInfo.InvariantCulture);
		var expectedPreparationDateLine = $"<tr><th align=\"left\" width=\"700\">Message Sent On</th><td align=\"left\" width=\"500\"> : {testPreparationDate}</td></tr>";

		dataProviderMock.Setup(x => x.PreparationDateAndTime).Returns(testPreparationDate);
		AssertHasLine("Preparation Date and Time", expectedPreparationDateLine);
	}

	public void TestLineCustomsOfficeOfDeparture()
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
		var expectedOfficeLine = $"<tr><th align=\"left\" width=\"700\">Customs Office of Departure</th><td align=\"left\" width=\"500\"> : {testOfficeCode} - {testOfficeDescription}</td></tr>";
		AssertHasLine("Office Of Departure includes description if office code has valid grouping", expectedOfficeLine);

		dataProviderMock.Setup(m => m.CustomsOfficeOfDeparture).Returns(testOfficeCodeWithMissingGrouping);
		expectedOfficeLine = $"<tr><th align=\"left\" width=\"700\">Customs Office of Departure</th><td align=\"left\" width=\"500\"> : {testOfficeCodeWithMissingGrouping}</td></tr>";
		AssertHasLine("Office Of Departure doesn't include description if office code has unexisting grouping", expectedOfficeLine);
	}

	public void TestLineLRN()
	{
		const string testLrn = "TST_LRN";
		const string expectedLrn = $"<tr><th align=\"left\" width=\"700\">LRN</th><td align=\"left\" width=\"500\"> : {testLrn}</td></tr>";

		dataProviderMock.Setup(m => m.LRN).Returns(testLrn);
		AssertHasLine("LRN", expectedLrn);
	}

	public void TestLineMRN()
	{
		const string testMrn = "TST_MRN";
		const string expectedMrn = $"<tr><th align=\"left\" width=\"700\">MRN</th><td align=\"left\" width=\"500\"> : {testMrn}</td></tr>";

		dataProviderMock.Setup(m => m.MRN).Returns(testMrn);
		AssertHasLine("MRN", expectedMrn);
	}

	public void TestLineAcceptanceDate()
	{
		var testAcceptanceDate = new DateTime(1994, 1, 1);
		var expectedAcceptanceDate = $"<tr><th align=\"left\" width=\"700\">Acceptance Date</th><td align=\"left\" width=\"500\"> : {testAcceptanceDate.ToShortDateString()}</td></tr>";

		dataProviderMock.Setup(m => m.DeclarationAcceptanceDate).Returns(testAcceptanceDate);
		AssertHasLine("Declaration Acceptance Date", expectedAcceptanceDate);
	}

	public void TestLineHolderOfTheTransitProcedure()
	{
		const string expectedLine = "<tr><th align=\"left\" width=\"700\">Holder of the Transit Procedure</th></tr>";
		AssertHasLine("Holder of the Transit Procedure", expectedLine);
	}

	public void TestLineHolderEori() => CombineAssertions(() =>
	{
		const string testEori = "Test EORI";
		const string emptyEoriLine = "<tr><td align=\"left\" width=\"700\">EORI</td><td align=\"left\" width=\"500\"> : </td></tr>";
		const string expectedLine = $"<tr><td align=\"left\" width=\"700\">EORI</td><td align=\"left\" width=\"500\"> : {testEori}</td></tr>";

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
		const string emptyTirNumberLine = "<tr><td align=\"left\" width=\"700\">TIR Holder Identification Number</td><td align=\"left\" width=\"500\"> : </td></tr>";
		const string expectedLine = $"<tr><td align=\"left\" width=\"700\">TIR Holder Identification Number</td><td align=\"left\" width=\"500\"> : {testTirNumber}</td></tr>";

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
		const string expectedLine = $"<tr><td align=\"left\" width=\"700\">Name</td><td align=\"left\" width=\"500\"> : {testHolderName}</td></tr>";

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
		expectedLine = $"<tr><td align=\"left\" width=\"700\">Street &amp; Address</td><td align=\"left\" width=\"500\"> : {testStreet}</td></tr>";
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
		var expectedLine = $"<tr><td align=\"left\" width=\"700\">Postcode</td><td align=\"left\" width=\"500\"> : {testPostCode2}</td></tr>";
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
		var expectedLine = $"<tr><td align=\"left\" width=\"700\">City</td><td align=\"left\" width=\"500\"> : {testCity2}</td></tr>";
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
		var expectedLine = $"<tr><td align=\"left\" width=\"700\">Country</td><td align=\"left\" width=\"500\"> : {CountryCodes.Poland}</td></tr>";
		AssertHasLine("Country is added from provider's Country", expectedLine);
	});

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		interpreter = new CC028CMessageInterpreter(nctsHeader.MovementHeader);
	}

	NctsHeader nctsHeader;
	CC028CMessageInterpreter interpreter;

	void AssertHasLine(string description, string expected) =>
		AssertEquals(description, true, interpreter.Interpret(dataProviderMock.Object).Contains(expected));

	void AssertNoLine(string description, string expected) =>
		AssertEquals(description, false, interpreter.Interpret(dataProviderMock.Object).Contains(expected));
}
