using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using USCustoms = Enterprise.Integration.Customs.US;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISHostWrapper))]
	sealed class DISHostWrapperTest : DISHostWrapperBaseTest<DISHostWrapper, DISDocument>
	{
		public void TestGetDefaultInvoiceLines()
		{
			var declaration = JobDeclaration;
			declaration[JobDeclarationSchema.JE_TransportMode.Name] = Core.Constants.TransportModes.Sea;
			var container = (BusinessObject)Factory.New<USCustoms.ICusContainer>();
			container[CusContainerSchema.CO_JE.Name] = declaration.PK;
			container[CusContainerSchema.CO_ContainerNumber.Name] = "ABCD34283";
			var container2 = (BusinessObject)Factory.New<USCustoms.ICusContainer>();
			container2[CusContainerSchema.CO_JE.Name] = declaration.PK;
			container2[CusContainerSchema.CO_ContainerNumber.Name] = "ABCD34284";
			var invoice = (BusinessObject)Factory.New<USCustoms.IJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.JZ_JE.Name] = declaration.PK;
			invoice[JobComInvoiceHeaderSchema.JZ_InvoiceNumber.Name] = "ABC123";
			var invoiceLine1 = (BusinessObject)Factory.New<USCustoms.IJobComInvoiceLine>();
			invoiceLine1[JobComInvoiceLineSchema.JI_JZ.Name] = invoice.PK;
			invoiceLine1[JobComInvoiceLineSchema.JI_LineNo.Name] = (short)1;
			invoiceLine1[JobComInvoiceLineSchema.JI_Description.Name] = "1";
			var containerLinePivot = (BusinessObject)Factory.New<USCustoms.ICusContainerInvoiceLinePivot>();
			containerLinePivot[CusContainerInvoiceLinePivotSchema.C2_CO.Name] = container.PK;
			containerLinePivot[CusContainerInvoiceLinePivotSchema.C2_JI.Name] = invoiceLine1.PK;
			var containerLinePivot2 = (BusinessObject)Factory.New<USCustoms.ICusContainerInvoiceLinePivot>();
			containerLinePivot2[CusContainerInvoiceLinePivotSchema.C2_CO.Name] = container2.PK;
			containerLinePivot2[CusContainerInvoiceLinePivotSchema.C2_JI.Name] = invoiceLine1.PK;
			var invoiceLine2 = (BusinessObject)Factory.New<USCustoms.IJobComInvoiceLine>();
			invoiceLine2[JobComInvoiceLineSchema.JI_JZ.Name] = invoice.PK;
			invoiceLine2[JobComInvoiceLineSchema.JI_LineNo.Name] = (short)2;
			invoiceLine2[JobComInvoiceLineSchema.JI_Description.Name] = "2";
			var invoice2 = (BusinessObject)Factory.New<USCustoms.IJobComInvoiceHeader>();
			invoice2[JobComInvoiceHeaderSchema.JZ_JE.Name] = declaration.PK;
			invoice2[JobComInvoiceHeaderSchema.JZ_InvoiceNumber.Name] = "ABC124";
			var invoiceLine3 = (BusinessObject)Factory.New<USCustoms.IJobComInvoiceLine>();
			invoiceLine3[JobComInvoiceLineSchema.JI_JZ.Name] = invoice2.PK;
			invoiceLine3[JobComInvoiceLineSchema.JI_LineNo.Name] = (short)1;
			invoiceLine3[JobComInvoiceLineSchema.JI_Description.Name] = "3";
			var containerLinePivot4 = (BusinessObject)Factory.New<USCustoms.ICusContainerInvoiceLinePivot>();
			containerLinePivot4[CusContainerInvoiceLinePivotSchema.C2_CO.Name] = container.PK;
			containerLinePivot4[CusContainerInvoiceLinePivotSchema.C2_JI.Name] = invoiceLine3.PK;
			Factory.Save();
			var declarationLoaded = new BusinessObjectFactory().Load<USCustoms.IJobDeclaration>(declaration.PK);
			var disWrapper = new DISHostWrapper((IUSDISHost)declarationLoaded);
			var lines = disWrapper.GetInvoiceLines("ABC124", 1, 0);
			AssertEquals(1, lines.Count());
			AssertEquals("3", lines.ElementAt(0).CommodityDescription);
			AssertEquals("ABCD34283", lines.ElementAt(0).ContainerNumber);
			lines = disWrapper.GetInvoiceLines("ABC124", 2, 2);
			AssertEquals(0, lines.Count());
			lines = disWrapper.GetInvoiceLines("ABC123", 1, 2);
			AssertEquals(3, lines.Count());
			lines = lines.OrderBy(x => x.CommodityDescription + x.ContainerNumber);
			AssertEquals("1", lines.ElementAt(0).CommodityDescription);
			AssertEquals("ABCD34283", lines.ElementAt(0).ContainerNumber);
			AssertEquals("1", lines.ElementAt(1).CommodityDescription);
			AssertEquals("ABCD34284", lines.ElementAt(1).ContainerNumber);
			AssertEquals("2", lines.ElementAt(2).CommodityDescription);
			AssertEquals("", lines.ElementAt(2).ContainerNumber);
		}

		public void TestGetAllInvoiceLines()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var invoiceMock = new Mock<ICommercialInvoiceDefault>();
			invoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("INV1"));
			var invoiceLineMock = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(1));
			var commodityLine = new Mock<IDISCommodityLine>();
			var invoiceLineMock2 = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock2.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(2));
			var commodityLine2 = new Mock<IDISCommodityLine>();
			var invoice2Mock = new Mock<ICommercialInvoiceDefault>();
			var invoiceLineMock3 = new Mock<IDISInvoiceLineDefault>();
			var commodityLine3 = new Mock<IDISCommodityLine>();
			invoiceMock.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLineDefault[] { invoiceLineMock.Object, invoiceLineMock2.Object });
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var lines = hostWrapper.GetAllInvoiceLines("INV1");
			AssertEquals(2, lines.Count());
			AssertEquals(invoiceLineMock.Object, lines.ElementAt(0));
			AssertEquals(invoiceLineMock2.Object, lines.ElementAt(1));
			Assert(!lines.Contains(invoiceLineMock3.Object));
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			invoiceMock.VerifyAll();
			invoiceLineMock.VerifyAll();
			commodityLine.VerifyAll();
			invoiceLineMock2.VerifyAll();
			commodityLine2.VerifyAll();
			invoice2Mock.VerifyAll();
			invoiceLineMock3.VerifyAll();
			commodityLine3.VerifyAll();
		}

		public void TestGetDefaultCommodityLines()
		{
			var declaration = JobDeclaration;
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
			vehicleDetailAddInfo[USVehicleDetailsAddInfoSchema.US_BuildMonth.Name] = "03";
			var vehicleDetail2 = (BusinessObject)Factory.New<USCustoms.IVehicleDetails>();
			vehicleDetail2[CusAddInfoSchema.B7_ParentID.Name] = vehicle.PK;
			vehicleDetail2[CusAddInfoSchema.B7_ParentTableCode.Name] = "B7";
			var vehicleDetailAddInfo2 = (BusinessObject)((USCustoms.IVehicleDetails)vehicleDetail2).AddInfo;
			vehicleDetailAddInfo2[USVehicleDetailsAddInfoSchema.US_BuildYear.Name] = "2013";
			Factory.Save();
			var declarationLoaded = new BusinessObjectFactory().Load<USCustoms.IJobDeclaration>(declaration.PK);
			var disWrapper = new DISHostWrapper((IUSDISHost)declarationLoaded);
			var lines = disWrapper.GetCommodityLines("ABC123", 1, 1);
			AssertEquals(2, lines.Count());
			var commodity = lines.ElementAt(0);
			AssertEquals(1, commodity.InvoiceLineNumber);
			AssertEquals(1, commodity.VehicleData.VNELineNumber);
			commodity = lines.ElementAt(1);
			AssertEquals(1, commodity.InvoiceLineNumber);
			AssertEquals(1, commodity.VehicleData.VNELineNumber);
			lines = disWrapper.GetCommodityLines("ABC123", 1, 0);
			AssertEquals("All commodity lines should be returned even though no vehicle line no is specified", 2, lines.Count());
		}

		public void TestGetEDoc()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var eDoc1 = new Mock<IeDoc>();
			var pk1 = ZGuid.NewZGuid();
			eDoc1.Setup(m => m.UniqueKey).Returns(pk1);
			eDoc1.Setup(m => m.IsDeleted).Returns(ZBool.False);
			var eDoc2 = new Mock<IeDoc>();
			var pk2 = ZGuid.NewZGuid();
			eDoc2.Setup(m => m.UniqueKey).Returns(pk2);
			eDoc2.Setup(m => m.IsDeleted).Returns(ZBool.False);
			usDISHost.Setup(m => m.EDocs).Returns(new IeDoc[] { eDoc1.Object, eDoc2.Object });
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			AssertEquals(eDoc1.Object, hostWrapper.GetEDoc(pk1));
			AssertEquals(eDoc2.Object, hostWrapper.GetEDoc(pk2));
			AssertNull(hostWrapper.GetEDoc(ZGuid.NewZGuid()));
			usDISHost.VerifyAll();
			eDoc1.VerifyAll();
			eDoc2.VerifyAll();
		}

		public void TestGetDefaultBondData()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var bondDataListMock = new Mock<IDISBondDataDefault>();
			bondDataListMock.Setup(m => m.Code).Returns(new ZString("1"));
			var bondDataListMock2 = new Mock<IDISBondDataDefault>();
			bondDataListMock2.Setup(m => m.Code).Returns(new ZString("2"));
			bondDataListMock2.Setup(m => m.Description).Returns(new ZString("blah2"));
			defaultValues.Setup(m => m.DefaultBondData).Returns(new IDISBondDataDefault[] { bondDataListMock.Object, bondDataListMock2.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			AssertEquals("blah2", hostWrapper.GetDefaultBondData("2").Description);
			AssertNull(hostWrapper.GetDefaultBondData("3"));
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			bondDataListMock.VerifyAll();
			bondDataListMock2.VerifyAll();
		}

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
			invoiceLineMock.Setup(m => m.CommodityDetails).Returns(commodityLineMock.Object);
			invoiceLineMock.Setup(m => m.VehicleData).Returns(vehicleAndEngineDataMock.Object);
			invoiceLineMock.Setup(m => m.EntryLineNumber).Returns(new ZInt(1));
			var vehicleAndEngineDataMock2 = new Mock<IDISVehicleAndEngineData>();
			vehicleAndEngineDataMock2.Setup(m => m.VNELineNumber).Returns(new ZInt(2));
			var commodityLineMock2 = new Mock<IDISCommodityLine>();
			commodityLineMock2.Setup(m => m.VehicleData).Returns(vehicleAndEngineDataMock2.Object);
			var invoiceLineMock2 = new Mock<IDISInvoiceLineDefault>();
			invoiceLineMock2.Setup(m => m.InvoiceLineNumber).Returns(new ZInt(1));
			invoiceLineMock2.Setup(m => m.CommodityDetails).Returns(commodityLineMock2.Object);
			invoiceLineMock2.Setup(m => m.VehicleData).Returns(vehicleAndEngineDataMock2.Object);
			invoiceLineMock2.Setup(m => m.EntryLineNumber).Returns(new ZInt(1));
			var invoiceMock = new Mock<ICommercialInvoiceDefault>();
			invoiceMock.Setup(m => m.InvoiceNumber).Returns(new ZString("INV1"));
			invoiceMock.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLineDefault[] { invoiceLineMock.Object, invoiceLineMock2.Object });
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var lines = hostWrapper.GetCommodityLinesWithRangesOfVNELines("INV1", 1, 1, 2);
			AssertEquals(2, lines.Count());
			lines = hostWrapper.GetCommodityLinesWithRangesOfVNELines("INV1", 1, 3, 4);
			AssertEquals(0, lines.Count());
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
			invoiceMock.Setup(m => m.InvoiceLines).Returns(new IDISInvoiceLineDefault[] { invoiceLineMock.Object, invoiceLineMock2.Object, invoiceLineMock3.Object });
			defaultValues.Setup(m => m.DefaultInvoiceData).Returns(new ICommercialInvoiceDefault[] { invoiceMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var lines = hostWrapper.GetCommodityLinesWithRangesOfInvoiceLines("INV1", 1, 2);
			AssertEquals(2, lines.Count());
			lines = hostWrapper.GetCommodityLinesWithRangesOfInvoiceLines("INV1", 1, 3);
			AssertEquals(3, lines.Count());
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			invoiceLineMock.VerifyAll();
			invoiceLineMock2.VerifyAll();
			invoiceLineMock3.VerifyAll();
			invoiceMock.VerifyAll();
		}

		protected override BusinessObject GetNewBusinessObject() => new DISHostWrapper((IUSDISHost)JobDeclaration);

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
