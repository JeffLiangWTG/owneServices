using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DISHostWrapperLookupsTest : TestCaseWithFactory
	{
		public void TestEDocsList()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var eDoc1 = new Mock<IeDoc>();
			eDoc1.Setup(m => m.UniqueKey).Returns(ZGuid.NewZGuid());
			eDoc1.Setup(m => m.DocType).Returns(new ZString("DT1"));
			eDoc1.Setup(m => m.FileName).Returns(new ZString("Example.txt"));
			eDoc1.Setup(m => m.DateAdded).Returns(ZDateTime.BrettsBirthday);
			eDoc1.Setup(m => m.Description).Returns(new ZString("description1"));
			eDoc1.Setup(m => m.IsDeleted).Returns(ZBool.False);
			var eDoc2 = new Mock<IeDoc>();
			eDoc2.Setup(m => m.UniqueKey).Returns(ZGuid.NewZGuid());
			eDoc2.Setup(m => m.DocType).Returns(new ZString("DT2"));
			eDoc2.Setup(m => m.FileName).Returns(new ZString("Example2.txt"));
			eDoc2.Setup(m => m.DateAdded).Returns(ZDateTime.BrettsBirthday.AddDays(1));
			eDoc2.Setup(m => m.Description).Returns(new ZString("description2"));
			eDoc2.Setup(m => m.IsDeleted).Returns(ZBool.False);
			usDISHost.Setup(m => m.EDocs).Returns(new IeDoc[] { eDoc1.Object, eDoc2.Object });
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var list = hostWrapper.Lookups.EDocsList;
			AssertEquals(2, list.Count);
			AssertEquals("DT1-Example.txt", list[0].Code);
			AssertEquals("DT2-Example2.txt", list[1].Code);
			usDISHost.VerifyAll();
			eDoc1.VerifyAll();
			eDoc2.VerifyAll();
		}

		public void TestInvoiceList()
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
			var list = hostWrapper.Lookups.InvoiceList;
			AssertEquals(1, list.Count);
			AssertEquals("423987432", list[0].Code);
			AssertEquals("Blah", list[0].Description);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			defaultInvoiceMock.VerifyAll();
		}

		public void TestCBPRequests()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var defaultRequestMock = new Mock<IDISCBPRequestDefault>();
			defaultRequestMock.Setup(m => m.ID).Returns(new ZString("423987432"));
			defaultRequestMock.Setup(m => m.Description).Returns(new ZString("Blah"));
			defaultValues.Setup(m => m.DefaultCBPRequests).Returns(new IDISCBPRequestDefault[] { defaultRequestMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var list = hostWrapper.Lookups.DefaultCBPRequestList;
			AssertEquals(3, list.Count);
			AssertEquals("423987432", list[0].Code);
			AssertEquals("Blah", list[0].Description);
			Assert(list.ContainsCode(MiscCBPRequestIDList.Codes.Unknown));
			Assert(list.ContainsCode(MiscCBPRequestIDList.Codes.Unsolicited));
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			defaultRequestMock.VerifyAll();
		}

		public void TestDefaultBondDetails()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var bondDataListMock = new Mock<IDISBondDataDefault>();
			bondDataListMock.Setup(m => m.Code).Returns(new ZString("1"));
			bondDataListMock.Setup(m => m.Description).Returns(new ZString("blah"));
			defaultValues.Setup(m => m.DefaultBondData).Returns(new IDISBondDataDefault[] { bondDataListMock.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var list = hostWrapper.Lookups.BondDataList;
			AssertEquals(1, list.Count);
			AssertEquals("1", list[0].Code);
			AssertEquals("blah", list[0].Description);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			bondDataListMock.VerifyAll();
		}

		public void TestShipmentList()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var defaultValues = new Mock<IUSDISDefaultValues>();
			var tradeTransaction1 = new Mock<IDISTradeTransaction>();
			tradeTransaction1.Setup(m => m.ShipmentNo).Returns(new ZString("001"));
			tradeTransaction1.Setup(m => m.XTN).Returns(new ZString("SV9-1234567"));
			var tradeTransaction2 = new Mock<IDISTradeTransaction>();
			tradeTransaction2.Setup(m => m.ShipmentNo).Returns(new ZString("002"));
			tradeTransaction2.Setup(m => m.XTN).Returns(new ZString("SV9-222222"));
			defaultValues.Setup(m => m.DefaultTradeTransactions).Returns(new IDISTradeTransaction[] { tradeTransaction1.Object, tradeTransaction2.Object });
			usDISHost.Setup(m => m.ValueProvider).Returns(defaultValues.Object);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var list = hostWrapper.Lookups.ShipmentList;
			AssertEquals(2, list.Count);
			AssertEquals("001", list[0].Code);
			AssertEquals("002", list[1].Code);
			usDISHost.VerifyAll();
			defaultValues.VerifyAll();
			tradeTransaction1.VerifyAll();
			tradeTransaction2.VerifyAll();
		}
	}
}
