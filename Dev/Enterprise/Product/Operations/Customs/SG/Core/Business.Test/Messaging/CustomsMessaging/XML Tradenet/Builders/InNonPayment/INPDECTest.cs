using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class INPDECTest : TestCaseWithFactory
	{
		public void TestINPDECMessage()
		{
			AssertEquals(CommonAccessReferenceCodeList.Codes.INPDEC, Message.MessageType);
			AssertEquals(CUSDECEDIMessage.Declaration, Message.MessageSubType);
			DataProvider.JobNumber = "CD2020";
			var dec = new TradenetDeclaration();
			AssertNull(dec.InboundMessage);
			var testIPTDECBuilder = new INPDEC(DataProvider);
			testIPTDECBuilder.Build(dec);
			AssertType<InNonPayment>(dec.InboundMessage.InNonPayment);
			AssertNull(dec.InboundMessage.InNonPaymentUpdate);
			AssertEquals("WTGCD2020", dec.InboundMessage.InNonPayment.Header.MessageReference);
		}

		public void TestStorageLocation()
		{
			DataProvider.PlaceOfStorage = "AUSYD";
			DataProvider.Is2bStoredBWCY = true;
			DataProvider.IsStorageInFTZ = false;
			Message.BuildCargo(InNonPayment);
			AssertEquals("AUSYD", InNonPayment.Cargo.StorageLocation.LocationCode);
			DataProvider.Is2bStoredBWCY = false;
			DataProvider.IsStorageInFTZ = true;
			Message.BuildCargo(InNonPayment);
			AssertEquals("AUSYD", InNonPayment.Cargo.StorageLocation.LocationCode);
			DataProvider.Is2bStoredBWCY = false;
			DataProvider.IsStorageInFTZ = false;
			InNonPayment.Cargo.StorageLocation = null;
			Message.BuildCargo(InNonPayment);
			AssertNull(InNonPayment.Cargo.StorageLocation);
		}

		public void TestLoadingNextPort()
		{
			DataProvider.NextPortOfCall = "AUSYD";
			DataProvider.HasOutwardTransport = DataProvider.IsSeaStoreDeclaration = false;
			Message.BuildTransport(InNonPayment);
			AssertNull(InNonPayment.Transport);
			DataProvider.HasOutwardTransport = DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			Message.BuildTransport(InNonPayment);
			var additionalVesselInformation = InNonPayment.Transport.OutwardTransport.AdditionalVesselInformation;
			AssertEquals("AUSYD", additionalVesselInformation.LoadingNextPort);
		}

		public void TestLoadingFinalPort()
		{
			DataProvider.HasOutwardTransport = true;
			DataProvider.IsSeaStoreDeclaration = true;
			DataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			DataProvider.FinalPortOfCall = "AUSYD";
			DataProvider.HasLiquorOrTobacco = false;
			Message.BuildTransport(InNonPayment);
			AssertNull(InNonPayment.Transport.OutwardTransport.AdditionalVesselInformation);
			DataProvider.HasLiquorOrTobacco = true;
			Message.BuildTransport(InNonPayment);
			var additionalVesselInformation = InNonPayment.Transport.OutwardTransport.AdditionalVesselInformation;
			AssertEquals("AUSYD", additionalVesselInformation.LoadingFinalPort);
		}

		public void TestFinalDestinationCountry()
		{
			DataProvider.CountryOfFinalDestination = "AU";
			DataProvider.HasOutwardTransport = true;
			DataProvider.IsSeaStoreDeclaration = false;
			DataProvider.DeclarationType = string.Empty;
			Message.BuildTransport(InNonPayment);
			var outwardTransport = InNonPayment.Transport.OutwardTransport;
			AssertNull(outwardTransport.FinalDestinationCountry);
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.REX;
			Message.BuildTransport(InNonPayment);
			outwardTransport = InNonPayment.Transport.OutwardTransport;
			AssertEquals("AU", outwardTransport.FinalDestinationCountry);
			outwardTransport.FinalDestinationCountry = null;
			DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.SFZ;
			Message.BuildTransport(InNonPayment);
			outwardTransport = InNonPayment.Transport.OutwardTransport;
			AssertEquals("AU", outwardTransport.FinalDestinationCountry);
		}

		public void TestExhibitionTemporaryImportPeriod()
		{
			DataProvider.StartDateOfTemporaryImport = new CargoWise.Types.ZDate(2015, 01, 26);
			DataProvider.EndDateOfTemporaryImport = new CargoWise.Types.ZDate(2015, 02, 01);
			DataProvider.IsTemporaryConsignment = false;
			Message.BuildCargo(InNonPayment);
			AssertNull(InNonPayment.Cargo.ExhibitionTemporaryImportPeriod);
			DataProvider.IsTemporaryConsignment = true;
			Message.BuildCargo(InNonPayment);
			var period = InNonPayment.Cargo.ExhibitionTemporaryImportPeriod;
			AssertEquals("20150126", period.StartDate);
			AssertEquals("20150201", period.EndDate);
		}

		public void TestOtherTariff()
		{
			DataProvider.Items = new ItemsTestClass[] { new ItemsTestClass { DutyRateUnit = SGConstants.LPA, OtherTaxPercentageRate = 5.35m, OtherTaxUnitRate = 15.80m, OtherTaxAmount = 5098.35m } };
			Message.BuildItem(InNonPayment);
			var otherTariff = InNonPayment.Item[0].Tariff.OtherTax;
			AssertEquals(5098.35m, otherTariff.DutyAmount);
			AssertEquals(5.35m, otherTariff.DutyRate);
			AssertEquals(SGConstants.LPA, otherTariff.DutyRateUnit);
			DataProvider.Items = new ItemsTestClass[] { new ItemsTestClass { DutyRateUnit = SGConstants.LPA, OtherTaxUnitRate = 15.80m, OtherTaxAmount = 5098.35m } };
			Message.BuildItem(InNonPayment);
			otherTariff = InNonPayment.Item[0].Tariff.OtherTax;
			AssertEquals(5098.35m, otherTariff.DutyAmount);
			AssertEquals(15.80m, otherTariff.DutyRate);
			AssertEquals(SGConstants.LPA, otherTariff.DutyRateUnit);
		}

		INPDEC Message => message ?? (message = new INPDEC(DataProvider));
		INPDEC message;
		INPDECTestClass DataProvider => dataProvider ?? (dataProvider = new INPDECTestClass());
		INPDECTestClass dataProvider;
		InNonPayment InNonPayment => inNonPayment ?? (inNonPayment = new InNonPayment());
		InNonPayment inNonPayment;
	}
}
