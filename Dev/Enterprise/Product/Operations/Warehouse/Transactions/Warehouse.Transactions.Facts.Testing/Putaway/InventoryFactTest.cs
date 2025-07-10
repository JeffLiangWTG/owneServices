using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehousePutaway;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class InventoryFactReceiveTest : InventoryFactTest<IWhsReceive, IWhsReceiveLine>
	{
		public void TestNullObject_Throws_IWhsDocketLine()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => new InventoryFact(
				jobMock.Object,
				null,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrgSupplierPart>(),
				Mock.Of<IPutawayProductFact>(),
				Mock.Of<IEquipmentFact>()));
		}

		public void TestNullObject_Throws_Client()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => new InventoryFact(
				jobMock.Object,
				docketLineMock.Object,
				null,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrgSupplierPart>(),
				Mock.Of<IPutawayProductFact>(),
				Mock.Of<IEquipmentFact>()));
		}

		public void TestNullObject_Throws_Part()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => new InventoryFact(
				jobMock.Object,
				docketLineMock.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				null,
				Mock.Of<IPutawayProductFact>(),
				Mock.Of<IEquipmentFact>()));
		}

		public void TestNullObject_Throws_ProductFact()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => new InventoryFact(
				jobMock.Object,
				docketLineMock.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrgSupplierPart>(),
				null,
				Mock.Of<IEquipmentFact>()));
		}

		public void TestHasPutawayTransfer()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var inventoryFact1 = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid(), hasPutawayTransfer: false);
			AssertEquals(nameof(IInventoryFact.HasPutawayTransfer), false, inventoryFact1.HasPutawayTransfer);
			AssertEquals(nameof(IInventoryFact.HasPutawayTransfer), false, ((IInventoryFact)inventoryFact1).HasPutawayTransfer);

			var inventoryFact2 = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid(), hasPutawayTransfer: true);
			AssertEquals(nameof(IInventoryFact.HasPutawayTransfer), true, inventoryFact2.HasPutawayTransfer);
			AssertEquals(nameof(IInventoryFact.HasPutawayTransfer), true, ((IInventoryFact)inventoryFact2).HasPutawayTransfer);
		}

		public void TestVASServiceAreaName()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.VASServiceAreaName), string.Empty, inventoryFact.VASServiceAreaName);
			AssertEquals(nameof(IInventoryFact.VASServiceAreaName), string.Empty, ((IInventoryFact)inventoryFact).VASServiceAreaName);
		}

		public void TestVASServiceAreaTypeCode()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid());

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.VASServiceAreaTypeCode), string.Empty, inventoryFact.VASServiceAreaTypeCode);
			AssertEquals(nameof(IInventoryFact.VASServiceAreaTypeCode), string.Empty, ((IInventoryFact)inventoryFact).VASServiceAreaTypeCode);
		}

		public void TestConsignee()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.ConsigneePK).Returns(ZGuid.BrettsGuid.ToGuid());

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.Consignee), ZGuid.BrettsGuid, inventoryFact.Consignee.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Consignee), ZGuid.BrettsGuid, ((IInventoryFact)inventoryFact).Consignee.Fact.PK);
			docketLineMock.VerifyGet(e => e.ConsigneePK);
		}

		public void TestConsignee_Null()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.Consignee), null, inventoryFact.Consignee.Fact);
			AssertEquals(nameof(IInventoryFact.Consignee), null, ((IInventoryFact)inventoryFact).Consignee.Fact);
		}

		public void TestServiceLevel()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.SupplierPK).Returns(ZGuid.BrettsGuid.ToGuid());
			jobMock.SetupGet(e => e.WD_RS_NKServiceLevel).Returns("STD");

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ServiceLevel), "STD", inventoryFact.ServiceLevel);
			AssertEquals(nameof(IInventoryFact.ServiceLevel), "STD", ((IInventoryFact)inventoryFact).ServiceLevel);
			jobMock.VerifyGet(e => e.WD_RS_NKServiceLevel);
		}

		public void TestReceiveReference()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_ExternalReference).Returns("W00001584");

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ReceiveReference), "W00001584", inventoryFact.ReceiveReference);
			AssertEquals(nameof(IInventoryFact.ReceiveReference), "W00001584", ((IInventoryFact)inventoryFact).ReceiveReference);
			jobMock.VerifyGet(e => e.WD_ExternalReference);
		}

		public void TestCustomerReference()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_CustomerReference).Returns("CustomerRefTest");

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.CustomerReference), "CustomerRefTest", inventoryFact.CustomerReference);
			AssertEquals(nameof(IInventoryFact.CustomerReference), "CustomerRefTest", ((IInventoryFact)inventoryFact).CustomerReference);
			jobMock.VerifyGet(e => e.WD_CustomerReference);
		}

		public void TestSupplier()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.SupplierPK).Returns(ZGuid.BrettsGuid.ToGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.Supplier), ZGuid.BrettsGuid, inventoryFact.Supplier.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Supplier), ZGuid.BrettsGuid, ((IInventoryFact)inventoryFact).Supplier.Fact.PK);
			jobMock.VerifyGet(e => e.SupplierPK);
		}

		public void TestSupplier_Null()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.Supplier), null, inventoryFact.Supplier.Fact);
			AssertEquals(nameof(IInventoryFact.Supplier), null, ((IInventoryFact)inventoryFact).Supplier.Fact);
		}

		public void TestReceiveType()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.SupplierPK).Returns(ZGuid.BrettsGuid.ToGuid());
			jobMock.SetupGet(e => e.WD_DocketSubType).Returns("CUS");

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ReceiveType), "CUS", inventoryFact.ReceiveType);
			AssertEquals(nameof(IInventoryFact.ReceiveType), "CUS", ((IInventoryFact)inventoryFact).ReceiveType);
			jobMock.VerifyGet(e => e.WD_DocketSubType);
		}

		public void TestArrivalDate()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.SupplierPK).Returns(ZGuid.BrettsGuid.ToGuid());
			var now = ZDateTimeOffset.Today;
			jobMock.SetupGet(e => e.WD_ArrivalDate).Returns(now);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ArrivalDate), now.ToDateTime(), inventoryFact.ArrivalDate);
			AssertEquals(nameof(IInventoryFact.ArrivalDate), now.ToDateTime(), ((IInventoryFact)inventoryFact).ArrivalDate);
			jobMock.VerifyGet(e => e.WD_ArrivalDate);
		}

		public void TestArrivalDate_Null()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.SupplierPK).Returns(ZGuid.BrettsGuid.ToGuid());
			jobMock.SetupGet(e => e.WD_ArrivalDate).Returns(ZDateTimeOffset.Empty);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ArrivalDate), null, inventoryFact.ArrivalDate);
			AssertEquals(nameof(IInventoryFact.ArrivalDate), null, ((IInventoryFact)inventoryFact).ArrivalDate);
			jobMock.VerifyGet(e => e.WD_ArrivalDate);
		}

		public void TestRequiredDate()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.SupplierPK).Returns(ZGuid.BrettsGuid.ToGuid());

			var now = ZDateTimeOffset.Today;
			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_RequiredByDate).Returns(now);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.RequiredDate), now.ToZDateTime(), inventoryFact.RequiredDate);
			AssertEquals(nameof(IInventoryFact.RequiredDate), now.ToZDateTime(), ((IInventoryFact)inventoryFact).RequiredDate);
			docketLineMock.VerifyGet(e => e.WE_RequiredByDate);
		}

		public void TestRequiredDate_Null()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.SupplierPK).Returns(ZGuid.BrettsGuid.ToGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_RequiredByDate).Returns(ZDateTimeOffset.Empty);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.RequiredDate), null, inventoryFact.RequiredDate);
			AssertEquals(nameof(IInventoryFact.RequiredDate), null, ((IInventoryFact)inventoryFact).RequiredDate);
			docketLineMock.VerifyGet(e => e.WE_RequiredByDate);
		}

		public void TestReceiveCategory()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.SupplierPK).Returns(ZGuid.BrettsGuid.ToGuid());
			jobMock.SetupGet(e => e.WD_ReceiveCategory).Returns("CUS");

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ReceiveCategoryCode), "CUS", inventoryFact.ReceiveCategoryCode);
			AssertEquals(nameof(IInventoryFact.ReceiveCategoryCode), "CUS", ((IInventoryFact)inventoryFact).ReceiveCategoryCode);
			jobMock.VerifyGet(e => e.WD_ReceiveCategory);
		}

		protected override InventoryFact GetInventoryFact(
			IWhsReceive job,
			IWhsReceiveLine docketLine,
			IOrgSupplierPart part,
			ZGuid partRelationFK,
			IRefEquipment equipment = null,
			bool isTsaKnownClient = false,
			bool isTsaPolicyRequired = false)
		{
			return GetInventoryFact(job, docketLine, part, partRelationFK, equipment, isTsaKnownClient, isTsaPolicyRequired);
		}

		InventoryFact GetInventoryFact(
			IWhsReceive job,
			IWhsReceiveLine docketLine,
			IOrgSupplierPart part,
			ZGuid partRelationFK,
			IRefEquipment equipment = null,
			bool isTsaKnownClient = false,
			bool isTsaPolicyRequired = false,
			bool hasPutawayTransfer = false)
		{
			var clientFact = new Mock<IOrganisationFact>();
			clientFact.Setup(p => p.PK).Returns(job?.WD_OH_Client.ToGuid() ?? Guid.NewGuid());

			IOrganisationFact supplierFact = null;
			if (job != null && job.SupplierPK != ZGuid.Empty)
			{
				var supplierFactMock = new Mock<IOrganisationFact>();
				supplierFactMock.Setup(p => p.PK).Returns(job.SupplierPK.ToGuid());
				supplierFact = supplierFactMock.Object;
			}

			IOrganisationFact consigneeFact = null;
			if (docketLine.ConsigneePK != ZGuid.Empty)
			{
				var consigneeFactMock = new Mock<IOrganisationFact>();
				consigneeFactMock.Setup(p => p.PK).Returns(docketLine.ConsigneePK.ToGuid());
				consigneeFact = consigneeFactMock.Object;
			}

			var productFact = new Mock<IPutawayProductFact>();
			productFact.Setup(p => p.PK).Returns(partRelationFK.ToGuid());

			IEquipmentFact equipmentFact = null;
			if (equipment != null)
			{
				var equipmentFactMock = new Mock<IEquipmentFact>();
				equipmentFactMock.Setup(p => p.PK).Returns(equipment.PK.ToGuid());
				equipmentFact = equipmentFactMock.Object;
			}

			return new InventoryFact(
				job,
				docketLine,
				clientFact.Object,
				supplierFact,
				consigneeFact,
				part,
				productFact.Object,
				equipmentFact,
				isTsaKnownClient,
				isTsaPolicyRequired,
				hasPutawayTransfer);
		}

		protected override IWhsReceive GetMockJob(ZGuid pk, ZGuid clientPK, ZGuid warehousePK)
		{
			var mock = new Mock<IWhsReceive>();
			mock.SetupGet(r => r.PK).Returns(pk);
			mock.SetupGet(r => r.WD_OH_Client).Returns(clientPK);
			mock.SetupGet(r => r.WD_WW_Whs).Returns(warehousePK);
			return mock.Object;
		}
	}

	class InventoryFactVASOrdersTest : InventoryFactTest<IWhsVASOrder, IWhsDocketLine>
	{
		public void TestNullObject_Throws_IWhsDocketLine()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var lineToPutawayMock = new Mock<ILineToPutawayInfo>();
			lineToPutawayMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => GetInventoryFact(jobMock.Object, lineToPutawayMock.Object, null, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid()));
		}

		public void TestNullObject_Throws_ILineToPutawayInfo()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => GetInventoryFact(jobMock.Object, null, Mock.Of<IWhsDocketLine>(), Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid()));
		}

		public void TestQuantityToPutaway()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsDocketLine>();
			docketLineMock.SetupGet(l => l.WE_TransactionQuantity).Returns(5m);

			var lineToPutawayMock = new Mock<ILineToPutawayInfo>();
			lineToPutawayMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			lineToPutawayMock.SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			lineToPutawayMock.SetupGet(l => l.QuantityToPutaway).Returns(10m);

			var inventoryFact1 = GetInventoryFact(jobMock.Object, lineToPutawayMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.Quantity), 10m, inventoryFact1.Quantity);
			AssertEquals(nameof(IInventoryFact.Quantity), 10m, ((IInventoryFact)inventoryFact1).Quantity);
		}

		public void TestPackQuantity()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsDocketLine>();
			docketLineMock.SetupGet(l => l.WE_PackQuantity).Returns(5m);

			var lineToPutawayMock = new Mock<ILineToPutawayInfo>();
			lineToPutawayMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			lineToPutawayMock.SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			lineToPutawayMock.SetupGet(l => l.PackQuantity).Returns(10m);

			var inventoryFact1 = GetInventoryFact(jobMock.Object, lineToPutawayMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.PackUnits), 10m, inventoryFact1.PackUnits);
			AssertEquals(nameof(IInventoryFact.PackUnits), 10m, ((IInventoryFact)inventoryFact1).PackUnits);
		}

		public void TestHoldCode_UsesILineToPutaway()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsDocketLine>();
			docketLineMock.SetupGet(l => l.WE_WHC_NKCurrentInventoryHeldCode).Returns("HEL");
			docketLineMock.SetupGet(l => l.WE_WHC_NKOriginalInventoryHeldCode).Returns("HEL");

			var lineToPutawayMock = new Mock<ILineToPutawayInfo>();
			lineToPutawayMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			lineToPutawayMock.SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			lineToPutawayMock.SetupGet(l => l.InventoryHeldCode).Returns("");

			var inventoryFact1 = GetInventoryFact(jobMock.Object, lineToPutawayMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.HoldCode), "", inventoryFact1.HoldCode);
			AssertEquals(nameof(IInventoryFact.HoldCode), "", ((IInventoryFact)inventoryFact1).HoldCode);
		}

		public void TestHasPutawayTransfer()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsDocketLine>();
			var lineToPutawayMock = new Mock<ILineToPutawayInfo>();
			lineToPutawayMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			lineToPutawayMock.SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);

			var inventoryFact1 = GetInventoryFact(jobMock.Object, lineToPutawayMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.HasPutawayTransfer), false, inventoryFact1.HasPutawayTransfer);
			AssertEquals(nameof(IInventoryFact.HasPutawayTransfer), false, ((IInventoryFact)inventoryFact1).HasPutawayTransfer);
		}

		public void TestVASServiceAreaName()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsDocketLine>();
			var lineToPutawayMock = new Mock<ILineToPutawayInfo>();
			lineToPutawayMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			lineToPutawayMock.SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			lineToPutawayMock.SetupGet(e => e.PalletID).Returns("P1");

			var inventoryFact = GetInventoryFact(jobMock.Object, lineToPutawayMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid(), vasServiceAreaName: "TST");
			AssertEquals(nameof(IInventoryFact.VASServiceAreaName), "TST", inventoryFact.VASServiceAreaName);
			AssertEquals(nameof(IInventoryFact.VASServiceAreaName), "TST", ((IInventoryFact)inventoryFact).VASServiceAreaName);
		}

		public void TestVASServiceAreaTypeCode()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsDocketLine>();
			var lineToPutawayMock = new Mock<ILineToPutawayInfo>();
			lineToPutawayMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			lineToPutawayMock.SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			lineToPutawayMock.SetupGet(e => e.PalletID).Returns("P1");

			var inventoryFact = GetInventoryFact(jobMock.Object, lineToPutawayMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid(), vasServiceAreaTypeCode: "TST");
			AssertEquals(nameof(IInventoryFact.VASServiceAreaTypeCode), "TST", inventoryFact.VASServiceAreaTypeCode);
			AssertEquals(nameof(IInventoryFact.VASServiceAreaTypeCode), "TST", ((IInventoryFact)inventoryFact).VASServiceAreaTypeCode);
		}

		public void TestServiceLevel()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ServiceLevel), string.Empty, inventoryFact.ServiceLevel);
			AssertEquals(nameof(IInventoryFact.ServiceLevel), string.Empty, ((IInventoryFact)inventoryFact).ServiceLevel);
		}

		public void TestReceiveReference()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WVO_CustomerReferenceNo).Returns("V0001");

			var docketLineMock = new Mock<IWhsDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ReceiveReference), "V0001", inventoryFact.ReceiveReference);
			AssertEquals(nameof(IInventoryFact.ReceiveReference), "V0001", ((IInventoryFact)inventoryFact).ReceiveReference);
		}

		public void TestCustomerReference()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WVO_CustomerReferenceNo).Returns("V0001");

			var docketLineMock = new Mock<IWhsDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.CustomerReference), "V0001", inventoryFact.CustomerReference);
			AssertEquals(nameof(IInventoryFact.CustomerReference), "V0001", ((IInventoryFact)inventoryFact).CustomerReference);
		}

		public void TestConsignee_Null()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.Consignee), null, inventoryFact.Consignee.Fact);
			AssertEquals(nameof(IInventoryFact.Consignee), null, ((IInventoryFact)inventoryFact).Consignee.Fact);
		}

		public void TestSupplier_Null()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.Supplier), null, inventoryFact.Supplier.Fact);
			AssertEquals(nameof(IInventoryFact.Supplier), null, ((IInventoryFact)inventoryFact).Supplier.Fact);
		}

		public void TestReceiveType()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ReceiveType), "VAS", inventoryFact.ReceiveType);
			AssertEquals(nameof(IInventoryFact.ReceiveType), "VAS", ((IInventoryFact)inventoryFact).ReceiveType);
		}

		public void TestArrivalDate_Null()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ArrivalDate), null, inventoryFact.ArrivalDate);
			AssertEquals(nameof(IInventoryFact.ArrivalDate), null, ((IInventoryFact)inventoryFact).ArrivalDate);
		}

		public void TestRequiredDate_Null()
		{
			var jobMock = new Mock<IWhsVASOrder>();
			jobMock.SetupGet(e => e.WarehousePK).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);

			var inventoryFact = GetInventoryFact(jobMock.Object, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.RequiredDate), null, inventoryFact.RequiredDate);
			AssertEquals(nameof(IInventoryFact.RequiredDate), null, ((IInventoryFact)inventoryFact).RequiredDate);
		}

		protected override InventoryFact GetInventoryFact(
			IWhsVASOrder job,
			IWhsDocketLine docketLine,
			IOrgSupplierPart part,
			ZGuid partRelationFK,
			IRefEquipment equipment = null,
			bool isTsaKnownClient = false,
			bool isTsaPolicyRequired = false)
		{
			// Do it this way so we can inherit the tests, the test mocks the from base class implement both interface
			var lineToPutawayMock = (ILineToPutawayInfo)docketLine;

			var docketLineMock = new Mock<IWhsDocketLine>(); // Just expose what we want accessed via docketline
			docketLineMock.SetupGet(l => l.PackUOM).Returns(docketLine.PackUOM);
			docketLineMock.SetupGet(l => l.WE_PartAttrib1).Returns(docketLine.WE_PartAttrib1);
			docketLineMock.SetupGet(l => l.WE_PartAttrib2).Returns(docketLine.WE_PartAttrib2);
			docketLineMock.SetupGet(l => l.WE_PartAttrib3).Returns(docketLine.WE_PartAttrib3);
			docketLineMock.SetupGet(l => l.WE_PackingDate).Returns(docketLine.WE_PackingDate);
			docketLineMock.SetupGet(l => l.WE_ExpiryDate).Returns(docketLine.WE_ExpiryDate);

			return GetInventoryFact(job, lineToPutawayMock, docketLineMock.Object, part, partRelationFK, equipment, isTsaKnownClient, isTsaPolicyRequired);
		}

		InventoryFact GetInventoryFact(
			IWhsVASOrder vasOrder,
			ILineToPutawayInfo lineToPutawayInfo,
			IWhsDocketLine docketLine,
			IOrgSupplierPart part,
			ZGuid partRelationFK,
			IRefEquipment equipment = null,
			bool isTsaKnownClient = false,
			bool isTsaPolicyRequired = false,
			string vasServiceAreaName = "",
			string vasServiceAreaTypeCode = "")
		{
			var clientFact = new Mock<IOrganisationFact>();
			clientFact.Setup(p => p.PK).Returns(ZGuid.BrettsGuid.ToGuid());

			var productFact = new Mock<IPutawayProductFact>();
			productFact.Setup(p => p.PK).Returns(partRelationFK.ToGuid());

			IEquipmentFact equipmentFact = null;
			if (equipment != null)
			{
				var equipmentFactMock = new Mock<IEquipmentFact>();
				equipmentFactMock.Setup(p => p.PK).Returns(equipment.PK.ToGuid());
				equipmentFact = equipmentFactMock.Object;
			}

			return new InventoryFact(
				vasOrder,
				lineToPutawayInfo,
				docketLine,
				clientFact.Object,
				part,
				productFact.Object,
				equipmentFact,
				isTsaKnownClient,
				isTsaPolicyRequired,
				vasServiceAreaName,
				vasServiceAreaTypeCode);
		}

		protected override IWhsVASOrder GetMockJob(ZGuid pk, ZGuid clientPK, ZGuid warehousePK)
		{
			var mock = new Mock<IWhsVASOrder>();
			mock.SetupGet(r => r.PK).Returns(pk);
			mock.SetupGet(r => r.WarehousePK).Returns(warehousePK);
			return mock.Object;
		}
	}

	abstract class InventoryFactTest<TJob, TDocketLine> : TestCase
		where TDocketLine : class, IWhsDocketLine
	{
		public void TestNullObject_Throws_IWhsDocket()
		{
			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.BrettsGuid);

			AssertExceptionThrown<ArgumentNullException>(() => GetInventoryFact(default, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid()));
		}

		public void TestNullObject_Throws_IOrgSupplierPart()
		{
			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.BrettsGuid);

			AssertExceptionThrown<ArgumentNullException>(() => GetInventoryFact(GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid()), docketLineMock.Object, null, ZGuid.NewZGuid()));
		}

		public void TestPK()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.PK), ZGuid.BrettsGuid, inventoryFact.PK);
			AssertEquals(nameof(IInventoryFact.PK), ZGuid.BrettsGuid, ((IInventoryFact)inventoryFact).PK);
		}

		public void TestClient()
		{
			var jobMock = GetMockJob(ZGuid.BrettsGuid, ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.Client), ZGuid.BrettsGuid, inventoryFact.Client.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Client), ZGuid.BrettsGuid, ((IInventoryFact)inventoryFact).Client.Fact.PK);
		}

		public void TestProduct()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.BrettsGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var partRelationFK = ZGuid.NewZGuid();
			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), partRelationFK);
			AssertEquals(nameof(IInventoryFact.Product), partRelationFK, inventoryFact.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), partRelationFK, ((IInventoryFact)inventoryFact).Product.Fact.PK);

			AssertEquals(nameof(IInventoryFact.PartPK), ZGuid.BrettsGuid, inventoryFact.PartPK);
			AssertEquals(nameof(IInventoryFact.PartPK), ZGuid.BrettsGuid, ((IInventoryFact)inventoryFact).PartPK);

			docketLineMock.As<ILineToPutawayInfo>().VerifyGet(e => e.ProductPK);
		}

		public void TestEquipment_NullIRefEquipment()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.Equipment), null, inventoryFact.Equipment.Fact);
			AssertEquals(nameof(IInventoryFact.Equipment), null, ((IInventoryFact)inventoryFact).Equipment.Fact);
		}

		public void TestEquipment()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var equipmentMock = new Mock<IRefEquipment>();
			equipmentMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid(), equipment: equipmentMock.Object);
			AssertEquals(nameof(IInventoryFact.Equipment), ZGuid.BrettsGuid, inventoryFact.Equipment.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Equipment), ZGuid.BrettsGuid, ((IInventoryFact)inventoryFact).Equipment.Fact.PK);
			docketLineMock.As<ILineToPutawayInfo>().VerifyGet(e => e.PK);
		}

		public void TestPalletID()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("PalletID");

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.PalletID), "PalletID", inventoryFact.PalletID);
			AssertEquals(nameof(IInventoryFact.PalletID), "PalletID", ((IInventoryFact)inventoryFact).PalletID);
			docketLineMock.As<ILineToPutawayInfo>().VerifyGet(e => e.PalletID);
		}

		public void TestPalletID_Empty()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns(string.Empty);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.PalletID), string.Empty, inventoryFact.PalletID);
			AssertEquals(nameof(IInventoryFact.PalletID), string.Empty, ((IInventoryFact)inventoryFact).PalletID);
			docketLineMock.As<ILineToPutawayInfo>().VerifyGet(e => e.PalletID);
		}

		public void TestQuantity()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.QuantityToPutaway).Returns(45m);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.Quantity), 45m, inventoryFact.Quantity);
			AssertEquals(nameof(IInventoryFact.Quantity), 45m, ((IInventoryFact)inventoryFact).Quantity);
			docketLineMock.As<ILineToPutawayInfo>().VerifyGet(e => e.QuantityToPutaway);
		}

		public void TestPackUnits()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PackQuantity).Returns(15m);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid(), null);
			AssertEquals(nameof(IInventoryFact.PackUnits), 15m, inventoryFact.PackUnits);
			AssertEquals(nameof(IInventoryFact.PackUnits), 15m, ((IInventoryFact)inventoryFact).PackUnits);
			docketLineMock.As<ILineToPutawayInfo>().VerifyGet(e => e.PackQuantity);
		}

		public void TestPackUQ()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PackType).Returns("BAG");

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.PackUQ), "BAG", inventoryFact.PackUQ);
			AssertEquals(nameof(IInventoryFact.PackUQ), "BAG", ((IInventoryFact)inventoryFact).PackUQ);
			docketLineMock.As<ILineToPutawayInfo>().VerifyGet(e => e.PackType);
		}

		public void TestUOMType()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PackUOM).Returns("SPC");

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.UOMType), "SPC", inventoryFact.UOMType);
			AssertEquals(nameof(IInventoryFact.UOMType), "SPC", ((IInventoryFact)inventoryFact).UOMType);
			docketLineMock.VerifyGet(e => e.PackUOM);
		}

		public void TestHoldCode()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.InventoryHeldCode).Returns("DAM");

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.HoldCode), "DAM", inventoryFact.HoldCode);
			AssertEquals(nameof(IInventoryFact.HoldCode), "DAM", ((IInventoryFact)inventoryFact).HoldCode);
			docketLineMock.As<ILineToPutawayInfo>().VerifyGet(e => e.InventoryHeldCode);
		}

		public void TestHoldCode_Empty()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PalletID).Returns("P1");
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.InventoryHeldCode).Returns(string.Empty);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.HoldCode), string.Empty, inventoryFact.HoldCode);
			AssertEquals(nameof(IInventoryFact.HoldCode), string.Empty, ((IInventoryFact)inventoryFact).HoldCode);
			docketLineMock.As<ILineToPutawayInfo>().VerifyGet(e => e.InventoryHeldCode);
		}

		public void TestPartAttribute1()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_PartAttrib1).Returns("Red");

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.PartAttribute1), "Red", inventoryFact.PartAttribute1);
			AssertEquals(nameof(IInventoryFact.PartAttribute1), "Red", ((IInventoryFact)inventoryFact).PartAttribute1);
			docketLineMock.VerifyGet(e => e.WE_PartAttrib1);
		}

		public void TestPartAttribute2()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_PartAttrib2).Returns("Red");

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.PartAttribute2), "Red", inventoryFact.PartAttribute2);
			AssertEquals(nameof(IInventoryFact.PartAttribute2), "Red", ((IInventoryFact)inventoryFact).PartAttribute2);
			docketLineMock.VerifyGet(e => e.WE_PartAttrib2);
		}

		public void TestPartAttribute3()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_PartAttrib3).Returns("Red");

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.PartAttribute3), "Red", inventoryFact.PartAttribute3);
			AssertEquals(nameof(IInventoryFact.PartAttribute3), "Red", ((IInventoryFact)inventoryFact).PartAttribute3);
			docketLineMock.VerifyGet(e => e.WE_PartAttrib3);
		}

		public void TestExpiryDate()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var now = ZDate.Today;
			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_ExpiryDate).Returns(now);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ExpiryDate), now, inventoryFact.ExpiryDate);
			AssertEquals(nameof(IInventoryFact.ExpiryDate), now, ((IInventoryFact)inventoryFact).ExpiryDate);
			docketLineMock.VerifyGet(e => e.WE_ExpiryDate);
		}

		public void TestExpiryDate_Null()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_ExpiryDate).Returns(ZDate.Empty);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ExpiryDate), null, inventoryFact.ExpiryDate);
			AssertEquals(nameof(IInventoryFact.ExpiryDate), null, ((IInventoryFact)inventoryFact).ExpiryDate);
			docketLineMock.VerifyGet(e => e.WE_ExpiryDate);
		}

		public void TestPackingDate()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var now = ZDate.Today;
			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_PackingDate).Returns(now);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.PackingDate), now, inventoryFact.PackingDate);
			AssertEquals(nameof(IInventoryFact.PackingDate), now, ((IInventoryFact)inventoryFact).PackingDate);
			docketLineMock.VerifyGet(e => e.WE_PackingDate);
		}

		public void TestPackingDate_Null()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_PackingDate).Returns(ZDate.Empty);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.PackingDate), null, inventoryFact.PackingDate);
			AssertEquals(nameof(IInventoryFact.PackingDate), null, ((IInventoryFact)inventoryFact).PackingDate);
			docketLineMock.VerifyGet(e => e.WE_PackingDate);
		}

		public void TestTSAPolicyRequired()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var inventoryFact1 = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid(), isTsaPolicyRequired: false);
			AssertEquals(nameof(IInventoryFact.TSAPolicyRequired), false, inventoryFact1.TSAPolicyRequired);
			AssertEquals(nameof(IInventoryFact.TSAPolicyRequired), false, ((IInventoryFact)inventoryFact1).TSAPolicyRequired);

			var inventoryFact2 = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid(), isTsaPolicyRequired: true);
			AssertEquals(nameof(IInventoryFact.TSAPolicyRequired), true, inventoryFact2.TSAPolicyRequired);
			AssertEquals(nameof(IInventoryFact.TSAPolicyRequired), true, ((IInventoryFact)inventoryFact2).TSAPolicyRequired);
		}

		public void TestIsTSAKnownClient()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var inventoryFact1 = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid(), isTsaKnownClient: false);
			AssertEquals(nameof(IInventoryFact.IsTSAKnownClient), false, inventoryFact1.IsTSAKnownClient);
			AssertEquals(nameof(IInventoryFact.IsTSAKnownClient), false, ((IInventoryFact)inventoryFact1).IsTSAKnownClient);

			var inventoryFact2 = GetInventoryFact(jobMock, docketLineMock.Object, Mock.Of<IOrgSupplierPart>(), ZGuid.NewZGuid(), isTsaKnownClient: true);
			AssertEquals(nameof(IInventoryFact.IsTSAKnownClient), true, inventoryFact2.IsTSAKnownClient);
			AssertEquals(nameof(IInventoryFact.IsTSAKnownClient), true, ((IInventoryFact)inventoryFact2).IsTSAKnownClient);
		}

		public void TestProductWeight()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var orgSupplierPartMock = new Mock<IOrgSupplierPart>();
			orgSupplierPartMock.Setup(op => op.OP_Weight).Returns(3.14m);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, orgSupplierPartMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ProductWeight), 3.14m, inventoryFact.ProductWeight);
			AssertEquals(nameof(IInventoryFact.ProductWeight), 3.14m, ((IInventoryFact)inventoryFact).ProductWeight);
			orgSupplierPartMock.Verify(op => op.OP_Weight, Times.Once);
		}

		public void TestProductWeightUQ()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var orgSupplierPartMock = new Mock<IOrgSupplierPart>();
			orgSupplierPartMock.Setup(op => op.OP_WeightUQ).Returns("KG");

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, orgSupplierPartMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ProductWeightUQ), "KG", inventoryFact.ProductWeightUQ);
			AssertEquals(nameof(IInventoryFact.ProductWeightUQ), "KG", ((IInventoryFact)inventoryFact).ProductWeightUQ);
			orgSupplierPartMock.Verify(op => op.OP_WeightUQ, Times.Once);
		}

		public void TestProductVolume()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var orgSupplierPartMock = new Mock<IOrgSupplierPart>();
			orgSupplierPartMock.Setup(op => op.OP_Cubic).Returns(3.14m);

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, orgSupplierPartMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ProductVolume), 3.14m, inventoryFact.ProductVolume);
			AssertEquals(nameof(IInventoryFact.ProductVolume), 3.14m, ((IInventoryFact)inventoryFact).ProductVolume);
			orgSupplierPartMock.Verify(op => op.OP_Cubic, Times.Once);
		}

		public void TestProductVolumeUQ()
		{
			var jobMock = GetMockJob(ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<TDocketLine>();
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.ProductPK).Returns(ZGuid.NewZGuid);
			docketLineMock.As<ILineToPutawayInfo>().SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var orgSupplierPartMock = new Mock<IOrgSupplierPart>();
			orgSupplierPartMock.Setup(op => op.OP_CubicUQ).Returns("M3");

			var inventoryFact = GetInventoryFact(jobMock, docketLineMock.Object, orgSupplierPartMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(IInventoryFact.ProductVolumeUQ), "M3", inventoryFact.ProductVolumeUQ);
			AssertEquals(nameof(IInventoryFact.ProductVolumeUQ), "M3", ((IInventoryFact)inventoryFact).ProductVolumeUQ);
			orgSupplierPartMock.Verify(op => op.OP_CubicUQ, Times.Once);
		}

		TJob GetMockJob(ZGuid clientPK, ZGuid warehousePK) => GetMockJob(ZGuid.BrettsGuid, clientPK, warehousePK);

		protected abstract TJob GetMockJob(ZGuid pk, ZGuid clientPK, ZGuid warehousePK);

		protected abstract InventoryFact GetInventoryFact(
			TJob job,
			TDocketLine docketLine,
			IOrgSupplierPart part,
			ZGuid partRelationFK,
			IRefEquipment equipment = null,
			bool isTsaKnownClient = false,
			bool isTsaPolicyRequired = false);
	}
}
