using System;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class TaskManagementUnloadLineFactTest : TestCase
	{
		public void TestNullObject_Throws_Grouping()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementUnloadLineFact(
				null,
				jobMock.Object,
				Mock.Of<IWhsReceiveLine>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>()));
		}

		public void TestNullObject_Throws_IWhsDocket()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementUnloadLineFact(
				Mock.Of<ITaskManagementGroupingFact>(),
				null,
				Mock.Of<IWhsReceiveLine>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>()));
		}

		public void TestNullObject_Throws_IWhsDocketLine()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementUnloadLineFact(
				Mock.Of<ITaskManagementGroupingFact>(),
				jobMock.Object,
				null,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>()));
		}

		public void TestNullObject_Throws_Client()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementUnloadLineFact(
				Mock.Of<ITaskManagementGroupingFact>(),
				jobMock.Object,
				docketLineMock.Object,
				null,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>()));
		}

		public void TestNullObject_Throws_ProductFact()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementUnloadLineFact(
				Mock.Of<ITaskManagementGroupingFact>(),
				jobMock.Object,
				docketLineMock.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IOrganisationFact>(),
				null));
		}

		public void TestPK()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.PK), ZGuid.BrettsGuid, unloadLineFact.PK);
		}

		public void TestGrouping()
		{
			var groupingMock = Mock.Of<ITaskManagementGroupingFact>();
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid(), groupingMock);
			AssertEquals(nameof(ITaskManagementUnloadLineFact.Grouping), groupingMock, unloadLineFact.Grouping.Fact);
		}

		public void TestClient()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.BrettsGuid, ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.Client), ZGuid.BrettsGuid, unloadLineFact.Client.Fact.PK);
		}

		public void TestProduct()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.BrettsGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			var partRelationFK = ZGuid.NewZGuid();
			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, partRelationFK);
			AssertEquals(nameof(ITaskManagementUnloadLineFact.Product), partRelationFK, unloadLineFact.Product.Fact.PK);
		}

		public void TestPalletID()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("PalletID");

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.PalletID), "PalletID", unloadLineFact.PalletID);
			docketLineMock.VerifyGet(e => e.WE_PalletID);
		}

		public void TestPalletID_Empty()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns(string.Empty);

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.PalletID), string.Empty, unloadLineFact.PalletID);
			docketLineMock.VerifyGet(e => e.WE_PalletID);
		}

		public void TestPackUQ()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_F3_NKPackType).Returns("BAG");

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.PackUQ), "BAG", unloadLineFact.PackUQ);
			docketLineMock.VerifyGet(e => e.WE_F3_NKPackType);
		}

		public void TestUOMType()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PackUOM).Returns("SPC");

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.UOMType), "SPC", unloadLineFact.UOMType);
			docketLineMock.VerifyGet(e => e.PackUOM);
		}

		public void TestHoldCode()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_WHC_NKCurrentInventoryHeldCode).Returns("DAM");

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.HoldCode), "DAM", unloadLineFact.HoldCode);
			docketLineMock.VerifyGet(e => e.WE_WHC_NKCurrentInventoryHeldCode);
		}

		public void TestHoldCode_Empty()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_WHC_NKCurrentInventoryHeldCode).Returns(string.Empty);

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.HoldCode), string.Empty, unloadLineFact.HoldCode);
			docketLineMock.VerifyGet(e => e.WE_WHC_NKCurrentInventoryHeldCode);
		}

		public void TestPartAttribute1()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_PartAttrib1).Returns("Red");

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.PartAttribute1), "Red", unloadLineFact.PartAttribute1);
			docketLineMock.VerifyGet(e => e.WE_PartAttrib1);
		}

		public void TestPartAttribute2()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_PartAttrib2).Returns("Red");

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.PartAttribute2), "Red", unloadLineFact.PartAttribute2);
			docketLineMock.VerifyGet(e => e.WE_PartAttrib2);
		}

		public void TestPartAttribute3()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_PartAttrib3).Returns("Red");

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.PartAttribute3), "Red", unloadLineFact.PartAttribute3);
			docketLineMock.VerifyGet(e => e.WE_PartAttrib3);
		}

		public void TestExpiryDate()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var now = ZDate.Today;
			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_ExpiryDate).Returns(now);

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.ExpiryDate), now, unloadLineFact.ExpiryDate);
			docketLineMock.VerifyGet(e => e.WE_ExpiryDate);
		}

		public void TestExpiryDate_Null()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_ExpiryDate).Returns(ZDate.Empty);

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.ExpiryDate), null, unloadLineFact.ExpiryDate);
			docketLineMock.VerifyGet(e => e.WE_ExpiryDate);
		}

		public void TestPackingDate()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var now = ZDate.Today;
			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_PackingDate).Returns(now);

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.PackingDate), now, unloadLineFact.PackingDate);
			docketLineMock.VerifyGet(e => e.WE_PackingDate);
		}

		public void TestPackingDate_Null()
		{
			var jobMock = GetReceiveMock(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.WE_PackingDate).Returns(ZDate.Empty);

			var unloadLineFact = GetUnloadLineFact(jobMock, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.PackingDate), null, unloadLineFact.PackingDate);
			docketLineMock.VerifyGet(e => e.WE_PackingDate);
		}

		public void TestConsignee()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.ConsigneePK).Returns(ZGuid.BrettsGuid.ToGuid());

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.Consignee), ZGuid.BrettsGuid, unloadLineFact.Consignee.Fact.PK);
			docketLineMock.VerifyGet(e => e.ConsigneePK);
		}

		public void TestConsignee_Null()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.Consignee), null, unloadLineFact.Consignee.Fact);
		}

		public void TestServiceLevel()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.SupplierPK).Returns(ZGuid.BrettsGuid.ToGuid());
			jobMock.SetupGet(e => e.WD_RS_NKServiceLevel).Returns("STD");

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.ServiceLevel), "STD", unloadLineFact.ServiceLevel);
			jobMock.VerifyGet(e => e.WD_RS_NKServiceLevel);
		}

		public void TestReceiveReference()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_ExternalReference).Returns("W00001584");

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.ReceiveReference), "W00001584", unloadLineFact.ReceiveReference);
			jobMock.VerifyGet(e => e.WD_ExternalReference);
		}

		public void TestCustomerReference()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_CustomerReference).Returns("CustomerRefTest");

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.CustomerReference), "CustomerRefTest", unloadLineFact.CustomerReference);
			jobMock.VerifyGet(e => e.WD_CustomerReference);
		}

		public void TestSupplier()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.SupplierPK).Returns(ZGuid.BrettsGuid.ToGuid());

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.Supplier), ZGuid.BrettsGuid, unloadLineFact.Supplier.Fact.PK);
			jobMock.VerifyGet(e => e.SupplierPK);
		}

		public void TestSupplier_Null()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.Supplier), null, unloadLineFact.Supplier.Fact);
		}

		public void TestReceiveType()
		{
			var jobMock = new Mock<IWhsReceive>();
			jobMock.SetupGet(e => e.WD_OH_Client).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.WD_WW_Whs).Returns(ZGuid.NewZGuid);
			jobMock.SetupGet(e => e.SupplierPK).Returns(ZGuid.BrettsGuid.ToGuid());
			jobMock.SetupGet(e => e.WD_DocketSubType).Returns("CUS");

			var docketLineMock = new Mock<IWhsReceiveLine>();
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.ReceiveType), "CUS", unloadLineFact.ReceiveType);
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
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.ArrivalDate), now.ToDateTime(), unloadLineFact.ArrivalDate);
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
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.ArrivalDate), null, unloadLineFact.ArrivalDate);
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
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.RequiredDate), now.ToZDateTime(), unloadLineFact.RequiredDate);
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
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.RequiredDate), null, unloadLineFact.RequiredDate);
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
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns("P1");
			docketLineMock.SetupGet(e => e.WE_OP).Returns(ZGuid.NewZGuid);
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.BrettsGuid);

			var unloadLineFact = GetUnloadLineFact(jobMock.Object, docketLineMock.Object, ZGuid.NewZGuid());
			AssertEquals(nameof(ITaskManagementUnloadLineFact.ReceiveCategoryCode), "CUS", unloadLineFact.ReceiveCategoryCode);
			jobMock.VerifyGet(e => e.WD_ReceiveCategory);
		}

		TaskManagementUnloadLineFact GetUnloadLineFact(
			IWhsReceive job,
			IWhsReceiveLine docketLine,
			ZGuid partRelationFK,
			ITaskManagementGroupingFact grouping = null)
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

			var productFact = new Mock<IProductFact>();
			productFact.Setup(p => p.PK).Returns(partRelationFK.ToGuid());

			return new TaskManagementUnloadLineFact(
				grouping ?? Mock.Of<ITaskManagementGroupingFact>(),
				job,
				docketLine,
				clientFact.Object,
				supplierFact,
				consigneeFact,
				productFact.Object);
		}

		IWhsReceive GetReceiveMock(ZGuid pk, ZGuid clientPK, ZGuid warehousePK)
		{
			var mock = new Mock<IWhsReceive>();
			mock.SetupGet(r => r.PK).Returns(pk);
			mock.SetupGet(r => r.WD_OH_Client).Returns(clientPK);
			mock.SetupGet(r => r.WD_WW_Whs).Returns(warehousePK);
			return mock.Object;
		}
	}
}
