using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class HolderOfTheTransitProcedureInterpreterTest : TestCaseWithFactory
{
	public void TestGetRows_HolderEORI() => CombineAssertions(() =>
	{
		const string testEori = "Test EORI";

		var actualValue = CreateInterpreterAndGetRow("EORI");
		AssertNull("EORI is not added if HolderOfTheTransitProcedure is null", actualValue);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		holderOfTheTransitProcedure = holderOfTheTransitProcedureMock.Object;
		actualValue = CreateInterpreterAndGetRow("EORI");
		AssertNull("EORI is not added if IdentificationNumber is null", actualValue);

		holderOfTheTransitProcedureMock.Setup(x => x.IdentificationNumber).Returns(string.Empty);
		actualValue = CreateInterpreterAndGetRow("EORI");
		AssertNull("EORI is not added if IdentificationNumber is empty", actualValue);

		holderOfTheTransitProcedureMock.Setup(x => x.IdentificationNumber).Returns(testEori);
		actualValue = CreateInterpreterAndGetRow("EORI");
		AssertEquals("EORI is added", testEori, actualValue.StringValue);
	});

	public void TestGetRows_HolderTirNumber() => CombineAssertions(() =>
	{
		const string testTirNumber = "Test TIR number";
		var actualValue = CreateInterpreterAndGetRow("TIR Holder Identification Number");
		AssertNull("TIR Number is not added if HolderOfTheTransitProcedure is null", actualValue);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		holderOfTheTransitProcedure = holderOfTheTransitProcedureMock.Object;
		actualValue = CreateInterpreterAndGetRow("TIR Holder Identification Number");
		AssertNull("TIR Number is not added if TIRHolderIdentificationNumber is null", actualValue);

		holderOfTheTransitProcedureMock.Setup(x => x.TIRHolderIdentificationNumber).Returns(string.Empty);
		actualValue = CreateInterpreterAndGetRow("TIR Holder Identification Number");
		AssertNull("TIR Number is not added if TIRHolderIdentificationNumber is empty", actualValue);

		holderOfTheTransitProcedureMock.Setup(x => x.TIRHolderIdentificationNumber).Returns(testTirNumber);
		actualValue = CreateInterpreterAndGetRow("TIR Holder Identification Number");
		AssertEquals("TIR Number is added", testTirNumber, actualValue.StringValue);
	});

	public void TestGetRows_Name() => CombineAssertions(() =>
	{
		const string testHolderName = "Test_Holder_Name";

		var actualValue = CreateInterpreterAndGetRowValue("Name");
		AssertEquals("Name is empty if HolderOfTheTransitProcedure is null", string.Empty, actualValue);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		holderOfTheTransitProcedure = holderOfTheTransitProcedureMock.Object;
		actualValue = CreateInterpreterAndGetRowValue("Name");
		AssertEquals("Name is empty if HolderOfTheTransitProcedure.Name is null", string.Empty, actualValue);

		holderOfTheTransitProcedureMock.Setup(x => x.Name).Returns(string.Empty);
		actualValue = CreateInterpreterAndGetRowValue("Name");
		AssertEquals("Name is empty if HolderOfTheTransitProcedure.Name is empty", string.Empty, actualValue);

		holderOfTheTransitProcedureMock.Setup(x => x.Name).Returns(testHolderName);
		actualValue = CreateInterpreterAndGetRowValue("Name");
		AssertEquals("Name is not empty", testHolderName, actualValue);
	});

	public void TestGetRows_StreetAndNumber() => CombineAssertions(() =>
	{
		const string testAddress1 = "Test Address1";
		const string testAddress2 = "Test Address2";
		const string testStreet = "Test Street";

		var actualValue = CreateInterpreterAndGetRowValue("Street & Address");
		AssertEquals("Street & Address is empty if HolderOfTheTransitProcedure and Principal.Address.Postcode are null", string.Empty, actualValue);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		holderOfTheTransitProcedure = holderOfTheTransitProcedureMock.Object;
		actualValue = CreateInterpreterAndGetRowValue("Street & Address");
		AssertEquals("Street & Address is empty if HolderOfTheTransitProcedure.Address and Principal.Address.Postcode are null", string.Empty, actualValue);

		nctsHeader.Principal.E2_OA_Address = Factory.New<OrgAddress>().PK;
		nctsHeader.Principal.Address.Address1 = testAddress1;
		actualValue = CreateInterpreterAndGetRowValue("Street & Address");
		AssertEquals("Street & Address is added from Principal.Address.Postcode Address1", testAddress1, actualValue);

		nctsHeader.Principal.Address.Address2 = testAddress2;
		var expectedLine = testAddress1 + " " + testAddress2;
		actualValue = CreateInterpreterAndGetRowValue("Street & Address");
		AssertEquals("Street & Address is added from Principal.Address.Postcode Address1 and Address2", expectedLine, actualValue);

		nctsHeader.Principal.Address.Address1 = ZString.Empty;
		actualValue = CreateInterpreterAndGetRowValue("Street & Address");
		AssertEquals("Street & Address is added from Principal.Address.Postcode Address2", testAddress2, actualValue);

		var addressMock = new Mock<IAddress>();
		holderOfTheTransitProcedureMock.Setup(x => x.Address).Returns(addressMock.Object);
		actualValue = CreateInterpreterAndGetRowValue("Street & Address");
		AssertEquals("Street & Address is added from Principal.Address.Postcode if provider's StreetAndNumber is null", testAddress2, actualValue);

		addressMock.Setup(x => x.StreetAndNumber).Returns(string.Empty);
		actualValue = CreateInterpreterAndGetRowValue("Street & Address");
		AssertEquals("Street & Address is added from Principal.Address.Postcode if provider's StreetAndNumber is empty", testAddress2, actualValue);

		addressMock.Setup(x => x.StreetAndNumber).Returns(testStreet);
		actualValue = CreateInterpreterAndGetRowValue("Street & Address");
		AssertEquals("Street & Address is added from provider's StreetAndNumber", testStreet, actualValue);
	});

	public void TestGetRows_PostCode() => CombineAssertions(() =>
	{
		const string testPostCode1 = "01-101";
		const string testPostCode2 = "02-202";

		var actualValue = CreateInterpreterAndGetRowValue("Postcode");
		AssertEquals("Post Code is empty if HolderOfTheTransitProcedure and Principal.Address.Postcode are null", string.Empty, actualValue);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		holderOfTheTransitProcedure = holderOfTheTransitProcedureMock.Object;
		actualValue = CreateInterpreterAndGetRowValue("Postcode");
		AssertEquals("Post Code is empty if HolderOfTheTransitProcedure.Address and Principal.Address.Postcode are null", string.Empty, actualValue);

		nctsHeader.Principal.E2_OA_Address = Factory.New<OrgAddress>().PK;
		nctsHeader.Principal.Address.Postcode = testPostCode1;
		actualValue = CreateInterpreterAndGetRowValue("Postcode");
		AssertEquals("Post Code is added from Principal.Address.Postcode", testPostCode1, actualValue);

		var addressMock = new Mock<IAddress>();
		holderOfTheTransitProcedureMock.Setup(x => x.Address).Returns(addressMock.Object);
		actualValue = CreateInterpreterAndGetRowValue("Postcode");
		AssertEquals("Post Code is added from Principal.Address.Postcode if provider's PostCode is null", testPostCode1, actualValue);

		addressMock.Setup(x => x.PostCode).Returns(string.Empty);
		actualValue = CreateInterpreterAndGetRowValue("Postcode");
		AssertEquals("Post Code is added from Principal.Address.Postcode if provider's PostCode is empty", testPostCode1, actualValue);

		addressMock.Setup(x => x.PostCode).Returns(testPostCode2);
		actualValue = CreateInterpreterAndGetRowValue("Postcode");
		AssertEquals("Post Code is added from provider's PostCode", testPostCode2, actualValue);
	});

	public void TestGetRows_City() => CombineAssertions(() =>
	{
		const string testCity1 = "Test City1";
		const string testCity2 = "Test City2";

		var actualValue = CreateInterpreterAndGetRowValue("City");
		AssertEquals("City is empty if HolderOfTheTransitProcedure and Principal.Address.City are null", string.Empty, actualValue);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		holderOfTheTransitProcedure = holderOfTheTransitProcedureMock.Object;
		actualValue = CreateInterpreterAndGetRowValue("City");
		AssertEquals("City is empty if HolderOfTheTransitProcedure.Address and Principal.Address.City are null", string.Empty, actualValue);

		nctsHeader.Principal.E2_OA_Address = Factory.New<OrgAddress>().PK;
		nctsHeader.Principal.Address.City = testCity1;
		actualValue = CreateInterpreterAndGetRowValue("City");
		AssertEquals("City is added from Principal.Address.City", testCity1, actualValue);

		var addressMock = new Mock<IAddress>();
		holderOfTheTransitProcedureMock.Setup(x => x.Address).Returns(addressMock.Object);
		actualValue = CreateInterpreterAndGetRowValue("City");
		AssertEquals("City is added from Principal.Address.City if provider's City is null", testCity1, actualValue);

		addressMock.Setup(x => x.City).Returns(string.Empty);
		actualValue = CreateInterpreterAndGetRowValue("City");
		AssertEquals("City is added from Principal.Address.City if provider's City is empty", testCity1, actualValue);

		addressMock.Setup(x => x.City).Returns(testCity2);
		actualValue = CreateInterpreterAndGetRowValue("City");
		AssertEquals("City is added from provider's City", testCity2, actualValue);
	});

	public void TestGetRows_Country() => CombineAssertions(() =>
	{
		var actualValue = CreateInterpreterAndGetRowValue("Country");
		AssertEquals("Country is empty if HolderOfTheTransitProcedure and Principal.Address.Country are null", string.Empty, actualValue);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		holderOfTheTransitProcedure = holderOfTheTransitProcedureMock.Object;
		actualValue = CreateInterpreterAndGetRowValue("Country");
		AssertEquals("Country is empty if HolderOfTheTransitProcedure.Address and Principal.Address.Country are null", string.Empty, actualValue);

		nctsHeader.Principal.E2_OA_Address = Factory.New<OrgAddress>().PK;
		nctsHeader.Principal.Address.OA_RN_NKCountryCode = CountryCodes.Australia;
		actualValue = CreateInterpreterAndGetRowValue("Country");
		AssertEquals("Country is added from Principal.Address.Country", CountryCodes.Australia, actualValue);

		var addressMock = new Mock<IAddress>();
		holderOfTheTransitProcedureMock.Setup(x => x.Address).Returns(addressMock.Object);
		actualValue = CreateInterpreterAndGetRowValue("Country");
		AssertEquals("Country is added from Principal.Address.Country if provider's Country is null", CountryCodes.Australia, actualValue);

		addressMock.Setup(x => x.CountryCode).Returns(string.Empty);
		actualValue = CreateInterpreterAndGetRowValue("Country");
		AssertEquals("Country is added from Principal.Address.Country if provider's Country is empty", CountryCodes.Australia, actualValue);

		addressMock.Setup(x => x.CountryCode).Returns(CountryCodes.Poland);
		actualValue = CreateInterpreterAndGetRowValue("Country");
		AssertEquals("Country is added from provider's Country", CountryCodes.Poland, actualValue);
	});

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
	}

	IHolderOfTheTransitProcedure holderOfTheTransitProcedure;
	NctsHeader nctsHeader;

	IParamStringValue CreateInterpreterAndGetRow(string rowKey)
		=> new HolderOfTheTransitProcedureInterpreter(holderOfTheTransitProcedure).GetRows(nctsHeader).FirstOrDefault(x => x.Name == rowKey);

	string CreateInterpreterAndGetRowValue(string rowKey)
		=> new HolderOfTheTransitProcedureInterpreter(holderOfTheTransitProcedure).GetRows(nctsHeader).Single(x => x.Name == rowKey).StringValue.ToString();
}
