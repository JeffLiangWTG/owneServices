using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

static class CC029TestHelper
{
	public record CC029Mocks
	{
		public Mock<IIE029> IE029 { get; set; }
		public Mock<ICC029CTransitOperation> TransitOperation { get; set; }
		public Mock<ICC029CConsignment> Consignment { get; set; }
		public Mock<IRepresentative> Representative { get; set; }
		public Mock<IHolderOfTheTransitProcedureWithContactInfo> HolderOfTheTransitProcedure { get; set; }
		public Mock<IAddress> HolderOfTheTransitProcedureAddress { get; set; }
	}

	public static CC029Mocks SetupDataProviderMock(Mock<IIE029> dataProvider)
	{
		var result = new CC029Mocks { IE029 = dataProvider };

		dataProvider.Setup(m => m.MessageType).Returns("CC928C");
		dataProvider.Setup(m => m.LRN).Returns("TST_LRN");
		dataProvider.Setup(m => m.MRN).Returns("TST_MRN");
		dataProvider.Setup(m => m.PreparationDateAndTime).Returns("TST_PreparationDateAndTime");
		dataProvider.Setup(m => m.CustomsOfficeOfDeparture).Returns($"{CountryCodes.Poland}2233");
		dataProvider.Setup(m => m.CustomsOfficeOfDestinationDeclared).Returns($"{CountryCodes.Poland}4455");

		result.TransitOperation = new Mock<ICC029CTransitOperation>();
		result.TransitOperation.Setup(m => m.DeclarationType).Returns("TST_DeclarationType");
		result.TransitOperation.Setup(m => m.TIRCarnetNumber).Returns("TST_TIRCarnetNumber");
		result.TransitOperation.Setup(m => m.ReleaseDate).Returns(new DateTime(2024, 02, 12));
		dataProvider.Setup(m => m.TransitOperation).Returns(result.TransitOperation.Object);

		result.Consignment = new Mock<ICC029CConsignment>();
		result.Consignment.Setup(m => m.GrossMass).Returns(12);
		dataProvider.Setup(m => m.Consignment).Returns(result.Consignment.Object);

		result.Representative = new Mock<IRepresentative>();
		result.Representative.Setup(m => m.IdentificationNumber).Returns("TST_Representative_EORI");
		dataProvider.Setup(m => m.Representative).Returns(result.Representative.Object);

		result.HolderOfTheTransitProcedure = new Mock<IHolderOfTheTransitProcedureWithContactInfo>();
		result.HolderOfTheTransitProcedure.Setup(m => m.IdentificationNumber).Returns("TST_HolderOfTheTransitProcedure_EORI");
		result.HolderOfTheTransitProcedure.Setup(m => m.TIRHolderIdentificationNumber).Returns("TST_TIRHolderIdentificationNumber");
		result.HolderOfTheTransitProcedure.Setup(m => m.Name).Returns("TST_HolderOfTheTransitProcedure_Name");
		{
			result.HolderOfTheTransitProcedureAddress = new Mock<IAddress>();
			result.HolderOfTheTransitProcedureAddress.Setup(m => m.StreetAndNumber).Returns("TST_StreetAndNumber");
			result.HolderOfTheTransitProcedureAddress.Setup(m => m.PostCode).Returns("TST_PostCode");
			result.HolderOfTheTransitProcedureAddress.Setup(m => m.City).Returns("TST_City");
			result.HolderOfTheTransitProcedureAddress.Setup(m => m.CountryCode).Returns("TST_CountryCode");
			result.HolderOfTheTransitProcedure.Setup(m => m.Address).Returns(result.HolderOfTheTransitProcedureAddress.Object);
		}
		dataProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns(result.HolderOfTheTransitProcedure.Object);

		return result;
	}
}
