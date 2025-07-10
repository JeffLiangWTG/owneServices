using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

public class HtmlBlockWriterHelperTest : TestCase
{
	public void TestInterpret_Guarantor()
	{
		var htmlWriterMock = new Mock<IHtmlBlockWriter>(MockBehavior.Strict);
		var values = new ParamValueCollection {
			{ "EORI", "PL123456789" },
			{ "Name", "Entity Name" },
			{ "Street & Address", "ul. Radosna 6" },
			{ "Postcode", "44-000" },
			{ "City", "Warsaw" },
			{ "Country", "PL" },
		};
		htmlWriterMock.Setup(writer => writer.WriteParamValueSequence(
				new("Guarantor"),
				values,
				default,
				true))
			.Verifiable(Times.Once);
		var guarantor = Mock.Of<IGuarantor>(g =>
			g.Name == "Entity Name" &&
			g.IdentificationNumber == "PL123456789" &&
			g.Address == Mock.Of<IAddress>(a =>
				a.StreetAndNumber == "ul. Radosna 6" &&
				a.City == "Warsaw" &&
				a.PostCode == "44-000" &&
				a.CountryCode == "PL"));

		htmlWriterMock.Object.Interpret(guarantor);

		AssertNoExceptionThrown(() => htmlWriterMock.VerifyAll());
	}

	public void TestInterpret_Guarantor_MissingData()
	{
		var htmlWriterMock = new Mock<IHtmlBlockWriter>(MockBehavior.Strict);
		var values = new ParamValueCollection {
			{ "EORI", ZString.Empty },
			{ "Name", ZString.Empty },
			{ "Street & Address", ZString.Empty },
			{ "Postcode", ZString.Empty },
			{ "City", ZString.Empty },
			{ "Country", ZString.Empty },
		};
		htmlWriterMock.Setup(writer => writer.WriteParamValueSequence(
				new("Guarantor"),
				values,
				default,
				true))
			.Verifiable(Times.Once);
		var guarantor = Mock.Of<IGuarantor>();

		htmlWriterMock.Object.Interpret(guarantor);

		AssertNoExceptionThrown(() => htmlWriterMock.VerifyAll());
	}

	public void TestInterpret_Guarantor_Null()
	{
		var htmlWriterMock = new Mock<IHtmlBlockWriter>(MockBehavior.Strict);
		var values = new ParamValueCollection {
			{ "EORI", ZString.Empty },
			{ "Name", ZString.Empty },
			{ "Street & Address", ZString.Empty },
			{ "Postcode", ZString.Empty },
			{ "City", ZString.Empty },
			{ "Country", ZString.Empty },
		};
		htmlWriterMock.Setup(writer => writer.WriteParamValueSequence(
				new("Guarantor"),
				values,
				default,
				true))
			.Verifiable(Times.Once);
		htmlWriterMock.Object.Interpret(null);

		AssertNoExceptionThrown(() => htmlWriterMock.VerifyAll());
	}
}
