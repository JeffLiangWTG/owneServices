using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISCommodityLineValidationTest : TestCaseWithFactory
	{
		public void TestValidateInvoiceNumber()
		{
			var usDISHost = GetDISHostMock();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var commodityData = disDocument.CommodityData.AddNew();
			commodityData.InvoiceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(commodityData.InvoiceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			commodityData.InvoiceNumber = "INV12";
			AssertNoMessageErrorContaining(commodityData.InvoiceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(commodityData.InvoiceNumberInfo, ListValidation.InvalidCodeMessageError);
			commodityData.InvoiceNumber = "INV1";
			AssertNoMessageErrorContaining(commodityData.InvoiceNumberInfo, ListValidation.InvalidCodeMessageError);
			usDISHost.VerifyAll();
		}

		public void TestValidateInvoiceLineNumber()
		{
			var usDISHost = GetDISHostMock();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var commodityData = disDocument.CommodityData.AddNew();
			commodityData.InvoiceNumber = "INV1";
			commodityData.InvoiceLineNumber = 0;
			AssertHasMessageErrorContaining(commodityData.InvoiceLineNumberInfo, DISCommodityLineValidation.PleaseEnterInvoiceLineNumber);
			commodityData.InvoiceLineNumber = 2;
			AssertNoMessageErrorContaining(commodityData.InvoiceLineNumberInfo, DISCommodityLineValidation.PleaseEnterInvoiceLineNumber);
			AssertHasMessageErrorContaining(commodityData.InvoiceLineNumberInfo, "Invoice Line # '2' does not exist on Invoice # 'INV1'");
			commodityData.InvoiceLineNumber = 1;
			AssertNoMessageErrorContaining(commodityData.InvoiceLineNumberInfo, "Invoice Line # '2' does not exist on Invoice # 'INV1'");
			usDISHost.VerifyAll();
		}

		public void TestValidateInvoiceLineNumberTo()
		{
			var usDISHost = GetDISHostMock();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var commodityData = disDocument.CommodityData.AddNew();
			commodityData.InvoiceNumber = "INV1";
			commodityData.InvoiceLineNumber = 1;
			commodityData.InvoiceLineTo = -1;
			AssertHasMessageErrorContaining(commodityData.InvoiceLineToInfo, DISCommodityLineValidation.PleaseEnterValidInvoiceLineNumber);
			commodityData.InvoiceLineTo = 2;
			AssertNoMessageErrorContaining(commodityData.InvoiceLineToInfo, DISCommodityLineValidation.PleaseEnterValidInvoiceLineNumber);
			AssertHasMessageErrorContaining(commodityData.InvoiceLineToInfo, "Invoice Line # '2' does not exist on Invoice # 'INV1'");
			commodityData.InvoiceLineTo = 1;
			AssertNoMessageErrorContaining(commodityData.InvoiceLineToInfo, "Invoice Line # '2' does not exist on Invoice # 'INV1'");
			usDISHost.VerifyAll();
		}

		public void TestValidateVehicleLineNumber()
		{
			var usDISHost = GetDISHostMock();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var commodityData = disDocument.CommodityData.AddNew();
			commodityData.InvoiceNumber = "INV1";
			commodityData.InvoiceLineNumber = 1;
			commodityData.VNELineNumber = 2;
			AssertHasMessageErrorContaining(commodityData.VNELineNumberInfo, "Vehicle (VNE) Line # '2' does not exist on Invoice # 'INV1' and Invoice Line # '1'.");
			commodityData.VNELineNumber = 1;
			AssertNoMessageErrorContaining(commodityData.VNELineNumberInfo, "Vehicle (VNE) Line # '2' does not exist on Invoice # 'INV1' and Invoice Line # '1'.");
			usDISHost.VerifyAll();
		}

		public void TestValidateVehicleLineNumberTo()
		{
			var usDISHost = GetDISHostMock();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var commodityData = disDocument.CommodityData.AddNew();
			commodityData.InvoiceNumber = "INV1";
			commodityData.InvoiceLineNumber = 1;
			commodityData.VNELineNumber = 1;
			commodityData.VNELineNumberTo = -1;
			AssertHasMessageErrorContaining(commodityData.VNELineNumberToInfo, DISCommodityLineValidation.InvalidVehicleLineNumber);
			commodityData.VNELineNumber = 2;
			commodityData.VNELineNumberTo = 1;
			AssertNoMessageErrorContaining(commodityData.VNELineNumberToInfo, DISCommodityLineValidation.InvalidVehicleLineNumber);
			AssertHasMessageErrorContaining(commodityData.VNELineNumberToInfo, DISCommodityLineValidation.InvalidVNELineTo);
			commodityData.VNELineNumber = 1;
			commodityData.VNELineNumberTo = 2;
			AssertNoMessageErrorContaining(commodityData.VNELineNumberToInfo, DISCommodityLineValidation.InvalidVNELineTo);
			AssertHasMessageErrorContaining(commodityData.VNELineNumberToInfo, "Vehicle (VNE) Line # '2' does not exist on Invoice # 'INV1' and Invoice Line # '1'.");
			commodityData.VNELineNumberTo = 1;
			AssertNoMessageErrorContaining(commodityData.VNELineNumberToInfo, "Vehicle (VNE) Line # '2' does not exist on Invoice # 'INV1' and Invoice Line # '1'.");
			usDISHost.VerifyAll();
		}

		Mock<IUSDISHost> GetDISHostMock()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var invoiceMock = new Mock<ICommercialInvoiceDefault>();
			invoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("INV1"));
			invoiceMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			var invoiceLineMock = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(1));
			var commodityLineMock = new Mock<IDISCommodityLine>();
			commodityLineMock.Setup(m => m.ArrivalDate).Returns(new ZDateTime(12, 10, 10));
			commodityLineMock.Setup(m => m.CommodityDescription).Returns(new ZString("commLineDesc"));
			commodityLineMock.Setup(m => m.ContainerNumber).Returns(new ZString("conNum1"));
			commodityLineMock.Setup(m => m.CountryOfOrigin).Returns(new ZString("Aus"));
			commodityLineMock.Setup(m => m.EntryLineNumber).Returns(new ZInt(1));
			commodityLineMock.Setup(m => m.HTSNumber).Returns(new ZString("HTSNo-1"));
			commodityLineMock.Setup(m => m.PortOfEntry).Returns(new ZString("PortEntry-1"));
			commodityLineMock.Setup(m => m.PortOfLoading).Returns(new ZString("PortOfLoading-1"));
			commodityLineMock.Setup(m => m.PortOfUnlading).Returns(new ZString("PortOfUnlading1"));
			commodityLineMock.Setup(m => m.SealNumber).Returns(new ZString("SelaNo-1"));
			commodityLineMock.Setup(m => m.TradeParties).Returns(System.Array.Empty<IDISTradeParty>());
			var vehicleAndEngineDataMock = new Mock<IDISVehicleAndEngineData>();
			vehicleAndEngineDataMock.Setup(m => m.EngineManufactureDate).Returns(new ZDateTime(10, 11, 12));
			vehicleAndEngineDataMock.Setup(m => m.EngineManufacturer).Returns(new ZString("engineManufacturer1"));
			vehicleAndEngineDataMock.Setup(m => m.EngineModel).Returns(new ZString("engineModel1"));
			vehicleAndEngineDataMock.Setup(m => m.EngineSerialNumber).Returns(new ZString("engineSerialNumber1"));
			vehicleAndEngineDataMock.Setup(m => m.Manufacturer).Returns(new ZString("manufacturer1"));
			vehicleAndEngineDataMock.Setup(m => m.Model).Returns(new ZString("model1"));
			vehicleAndEngineDataMock.Setup(m => m.SerialNumber).Returns(new ZString("serialNu1"));
			vehicleAndEngineDataMock.Setup(m => m.VIN).Returns(new ZString("vin1"));
			vehicleAndEngineDataMock.Setup(m => m.VNELineNumber).Returns(new ZInt(1));
			vehicleAndEngineDataMock.Setup(m => m.ManufactureYear).Returns(new ZString("1998"));
			vehicleAndEngineDataMock.Setup(m => m.ManufactureMonth).Returns(new ZString("1"));
			commodityLineMock.Setup(m => m.VehicleData).Returns(vehicleAndEngineDataMock.Object);
			invoiceLineMock.Setup(m => m.CommodityDetails).Returns(commodityLineMock.Object);
			invoiceMock.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLineDefault[] { invoiceLineMock.Object });
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			return usDISHost;
		}
	}
}
