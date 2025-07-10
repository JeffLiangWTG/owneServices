using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using USCustoms = Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISCommodityLine))]
	sealed class DISCommodityLineTest : XmlSerializableNonPersistentBusinessObjectTest<DISCommodityLine>
	{
		public void TestRangeOfLinesWithVNELineTo()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var vehicleAndEngineDataMock = new Mock<IDISVehicleAndEngineData>();
			vehicleAndEngineDataMock.Setup(m => m.VNELineNumber).Returns(new ZInt(1));
			var commodityLineMock = new Mock<IDISCommodityLine>();
			commodityLineMock.Setup(m => m.VehicleData).Returns(vehicleAndEngineDataMock.Object);
			var invoiceLineMock = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(1));
			invoiceLineMock.Setup(m => m.EntryLineNumber).Returns(new ZInt(1));
			invoiceLineMock.Setup(m => m.CommodityDetails).Returns(commodityLineMock.Object);
			invoiceLineMock.Setup(m => m.VehicleData).Returns(vehicleAndEngineDataMock.Object);
			var vehicleAndEngineDataMock2 = new Mock<IDISVehicleAndEngineData>();
			vehicleAndEngineDataMock2.Setup(m => m.VNELineNumber).Returns(new ZInt(2));
			var commodityLineMock2 = new Mock<IDISCommodityLine>();
			commodityLineMock2.Setup(m => m.VehicleData).Returns(vehicleAndEngineDataMock2.Object);
			var invoiceLineMock2 = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock2.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(1));
			invoiceLineMock2.Setup(m => m.CommodityDetails).Returns(commodityLineMock2.Object);
			invoiceLineMock2.Setup(m => m.EntryLineNumber).Returns(new ZInt(1));
			invoiceLineMock2.Setup(m => m.VehicleData).Returns(vehicleAndEngineDataMock2.Object);
			var invoiceMock = new Mock<ICommercialInvoiceDefault>();
			invoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("INV1"));
			invoiceMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			invoiceMock.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLineDefault[] { invoiceLineMock.Object, invoiceLineMock2.Object });
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var commodityLine = new DISCommodityLine(disDocument);
			commodityLine.InvoiceNumber = "INV1";
			commodityLine.InvoiceLineNumber = 1;
			commodityLine.VNELineNumber = 1;
			commodityLine.VNELineNumberTo = 2;
			var lines = commodityLine.CommodityLines;
			AssertEquals(2, lines.Count());
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			vehicleAndEngineDataMock.VerifyAll();
			commodityLineMock.VerifyAll();
			invoiceLineMock.VerifyAll();
			vehicleAndEngineDataMock2.VerifyAll();
			commodityLineMock2.VerifyAll();
			invoiceLineMock2.VerifyAll();
			invoiceMock.VerifyAll();
		}

		public void TestRangeOfLinesWithInvoiceLineTo()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var invoiceLineMock = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(1));
			var invoiceLineMock2 = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock2.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(2));
			var invoiceLineMock3 = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock3.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(3));
			var invoiceMock = new Mock<ICommercialInvoiceDefault>();
			invoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("INV1"));
			invoiceMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			invoiceMock.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLineDefault[] { invoiceLineMock.Object, invoiceLineMock2.Object, invoiceLineMock3.Object });
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var commodityLine = new DISCommodityLine(disDocument);
			commodityLine.InvoiceNumber = "INV1";
			commodityLine.InvoiceLineNumber = 1;
			commodityLine.InvoiceLineTo = 2;
			var lines = commodityLine.CommodityLines;
			AssertEquals(2, lines.Count());
			commodityLine.InvoiceLineTo = 3;
			lines = commodityLine.CommodityLines;
			AssertEquals(3, lines.Count());
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			invoiceLineMock.VerifyAll();
			invoiceLineMock2.VerifyAll();
			invoiceLineMock3.VerifyAll();
			invoiceMock.VerifyAll();
		}

		public void TestReadOnlyWhenLineToIsEntered()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var invoiceLineMock = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(1));
			var invoiceMock = new Mock<ICommercialInvoiceDefault>();
			invoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("INV1"));
			invoiceMock.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLineDefault[] { invoiceLineMock.Object });
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var commodityLine = new DISCommodityLine(disDocument);
			commodityLine.InvoiceLineTo = 2;
			AssertEquals(0, commodityLine.VNELineNumber);
			AssertEquals(0, commodityLine.VNELineNumberTo);
			Assert(commodityLine.VNELineNumberInfo.ReadOnly);
			Assert(commodityLine.VNELineNumberToInfo.ReadOnly);
			commodityLine.InvoiceLineTo = 0;
			Assert(!commodityLine.VNELineNumberInfo.ReadOnly);
			Assert(!commodityLine.VNELineNumberToInfo.ReadOnly);
			commodityLine.VNELineNumber = 1;
			commodityLine.VNELineNumberTo = 2;
			Assert(!commodityLine.VNELineNumberInfo.ReadOnly);
			Assert(!commodityLine.VNELineNumberToInfo.ReadOnly);
			Assert(commodityLine.InvoiceLineToInfo.ReadOnly);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			invoiceLineMock.VerifyAll();
			invoiceMock.VerifyAll();
		}

		public void TestDefaultInvoiceList()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var defaultInvoiceMock = new Mock<ICommercialInvoiceDefault>();
			defaultInvoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("423987432"));
			defaultInvoiceMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { defaultInvoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var commodityLine = new DISCommodityLine(disDocument);
			var list = commodityLine.DefaultInvoiceList;
			AssertEquals(1, list.Count);
			AssertEquals("423987432", list[0].Code);
			AssertEquals("Blah", list[0].Description);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			defaultInvoiceMock.VerifyAll();
		}

		public void TestGetDefaultCommodityLines()
		{
			var declaration = new TestHelper(Factory).GetJobDeclaration();
			declaration[JobDeclarationSchema.JE_TransportMode.Name] = Core.Constants.TransportModes.Sea;
			var invoice = (BusinessObject)Factory.New<USCustoms.IJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.JZ_JE.Name] = declaration.PK;
			invoice[JobComInvoiceHeaderSchema.JZ_InvoiceNumber.Name] = "ABC123";
			var invoiceLine1 = (BusinessObject)Factory.New<USCustoms.IJobComInvoiceLine>();
			invoiceLine1[JobComInvoiceLineSchema.JI_JZ.Name] = invoice.PK;
			invoiceLine1[JobComInvoiceLineSchema.JI_LineNo.Name] = (short)1;
			invoiceLine1[JobComInvoiceLineSchema.JI_Description.Name] = "1";
			var vehicle = (BusinessObject)Factory.New<USCustoms.IVehicle>();
			vehicle[CusAddInfoSchema.B7_ParentID.Name] = invoiceLine1.PK;
			vehicle[CusAddInfoSchema.B7_ParentTableCode.Name] = "JI";
			var vehicleAddInfo = (BusinessObject)((USCustoms.IVehicle)vehicle).AddInfo;
			vehicleAddInfo[USVehicleAddInfoSchema.US_LineNo.Name] = (short)1;
			var vehicleDetail = (BusinessObject)Factory.New<USCustoms.IVehicleDetails>();
			vehicleDetail[CusAddInfoSchema.B7_ParentID.Name] = vehicle.PK;
			vehicleDetail[CusAddInfoSchema.B7_ParentTableCode.Name] = "B7";
			var vehicleDetailAddInfo = (BusinessObject)((USCustoms.IVehicleDetails)vehicleDetail).AddInfo;
			vehicleDetailAddInfo[USVehicleDetailsAddInfoSchema.US_BuildMonth.Name] = "01";
			vehicleDetailAddInfo[USVehicleDetailsAddInfoSchema.US_BuildYear.Name] = "2013";
			var vehicleDetail2 = (BusinessObject)Factory.New<USCustoms.IVehicleDetails>();
			vehicleDetail2[CusAddInfoSchema.B7_ParentID.Name] = vehicle.PK;
			vehicleDetail2[CusAddInfoSchema.B7_ParentTableCode.Name] = "B7";
			var vehicleDetailAddInfo2 = (BusinessObject)((USCustoms.IVehicleDetails)vehicleDetail2).AddInfo;
			vehicleDetailAddInfo2[USVehicleDetailsAddInfoSchema.US_BuildMonth.Name] = "04";
			vehicleDetailAddInfo2[USVehicleDetailsAddInfoSchema.US_BuildYear.Name] = "2014";
			var invoiceLine2 = (BusinessObject)Factory.New<USCustoms.IJobComInvoiceLine>();
			invoiceLine2[JobComInvoiceLineSchema.JI_JZ.Name] = invoice.PK;
			invoiceLine2[JobComInvoiceLineSchema.JI_LineNo.Name] = (short)2;
			invoiceLine2[JobComInvoiceLineSchema.JI_Description.Name] = "2";
			Factory.Save();
			var declarationLoaded = new BusinessObjectFactory().Load<USCustoms.IJobDeclaration>(declaration.PK);
			var invoiceLine2Loaded = (BusinessObject)declarationLoaded.Factory.Load<USCustoms.IJobComInvoiceLine>(invoiceLine2.PK);
			invoiceLine2Loaded[JobComInvoiceLineSchema.JI_LineNo.Name] = (short)2;
			var disWrapper = new DISHostWrapper((IUSDISHost)declarationLoaded);
			var disDoc = disWrapper.DISDocuments.AddNew();
			var commodityLine = disDoc.CommodityData.AddNew();
			commodityLine.InvoiceNumber = "ABC123";
			commodityLine.InvoiceLineNumber = 1;
			commodityLine.VNELineNumber = 1;
			AssertEquals(2, commodityLine.CommodityLines.Count());
			commodityLine.InvoiceLineNumber = 2;
			commodityLine.VNELineNumber = 0;
			AssertEquals(1, commodityLine.CommodityLines.Count());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDeclaration = new TestHelper(Factory).GetJobDeclaration();
			var hostWrapper = new DISHostWrapper((IUSDISHost)jobDeclaration);
			var disDocument = new DISDocument(hostWrapper);
			return new DISCommodityLine(disDocument);
		}
	}
}
