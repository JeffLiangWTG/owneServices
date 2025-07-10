using System;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.Business;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class TaskManagementPickLineFactTest : TestCase
	{
		public void TestConstructor_Throws_Pick()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementPickLineFact(
				Guid.NewGuid(),
				Guid.NewGuid(),
				null,
				Mock.Of<IWhsPickAvailableInventory>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				"BOX",
				"CAS",
				0m,
				0,
				0,
				0,
				"KG",
				0,
				"M3"));
		}

		public void TestConstructor_Throws_AvailableInventory()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementPickLineFact(
				Guid.NewGuid(),
				Guid.NewGuid(),
				Mock.Of<IWhsPick>(),
				null,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				"BOX",
				"CAS",
				0m,
				0,
				0,
				0,
				"KG",
				0,
				"M3"));
		}

		public void TestConstructor_Throws_Client()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementPickLineFact(
				Guid.NewGuid(),
				Guid.NewGuid(),
				Mock.Of<IWhsPick>(),
				Mock.Of<IWhsPickAvailableInventory>(),
				null,
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				"BOX",
				"CAS",
				0m,
				0,
				0,
				0,
				"KG",
				0,
				"M3"));
		}

		public void TestConstructor_Throws_Product()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementPickLineFact(
				Guid.NewGuid(),
				Guid.NewGuid(),
				Mock.Of<IWhsPick>(),
				Mock.Of<IWhsPickAvailableInventory>(),
				Mock.Of<IOrganisationFact>(),
				null,
				Mock.Of<ITaskManagementLocationFact>(),
				"BOX",
				"CAS",
				0m,
				0,
				0,
				0,
				"KG",
				0,
				"M3"));
		}

		public void TestConstructor_Throws_Location()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementPickLineFact(
				Guid.NewGuid(),
				Guid.NewGuid(),
				Mock.Of<IWhsPick>(),
				Mock.Of<IWhsPickAvailableInventory>(),
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				null,
				"BOX",
				"CAS",
				0m,
				0,
				0,
				0,
				"KG",
				0,
				"M3"));
		}

		public void TestConstructor_Throws_PackUQ()
		{
			var pick = new Mock<IWhsPick>();
			var availableInventory = new Mock<IWhsPickAvailableInventory>();
			availableInventory.SetupGet(a => a.ArrivalDate).Returns(ZDateTimeOffset.Today);

			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementPickLineFact(
				Guid.NewGuid(),
				Guid.NewGuid(),
				pick.Object,
				availableInventory.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				null,
				"CAS",
				0m,
				0,
				0,
				0,
				"KG",
				0,
				"M3"));
		}

		public void TestConstructor_Throws_UOMType()
		{
			var pick = new Mock<IWhsPick>();
			var availableInventory = new Mock<IWhsPickAvailableInventory>();
			availableInventory.SetupGet(a => a.ArrivalDate).Returns(ZDateTimeOffset.Today);

			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementPickLineFact(
				Guid.NewGuid(),
				Guid.NewGuid(),
				pick.Object,
				availableInventory.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				"BOX",
				null,
				0m,
				0,
				0,
				0,
				"KG",
				0,
				"M3"));
		}

		public void TestConstructor_Throws_NullWeightUQ()
		{
			var pick = new Mock<IWhsPick>();
			var availableInventory = new Mock<IWhsPickAvailableInventory>();
			availableInventory.SetupGet(a => a.ArrivalDate).Returns(ZDateTimeOffset.Today);

			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementPickLineFact(
				Guid.NewGuid(),
				Guid.NewGuid(),
				pick.Object,
				availableInventory.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				"BOX",
				"CAS",
				0m,
				0,
				0,
				0,
				null,
				0,
				"M3"));
		}

		public void TestConstructor_Throws_NullVolumeUQ()
		{
			var pick = new Mock<IWhsPick>();
			var availableInventory = new Mock<IWhsPickAvailableInventory>();
			availableInventory.SetupGet(a => a.ArrivalDate).Returns(ZDateTimeOffset.Today);

			AssertExceptionThrown<ArgumentNullException>(() => new TaskManagementPickLineFact(
				Guid.NewGuid(),
				Guid.NewGuid(),
				pick.Object,
				availableInventory.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				"BOX",
				"CAS",
				0m,
				0,
				0,
				0,
				"KG",
				0,
				null));
		}

		public void TestConstructor() => TestConstructor(isCustomsTransaction: false, isWorkOrderPick: false);
		public void TestConstructor_IsCustomsTransaction() => TestConstructor(isCustomsTransaction: true, isWorkOrderPick: false);
		public void TestConstructor_IsWorkOrderPick() => TestConstructor(isCustomsTransaction: false, isWorkOrderPick: true);

		void TestConstructor(bool isCustomsTransaction, bool isWorkOrderPick)
		{
			var pk = Guid.NewGuid();

			var pick = new Mock<IWhsPick>();
			pick.SetupGet(p => p.IsWorkOrderPick).Returns(isWorkOrderPick);
			pick.SetupGet(p => p.IsCustomsTransaction).Returns(isCustomsTransaction);

			var palletId = "PALLET789";
			var partAttrib1 = "ATTR1";
			var partAttrib2 = "ATTR2";
			var partAttrib3 = "ATTR3";
			var serialNumber = "SN123456";
			var arrivalDate = new ZDateTimeOffset(2025, 5, 15);
			var expiryDate = new ZDate(2026, 6, 14);
			var packingDate = new ZDate(2025, 5, 10);
			var bondedEntryKey = "BEK123";
			var bondedEntryDate = new ZDate(2025, 5, 5);

			var availableInventory = new Mock<IWhsPickAvailableInventory>();
			availableInventory.SetupGet(a => a.PalletID).Returns(palletId);
			availableInventory.SetupGet(a => a.PartAttrib1).Returns(partAttrib1);
			availableInventory.SetupGet(a => a.PartAttrib2).Returns(partAttrib2);
			availableInventory.SetupGet(a => a.PartAttrib3).Returns(partAttrib3);
			availableInventory.SetupGet(a => a.SerialNumber).Returns(serialNumber);
			availableInventory.SetupGet(a => a.ArrivalDate).Returns(arrivalDate);
			availableInventory.SetupGet(a => a.ExpiryDate).Returns(expiryDate);
			availableInventory.SetupGet(a => a.PackingDate).Returns(packingDate);
			availableInventory.SetupGet(a => a.BondedEntryKey).Returns(bondedEntryKey);
			availableInventory.SetupGet(a => a.BondedEntryDate).Returns(bondedEntryDate);

			var client = Mock.Of<IOrganisationFact>();
			var product = Mock.Of<IProductFact>();
			var location = Mock.Of<ITaskManagementLocationFact>();

			var packUQ = "BOX";
			var uomType = "CAS";

			var numberOfUnits = 10;
			var numberOfPacks = 2;
			var weight = 15.5m;
			var weightUQ = "KG";
			var volume = 2.75m;
			var volumeUQ = "M3";

			var fact = new TaskManagementPickLineFact(
				pk,
				pk,
				pick.Object,
				availableInventory.Object,
				client,
				product,
				location,
				packUQ,
				uomType,
				numberOfUnits - 0.1m,
				numberOfUnits,
				numberOfPacks,
				weight,
				weightUQ,
				volume,
				volumeUQ);

			AssertEquals(nameof(fact.PK), pk, fact.PK);
			AssertEquals(nameof(fact.Grouping.Fact), fact, fact.Grouping.Fact);
			AssertEquals(nameof(fact.Client.Fact), client, fact.Client.Fact);
			AssertEquals(nameof(fact.Product.Fact), product, fact.Product.Fact);
			AssertEquals(nameof(fact.Location.Fact), location, fact.Location.Fact);

			AssertEquals(nameof(fact.IsWorkOrder), isWorkOrderPick, fact.IsWorkOrder);
			AssertEquals(nameof(fact.IsCustomsTransaction), isCustomsTransaction, fact.IsCustomsTransaction);

			AssertEquals(nameof(fact.PalletID), palletId, fact.PalletID);
			AssertEquals(nameof(fact.PackUQ), packUQ, fact.PackUQ);
			AssertEquals(nameof(fact.UOMType), uomType, fact.UOMType);
			AssertEquals(nameof(fact.PartAttribute1), partAttrib1, fact.PartAttribute1);
			AssertEquals(nameof(fact.PartAttribute2), partAttrib2, fact.PartAttribute2);
			AssertEquals(nameof(fact.PartAttribute3), partAttrib3, fact.PartAttribute3);
			AssertEquals(nameof(fact.SerialNumber), serialNumber, fact.SerialNumber);
			AssertEquals(nameof(fact.ArrivalDate), arrivalDate.ToDateTime(), fact.ArrivalDate);
			AssertEquals(nameof(fact.ExpiryDate), expiryDate.ToDateTime(), fact.ExpiryDate);
			AssertEquals(nameof(fact.PackingDate), packingDate.ToDateTime(), fact.PackingDate);
			AssertEquals(nameof(fact.BondedEntryKey), bondedEntryKey, fact.BondedEntryKey);
			AssertEquals(nameof(fact.BondedEntryDate), bondedEntryDate.ToDateTime(), fact.BondedEntryDate);

			AssertEquals(nameof(fact.AssignedTask), Guid.Empty, fact.AssignedTask);
			AssertEquals(nameof(fact.NumberOfLines), 1, fact.NumberOfLines);
			AssertEquals(nameof(fact.Quantity), numberOfUnits - 0.1m, fact.Quantity);
			AssertEquals(nameof(fact.NumberOfUnits), numberOfUnits, fact.NumberOfUnits);
			AssertEquals(nameof(fact.NumberOfPacks), numberOfPacks, fact.NumberOfPacks);
			AssertEquals(nameof(fact.Weight), weight, fact.Weight);
			AssertEquals(nameof(fact.WeightUQ), weightUQ, fact.WeightUQ);
			AssertEquals(nameof(fact.Volume), volume, fact.Volume);
			AssertEquals(nameof(fact.VolumeUQ), volumeUQ, fact.VolumeUQ);
		}

		public void TestSplit_Throws_WhenPacksIsInvalid()
		{
			var pickableDocket = new Mock<IWhsPickableDocket>();
			pickableDocket.SetupGet(p => p.WD_RequiredDate).Returns(ZDateTimeOffset.Today);

			var availableInventory = new Mock<IWhsPickAvailableInventory>();
			availableInventory.SetupGet(a => a.ArrivalDate).Returns(ZDateTimeOffset.Today);

			var fact = new TaskManagementPickLineFact(
				Guid.NewGuid(),
				Guid.NewGuid(),
				Mock.Of<IWhsPick>(),
				availableInventory.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				"BOX",
				"CAS",
				100,
				100, // 10 units per pack
				10,
				50m, // 5.0 weight per pack
				"KG",
				25m, // 2.5 volume per pack
				"M3");

			AssertExceptionThrown<ArgumentOutOfRangeException>(() => fact.Split(0));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => fact.Split(10));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => fact.Split(11));
		}

		public void TestSplit()
		{
			var originalUnits = 100;
			var originalPacks = 10;
			var originalWeight = 50.0m;
			var originalVolume = 25.0m;
			var packsToSplit = 4;

			var availableInventory = new Mock<IWhsPickAvailableInventory>();
			availableInventory.SetupGet(a => a.ArrivalDate).Returns(ZDateTimeOffset.Today);

			var pk =  Guid.NewGuid();
			var availInvSplitPK = Guid.NewGuid();
			var fact = new TaskManagementPickLineFact(
				pk,
				availInvSplitPK,
				Mock.Of<IWhsPick>(),
				availableInventory.Object,
				Mock.Of<IOrganisationFact>(),
				Mock.Of<IProductFact>(),
				Mock.Of<ITaskManagementLocationFact>(),
				"BOX",
				"CAS",
				originalUnits,
				originalUnits,
				originalPacks,
				originalWeight,
				"KG",
				originalVolume,
				"M3");

			var splitFact1 = (TaskManagementPickLineFact)fact.Split(packsToSplit);

			// Check the split fact
			AssertNotEquals(nameof(splitFact1.PK), pk, splitFact1.PK);
			AssertEquals(nameof(splitFact1.AvailableInventorySplitPK), availInvSplitPK, splitFact1.AvailableInventorySplitPK);
			AssertEquals(nameof(splitFact1.NumberOfPacks), packsToSplit, splitFact1.NumberOfPacks);
			AssertEquals(nameof(splitFact1.NumberOfUnits), 40, splitFact1.NumberOfUnits); // 100 * (4/10)
			AssertEquals(nameof(splitFact1.Quantity), 40m, splitFact1.Quantity); // 100 * (4/10)
			AssertEquals(nameof(splitFact1.Weight), 20.0m, splitFact1.Weight); // 50.0 * (4/10)
			AssertEquals(nameof(splitFact1.Volume), 10.0m, splitFact1.Volume); // 25.0 * (4/10)

			// Check the original fact was reduced
			AssertEquals(nameof(fact.NumberOfPacks), originalPacks - packsToSplit, fact.NumberOfPacks);
			AssertEquals(nameof(fact.NumberOfUnits), originalUnits - 40, fact.NumberOfUnits);
			AssertEquals(nameof(fact.Quantity), originalUnits - 40m, fact.Quantity);
			AssertEquals(nameof(fact.Weight), originalWeight - 20.0m, fact.Weight);
			AssertEquals(nameof(fact.Volume), originalVolume - 10.0m, fact.Volume);

			// Check available inventory split pk is maintained with further splits
			var splitFact2 = (TaskManagementPickLineFact)splitFact1.Split(2);
			AssertNotEquals(nameof(splitFact1.PK), pk, splitFact2.PK);
			AssertNotEquals(nameof(splitFact1.PK), splitFact1.PK, splitFact2.PK);
			AssertEquals(nameof(splitFact1.AvailableInventorySplitPK), availInvSplitPK, splitFact1.AvailableInventorySplitPK);
		}

		public void TestSplit_SetsAllProperties() => TestSplit_SetsAllProperties(isCustomsTransaction: false, isWorkOrderPick: false);
		public void TestSplit_SetsAllProperties_IsCustomsTransaction() => TestSplit_SetsAllProperties(isCustomsTransaction: true, isWorkOrderPick: false);
		public void TestSplit_SetsAllProperties_IsWorkOrderPick() => TestSplit_SetsAllProperties(isCustomsTransaction: false, isWorkOrderPick: true);

		void TestSplit_SetsAllProperties(bool isCustomsTransaction, bool isWorkOrderPick)
		{
			var pk = Guid.NewGuid();
			var pick = new Mock<IWhsPick>();
			pick.SetupGet(p => p.IsWorkOrderPick).Returns(isWorkOrderPick);
			pick.SetupGet(p => p.IsCustomsTransaction).Returns(isCustomsTransaction);

			var palletId = "PALLET789";
			var partAttrib1 = "ATTR1";
			var partAttrib2 = "ATTR2";
			var partAttrib3 = "ATTR3";
			var serialNumber = "SN123456";
			var arrivalDate = new ZDateTimeOffset(2025, 5, 15);
			var expiryDate = new ZDate(2026, 6, 14);
			var packingDate = new ZDate(2025, 5, 10);
			var bondedEntryKey = "BEK123";
			var bondedEntryDate = new ZDate(2025, 5, 5);

			var availableInventory = new Mock<IWhsPickAvailableInventory>();
			availableInventory.SetupGet(a => a.PalletID).Returns(palletId);
			availableInventory.SetupGet(a => a.PartAttrib1).Returns(partAttrib1);
			availableInventory.SetupGet(a => a.PartAttrib2).Returns(partAttrib2);
			availableInventory.SetupGet(a => a.PartAttrib3).Returns(partAttrib3);
			availableInventory.SetupGet(a => a.SerialNumber).Returns(serialNumber);
			availableInventory.SetupGet(a => a.ArrivalDate).Returns(arrivalDate);
			availableInventory.SetupGet(a => a.ExpiryDate).Returns(expiryDate);
			availableInventory.SetupGet(a => a.PackingDate).Returns(packingDate);
			availableInventory.SetupGet(a => a.BondedEntryKey).Returns(bondedEntryKey);
			availableInventory.SetupGet(a => a.BondedEntryDate).Returns(bondedEntryDate);

			var client = Mock.Of<IOrganisationFact>();
			var product = Mock.Of<IProductFact>();
			var location = Mock.Of<ITaskManagementLocationFact>();

			var packUQ = "BOX";
			var uomType = "CAS";

			var originalUnits = 100;
			var originalPacks = 10;
			var originalWeight = 50.0m;
			var weightUQ = "KG";
			var originalVolume = 25.0m;
			var volumeUQ = "M3";
			var packsToSplit = 4;

			var fact = new TaskManagementPickLineFact(
				pk,
				pk,
				pick.Object,
				availableInventory.Object,
				client,
				product,
				location,
				packUQ,
				uomType,
				originalUnits - 0.1m,
				originalUnits,
				originalPacks,
				originalWeight,
				weightUQ,
				originalVolume,
				volumeUQ);

			var splitFact = (TaskManagementPickLineFact)fact.Split(packsToSplit);
			AssertNotEquals(nameof(splitFact.PK), pk, splitFact.PK);
			AssertEquals(nameof(splitFact.AvailableInventorySplitPK), pk, splitFact.AvailableInventorySplitPK);
			AssertEquals(nameof(splitFact.Grouping.Fact), splitFact, splitFact.Grouping.Fact);
			AssertEquals(nameof(splitFact.Client.Fact), client, splitFact.Client.Fact);
			AssertEquals(nameof(splitFact.Product.Fact), product, splitFact.Product.Fact);
			AssertEquals(nameof(splitFact.Location.Fact), location, splitFact.Location.Fact);
			AssertEquals(nameof(splitFact.IsWorkOrder), isWorkOrderPick, splitFact.IsWorkOrder);
			AssertEquals(nameof(splitFact.IsCustomsTransaction), isCustomsTransaction, splitFact.IsCustomsTransaction);
			AssertEquals(nameof(splitFact.PalletID), palletId, splitFact.PalletID);
			AssertEquals(nameof(splitFact.PackUQ), packUQ, splitFact.PackUQ);
			AssertEquals(nameof(splitFact.UOMType), uomType, splitFact.UOMType);
			AssertEquals(nameof(splitFact.PartAttribute1), partAttrib1, splitFact.PartAttribute1);
			AssertEquals(nameof(splitFact.PartAttribute2), partAttrib2, splitFact.PartAttribute2);
			AssertEquals(nameof(splitFact.PartAttribute3), partAttrib3, splitFact.PartAttribute3);
			AssertEquals(nameof(splitFact.SerialNumber), serialNumber, splitFact.SerialNumber);
			AssertEquals(nameof(splitFact.ArrivalDate), arrivalDate.ToDateTime(), splitFact.ArrivalDate);
			AssertEquals(nameof(splitFact.ExpiryDate), expiryDate.ToDateTime(), splitFact.ExpiryDate);
			AssertEquals(nameof(splitFact.PackingDate), packingDate.ToDateTime(), splitFact.PackingDate);
			AssertEquals(nameof(splitFact.BondedEntryKey), bondedEntryKey, splitFact.BondedEntryKey);
			AssertEquals(nameof(splitFact.BondedEntryDate), bondedEntryDate.ToDateTime(), splitFact.BondedEntryDate);
			AssertEquals(nameof(splitFact.AssignedTask), Guid.Empty, splitFact.AssignedTask);
			AssertEquals(nameof(splitFact.NumberOfLines), 1, splitFact.NumberOfLines);
			AssertEquals(nameof(splitFact.NumberOfPacks), packsToSplit, splitFact.NumberOfPacks);
			AssertEquals(nameof(splitFact.NumberOfUnits), 40, splitFact.NumberOfUnits); // 100 * (4/10)
			AssertEquals(nameof(splitFact.Quantity), 40m, splitFact.Quantity); // 100 * (4/10)
			AssertEquals(nameof(splitFact.Weight), 20.0m, splitFact.Weight); // 50.0 * (4/10)
			AssertEquals(nameof(splitFact.WeightUQ), weightUQ, splitFact.WeightUQ);
			AssertEquals(nameof(splitFact.Volume), 10.0m, splitFact.Volume); // 25.0 * (4/10)
			AssertEquals(nameof(splitFact.VolumeUQ), volumeUQ, splitFact.VolumeUQ);
			AssertEquals(nameof(fact.NumberOfPacks), originalPacks - packsToSplit, fact.NumberOfPacks);
			AssertEquals(nameof(fact.NumberOfUnits), originalUnits - 40, fact.NumberOfUnits);
			AssertEquals(nameof(fact.Quantity), (originalUnits - 0.1m) - 40m, fact.Quantity);
			AssertEquals(nameof(fact.Weight), originalWeight - 20.0m, fact.Weight);
			AssertEquals(nameof(fact.Volume), originalVolume - 10.0m, fact.Volume);
		}
	}
}
