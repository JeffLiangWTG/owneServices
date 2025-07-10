using System;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class TaskManagementTransferLineFactTest : TestCase
	{
		public void TestNullObject_Throws_Grouping()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementTransferLineFact(
				null,
				Mock.Of<IWhsTransfer>(),
				Mock.Of<IWhsTransferLine>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				null,
				true));
		}

		public void TestNullObject_Throws_IWhsDocket()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementTransferLineFact(
				Mock.Of<ITaskManagementGroupingFact>(),
				null,
				Mock.Of<IWhsTransferLine>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				null,
				true));
		}

		public void TestNullObject_Throws_IWhsDocketLine()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementTransferLineFact(
				Mock.Of<ITaskManagementGroupingFact>(),
				Mock.Of<IWhsTransfer>(),
				null,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				null,
				true));
		}

		public void TestNullObject_Throws_Client()
		{
			var docketLineMock = new Mock<IWhsTransferLine>();
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementTransferLineFact(
				Mock.Of<ITaskManagementGroupingFact>(),
				Mock.Of<IWhsTransfer>(),
				docketLineMock.Object,
				null,
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				null,
				true));
		}

		public void TestNullObject_Throws_ProductFact()
		{
			var docketLineMock = new Mock<IWhsTransferLine>();
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementTransferLineFact(
				Mock.Of<ITaskManagementGroupingFact>(),
				Mock.Of<IWhsTransfer>(),
				docketLineMock.Object,
				Mock.Of<IOrganisationFact>(),
				null,
				Mock.Of<ITaskManagementLocationFact>(),
				null,
				true));
		}

		public void TestNullObject_Throws_TransferFromLocation()
		{
			var docketLineMock = new Mock<IWhsTransferLine>();
			docketLineMock.SetupGet(e => e.PK).Returns(ZGuid.NewZGuid);

			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementTransferLineFact(
				Mock.Of<ITaskManagementGroupingFact>(),
				Mock.Of<IWhsTransfer>(),
				docketLineMock.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				null,
				null,
				true));
		}

		public void TestConstructor()
		{
			var docketId = "W01";
			var externalRef = "EXTREF";
			var subType = "RPL";
			var jobMock = new Mock<IWhsTransfer>();
			jobMock.SetupGet(e => e.WD_DocketID).Returns(docketId);
			jobMock.SetupGet(e => e.WD_ExternalReference).Returns(externalRef);
			jobMock.SetupGet(e => e.DocketSubType).Returns(subType);

			var pk = ZGuid.NewZGuid();
			var holdCode = "DAM";
			var packUQ = "BOX";
			var uomType = "CAS";
			var fromPalletID = "PLT123";
			var toPalletID = "PLT456";
			var today = ZDate.Today;
			var expiry = today.AddDays(7);
			var packing = today.AddDays(-7);
			var arrival = today.ToZDateTime().ToOffset();
			var docketLineMock = new Mock<IWhsTransferLine>();
			docketLineMock.SetupGet(e => e.PK).Returns(pk);
			docketLineMock.SetupGet(e => e.WE_WHC_NKOriginalInventoryHeldCode).Returns(holdCode);
			docketLineMock.SetupGet(e => e.WE_F3_NKPackType).Returns(packUQ);
			docketLineMock.SetupGet(e => e.PackUOM).Returns(uomType);
			docketLineMock.SetupGet(e => e.WE_TransferFromPalletId).Returns(fromPalletID);
			docketLineMock.SetupGet(e => e.WE_PalletID).Returns(toPalletID);
			docketLineMock.SetupGet(e => e.WE_PartAttrib1).Returns("1");
			docketLineMock.SetupGet(e => e.WE_PartAttrib2).Returns("2");
			docketLineMock.SetupGet(e => e.WE_PartAttrib3).Returns("3");
			docketLineMock.SetupGet(e => e.WE_ExpiryDate).Returns(expiry);
			docketLineMock.SetupGet(e => e.WE_PackingDate).Returns(packing);
			docketLineMock.SetupGet(e => e.WE_AdjustmentArrivalDate).Returns(arrival);

			var grouping = Mock.Of<ITaskManagementGroupingFact>();
			var client = Mock.Of<IOrganisationFact>();
			var product = Mock.Of<IProductFact>();
			var fromLocation = Mock.Of<ITaskManagementLocationFact>();
			var toLocation = Mock.Of<ITaskManagementLocationFact>();

			var fact = new TaskManagementTransferLineFact(
				grouping,
				jobMock.Object,
				docketLineMock.Object,
				client,
				product,
				fromLocation,
				toLocation,
				false);

			AssertEquals(nameof(fact.PK), pk, fact.PK);
			AssertEquals(nameof(fact.Grouping.Fact), grouping, fact.Grouping.Fact);
			AssertEquals(nameof(fact.Client.Fact), client, fact.Client.Fact);
			AssertEquals(nameof(fact.Product.Fact), product, fact.Product.Fact);
			AssertEquals(nameof(fact.FromLocation.Fact), fromLocation, fact.FromLocation.Fact);
			AssertEquals(nameof(fact.ToLocation.Fact), toLocation, fact.ToLocation.Fact);
			AssertEquals(nameof(fact.DocketID), docketId, fact.DocketID);
			AssertEquals(nameof(fact.Reference), externalRef, fact.Reference);
			AssertEquals(nameof(fact.TransferType), subType, fact.TransferType);
			AssertEquals(nameof(fact.HoldCode), holdCode, fact.HoldCode);
			AssertEquals(nameof(fact.PackUQ), packUQ, fact.PackUQ);
			AssertEquals(nameof(fact.UOMType), uomType, fact.UOMType);
			AssertEquals(nameof(fact.FromPalletID), fromPalletID, fact.FromPalletID);
			AssertEquals(nameof(fact.ToPalletID), toPalletID, fact.ToPalletID);
			AssertEquals(nameof(fact.PartAttribute1), "1", fact.PartAttribute1);
			AssertEquals(nameof(fact.PartAttribute2), "2", fact.PartAttribute2);
			AssertEquals(nameof(fact.PartAttribute3), "3", fact.PartAttribute3);
			AssertEquals(nameof(fact.ExpiryDate), expiry,  fact.ExpiryDate);
			AssertEquals(nameof(fact.PackingDate), packing, fact.PackingDate);
			AssertEquals(nameof(fact.ArrivalDate), arrival.ToDateTime(), fact.ArrivalDate);
		}

		public void TestConstructor_HasAwaitingPicks_False() => TestConstructor_HasAwaitingPicks(hasAwaitingPicks: false);
		public void TestConstructor_HasAwaitingPicks_True() => TestConstructor_HasAwaitingPicks(hasAwaitingPicks: true);

		void TestConstructor_HasAwaitingPicks(bool hasAwaitingPicks)
		{
			var pk = ZGuid.NewZGuid();
			var docketLineMock = new Mock<IWhsTransferLine>();
			docketLineMock.SetupGet(e => e.PK).Returns(pk);

			var fact = new TaskManagementTransferLineFact(
				Mock.Of<ITaskManagementGroupingFact>(),
				Mock.Of<IWhsTransfer>(),
				docketLineMock.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				hasAwaitingPicks);

			AssertEquals(nameof(fact.PK), pk, fact.PK);
			AssertEquals(nameof(fact.HasAwaitingPicks), hasAwaitingPicks, fact.HasAwaitingPicks);
		}

		public void TestConstructor_NullDates()
		{
			var pk = ZGuid.NewZGuid();
			var docketLineMock = new Mock<IWhsTransferLine>();
			docketLineMock.SetupGet(e => e.PK).Returns(pk);
			docketLineMock.SetupGet(e => e.WE_ExpiryDate).Returns(ZDate.Empty);
			docketLineMock.SetupGet(e => e.WE_PackingDate).Returns(ZDate.Empty);
			docketLineMock.SetupGet(e => e.WE_AdjustmentArrivalDate).Returns(ZDateTimeOffset.Empty);

			var fact = new TaskManagementTransferLineFact(
				Mock.Of<ITaskManagementGroupingFact>(),
				Mock.Of<IWhsTransfer>(),
				docketLineMock.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				false);

			AssertEquals(nameof(fact.PK), pk, fact.PK);
			AssertNull(nameof(fact.ExpiryDate), fact.ExpiryDate);
			AssertNull(nameof(fact.PackingDate), fact.PackingDate);
			AssertNull(nameof(fact.ArrivalDate), fact.ArrivalDate);
		}

		public void TestConstructor_NullToLocation()
		{
			var pk = ZGuid.NewZGuid();
			var docketLineMock = new Mock<IWhsTransferLine>();
			docketLineMock.SetupGet(e => e.PK).Returns(pk);

			var location = Mock.Of<ITaskManagementLocationFact>();
			var fact = new TaskManagementTransferLineFact(
				Mock.Of<ITaskManagementGroupingFact>(),
				Mock.Of<IWhsTransfer>(),
				docketLineMock.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				location,
				null,
				false);

			AssertEquals(nameof(fact.PK), pk, fact.PK);
			AssertEquals(nameof(fact.FromLocation.Fact), location, fact.FromLocation.Fact);
			AssertNull(nameof(fact.ToLocation.Fact), fact.ToLocation.Fact);
		}
	}
}
