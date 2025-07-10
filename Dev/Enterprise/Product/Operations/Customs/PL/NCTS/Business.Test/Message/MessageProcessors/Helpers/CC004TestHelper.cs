using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

static class CC004TestHelper
{
	public record CC004Mocks
	{
		public Mock<IIE004> IE004 { get; set; }
		public Mock<ICC004CTransitOperation> TransitOperation { get; set; }
		public Mock<IHolderOfTheTransitProcedure> HolderOfTheTransitProcedure { get; set; }
	}

	public static CC004Mocks SetupDataProviderMock(Mock<IIE004> dataProvider)
	{
		var result = new CC004Mocks { IE004 = dataProvider };
		const string testEori = "Test EORI";
		const string testTirNumber = "Test TIR number";
		const string testHolderName = "Test_Holder_Name";
		const string testStreetAndAddress = "Test Address1";
		const string testPostCode = "01-101";
		const string testCity = "Test City1";

		dataProvider.Setup(m => m.MessageType).Returns("CC928C");
		dataProvider.Setup(m => m.PreparationDateAndTime).Returns("TST_PreparationDateAndTime");
		dataProvider.Setup(m => m.CustomsOfficeOfDeparture).Returns($"{CountryCodes.Poland}2233");

		result.TransitOperation = new Mock<ICC004CTransitOperation>();
		dataProvider.Setup(m => m.LRN).Returns("TST_LRN");
		dataProvider.Setup(m => m.MRN).Returns("TST_MRN");
		result.TransitOperation.Setup(m => m.AmendmentAcceptanceDateAndTime).Returns("1900-01-01T01:01:01+05:30");
		result.TransitOperation.Setup(m => m.AmendmentSubmissionDateAndTime).Returns("1900-01-01T01:01:01+05:30");
		dataProvider.Setup(m => m.TransitOperation).Returns(result.TransitOperation.Object);

		var holderOfTheTransitProcedureMock = new Mock<IHolderOfTheTransitProcedure>();
		dataProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);
		holderOfTheTransitProcedureMock.Setup(x => x.IdentificationNumber).Returns(testEori);
		holderOfTheTransitProcedureMock.Setup(x => x.TIRHolderIdentificationNumber).Returns(testTirNumber);
		holderOfTheTransitProcedureMock.Setup(x => x.Name).Returns(testHolderName);
		var addressMock = new Mock<IAddress>();
		holderOfTheTransitProcedureMock.Setup(x => x.Address).Returns(addressMock.Object);
		addressMock.Setup(x => x.StreetAndNumber).Returns(testStreetAndAddress);
		addressMock.Setup(x => x.PostCode).Returns(testPostCode);
		addressMock.Setup(x => x.City).Returns(testCity);
		addressMock.Setup(x => x.CountryCode).Returns(CountryCodes.Poland);
		dataProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns(holderOfTheTransitProcedureMock.Object);

		return result;
	}
}
