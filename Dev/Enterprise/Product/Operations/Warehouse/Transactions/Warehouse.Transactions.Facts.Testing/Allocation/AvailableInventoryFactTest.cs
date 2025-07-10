using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration.Business;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class AvailableInventoryFactTest : TestCaseWithFactory
	{
		#region TestConstructor_NullArguments_Throws

		public void TestConstructor_NullArguments_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new AvailableInventoryFact(null, Mock.Of<IAllocationLocationFact>(), ZGuid.BrettsGuid, false, 1m, 1m, q => q, v => Enumerable.Empty<IAvailableInventoryFact>()));
			AssertExceptionThrown<ArgumentNullException>(() => new AvailableInventoryFact(Mock.Of<IWhsPickAvailableInventory>(), null, ZGuid.BrettsGuid, false, 1m, 1m, q => q, v => Enumerable.Empty<IAvailableInventoryFact>()));
			AssertExceptionThrown<ArgumentNullException>(() => new AvailableInventoryFact(Mock.Of<IWhsPickAvailableInventory>(), Mock.Of<IAllocationLocationFact>(), ZGuid.BrettsGuid, false, 1m, 1m, q => q, null));
			AssertExceptionThrown<ArgumentNullException>(() => new AvailableInventoryFact(Mock.Of<IWhsPickAvailableInventory>(), Mock.Of<IAllocationLocationFact>(), ZGuid.BrettsGuid, false, 1m, 1m, null, v => Enumerable.Empty<IAvailableInventoryFact>()));
		}

		#endregion

		#region TestProperties

		public void TestProperties_NullDates() => TestProperties(withOptionalDates: false, canAllocate: true);
		public void TestProperties_WithOptionalDates() => TestProperties(withOptionalDates: true, canAllocate: true);
		public void TestProperties_CannotAllocateDifferentlyToPickStrategy() => TestProperties(withOptionalDates: true, canAllocate: false);

		void TestProperties(bool withOptionalDates, bool canAllocate)
		{
			// Put unique values for all values that support enough values
			var pk = Guid.NewGuid();
			var orderedInvPk = Guid.NewGuid();

			var location = Mock.Of<IAllocationLocationFact>();

			var today = ZDateTime.Today;
			var packingDate = withOptionalDates ? today : ZDateTime.Empty;
			var expiryDate = withOptionalDates ? packingDate.AddDays(1) : ZDateTime.Empty;
			var bondedEntryDate = withOptionalDates ? packingDate.AddDays(2) : ZDateTime.Empty;
			var arrivalDate = today.ToOffset().AddDays(3);

			var availableInventoryMock = new Mock<IWhsPickAvailableInventory>();
			availableInventoryMock.Setup(inv => inv.PK).Returns(pk);
			availableInventoryMock.Setup(inv => inv.PalletID).Returns("PID");
			availableInventoryMock.Setup(inv => inv.PartAttrib1).Returns("PA1");
			availableInventoryMock.Setup(inv => inv.PartAttrib2).Returns("PA2");
			availableInventoryMock.Setup(inv => inv.PartAttrib3).Returns("PA3");
			availableInventoryMock.Setup(inv => inv.SerialNumber).Returns("SN");
			availableInventoryMock.Setup(inv => inv.BondedEntryKey).Returns("BEK");
			availableInventoryMock.Setup(inv => inv.ArrivalDate).Returns(arrivalDate);
			availableInventoryMock.Setup(inv => inv.PackingDate).Returns(packingDate.Date);
			availableInventoryMock.Setup(inv => inv.ExpiryDate).Returns(expiryDate.Date);
			availableInventoryMock.Setup(inv => inv.BondedEntryDate).Returns(bondedEntryDate);
			availableInventoryMock.Setup(inv => inv.VFDPerStockUnit).Returns(15.5m);

			var siblings = new[] { Mock.Of<IAvailableInventoryFact>(), Mock.Of<IAvailableInventoryFact>() };

			var lastQtyChecked = -1m;
			var lastQtyToReduce = -1m;
			var availableInventoryFact = new AvailableInventoryFact(
				availableInventoryMock.Object,
				location,
				orderedInvPk,
				canAllocateInQuantitiesDifferentToPickStrategy: canAllocate,
				quantity: 5m,
				palletSize: 1m,
				q =>
				{
					lastQtyChecked = q;
					return 1m;
				},
				v =>
				{
					lastQtyToReduce = v;
				});

			// Loop twice to make sure we store these values rather than wrapping the bizo values (bad for performance)
			for (var i = 0; i < 2; i++)
			{
				CombineAssertions(() =>
				{
					AssertEquals(nameof(AvailableInventoryFact.PK), pk, availableInventoryFact.PK);
					AssertEquals(nameof(AvailableInventoryFact.OrderedInventoryPK), orderedInvPk, availableInventoryFact.OrderedInventoryPK);
					AssertEquals(nameof(AvailableInventoryFact.Quantity), 5m, availableInventoryFact.Quantity);
					AssertEquals(nameof(AvailableInventoryFact.Location), location, availableInventoryFact.Location.Fact);
					AssertEquals(nameof(AvailableInventoryFact.PartAttribute1), "PID", availableInventoryFact.PalletID);
					AssertEquals(nameof(AvailableInventoryFact.PartAttribute1), "PA1", availableInventoryFact.PartAttribute1);
					AssertEquals(nameof(AvailableInventoryFact.PartAttribute2), "PA2", availableInventoryFact.PartAttribute2);
					AssertEquals(nameof(AvailableInventoryFact.PartAttribute3), "PA3", availableInventoryFact.PartAttribute3);
					AssertEquals(nameof(AvailableInventoryFact.SerialNumber), "SN", availableInventoryFact.SerialNumber);
					AssertEquals(nameof(AvailableInventoryFact.BondedEntryKey), "BEK", availableInventoryFact.BondedEntryKey);
					AssertEquals(nameof(AvailableInventoryFact.ArrivalDate), arrivalDate.ToZDateTime(), availableInventoryFact.ArrivalDate);
					AssertEquals(nameof(AvailableInventoryFact.VFDPerStockUnit), 15.5m, availableInventoryFact.VFDPerStockUnit);

					if (withOptionalDates)
					{
						AssertEquals(nameof(AvailableInventoryFact.PackingDate), packingDate, availableInventoryFact.PackingDate);
						AssertEquals(nameof(AvailableInventoryFact.ExpiryDate), expiryDate, availableInventoryFact.ExpiryDate);
						AssertEquals(nameof(AvailableInventoryFact.BondedEntryDate), bondedEntryDate, availableInventoryFact.BondedEntryDate);
					}
					else
					{
						AssertNull(nameof(AvailableInventoryFact.PackingDate), availableInventoryFact.PackingDate);
						AssertNull(nameof(AvailableInventoryFact.ExpiryDate), availableInventoryFact.ExpiryDate);
						AssertNull(nameof(AvailableInventoryFact.BondedEntryDate), availableInventoryFact.BondedEntryDate);
					}

					AssertEquals(nameof(AvailableInventoryFact.IsPalletOverflow), false, availableInventoryFact.IsPalletOverflow);
				});
			}

			AssertEquals("Precondition: UpdateRelatedRecords not yet called.", -1m, lastQtyToReduce);

			availableInventoryFact.DecreaseQuantity(1m);
			AssertEquals("UpdateRelatedRecords called.", 1m, lastQtyToReduce);

			availableInventoryFact.DecreaseQuantity(2m);
			AssertEquals("UpdateRelatedRecords called.", 2m, lastQtyToReduce);

			AssertEquals("Precondition: GetQuantityThatCanBeAllocated not yet called.", -1m, lastQtyChecked);

			AssertEquals(1m, availableInventoryFact.GetQuantityThatCanBeAllocatedViaPickStrategy(2m));
			AssertEquals("GetQuantityThatCanBeAllocated called.", 2m, lastQtyToReduce);

			AssertEquals(1m, availableInventoryFact.GetQuantityThatCanBeAllocatedViaPickStrategy(3m));
			AssertEquals("GetQuantityThatCanBeAllocated called.", 3m, lastQtyChecked);

			availableInventoryMock.Verify(inv => inv.PalletID, Times.Once);
			availableInventoryMock.Verify(inv => inv.PartAttrib1, Times.Once);
			availableInventoryMock.Verify(inv => inv.PartAttrib2, Times.Once);
			availableInventoryMock.Verify(inv => inv.PartAttrib3, Times.Once);
			availableInventoryMock.Verify(inv => inv.SerialNumber, Times.Once);
			availableInventoryMock.Verify(inv => inv.BondedEntryKey, Times.Once);
			availableInventoryMock.Verify(inv => inv.ArrivalDate, Times.Once);
			availableInventoryMock.Verify(inv => inv.PackingDate, Times.Once);
			availableInventoryMock.Verify(inv => inv.ExpiryDate, Times.Once);
			availableInventoryMock.Verify(inv => inv.BondedEntryDate, Times.Once);
			availableInventoryMock.Verify(inv => inv.VFDPerStockUnit, Times.Once);
		}

		#endregion

		#region TestIsPalletOverflow

		public void TestIsPalletOverflow()
		{
			var location = Mock.Of<IAllocationLocationFact>();

			var product = new Mock<IOrgSupplierPart>();

			var availableInventoryMock = new Mock<IWhsPickAvailableInventory>();
			availableInventoryMock.Setup(inv => inv.PK).Returns(Guid.NewGuid());
			availableInventoryMock.Setup(inv => inv.ArrivalDate).Returns(ZDateTimeOffset.Today);
			availableInventoryMock.Setup(inv => inv.SupplierPart).Returns(product.Object);

			var availableInventoryFact = new AvailableInventoryFact(availableInventoryMock.Object, location, Guid.NewGuid(), true, 11m, 5m, q => q, v => Enumerable.Empty<IAvailableInventoryFact>());
			AssertEquals(nameof(IAvailableInventoryFact.IsPalletOverflow), true, availableInventoryFact.IsPalletOverflow);

			availableInventoryFact.DecreaseQuantity(1m); // 10m = 2 * 5
			AssertEquals(nameof(IAvailableInventoryFact.IsPalletOverflow), false, availableInventoryFact.IsPalletOverflow);

			availableInventoryFact.DecreaseQuantity(1m); // 9m = 5 + 4
			AssertEquals(nameof(IAvailableInventoryFact.IsPalletOverflow), true, availableInventoryFact.IsPalletOverflow);

			availableInventoryFact.DecreaseQuantity(3m); // 6m = 5 + 1
			AssertEquals(nameof(IAvailableInventoryFact.IsPalletOverflow), true, availableInventoryFact.IsPalletOverflow);

			availableInventoryFact.DecreaseQuantity(1m); // 5m
			AssertEquals(nameof(IAvailableInventoryFact.IsPalletOverflow), false, availableInventoryFact.IsPalletOverflow);

			availableInventoryFact.DecreaseQuantity(1m); // 4m
			AssertEquals(nameof(IAvailableInventoryFact.IsPalletOverflow), false, availableInventoryFact.IsPalletOverflow);
		}

		#endregion
	}
}
