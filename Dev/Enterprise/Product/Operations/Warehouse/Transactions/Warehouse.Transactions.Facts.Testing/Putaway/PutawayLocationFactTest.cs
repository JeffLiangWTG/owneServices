using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.ProductionRules.Business.ProductWarehousePutaway;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	public class PutawayLocationFactTest : TestCase
	{
		#region TestProperties

		public void TestProperties_IsPartialPallet() => TestProperties(true, false, false, false, false);
		public void TestProperties_IsDynamicPickFace() => TestProperties(false, true, false, false, false);
		public void TestProperties_IsFixedPickFace() => TestProperties(false, false, true, false, false);
		public void TestProperties_IsFixedPickFaceFull() => TestProperties(false, false, false, true, false);
		public void TestProperties_IsTsaKnownLocation() => TestProperties(false, false, false, false, true);
		public void TestProperties_NoBooleansTrue() => TestProperties(false, false, false, false, false);
		public void TestProperties_AllBooleansTrue() => TestProperties(true, true, true, true, true);

		void TestProperties(bool isPartialPallet, bool isDynamicPickFace, bool isFixedPickFace, bool isFixedPickFaceFull, bool isTsaKnownLocation)
		{
			// Put unique values for all values that support enough values, test combos for booleans which can only have two values
			var pk = Guid.NewGuid();
			var locationPk = Guid.NewGuid();
			var warehousePk = Guid.NewGuid();

			var locationTypeCode = "TYP";
			var locationClass = "NOR";
			var areaName = "PUT";
			var rowName = "A";

			var column = 1;
			var level = 2;
			var tray = 3;
			var putawaySequence = 4;

			var locationStatus = "AVL";
			var areaTypeCode = "HEL";

			var availableWeight = 42.42m;
			var maxWeightUnit = "KG";

			var availableVolume = 3.1415m;
			var maxVolumeUnit = "M3";

			var availableUnits = 2.718m;
			var currentAndPendingStock = 9.81m;

			var partialPalletID = "PLT-123";

			var pickFaceProductPk = Guid.NewGuid();
			var pickFaceClientPK = Guid.NewGuid();
			var lastAllocatedOrChangedID = Guid.NewGuid();

			var palletSpaces = 20;
			var palletQuantity = 18;

			var putawayLocationFact = new PutawayLocationFact(
				pk,
				locationPk,
				warehousePk,
				locationTypeCode,
				locationClass,
				areaName,
				rowName,
				column,
				level,
				tray,
				putawaySequence,
				locationStatus,
				areaTypeCode,
				availableWeight,
				0m,
				maxWeightUnit,
				availableVolume,
				0m,
				maxVolumeUnit,
				availableUnits,
				0m,
				currentAndPendingStock,
				isPartialPallet,
				partialPalletID,
				isDynamicPickFace,
				isFixedPickFace,
				isFixedPickFaceFull,
				pickFaceProductPk,
				pickFaceClientPK,
				isTsaKnownLocation,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				palletSpaces,
				palletQuantity,
				null,
				lastAllocatedOrChangedID);

			AssertEquals(nameof(PutawayLocationFact.PK), pk, putawayLocationFact.PK);
			AssertEquals(nameof(PutawayLocationFact.LocationPK), locationPk, putawayLocationFact.LocationPK);
			AssertEquals(nameof(PutawayLocationFact.WarehousePK), warehousePk, putawayLocationFact.WarehousePK);
			AssertEquals(nameof(PutawayLocationFact.LocationTypeCode), locationTypeCode, putawayLocationFact.LocationTypeCode);
			AssertEquals(nameof(PutawayLocationFact.LocationClass), locationClass, putawayLocationFact.LocationClass);
			AssertEquals(nameof(PutawayLocationFact.AreaName), areaName, putawayLocationFact.AreaName);
			AssertEquals(nameof(PutawayLocationFact.RowName), rowName, putawayLocationFact.RowName);
			AssertEquals(nameof(PutawayLocationFact.Column), column, putawayLocationFact.Column);
			AssertEquals(nameof(PutawayLocationFact.Level), level, putawayLocationFact.Level);
			AssertEquals(nameof(PutawayLocationFact.Tray), tray, putawayLocationFact.Tray);
			AssertEquals(nameof(PutawayLocationFact.PutawaySequence), putawaySequence, putawayLocationFact.PutawaySequence);
			AssertEquals(nameof(PutawayLocationFact.LocationStatus), locationStatus, putawayLocationFact.LocationStatus);
			AssertEquals(nameof(PutawayLocationFact.AreaTypeCode), areaTypeCode, putawayLocationFact.AreaTypeCode);
			AssertEquals(nameof(PutawayLocationFact.AvailableWeight), availableWeight, putawayLocationFact.AvailableWeight);
			AssertEquals(nameof(PutawayLocationFact.MaxWeightUnit), maxWeightUnit, putawayLocationFact.MaxWeightUnit);
			AssertEquals(nameof(PutawayLocationFact.AvailableVolume), availableVolume, putawayLocationFact.AvailableVolume);
			AssertEquals(nameof(PutawayLocationFact.MaxVolumeUnit), maxVolumeUnit, putawayLocationFact.MaxVolumeUnit);
			AssertEquals(nameof(PutawayLocationFact.AvailableUnits), availableUnits, putawayLocationFact.AvailableUnits);
			AssertEquals(nameof(PutawayLocationFact.CurrentAndIncomingStock), currentAndPendingStock, putawayLocationFact.CurrentAndIncomingStock);
			AssertEquals(nameof(PutawayLocationFact.IsPartialPallet), isPartialPallet, putawayLocationFact.IsPartialPallet);
			AssertEquals(nameof(PutawayLocationFact.PartialPalletID), partialPalletID, putawayLocationFact.PartialPalletID);
			AssertEquals(nameof(PutawayLocationFact.IsDynamicPickFace), isDynamicPickFace, putawayLocationFact.IsDynamicPickFace);
			AssertEquals(nameof(PutawayLocationFact.IsFixedPickFace), isFixedPickFace, putawayLocationFact.IsFixedPickFace);
			AssertEquals(nameof(PutawayLocationFact.IsFixedPickFaceFull), isFixedPickFaceFull, putawayLocationFact.IsFixedPickFaceFull);
			AssertEquals(nameof(PutawayLocationFact.ProductPK), pickFaceProductPk, putawayLocationFact.ProductPK);
			AssertEquals(nameof(PutawayLocationFact.ClientPK), pickFaceClientPK, putawayLocationFact.ClientPK);
			AssertEquals(nameof(PutawayLocationFact.IsTSAKnownLocation), isTsaKnownLocation, putawayLocationFact.IsTSAKnownLocation);
			AssertEquals(nameof(PutawayLocationFact.PalletSpaces), palletSpaces, putawayLocationFact.PalletSpaces);
			AssertEquals(nameof(PutawayLocationFact.PalletQuantity), palletQuantity, putawayLocationFact.PalletQuantity);
			AssertEquals(nameof(PutawayLocationFact.LastAllocatedOrChangedID), lastAllocatedOrChangedID, putawayLocationFact.LastAllocatedOrChangedID);
		}

		#endregion

		#region TestProperties_ParentLocation

		public void TestProperties_ParentLocation_Capacity()
		{
			var parentLocation = GetPutawayLocationFact(availableUnits: 3m, maxUnits: 10m);
			var locationFact1 = GetPutawayLocationFact(availableUnits: 5m, maxUnits: 10m, parentLocation: parentLocation);
			AssertEquals("Should have unit capacity.", true, locationFact1.HasUnitCapacity);
			AssertEquals("AvailableUnits should be limited by parent location.", 3m, locationFact1.AvailableUnits);

			var locationFact2 = GetPutawayLocationFact(availableUnits: 2m, maxUnits: 10m, parentLocation: parentLocation);
			AssertEquals("Should have unit capacity.", true, locationFact2.HasUnitCapacity);
			AssertEquals("AvailableUnits should be limited by its own capacity.", 2m, locationFact2.AvailableUnits);

			var locationFact3 = GetPutawayLocationFact(parentLocation: parentLocation);
			AssertEquals("Should have unit capacity.", true, locationFact3.HasUnitCapacity);
			AssertEquals("AvailableUnits should be limited by by parent location.", 3m, locationFact3.AvailableUnits);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(1m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			parentLocation.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("AvailableUnits should be limited by parent location and update with it.", 1m, locationFact1.AvailableUnits);
			AssertEquals("AvailableUnits should be limited by parent location and update with it.", 1m, locationFact2.AvailableUnits);
			AssertEquals("AvailableUnits should be limited by parent location and update with it.", 1m, locationFact3.AvailableUnits);
		}

		public void TestProperties_ParentLocation_NoCapacitySetOnParent()
		{
			var parentLocation = GetPutawayLocationFact();
			var locationFact1 = GetPutawayLocationFact(availableUnits: 5m, maxUnits: 10m, parentLocation: parentLocation);
			AssertEquals("Should have unit capacity.", true, locationFact1.HasUnitCapacity);
			AssertEquals("AvailableUnits should be limited by its own capacity.", 5m, locationFact1.AvailableUnits);

			var locationFact2 = GetPutawayLocationFact(availableUnits: 2m, maxUnits: 10m, parentLocation: parentLocation);
			AssertEquals("Should have unit capacity.", true, locationFact2.HasUnitCapacity);
			AssertEquals("AvailableUnits should be limited by its own capacity.", 2m, locationFact2.AvailableUnits);
		}

		public void TestProperties_ParentLocation_Weight()
		{
			// For weight/volume we disregard the location's weight/volume to avoid conversions and because partial pallets dont have a weight/vol limit
			var parentLocation = GetPutawayLocationFact(availableWeight: 3m, maxWeight: 10m, weightUQ: Core.Constants.Weight.Kilograms);
			var locationFact1 = GetPutawayLocationFact(availableWeight: 1m, maxWeight: 10m, weightUQ: Core.Constants.Weight.Kilograms, parentLocation: parentLocation);
			AssertEquals("Should have MaxWeightUnit.", Core.Constants.Weight.Kilograms, locationFact1.MaxWeightUnit);
			AssertEquals("AvailableWeight should be limited by parent location and disregard current weight.", 3m, locationFact1.AvailableWeight);

			var locationFact2 = GetPutawayLocationFact(parentLocation: parentLocation);
			AssertEquals("Should have MaxWeightUnit.", Core.Constants.Weight.Kilograms, locationFact2.MaxWeightUnit);
			AssertEquals("AvailableWeight should be limited by parent location.", 3m, locationFact2.AvailableWeight);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(1m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));
			inv1.Setup(inv => inv.ProductWeight).Returns(1m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns(Core.Constants.Weight.Kilograms);

			parentLocation.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should have MaxWeightUnit.", Core.Constants.Weight.Kilograms, locationFact1.MaxWeightUnit);
			AssertEquals("Should have MaxWeightUnit.", Core.Constants.Weight.Kilograms, locationFact2.MaxWeightUnit);
			AssertEquals("AvailableWeight should be limited by parent location.", 1m, locationFact1.AvailableWeight);
			AssertEquals("AvailableWeight should be limited by parent location.", 1m, locationFact2.AvailableWeight);
		}

		public void TestProperties_ParentLocation_Volume()
		{
			// For weight/volume we disregard the location's weight/volume to avoid conversions and because partial pallets dont have a weight/vol limit
			var parentLocation = GetPutawayLocationFact(availableVolume: 3m, maxVolume: 10m, volumeUQ: Core.Constants.Volume.CubicMetres);
			var locationFact1 = GetPutawayLocationFact(availableVolume: 1m, maxVolume: 10m, volumeUQ: Core.Constants.Volume.CubicMetres, parentLocation: parentLocation);
			AssertEquals("Should have MaxVolumeUnit.", Core.Constants.Volume.CubicMetres, locationFact1.MaxVolumeUnit);
			AssertEquals("AvailableVolume should be limited by parent location and disregard current weight.", 3m, locationFact1.AvailableVolume);

			var locationFact2 = GetPutawayLocationFact(parentLocation: parentLocation);
			AssertEquals("Should have MaxVolumeUnit.", Core.Constants.Volume.CubicMetres, locationFact2.MaxVolumeUnit);
			AssertEquals("AvailableVolume should be limited by parent location.", 3m, locationFact2.AvailableVolume);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(1m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));
			inv1.Setup(inv => inv.ProductVolume).Returns(1m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns(Core.Constants.Volume.CubicMetres);

			parentLocation.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should have MaxVolumeUnit.", Core.Constants.Volume.CubicMetres, locationFact1.MaxVolumeUnit);
			AssertEquals("Should have MaxVolumeUnit.", Core.Constants.Volume.CubicMetres, locationFact2.MaxVolumeUnit);
			AssertEquals("AvailableVolume should be limited by parent location.", 1m, locationFact1.AvailableVolume);
			AssertEquals("AvailableVolume should be limited by parent location.", 1m, locationFact2.AvailableVolume);
		}

		public void TestProperties_ParentLocation_PalletCount()
		{
			var parentLocation = GetPutawayLocationFact();
			AssertEquals("palletQuantity of parent Location is 18", 18, parentLocation.PalletQuantity);
			var locationFact1 = GetPutawayLocationFact(palletQuantity: 5, parentLocation: parentLocation);
			AssertEquals("palletQuantity should be limited by parent location.", 18, locationFact1.PalletQuantity);

			var locationFact2 = GetPutawayLocationFact(parentLocation: parentLocation);
			AssertEquals("palletQuantity should be limited by parent location.", 18, locationFact2.PalletQuantity);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(2m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));
			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(2m);
			inv2.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			parentLocation.UpdateDataAfterPutawayAllocation(new[] { inv1.Object, inv2.Object });
			AssertEquals("palletQuantity of parent Location should change to 19", 19, parentLocation.PalletQuantity);
			AssertEquals("palletQuantity should be limited by parent location.", 19, locationFact1.PalletQuantity);
			AssertEquals("palletQuantity should be limited by parent location.", 19, locationFact2.PalletQuantity);
		}

		public void TestProperties_ParentLocation_PalletCount_UpdateForChildLocation()
		{
			var parentLocation = GetPutawayLocationFact();
			AssertEquals("palletQuantity of parent Location is 18", 18, parentLocation.PalletQuantity);
			var locationFact1 = GetPutawayLocationFact(palletQuantity: 5, parentLocation: parentLocation);
			AssertEquals("palletQuantity should be limited by parent location.", 18, locationFact1.PalletQuantity);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(2m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));
			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(2m);
			inv2.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			locationFact1.UpdateDataAfterPutawayAllocation(new[] { inv1.Object, inv2.Object });
			AssertEquals("palletQuantity of parent Location should change to 19", 19, parentLocation.PalletQuantity);
			AssertEquals("palletQuantity should be limited by parent location.", 19, locationFact1.PalletQuantity);
		}

		#endregion

		#region TestLocationStatusMatchesInventory

		public void TestLocationStatusMatchesInventory_Normal() => TestLocationStatusMatchesInventory(string.Empty, LocationStatus.Codes.Normal);
		public void TestLocationStatusMatchesInventory_Damaged() => TestLocationStatusMatchesInventory("DAM", LocationStatus.Codes.Damaged);
		public void TestLocationStatusMatchesInventory_Held() => TestLocationStatusMatchesInventory("HEL", LocationStatus.Codes.Held);
		public void TestLocationStatusMatchesInventory_Held_CustomCode() => TestLocationStatusMatchesInventory("QC1", LocationStatus.Codes.Held);

		void TestLocationStatusMatchesInventory(string holdCode, string locationStatus)
		{
			var trueLocation = GetPutawayLocationFact(locationStatus: locationStatus);

			var trueInventory = new Mock<IInventoryFact>();
			trueInventory.Setup(i => i.HoldCode).Returns(holdCode);
			AssertEquals(nameof(IPutawayLocationFact.LocationStatusMatchesInventory), true, trueLocation.LocationStatusMatchesInventory(trueInventory.Object));

			var otherHoldCodes = holdCode == "HEL" || holdCode == "QC1" ? new[] { string.Empty, "DAM" } : new[] { string.Empty, "DAM", "HEL", "QC1" }.Except(new[] { holdCode });
			var otherLocationStatuses = new[] { LocationStatus.Codes.Normal, LocationStatus.Codes.Held, LocationStatus.Codes.Damaged }.Except(new[] { locationStatus });

			foreach (var otherHoldCode in otherHoldCodes)
			{
				var falseInventory = new Mock<IInventoryFact>();
				falseInventory.Setup(i => i.HoldCode).Returns(otherHoldCode);

				AssertEquals(nameof(IPutawayLocationFact.LocationStatusMatchesInventory), false, trueLocation.LocationStatusMatchesInventory(falseInventory.Object));
			}

			foreach (var otherLocationStatus in otherLocationStatuses)
			{
				var falseLocation = GetPutawayLocationFact(locationStatus: otherLocationStatus);
				AssertEquals(nameof(IPutawayLocationFact.LocationStatusMatchesInventory), false, falseLocation.LocationStatusMatchesInventory(trueInventory.Object));
			}
		}

		#endregion

		#region TestLocationStatusMatchesInventory_Pallet

		public void TestLocationStatusMatchesInventory_Pallet_Normal() => TestLocationStatusMatchesInventory_Pallet(string.Empty, string.Empty, LocationStatus.Codes.Normal);
		public void TestLocationStatusMatchesInventory_Pallet_Damaged() => TestLocationStatusMatchesInventory_Pallet("DAM", "DAM", LocationStatus.Codes.Damaged);
		public void TestLocationStatusMatchesInventory_Pallet_Held() => TestLocationStatusMatchesInventory_Pallet("HEL", "HEL", LocationStatus.Codes.Held);
		public void TestLocationStatusMatchesInventory_Pallet_Held_CustomCode() => TestLocationStatusMatchesInventory_Pallet("QC1", "QC1", LocationStatus.Codes.Held);
		public void TestLocationStatusMatchesInventory_Pallet_NormalPrioritised_Damaged() => TestLocationStatusMatchesInventory_Pallet(string.Empty, "DAM", LocationStatus.Codes.Normal);
		public void TestLocationStatusMatchesInventory_Pallet_NormalPrioritised_Held() => TestLocationStatusMatchesInventory_Pallet(string.Empty, "HEL", LocationStatus.Codes.Normal);
		public void TestLocationStatusMatchesInventory_Pallet_NormalPrioritised_Held_CustomCode() => TestLocationStatusMatchesInventory_Pallet(string.Empty, "QC1", LocationStatus.Codes.Normal);
		public void TestLocationStatusMatchesInventory_Pallet_HeldPrioritisedOverDamaged() => TestLocationStatusMatchesInventory_Pallet("DAM", "HEL", LocationStatus.Codes.Held);
		public void TestLocationStatusMatchesInventory_Pallet_HeldPrioritisedOverDamaged_CustomCode() => TestLocationStatusMatchesInventory_Pallet("DAM", "QC1", LocationStatus.Codes.Held);

		void TestLocationStatusMatchesInventory_Pallet(string holdCode1, string holdCode2, string locationStatus)
		{
			var trueLocation = GetPutawayLocationFact(locationStatus: locationStatus);

			var trueInventory1 = new Mock<IInventoryFact>();
			trueInventory1.Setup(i => i.HoldCode).Returns(holdCode1);

			var trueInventory2 = new Mock<IInventoryFact>();
			trueInventory2.Setup(i => i.HoldCode).Returns(holdCode2);

			var inventories = new[] { trueInventory1.Object, trueInventory2.Object };

			AssertEquals(nameof(IPutawayLocationFact.LocationStatusMatchesInventory), true, trueLocation.LocationStatusMatchesInventory(inventories));

			foreach (var otherLocationStatus in new[] { LocationStatus.Codes.Normal, LocationStatus.Codes.Held, LocationStatus.Codes.Damaged }.Except(new[] { locationStatus }))
			{
				var falseLocation = GetPutawayLocationFact(locationStatus: otherLocationStatus);
				AssertEquals(nameof(IPutawayLocationFact.LocationStatusMatchesInventory), false, falseLocation.LocationStatusMatchesInventory(inventories));
			}
		}

		#endregion

		#region TestContainsThisProduct

		public void TestContainsThisProduct_True() => TestContainsThisProduct(true);
		public void TestContainsThisProduct_False() => TestContainsThisProduct(false);

		void TestContainsThisProduct(bool expectedValue)
		{
			var relation1FK = Guid.NewGuid();
			var relation2FK = Guid.NewGuid();

			var relationsHashSet = new HashSet<Guid>();

			if (expectedValue)
			{
				relationsHashSet.Add(relation1FK);
			}

			var productsDataJson = JsonConvert.SerializeObject(relationsHashSet);
			var putawayLocationFact = GetPutawayLocationFact(productData: productsDataJson);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));

			AssertEquals(nameof(IPutawayLocationFact.ContainsThisProduct), expectedValue, putawayLocationFact.ContainsThisProduct(inv1.Object));
			AssertEquals("Should *not* contain this product.", false, putawayLocationFact.ContainsThisProduct(inv2.Object));
		}

		#endregion

		#region TestContainsThisProduct_EmptyJson

		public void TestContainsThisProduct_EmptyJson()
		{
			var relation1FK = Guid.NewGuid();
			var putawayLocationFact = GetPutawayLocationFact(productData: "[]");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));

			AssertEquals("Should *not* contain this product.", false, putawayLocationFact.ContainsThisProduct(inv1.Object));
		}

		#endregion

		#region TestContainsThisProduct_IEnumerableOverload

		public void TestContainsThisProduct_IEnumerableOverload()
		{
			var relation1FK = Guid.NewGuid();
			var relation2FK = Guid.NewGuid();
			var relation3FK = Guid.NewGuid();

			var productsDataJson = JsonConvert.SerializeObject(new HashSet<Guid>(new[] { relation1FK, relation2FK }));
			var putawayLocationFact = GetPutawayLocationFact(productData: productsDataJson);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));

			var inv3 = new Mock<IInventoryFact>();
			inv3.Setup(inv => inv.Product).Returns(CreateProductJoin(relation3FK));

			AssertEquals("Should contain this product if all products are in the location.", true, putawayLocationFact.ContainsThisProduct(new[] { inv1.Object, inv2.Object }));
			AssertEquals("Should contain this product if all products are in the location.", true, putawayLocationFact.ContainsThisProduct(new[] { inv1.Object }));

			AssertEquals("Should *not* contain this product if any products are not in the location.", false, putawayLocationFact.ContainsThisProduct(new[] { inv3.Object }));
			AssertEquals("Should *not* contain this product if any products are not in the location.", false, putawayLocationFact.ContainsThisProduct(new[] { inv1.Object, inv3.Object }));
		}

		#endregion

		#region TestContainsOtherAttributeCombinations

		public void TestContainsOtherAttributeCombinations_Empty()
		{
			var relationFK = Guid.NewGuid();
			var putawayLocationFact = GetPutawayLocationFact();

			var invMock = new Mock<IInventoryFact>();
			invMock.Setup(inv => inv.Product).Returns(CreateProductJoin(relationFK));

			AssertEquals("Should *not* contain other attribute combinations.", false, putawayLocationFact.ContainsOtherAttributeCombinations(invMock.Object));
			AssertEquals("Should *not* contain other attribute combinations.", false, putawayLocationFact.ContainsOtherAttributeCombinations(new[] { invMock.Object }));
		}

		public void TestContainsOtherAttributeCombinations_PA1() => TestContainsOtherAttributeCombinations(invMock => invMock.Setup(inv => inv.PartAttribute1).Returns("OTHER"));

		public void TestContainsOtherAttributeCombinations_PA2() => TestContainsOtherAttributeCombinations(invMock => invMock.Setup(inv => inv.PartAttribute2).Returns("OTHER"));

		public void TestContainsOtherAttributeCombinations_PA3() => TestContainsOtherAttributeCombinations(invMock => invMock.Setup(inv => inv.PartAttribute3).Returns("OTHER"));

		public void TestContainsOtherAttributeCombinations_ExpiryDate()
			=> TestContainsOtherAttributeCombinations(invMock => invMock.Setup(inv => inv.ExpiryDate).Returns(ZDateTime.Today.ToDateTime()));

		public void TestContainsOtherAttributeCombinations_PackingDate()
			=> TestContainsOtherAttributeCombinations(invMock => invMock.Setup(inv => inv.PackingDate).Returns(ZDateTime.Today.ToDateTime()));

		void TestContainsOtherAttributeCombinations(Action<Mock<IInventoryFact>> setToOtherValue)
		{
			var relationFK = Guid.NewGuid();
			var pa1Data = GetLocationCacheAttributeData(relationFK, "PA1", 1);
			var pa2Data = GetLocationCacheAttributeData(relationFK, "PA2", 1);
			var pa3Data = GetLocationCacheAttributeData(relationFK, "PA3", 1);
			var packingData = GetLocationCacheAttributeData(relationFK, ZDateTime.BrettsBirthday.ToDateTime(), 1);
			var expiryData = GetLocationCacheAttributeData(relationFK, ZDateTime.BrettsBirthday.AddYears(1).ToDateTime(), 1);
			var putawayLocationFact = GetPutawayLocationFact(productData: null, pa1Data, pa2Data, pa3Data, expiryData, packingData);

			var invMock = new Mock<IInventoryFact>();
			invMock.Setup(inv => inv.Product).Returns(CreateProductJoin(relationFK));
			invMock.Setup(inv => inv.PartAttribute1).Returns("PA1");
			invMock.Setup(inv => inv.PartAttribute2).Returns("PA2");
			invMock.Setup(inv => inv.PartAttribute3).Returns("PA3");
			invMock.Setup(inv => inv.PackingDate).Returns(ZDateTime.BrettsBirthday.ToDateTime());
			invMock.Setup(inv => inv.ExpiryDate).Returns(ZDateTime.BrettsBirthday.AddYears(1).ToDateTime());

			AssertEquals("Precondition: Should *not* contain other attribute combinations.", false, putawayLocationFact.ContainsOtherAttributeCombinations(invMock.Object));
			AssertEquals("Precondition: Should *not* contain other attribute combinations.", false, putawayLocationFact.ContainsOtherAttributeCombinations(new[] { invMock.Object }));

			setToOtherValue(invMock);
			AssertEquals("Should contain other attribute combinations.", true, putawayLocationFact.ContainsOtherAttributeCombinations(invMock.Object));
			AssertEquals("Should contain other attribute combinations.", true, putawayLocationFact.ContainsOtherAttributeCombinations(new[] { invMock.Object }));
		}

		public void TestContainsOtherAttributeCombinations_DifferentProduct()
		{
			var relation1FK = Guid.NewGuid();
			var relation2FK = Guid.NewGuid();

			var pa1Data = GetLocationCacheAttributeData(relation1FK, "PA1", 1);
			var pa2Data = GetLocationCacheAttributeData(relation1FK, "PA2", 1);
			var pa3Data = GetLocationCacheAttributeData(relation1FK, "PA3", 1);
			var packingData = GetLocationCacheAttributeData(relation1FK, ZDateTime.BrettsBirthday.ToDateTime(), 1);
			var expiryData = GetLocationCacheAttributeData(relation1FK, ZDateTime.BrettsBirthday.AddYears(1).ToDateTime(), 1);
			var putawayLocationFact = GetPutawayLocationFact(productData: null, pa1Data, pa2Data, pa3Data, expiryData, packingData);

			var invMock = new Mock<IInventoryFact>();
			invMock.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));
			invMock.Setup(inv => inv.PartAttribute1).Returns("OTHER");
			invMock.Setup(inv => inv.PartAttribute2).Returns("OTHER");
			invMock.Setup(inv => inv.PartAttribute3).Returns("OTHER");
			invMock.Setup(inv => inv.PackingDate).Returns(ZDateTime.BrettsBirthday.AddYears(10).ToDateTime());
			invMock.Setup(inv => inv.ExpiryDate).Returns(ZDateTime.BrettsBirthday.AddYears(5).ToDateTime());

			AssertEquals("Should contain other attribute combinations.", true, putawayLocationFact.ContainsOtherAttributeCombinations(invMock.Object));
			AssertEquals("Should contain other attribute combinations.", true, putawayLocationFact.ContainsOtherAttributeCombinations(new[] { invMock.Object }));

			invMock.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));
			AssertEquals("Should *not* contain other attribute combinations.", false, putawayLocationFact.ContainsOtherAttributeCombinations(invMock.Object));
			AssertEquals("Should *not* contain other attribute combinations.", false, putawayLocationFact.ContainsOtherAttributeCombinations(new[] { invMock.Object }));
		}

		public void TestContainsOtherAttributeCombinations_Enumerable()
		{
			var relation1FK = Guid.NewGuid();
			var relation2FK = Guid.NewGuid();

			var pa1Data = GetLocationCacheAttributeData((relation1FK, "PA1", 1), (relation2FK, "P21", 1));
			var pa2Data = GetLocationCacheAttributeData((relation1FK, "PA2", 1), (relation2FK, "P22", 1));
			var pa3Data = GetLocationCacheAttributeData((relation1FK, "PA3", 1), (relation2FK, "P23", 1));
			var packingData = GetLocationCacheAttributeData((relation1FK, ZDateTime.BrettsBirthday.ToDateTime(), 1), (relation2FK, ZDateTime.BrettsBirthday.ToDateTime(), 1));
			var expiryData = GetLocationCacheAttributeData((relation1FK, ZDateTime.BrettsBirthday.AddYears(1).ToDateTime(), 1), (relation2FK, ZDateTime.BrettsBirthday.AddYears(1).ToDateTime(), 1));
			var putawayLocationFact = GetPutawayLocationFact(productData: null, pa1Data, pa2Data, pa3Data, expiryData, packingData);

			var invMock1 = new Mock<IInventoryFact>();
			invMock1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));
			invMock1.Setup(inv => inv.PartAttribute1).Returns("PA1");
			invMock1.Setup(inv => inv.PartAttribute2).Returns("PA2");
			invMock1.Setup(inv => inv.PartAttribute3).Returns("PA3");
			invMock1.Setup(inv => inv.PackingDate).Returns(ZDateTime.BrettsBirthday.ToDateTime());
			invMock1.Setup(inv => inv.ExpiryDate).Returns(ZDateTime.BrettsBirthday.AddYears(1).ToDateTime());

			var invMock2 = new Mock<IInventoryFact>();
			invMock2.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));
			invMock2.Setup(inv => inv.PartAttribute1).Returns("P21");
			invMock2.Setup(inv => inv.PartAttribute2).Returns("P22");
			invMock2.Setup(inv => inv.PartAttribute3).Returns("P23");
			invMock2.Setup(inv => inv.PackingDate).Returns(ZDateTime.BrettsBirthday.ToDateTime());
			invMock2.Setup(inv => inv.ExpiryDate).Returns(ZDateTime.BrettsBirthday.AddYears(1).ToDateTime());

			var invMock3 = new Mock<IInventoryFact>();
			invMock3.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));
			invMock3.Setup(inv => inv.PartAttribute1).Returns("PA1");
			invMock3.Setup(inv => inv.PartAttribute2).Returns("PA2");
			invMock3.Setup(inv => inv.PartAttribute3).Returns("PA3");
			invMock3.Setup(inv => inv.PackingDate).Returns(ZDateTime.BrettsBirthday.ToDateTime());
			invMock3.Setup(inv => inv.ExpiryDate).Returns(ZDateTime.BrettsBirthday.AddYears(1).ToDateTime());

			AssertEquals("Should *not* contain other attribute combinations.", false, putawayLocationFact.ContainsOtherAttributeCombinations(new[] { invMock1.Object, invMock2.Object }));
			AssertEquals("Should contain other attribute combinations.", true, putawayLocationFact.ContainsOtherAttributeCombinations(new[] { invMock1.Object, invMock2.Object, invMock3.Object }));
		}

		public void TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_PA1() => TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet(invMock => invMock.Setup(inv => inv.PartAttribute1).Returns("OTHER"));

		public void TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_PA2() => TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet(invMock => invMock.Setup(inv => inv.PartAttribute2).Returns("OTHER"));

		public void TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_PA3() => TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet(invMock => invMock.Setup(inv => inv.PartAttribute3).Returns("OTHER"));

		public void TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_ExpiryDate()
			=> TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet(invMock => invMock.Setup(inv => inv.ExpiryDate).Returns(ZDateTime.Today.ToDateTime()));

		public void TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_PackingDate()
			=> TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet(invMock => invMock.Setup(inv => inv.PackingDate).Returns(ZDateTime.Today.ToDateTime()));

		public void TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet(Action<Mock<IInventoryFact>> setToOtherValue)
		{
			var relationFK = Guid.NewGuid();
			var putawayLocationFact = GetPutawayLocationFact();

			var invMock1 = new Mock<IInventoryFact>();
			invMock1.Setup(inv => inv.Product).Returns(CreateProductJoin(relationFK));
			invMock1.Setup(inv => inv.PartAttribute1).Returns("PA1");
			invMock1.Setup(inv => inv.PartAttribute2).Returns("PA2");
			invMock1.Setup(inv => inv.PartAttribute3).Returns("PA3");
			invMock1.Setup(inv => inv.PackingDate).Returns(ZDateTime.BrettsBirthday.ToDateTime());
			invMock1.Setup(inv => inv.ExpiryDate).Returns(ZDateTime.BrettsBirthday.AddYears(1).ToDateTime());

			var invMock2 = new Mock<IInventoryFact>();
			invMock2.Setup(inv => inv.Product).Returns(CreateProductJoin(relationFK));
			invMock2.Setup(inv => inv.PartAttribute1).Returns("PA1");
			invMock2.Setup(inv => inv.PartAttribute2).Returns("PA2");
			invMock2.Setup(inv => inv.PartAttribute3).Returns("PA3");
			invMock2.Setup(inv => inv.PackingDate).Returns(ZDateTime.BrettsBirthday.ToDateTime());
			invMock2.Setup(inv => inv.ExpiryDate).Returns(ZDateTime.BrettsBirthday.AddYears(1).ToDateTime());

			AssertEquals("Precondition: Should *not* contain other attribute combinations.", false, putawayLocationFact.ContainsOtherAttributeCombinations(new[] { invMock1.Object, invMock2.Object }));

			setToOtherValue(invMock2);
			AssertEquals("Should contain other attribute combinations.", true, putawayLocationFact.ContainsOtherAttributeCombinations(new[] { invMock1.Object, invMock2.Object }));
		}

		public void TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_Empty_PA1() => TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_Empty(invMock => invMock.Setup(inv => inv.PartAttribute1).Returns(string.Empty));

		public void TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_Empty_PA2() => TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_Empty(invMock => invMock.Setup(inv => inv.PartAttribute2).Returns(string.Empty));

		public void TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_Empty_PA3() => TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_Empty(invMock => invMock.Setup(inv => inv.PartAttribute3).Returns(string.Empty));

		public void TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_Empty_ExpiryDate()
			=> TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_Empty(invMock => invMock.Setup(inv => inv.ExpiryDate).Returns((DateTime?)null));

		public void TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_Empty_PackingDate()
			=> TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_Empty(invMock => invMock.Setup(inv => inv.PackingDate).Returns((DateTime?)null));

		public void TestContainsOtherAttributeCombinations_Enumerable_CheckForConsistencyOnPallet_Empty(Action<Mock<IInventoryFact>> setToEmptyValue)
		{
			var relationFK = Guid.NewGuid();
			var putawayLocationFact = GetPutawayLocationFact();

			var invMock1 = new Mock<IInventoryFact>();
			invMock1.Setup(inv => inv.Product).Returns(CreateProductJoin(relationFK));
			invMock1.Setup(inv => inv.PartAttribute1).Returns("PA1");
			invMock1.Setup(inv => inv.PartAttribute2).Returns("PA2");
			invMock1.Setup(inv => inv.PartAttribute3).Returns("PA3");
			invMock1.Setup(inv => inv.PackingDate).Returns(ZDateTime.BrettsBirthday.ToDateTime());
			invMock1.Setup(inv => inv.ExpiryDate).Returns(ZDateTime.BrettsBirthday.AddYears(1).ToDateTime());

			var invMock2 = new Mock<IInventoryFact>();
			invMock2.Setup(inv => inv.Product).Returns(CreateProductJoin(relationFK));
			invMock2.Setup(inv => inv.PartAttribute1).Returns("PA1");
			invMock2.Setup(inv => inv.PartAttribute2).Returns("PA2");
			invMock2.Setup(inv => inv.PartAttribute3).Returns("PA3");
			invMock2.Setup(inv => inv.PackingDate).Returns(ZDateTime.BrettsBirthday.ToDateTime());
			invMock2.Setup(inv => inv.ExpiryDate).Returns(ZDateTime.BrettsBirthday.AddYears(1).ToDateTime());

			AssertEquals("Precondition: Should *not* contain other attribute combinations.", false, putawayLocationFact.ContainsOtherAttributeCombinations(new[] { invMock1.Object, invMock2.Object }));

			setToEmptyValue(invMock2);
			AssertEquals("Should *not* contain other attribute combinations.", false, putawayLocationFact.ContainsOtherAttributeCombinations(new[] { invMock1.Object, invMock2.Object }));
		}

		string GetLocationCacheAttributeData<T>(Guid productPK, T value, int numberOfAttributes) => GetLocationCacheAttributeData((productPK, value, numberOfAttributes));

		string GetLocationCacheAttributeData<T>(params (Guid productPK, T value, int numberOfAttributes)[] attributeDatas)
		{
			var dict = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();

			foreach (var (productPK, value, numberOfAttributes) in attributeDatas)
			{
				var attributeData = new WhsPutawayLocationCacheAttributeData<T>();
				attributeData.Attribute = value;
				attributeData.NumberOfAttributes = numberOfAttributes;

				dict[productPK] = attributeData;
			}

			return JsonConvert.SerializeObject(dict);
		}

		#endregion

		#region TestNumberOfOtherProducts

		public void TestNumberOfOtherProducts_ContainsProduct() => TestNumberOfOtherProducts(true);
		public void TestContainsThisProduct_DoesNotContainProduct() => TestNumberOfOtherProducts(false);

		void TestNumberOfOtherProducts(bool containsThisProduct)
		{
			var relation1FK = Guid.NewGuid();
			var relation2FK = Guid.NewGuid();
			var relation3FK = Guid.NewGuid();

			var relationsHashSet = new HashSet<Guid>(new[] { relation2FK });
			if (containsThisProduct)
			{
				relationsHashSet.Add(relation1FK);
			}

			var productsDataJson = JsonConvert.SerializeObject(relationsHashSet);
			var putawayLocationFact = GetPutawayLocationFact(productData: productsDataJson);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));

			var inv3 = new Mock<IInventoryFact>();
			inv3.Setup(inv => inv.Product).Returns(CreateProductJoin(relation3FK));

			AssertEquals("Should contain one other product.", 1, putawayLocationFact.NumberOfOtherProducts(inv1.Object));
			AssertEquals(nameof(IPutawayLocationFact.NumberOfOtherProducts), containsThisProduct ? 1 : 0, putawayLocationFact.NumberOfOtherProducts(inv2.Object));
			AssertEquals(nameof(IPutawayLocationFact.NumberOfOtherProducts), containsThisProduct ? 2 : 1, putawayLocationFact.NumberOfOtherProducts(inv3.Object));
		}

		#endregion

		#region TestNumberOfOtherProducts_EmptyJson

		public void TestNumberOfOtherProducts_EmptyJson()
		{
			var relation1FK = Guid.NewGuid();
			var putawayLocationFact = GetPutawayLocationFact(productData: "[]");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));

			AssertEquals(nameof(IPutawayLocationFact.NumberOfOtherProducts), 0, putawayLocationFact.NumberOfOtherProducts(inv1.Object));
		}

		#endregion

		#region TestNumberOfOtherProducts_IEnumerableOverload

		public void TestNumberOfOtherProducts_IEnumerableOverload()
		{
			var relation1FK = Guid.NewGuid();
			var relation2FK = Guid.NewGuid();
			var relation3FK = Guid.NewGuid();

			var productsDataJson = JsonConvert.SerializeObject(new HashSet<Guid>(new[] { relation1FK, relation2FK }));
			var putawayLocationFact = GetPutawayLocationFact(productData: productsDataJson);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));

			var inv3 = new Mock<IInventoryFact>();
			inv3.Setup(inv => inv.Product).Returns(CreateProductJoin(relation3FK));

			AssertEquals(nameof(IPutawayLocationFact.NumberOfOtherProducts), 0, putawayLocationFact.NumberOfOtherProducts(new[] { inv1.Object, inv2.Object }));
			AssertEquals(nameof(IPutawayLocationFact.NumberOfOtherProducts), 0, putawayLocationFact.NumberOfOtherProducts(new[] { inv1.Object, inv2.Object, inv3.Object }));
			AssertEquals(nameof(IPutawayLocationFact.NumberOfOtherProducts), 1, putawayLocationFact.NumberOfOtherProducts(new[] { inv1.Object, inv1.Object }));
			AssertEquals(nameof(IPutawayLocationFact.NumberOfOtherProducts), 1, putawayLocationFact.NumberOfOtherProducts(new[] { inv2.Object, inv3.Object }));
			AssertEquals(nameof(IPutawayLocationFact.NumberOfOtherProducts), 2, putawayLocationFact.NumberOfOtherProducts(new[] { inv3.Object }));
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_InvalidPutawayQty

		public void TestUpdateDataAfterPutawayAllocation_InvalidPutawayQty()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableUnits: 5m, maxUnits: 10m);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertExceptionThrown<ArgumentException>(() => putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 0m));
			AssertExceptionThrown<ArgumentException>(() => putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, -1m));
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableUnits

		public void TestUpdateDataAfterPutawayAllocation_AvailableUnits()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableUnits: 5m, maxUnits: 10m);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: AvailableUnits.", 5m, putawayLocationFact.AvailableUnits);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should have updated AvailableUnits.", 3m, putawayLocationFact.AvailableUnits);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableUnits_NoMaxQuantitySpecified

		public void TestUpdateDataAfterPutawayAllocation_AvailableUnits_NoMaxQuantitySpecified()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableUnits: 5m);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			AssertEquals("Precondition: AvailableUnits.", 5m, putawayLocationFact.AvailableUnits);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should *not* have updated AvailableUnits.", 5m, putawayLocationFact.AvailableUnits);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableWeight

		public void TestUpdateDataAfterPutawayAllocation_AvailableWeight()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableWeight: 30m, maxWeight: 40m, weightUQ: "KG");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			AssertEquals("Precondition: AvailableWeight.", 30m, putawayLocationFact.AvailableWeight);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should have updated AvailableWeight.", 10m, putawayLocationFact.AvailableWeight);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableWeight_RequiresConversion

		public void TestUpdateDataAfterPutawayAllocation_AvailableWeight_RequiresConversion()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableWeight: 20m, maxWeight: 30m, weightUQ: "KG");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(22m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("LB"); // 44lbs = 19.9581 kg

			AssertEquals("Precondition: AvailableWeight.", 20m, putawayLocationFact.AvailableWeight);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should have updated AvailableWeight.", 0.041936m, putawayLocationFact.AvailableWeight);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableWeight_NoMaxWeightSpecified

		public void TestUpdateDataAfterPutawayAllocation_AvailableWeight_NoMaxWeightSpecified()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableWeight: 20m, maxWeight: 0m, weightUQ: "KG");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			AssertEquals("Precondition: AvailableWeight.", 20m, putawayLocationFact.AvailableWeight);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should *not* have updated AvailableWeight.", 20m, putawayLocationFact.AvailableWeight);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableWeight_NoMaxWeightUQSpecified

		public void TestUpdateDataAfterPutawayAllocation_AvailableWeight_NoMaxWeightUQSpecified()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableWeight: 20m, maxWeight: 30m, weightUQ: "");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			AssertEquals("Precondition: AvailableWeight.", 20m, putawayLocationFact.AvailableWeight);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should *not* have updated AvailableWeight.", 20m, putawayLocationFact.AvailableWeight);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableWeight_NoProductWeightSpecified

		public void TestUpdateDataAfterPutawayAllocation_AvailableWeight_NoProductWeightSpecified()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableWeight: 20m, maxWeight: 30m, weightUQ: "KG");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(0m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			AssertEquals("Precondition: AvailableWeight.", 20m, putawayLocationFact.AvailableWeight);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should *not* have updated AvailableWeight.", 20m, putawayLocationFact.AvailableWeight);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableWeight_NoProductWeightUQSpecified

		public void TestUpdateDataAfterPutawayAllocation_AvailableWeight_NoProductWeightUQSpecified()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableWeight: 20m, maxWeight: 30m, weightUQ: "KG");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("");

			AssertEquals("Precondition: AvailableWeight.", 20m, putawayLocationFact.AvailableWeight);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should *not* have updated AvailableWeight.", 20m, putawayLocationFact.AvailableWeight);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableVolume

		public void TestUpdateDataAfterPutawayAllocation_AvailableVolume()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableVolume: 15m, maxVolume: 20m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: AvailableVolume.", 15m, putawayLocationFact.AvailableVolume);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should have updated AvailableVolume.", 5m, putawayLocationFact.AvailableVolume);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableVolume_RequiresConversion

		public void TestUpdateDataAfterPutawayAllocation_AvailableVolume_RequiresConversion()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableVolume: 20m, maxVolume: 25m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductVolume).Returns(13m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("CY"); // 26CY = 19.8784M3

			AssertEquals("Precondition: AvailableVolume.", 20m, putawayLocationFact.AvailableVolume);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should have updated AvailableVolume.", 0.121574m, putawayLocationFact.AvailableVolume);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableVolume_NoMaxVolumeSpecified

		public void TestUpdateDataAfterPutawayAllocation_AvailableVolume_NoMaxVolumeSpecified()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableVolume: 20m, maxVolume: 0m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: AvailableVolume.", 20m, putawayLocationFact.AvailableVolume);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should *not* have updated AvailableVolume.", 20m, putawayLocationFact.AvailableVolume);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableVolume_NoMaxVolumeUQSpecified

		public void TestUpdateDataAfterPutawayAllocation_AvailableVolume_NoMaxVolumeUQSpecified()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableVolume: 20m, maxVolume: 15m, volumeUQ: "");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: AvailableVolume.", 20m, putawayLocationFact.AvailableVolume);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should *not* have updated AvailableVolume.", 20m, putawayLocationFact.AvailableVolume);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableVolume_NoProductVolumeSpecified

		public void TestUpdateDataAfterPutawayAllocation_AvailableVolume_NoProductVolumeSpecified()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableVolume: 20m, maxVolume: 15m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductVolume).Returns(0m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: AvailableVolume.", 20m, putawayLocationFact.AvailableVolume);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should *not* have updated AvailableVolume.", 20m, putawayLocationFact.AvailableVolume);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_AvailableVolume_NoProductVolumeUQSpecified

		public void TestUpdateDataAfterPutawayAllocation_AvailableVolume_NoProductVolumeUQSpecified()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableVolume: 20m, maxVolume: 15m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("");

			AssertEquals("Precondition: AvailableVolume.", 20m, putawayLocationFact.AvailableVolume);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should *not* have updated AvailableVolume.", 20m, putawayLocationFact.AvailableVolume);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_WithParentLocation

		public void TestUpdateDataAfterPutawayAllocation_WithParentLocation()
		{
			var parentLocation = GetPutawayLocationFact(availableUnits: 20m, maxUnits: 50m, availableWeight: 100m, maxWeight: 60m, weightUQ: "KG", availableVolume: 70m, maxVolume: 80m, currentAndIncomingStock: 10m, volumeUQ: "M3");
			var putawayLocationFact = GetPutawayLocationFact(availableUnits: 8m, maxUnits: 10m, availableWeight: 50m, maxWeight: 60m, weightUQ: "KG", availableVolume: 120m, maxVolume: 80m, currentAndIncomingStock: 1m, volumeUQ: "M3", parentLocation: parentLocation);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));
			inv1.Setup(inv => inv.ProductWeight).Returns(7m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");
			inv1.Setup(inv => inv.ProductVolume).Returns(3m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: AvailableUnits.", 8m, putawayLocationFact.AvailableUnits);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("Should have updated CurrentAndIncomingStock on the current location.", 3m, putawayLocationFact.CurrentAndIncomingStock);
			AssertEquals("Should have updated AvailableUnits on the current location.", 6m, putawayLocationFact.AvailableUnits);
			AssertEquals("Should have updated AvailableWeight on the current location.", 86m, putawayLocationFact.AvailableWeight);
			AssertEquals("Should have updated AvailableVolume on the current location.", 64m, putawayLocationFact.AvailableVolume);

			AssertEquals("Should have updated CurrentAndIncomingStock on the current location.", 12m, parentLocation.CurrentAndIncomingStock);
			AssertEquals("Should have updated AvailableUnits on the parent location.", 18m, parentLocation.AvailableUnits);
			AssertEquals("Should have updated AvailableWeight on the parent location.", 86m, parentLocation.AvailableWeight);
			AssertEquals("Should have updated AvailableVolume on the parent location.", 64m, parentLocation.AvailableVolume);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_Enumerable

		public void TestUpdateDataAfterPutawayAllocation_Enumerable()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableUnits: 8m, maxUnits: 10m, availableWeight: 50m, maxWeight: 60m, weightUQ: "KG", availableVolume: 70m, maxVolume: 80m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(2m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));
			inv1.Setup(inv => inv.ProductWeight).Returns(7m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");
			inv1.Setup(inv => inv.ProductVolume).Returns(3m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(3m);
			inv2.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));
			inv2.Setup(inv => inv.ProductWeight).Returns(6m);
			inv2.Setup(inv => inv.ProductWeightUQ).Returns("KG");
			inv2.Setup(inv => inv.ProductVolume).Returns(8m);
			inv2.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: AvailableUnits.", 8m, putawayLocationFact.AvailableUnits);
			AssertEquals("Precondition: AvailableWeight.", 50m, putawayLocationFact.AvailableWeight);
			AssertEquals("Precondition: AvailableVolume.", 70m, putawayLocationFact.AvailableVolume);

			putawayLocationFact.UpdateDataAfterPutawayAllocation(new[] { inv1.Object, inv2.Object });
			AssertEquals("Should have updated AvailableUnits.", 3m, putawayLocationFact.AvailableUnits);
			AssertEquals("Should have updated AvailableWeight.", 18m, putawayLocationFact.AvailableWeight);
			AssertEquals("Should have updated AvailableVolume.", 40m, putawayLocationFact.AvailableVolume);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_Enumerable_WithParentLocation

		public void TestUpdateDataAfterPutawayAllocation_Enumerable_WithParentLocation()
		{
			var parentLocation = GetPutawayLocationFact(availableUnits: 20m, maxUnits: 50m, availableWeight: 100m, maxWeight: 60m, weightUQ: "KG", availableVolume: 70m, maxVolume: 80m, volumeUQ: "M3");
			var putawayLocationFact = GetPutawayLocationFact(availableUnits: 8m, maxUnits: 10m, availableWeight: 50m, maxWeight: 60m, weightUQ: "KG", availableVolume: 70m, maxVolume: 80m, volumeUQ: "M3", parentLocation: parentLocation);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(2m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));
			inv1.Setup(inv => inv.ProductWeight).Returns(7m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");
			inv1.Setup(inv => inv.ProductVolume).Returns(3m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(3m);
			inv2.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));
			inv2.Setup(inv => inv.ProductWeight).Returns(6m);
			inv2.Setup(inv => inv.ProductWeightUQ).Returns("KG");
			inv2.Setup(inv => inv.ProductVolume).Returns(8m);
			inv2.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: AvailableUnits.", 8m, putawayLocationFact.AvailableUnits);
			AssertEquals("Precondition: AvailableWeight.", 100m, putawayLocationFact.AvailableWeight);
			AssertEquals("Precondition: AvailableVolume.", 70m, putawayLocationFact.AvailableVolume);

			putawayLocationFact.UpdateDataAfterPutawayAllocation(new[] { inv1.Object, inv2.Object });
			AssertEquals("Should have updated AvailableUnits on the current location.", 3m, putawayLocationFact.AvailableUnits);
			AssertEquals("Should have updated AvailableWeight on the current location.", 68m, putawayLocationFact.AvailableWeight);
			AssertEquals("Should have updated AvailableVolume on the current location.", 40m, putawayLocationFact.AvailableVolume);

			AssertEquals("Should have updated AvailableUnits on the parent location.", 15m, parentLocation.AvailableUnits);
			AssertEquals("Should have updated AvailableWeight on the parent location.", 68m, parentLocation.AvailableWeight);
			AssertEquals("Should have updated AvailableVolume on the parent location.", 40m, parentLocation.AvailableVolume);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_CurrentAndIncomingStock

		public void TestUpdateDataAfterPutawayAllocation_CurrentAndIncomingStock()
		{
			var putawayLocationFact = GetPutawayLocationFact(currentAndIncomingStock: 0m);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: CurrentAndIncomingStock.", 0m, putawayLocationFact.CurrentAndIncomingStock);
			AssertEquals("Precondition: IsEmpty.", true, putawayLocationFact.IsEmpty());

			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("CurrentAndIncomingStock.", 2m, putawayLocationFact.CurrentAndIncomingStock);
			AssertEquals("IsEmpty.", false, putawayLocationFact.IsEmpty());

			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 3m);
			AssertEquals("CurrentAndIncomingStock.", 5m, putawayLocationFact.CurrentAndIncomingStock);
			AssertEquals("IsEmpty.", false, putawayLocationFact.IsEmpty());
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_Enumerable_CurrentAndIncomingStock

		public void TestUpdateDataAfterPutawayAllocation_Enumerable_CurrentAndIncomingStock()
		{
			var putawayLocationFact = GetPutawayLocationFact(currentAndIncomingStock: 0m);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(2m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(3m);
			inv2.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv2.Setup(inv => inv.ProductWeight).Returns(10m);
			inv2.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			inv2.Setup(inv => inv.ProductVolume).Returns(5m);
			inv2.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: CurrentAndIncomingStock.", 0m, putawayLocationFact.CurrentAndIncomingStock);
			AssertEquals("Precondition: IsEmpty.", true, putawayLocationFact.IsEmpty());

			putawayLocationFact.UpdateDataAfterPutawayAllocation(new[] { inv1.Object, inv2.Object });
			AssertEquals("CurrentAndIncomingStock.", 5m, putawayLocationFact.CurrentAndIncomingStock);
			AssertEquals("IsEmpty.", false, putawayLocationFact.IsEmpty());
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_IsFixedPickFaceFull

		public void TestUpdateDataAfterPutawayAllocation_IsFixedPickFaceFull()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableUnits: 5m, maxUnits: 10m, isFixedPickFace: true);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: IsFixedPickFaceFull.", false, putawayLocationFact.IsFixedPickFaceFull);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("IsFixedPickFaceFull.", false, putawayLocationFact.IsFixedPickFaceFull);

			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 3m);
			AssertEquals("IsFixedPickFaceFull.", true, putawayLocationFact.IsFixedPickFaceFull);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_IsFixedPickFaceFull_DynamicPickFace

		public void TestUpdateDataAfterPutawayAllocation_IsFixedPickFaceFull_DynamicPickFace()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableUnits: 5m, maxUnits: 10m, isDynamicPickFace: true);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: IsFixedPickFaceFull.", false, putawayLocationFact.IsFixedPickFaceFull);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("IsFixedPickFaceFull.", false, putawayLocationFact.IsFixedPickFaceFull);

			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 3m);
			AssertEquals("IsFixedPickFaceFull.", false, putawayLocationFact.IsFixedPickFaceFull);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_PalletQuantity

		public void TestUpdateDataAfterPutawayAllocation_PalletQuantity()
		{
			var putawayLocationFact = GetPutawayLocationFact(palletQuantity: 12);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(5m);
			inv2.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			AssertEquals("Precondition: PalletQuantity.", 12, putawayLocationFact.PalletQuantity);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(new[] { inv1.Object });
			AssertEquals("PalletQuantity.", 13, putawayLocationFact.PalletQuantity);

			putawayLocationFact.UpdateDataAfterPutawayAllocation(new[] { inv2.Object });
			AssertEquals("PalletQuantity.", 14, putawayLocationFact.PalletQuantity);
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_IsFixedPickFaceFull_RegularLocation

		public void TestUpdateDataAfterPutawayAllocation_IsFixedPickFaceFull_RegularLocation()
		{
			var putawayLocationFact = GetPutawayLocationFact(availableUnits: 5m, maxUnits: 10m);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(Mock.Of<IPutawayProductFact>()));

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertEquals("Precondition: IsFixedPickFaceFull.", false, putawayLocationFact.IsFixedPickFaceFull);
			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 2m);
			AssertEquals("IsFixedPickFaceFull.", false, putawayLocationFact.IsFixedPickFaceFull);

			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv1.Object, 3m);
			AssertEquals("IsFixedPickFaceFull.", false, putawayLocationFact.IsFixedPickFaceFull);
		}

		#endregion

		#region UpdateDataAfterPutawayAllocation_ContainsThisProduct_SingleInventory

		public void TestUpdateDataAfterPutawayAllocation_ContainsThisProduct_SingleInventory_Initialised()
			=> TestUpdateDataAfterPutawayAllocation_ContainsThisProduct_SingleInventory(true);

		public void TestUpdateDataAfterPutawayAllocation_ContainsThisProduct_SingleInventory_NotInitialised()
			=> TestUpdateDataAfterPutawayAllocation_ContainsThisProduct_SingleInventory(false);

		void TestUpdateDataAfterPutawayAllocation_ContainsThisProduct_SingleInventory(bool initialised)
		{
			var relation1FK = Guid.NewGuid();
			var relation2FK = Guid.NewGuid();

			var relationsHashSet = new HashSet<Guid>(new[] { relation1FK });

			var productsDataJson = JsonConvert.SerializeObject(relationsHashSet);
			var putawayLocationFact = GetPutawayLocationFact(productData: productsDataJson);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));
			inv1.Setup(inv => inv.Quantity).Returns(1m);

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));
			inv2.Setup(inv => inv.Quantity).Returns(1m);

			if (initialised)
			{
				AssertEquals("Precondition.", true, putawayLocationFact.ContainsThisProduct(inv1.Object));
				AssertEquals("Precondition.", false, putawayLocationFact.ContainsThisProduct(inv2.Object));
			}

			putawayLocationFact.UpdateDataAfterPutawayAllocation(new[] { inv2.Object });
			AssertEquals("Should contain this product.", true, putawayLocationFact.ContainsThisProduct(inv1.Object));
			AssertEquals("Should contain this product.", true, putawayLocationFact.ContainsThisProduct(inv2.Object));
		}

		#endregion

		#region UpdateDataAfterPutawayAllocation_ContainsThisProduct_WithParentLocation

		public void UpdateDataAfterPutawayAllocation_ContainsThisProduct_WithParentLocation()
		{
			var relation1FK = Guid.NewGuid();
			var relation2FK = Guid.NewGuid();

			var relationsHashSet = new HashSet<Guid>(new[] { relation1FK });

			var productsDataJson = JsonConvert.SerializeObject(relationsHashSet);
			var parentLocationFact = GetPutawayLocationFact(productData: productsDataJson);
			var putawayLocationFact = GetPutawayLocationFact(productData: productsDataJson, parentLocation: parentLocationFact);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));
			inv1.Setup(inv => inv.Quantity).Returns(1m);

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));
			inv2.Setup(inv => inv.Quantity).Returns(1m);

			AssertEquals("Precondition.", true, putawayLocationFact.ContainsThisProduct(inv1.Object));
			AssertEquals("Precondition.", true, parentLocationFact.ContainsThisProduct(inv1.Object));

			AssertEquals("Precondition.", false, putawayLocationFact.ContainsThisProduct(inv2.Object));
			AssertEquals("Precondition.", false, parentLocationFact.ContainsThisProduct(inv2.Object));

			putawayLocationFact.UpdateDataAfterPutawayAllocation(new[] { inv2.Object });
			AssertEquals("Should contain this product.", true, putawayLocationFact.ContainsThisProduct(inv1.Object));
			AssertEquals("Should contain this product.", true, putawayLocationFact.ContainsThisProduct(inv2.Object));

			AssertEquals("Should contain this product.", true, parentLocationFact.ContainsThisProduct(inv1.Object));
			AssertEquals("Should contain this product.", true, parentLocationFact.ContainsThisProduct(inv2.Object));
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_ContainsThisProduct_MultipleInventories

		public void TestUpdateDataAfterPutawayAllocation_ContainsThisProduct_MultipleInventories()
		{
			var relation1FK = Guid.NewGuid();
			var relation2FK = Guid.NewGuid();
			var putawayLocationFact = GetPutawayLocationFact();

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));
			inv1.Setup(inv => inv.Quantity).Returns(1m);

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));
			inv2.Setup(inv => inv.Quantity).Returns(1m);

			putawayLocationFact.UpdateDataAfterPutawayAllocation(new[] { inv1.Object, inv2.Object });
			AssertEquals("Should contain this product.", true, putawayLocationFact.ContainsThisProduct(inv1.Object));
			AssertEquals("Should contain this product.", true, putawayLocationFact.ContainsThisProduct(inv2.Object));
		}

		#endregion

		#region UpdateDataAfterPutawayAllocation_NumberOfOtherProducts

		public void TestUpdateDataAfterPutawayAllocation_NumberOfOtherProducts_Initialised()
			=> TestUpdateDataAfterPutawayAllocation_NumberOfOtherProducts(true);

		public void TestUpdateDataAfterPutawayAllocation_NumberOfOtherProducts_NotInitialised()
			=> TestUpdateDataAfterPutawayAllocation_NumberOfOtherProducts(false);

		void TestUpdateDataAfterPutawayAllocation_NumberOfOtherProducts(bool initialised)
		{
			var relation1FK = Guid.NewGuid();
			var relation2FK = Guid.NewGuid();

			var relationsHashSet = new HashSet<Guid>(new[] { relation1FK });

			var productsDataJson = JsonConvert.SerializeObject(relationsHashSet);
			var putawayLocationFact = GetPutawayLocationFact(productData: productsDataJson);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));

			if (initialised)
			{
				AssertEquals("Precondition.", 0, putawayLocationFact.NumberOfOtherProducts(inv1.Object));
				AssertEquals("Precondition.", 1, putawayLocationFact.NumberOfOtherProducts(inv2.Object));
			}

			putawayLocationFact.UpdateDataAfterPutawayAllocation(inv2.Object, 1m);
			AssertEquals("Should contain one other product.", 1, putawayLocationFact.NumberOfOtherProducts(inv1.Object));
			AssertEquals("Should contain one other product.", 1, putawayLocationFact.NumberOfOtherProducts(inv2.Object));
		}

		#endregion

		#region UpdateDataAfterPutawayAllocation_Enumerable_NumberOfOtherProducts_SingleInventory

		public void TestUpdateDataAfterPutawayAllocation_Enumerable_NumberOfOtherProducts_SingleInventory_Initialised()
			=> TestUpdateDataAfterPutawayAllocation_Enumerable_NumberOfOtherProducts_SingleInventory(true);

		public void TestUpdateDataAfterPutawayAllocation_Enumerable_NumberOfOtherProducts_SingleInventory_NotInitialised()
			=> TestUpdateDataAfterPutawayAllocation_Enumerable_NumberOfOtherProducts_SingleInventory(false);

		void TestUpdateDataAfterPutawayAllocation_Enumerable_NumberOfOtherProducts_SingleInventory(bool initialised)
		{
			var relation1FK = Guid.NewGuid();
			var relation2FK = Guid.NewGuid();

			var relationsHashSet = new HashSet<Guid>(new[] { relation1FK });

			var productsDataJson = JsonConvert.SerializeObject(relationsHashSet);
			var putawayLocationFact = GetPutawayLocationFact(productData: productsDataJson);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(1m);
			inv1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(1m);
			inv2.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));

			if (initialised)
			{
				AssertEquals("Precondition.", 0, putawayLocationFact.NumberOfOtherProducts(inv1.Object));
				AssertEquals("Precondition.", 1, putawayLocationFact.NumberOfOtherProducts(inv2.Object));
			}

			putawayLocationFact.UpdateDataAfterPutawayAllocation(new[] { inv2.Object });
			AssertEquals("Should contain one other product.", 1, putawayLocationFact.NumberOfOtherProducts(inv1.Object));
			AssertEquals("Should contain one other product.", 1, putawayLocationFact.NumberOfOtherProducts(inv2.Object));
		}

		#endregion

		#region TestUpdateDataAfterPutawayAllocation_Enumerable_NumberOfOtherProducts_MultipleInventories

		public void TestUpdateDataAfterPutawayAllocation_Enumerable_NumberOfOtherProducts_MultipleInventories()
		{
			var relation1FK = Guid.NewGuid();
			var relation2FK = Guid.NewGuid();
			var putawayLocationFact = GetPutawayLocationFact();

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Product).Returns(CreateProductJoin(relation1FK));
			inv1.Setup(inv => inv.Quantity).Returns(1m);

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));
			inv2.Setup(inv => inv.Quantity).Returns(1m);

			var inv3 = new Mock<IInventoryFact>();
			inv3.Setup(inv => inv.Product).Returns(CreateProductJoin(relation2FK));
			inv3.Setup(inv => inv.Quantity).Returns(1m);

			putawayLocationFact.UpdateDataAfterPutawayAllocation(new[] { inv1.Object, inv2.Object });
			AssertEquals("Should contain one other product.", 1, putawayLocationFact.NumberOfOtherProducts(inv1.Object));
			AssertEquals("Should contain one other product.", 1, putawayLocationFact.NumberOfOtherProducts(inv2.Object));
		}

		#endregion

		#region CanTransferToOrFromVASServiceArea

		public void TestCanTransferToOrFromVASServiceArea_FreeStore()
		{
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.FreeStore, AreaTypes.Codes.FreeStore, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.FreeStore, AreaTypes.Codes.DynamicPickFace, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.FreeStore, AreaTypes.Codes.Bonded, false);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.FreeStore, AreaTypes.Codes.Excise, false);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.FreeStore, AreaTypes.Codes.DockDoor, false);
		}

		public void TestCanTransferToOrFromVASServiceArea_Bonded()
		{
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.Bonded, AreaTypes.Codes.Bonded, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.Bonded, AreaTypes.Codes.Excise, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.Bonded, AreaTypes.Codes.DockDoor, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.Bonded, AreaTypes.Codes.FreeStore, false);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.Bonded, AreaTypes.Codes.DynamicPickFace, false);
		}

		public void TestCanTransferToOrFromVASServiceArea_Excise()
		{
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.Excise, AreaTypes.Codes.Excise, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.Excise, AreaTypes.Codes.Bonded, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.Excise, AreaTypes.Codes.DockDoor, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.Excise, AreaTypes.Codes.FreeStore, false);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.Excise, AreaTypes.Codes.DynamicPickFace, false);
		}

		public void TestCanTransferToOrFromVASServiceArea_DynamicPickFace()
		{
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.DynamicPickFace, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.DockDoor, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.FreeStore, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.Bonded, false);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.Excise, false);
		}

		public void TestCanTransferToOrFromVASServiceArea_DockDoor()
		{
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.DockDoor, AreaTypes.Codes.DockDoor, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.DockDoor, AreaTypes.Codes.DynamicPickFace, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.DockDoor, AreaTypes.Codes.Bonded, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.DockDoor, AreaTypes.Codes.Excise, true);
			TestCanTransferToOrFromVASServiceArea(AreaTypes.Codes.DockDoor, AreaTypes.Codes.FreeStore, false);
		}

		void TestCanTransferToOrFromVASServiceArea(string vasOrderServiceAreaType, string locationAreaType, bool expectedValue)
		{
			var putawayLocationFact = GetPutawayLocationFact(areaTypeCode: locationAreaType);

			var inventoryFact = new Mock<IInventoryFact>();
			inventoryFact.Setup(inv => inv.VASServiceAreaTypeCode).Returns(vasOrderServiceAreaType);

			AssertEquals(expectedValue, putawayLocationFact.CanTransferToOrFromVASServiceArea(inventoryFact.Object));
		}

		#endregion

		#region Implementation

		protected FactJoin<IPutawayProductFact> CreateProductJoin(Guid productPK)
		{
			var productMock = new Mock<IPutawayProductFact>();
			productMock.Setup(p => p.PK).Returns(productPK);
			return new FactJoin<IPutawayProductFact>(productMock.Object);
		}

		internal static PutawayLocationFact GetPutawayLocationFact(
			string productData = null,
			string pa1Data = null,
			string pa2Data = null,
			string pa3Data = null,
			string expiryData = null,
			string packingData = null,
			decimal availableWeight = 0m,
			decimal maxWeight = 0m,
			string weightUQ = "KG",
			decimal availableVolume = 0m,
			decimal maxVolume = 0m,
			string volumeUQ = "M3",
			decimal availableUnits = 0m,
			decimal maxUnits = 0m,
			decimal currentAndIncomingStock = 9.81m,
			bool isDynamicPickFace = false,
			bool isFixedPickFace = false,
			bool isFixedPickFaceFull = false,
			bool isPartialPallet = false,
			string locationStatus = null,
			string areaTypeCode = null,
			PutawayLocationFact parentLocation = null,
			int palletSpaces = 20,
			int palletQuantity = 18,
			Guid? lastAllocatedOrChangedID = null)
		{
			return new PutawayLocationFact(
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				"TYP",
				"NOR",
				"PUT",
				"A",
				1,
				2,
				3,
				4,
				locationStatus ?? "NOR",
				areaTypeCode ?? "FRE",
				availableWeight,
				maxWeight,
				weightUQ,
				availableVolume,
				maxVolume,
				volumeUQ,
				availableUnits,
				maxUnits,
				currentAndIncomingStock,
				isPartialPallet,
				"PLT-123",
				isDynamicPickFace,
				isFixedPickFace,
				isFixedPickFaceFull,
				Guid.NewGuid(),
				Guid.NewGuid(),
				false,
				productData ?? "[]",
				pa1Data ?? "{}",
				pa2Data ?? "{}",
				pa3Data ?? "{}",
				expiryData ?? "{}",
				packingData ?? "{}",
				palletSpaces,
				palletQuantity,
				parentLocation,
				lastAllocatedOrChangedID);
		}

		#endregion
	}

	class PutawayLocationFactStockQuantityThatCanBePutawayTest : TestCase
	{
		#region TestBaseCase

		public void TestBaseCase()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact();

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			AssertCanPutawayFunction(5m, putawayLocationFact, inv1.Object);

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(10m);
			AssertCanPutawayFunction(10m, putawayLocationFact, inv2.Object);
		}

		#endregion

		#region TestBaseCase_GenerousLimits

		public void TestBaseCase_GenerousLimits()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableUnits: 50m, maxUnits: 100m, availableWeight: 300m, maxWeight: 300m, weightUQ: "KG", availableVolume: 150m, maxVolume: 150m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertCanPutawayFunction(5m, putawayLocationFact, inv1.Object);

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(10m);
			AssertCanPutawayFunction(10m, putawayLocationFact, inv2.Object);
		}

		#endregion

		#region TestQuantityLimited

		public void TestQuantityLimited()
		{
			var putawayLocationFact_JustQuantity = PutawayLocationFactTest.GetPutawayLocationFact(availableUnits: 2m, maxUnits: 10m);
			var putawayLocationFact_WithOtherConstraints = PutawayLocationFactTest.GetPutawayLocationFact(availableUnits: 2m, maxUnits: 10m, availableWeight: 30m, maxWeight: 30m, weightUQ: "KG", availableVolume: 15m, maxVolume: 15m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertCanPutawayFunction(2m, putawayLocationFact_JustQuantity, inv1.Object);
			AssertCanPutawayFunction(2m, putawayLocationFact_WithOtherConstraints, inv1.Object);
		}

		#endregion

		#region TestQuantityLimited_WithParentLocation

		public void TestQuantityLimited_WithParentLocation()
		{
			var parentLocation = PutawayLocationFactTest.GetPutawayLocationFact(availableUnits: 2m, maxUnits: 10m);
			var locationFact1 = PutawayLocationFactTest.GetPutawayLocationFact(availableUnits: 5m, maxUnits: 10m, parentLocation: parentLocation);
			var locationFact2 = PutawayLocationFactTest.GetPutawayLocationFact(availableUnits: 1m, maxUnits: 10m, parentLocation: parentLocation);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertCanPutawayFunction(2m, locationFact1, inv1.Object);
			AssertCanPutawayFunction(1m, locationFact2, inv1.Object);
		}

		#endregion

		#region TestQuantityLimited_RequiresFlooring

		public void TestQuantityLimited_RequiresFlooring()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableUnits: 2.5m, maxUnits: 10m);
			AssertEquals("Precondition: HasUnitCapacity", true, putawayLocationFact.HasUnitCapacity);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			AssertCanPutawayFunction(2m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestQuantityLimited_NoMaxQuantitySpecified

		public void TestQuantityLimited_NoMaxQuantitySpecified()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableUnits: 2m);
			AssertEquals("Precondition: HasUnitCapacity", false, putawayLocationFact.HasUnitCapacity);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			AssertCanPutawayFunction(5m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestWeightLimited

		public void TestWeightLimited()
		{
			var putawayLocationFact_JustWeight = PutawayLocationFactTest.GetPutawayLocationFact(availableWeight: 20m, maxWeight: 30m, weightUQ: "KG");
			var putawayLocationFact_WithOtherConstraints = PutawayLocationFactTest.GetPutawayLocationFact(availableUnits: 3m, maxUnits: 10m, availableWeight: 20m, maxWeight: 30m, weightUQ: "KG", availableVolume: 15m, maxVolume: 15m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			AssertCanPutawayFunction(2m, putawayLocationFact_JustWeight, inv1.Object);
			AssertCanPutawayFunction(2m, putawayLocationFact_WithOtherConstraints, inv1.Object);
		}

		#endregion

		#region TestWeightLimited_WithParentLocation

		public void TestWeightLimited_WithParentLocation()
		{
			var parentLocation = PutawayLocationFactTest.GetPutawayLocationFact(availableWeight: 25m, maxWeight: 30m, weightUQ: Core.Constants.Weight.Kilograms);
			var locationFact1 = PutawayLocationFactTest.GetPutawayLocationFact(availableWeight: 35m, maxWeight: 30m, weightUQ: Core.Constants.Weight.Kilograms, parentLocation: parentLocation);
			var locationFact2 = PutawayLocationFactTest.GetPutawayLocationFact(availableWeight: 15m, maxWeight: 30m, weightUQ: Core.Constants.Weight.Kilograms, parentLocation: parentLocation);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns(Core.Constants.Weight.Kilograms);

			AssertCanPutawayFunction(2m, locationFact1, inv1.Object); // Always limited based on parent
			AssertCanPutawayFunction(2m, locationFact2, inv1.Object); // Always limited based on parent
		}

		#endregion

		#region TestWeightLimited_RequiresFlooring

		public void TestWeightLimited_RequiresFlooring()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableWeight: 20m, maxWeight: 30m, weightUQ: "KG");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductWeight).Returns(9m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			AssertCanPutawayFunction(2m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestWeightLimited_RequiresConversion

		public void TestWeightLimited_RequiresConversion()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableWeight: 20m, maxWeight: 30m, weightUQ: "KG");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductWeight).Returns(22m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("LB"); // 44lbs = 19.9581 kg

			AssertCanPutawayFunction(2m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestWeightLimited_NoMaxWeightSpecified

		public void TestWeightLimited_NoMaxWeightSpecified()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableWeight: 20m, maxWeight: 0m, weightUQ: "KG");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			AssertCanPutawayFunction(5m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestWeightLimited_NoMaxWeightUQSpecified

		public void TestWeightLimited_NoMaxWeightUQSpecified()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableWeight: 20m, maxWeight: 30m, weightUQ: "");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			AssertCanPutawayFunction(5m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestWeightLimited_NoProductWeightSpecified

		public void TestWeightLimited_NoProductWeightSpecified()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableWeight: 20m, maxWeight: 30m, weightUQ: "KG");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductWeight).Returns(0m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			AssertCanPutawayFunction(5m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestWeightLimited_NoProductWeightUQSpecified

		public void TestWeightLimited_NoProductWeightUQSpecified()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableWeight: 20m, maxWeight: 30m, weightUQ: "KG");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("");

			AssertCanPutawayFunction(5m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestVolumeLimited

		public void TestVolumeLimited()
		{
			var putawayLocationFact_JustVolume = PutawayLocationFactTest.GetPutawayLocationFact(availableVolume: 10m, maxVolume: 15m, volumeUQ: "M3");
			var putawayLocationFact_WithOtherConstraints = PutawayLocationFactTest.GetPutawayLocationFact(availableUnits: 3m, maxUnits: 10m, availableWeight: 40m, maxWeight: 50m, weightUQ: "KG", availableVolume: 10m, maxVolume: 15m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertCanPutawayFunction(2m, putawayLocationFact_JustVolume, inv1.Object);
			AssertCanPutawayFunction(2m, putawayLocationFact_WithOtherConstraints, inv1.Object);
		}

		#endregion

		#region TestVolumeLimited_WithParentLocation

		public void TestVolumeLimited_WithParentLocation()
		{
			var parentLocation = PutawayLocationFactTest.GetPutawayLocationFact(availableVolume: 25m, maxVolume: 30m, volumeUQ: Core.Constants.Volume.CubicMetres);
			var locationFact1 = PutawayLocationFactTest.GetPutawayLocationFact(availableVolume: 35m, maxVolume: 30m, volumeUQ: Core.Constants.Volume.CubicMetres, parentLocation: parentLocation);
			var locationFact2 = PutawayLocationFactTest.GetPutawayLocationFact(availableVolume: 15m, maxVolume: 30m, volumeUQ: Core.Constants.Volume.CubicMetres, parentLocation: parentLocation);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductVolume).Returns(10m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns(Core.Constants.Volume.CubicMetres);

			AssertCanPutawayFunction(2m, locationFact1, inv1.Object); // Always limited based on parent
			AssertCanPutawayFunction(2m, locationFact2, inv1.Object); // Always limited based on parent
		}

		#endregion

		#region TestVolumeLimited_RequiresFlooring

		public void TestVolumeLimited_RequiresFlooring()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableVolume: 10m, maxVolume: 15m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductVolume).Returns(4m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertCanPutawayFunction(2m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestVolumeLimited_RequiresConversion

		public void TestVolumeLimited_RequiresConversion()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableVolume: 20m, maxVolume: 25m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductVolume).Returns(13m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("CY"); // 26CY = 19.8784M3

			AssertCanPutawayFunction(2m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestVolumeLimited_NoMaxVolumeSpecified

		public void TestVolumeLimited_NoMaxVolumeSpecified()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableVolume: 10m, maxVolume: 0m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertCanPutawayFunction(5m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestVolumeLimited_NoMaxVolumeUQSpecified

		public void TestVolumeLimited_NoMaxVolumeUQSpecified()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableVolume: 10m, maxVolume: 15m, volumeUQ: "");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertCanPutawayFunction(5m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestVolumeLimited_NoProductVolumeSpecified

		public void TestVolumeLimited_NoProductVolumeSpecified()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableVolume: 10m, maxVolume: 15m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductVolume).Returns(0m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			AssertCanPutawayFunction(5m, putawayLocationFact, inv1.Object);
		}

		#endregion

		#region TestVolumeLimited_NoProductVolumeUQSpecified

		public void TestVolumeLimited_NoProductVolumeUQSpecified()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableVolume: 10m, maxVolume: 15m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);

			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("");

			AssertCanPutawayFunction(5m, putawayLocationFact, inv1.Object);
		}

		#endregion

		protected virtual void AssertCanPutawayFunction(decimal putawayQuantity, IPutawayLocationFact location, IInventoryFact inventory)
		{
			AssertEquals(nameof(IPutawayLocationFact.StockQuantityThatCanBePutaway), putawayQuantity, location.StockQuantityThatCanBePutaway(inventory));
			AssertEquals(nameof(IPutawayLocationFact.CanAllStockBePutaway), putawayQuantity == inventory.Quantity, location.CanAllStockBePutaway(inventory));
		}
	}

	class PutawayLocationFactCanAllStockBePutawayTest : PutawayLocationFactStockQuantityThatCanBePutawayTest
	{
		public void TestCanAllStockBePutaway_MultipleInventories()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableUnits: 13m, maxUnits: 20m, availableWeight: 57m, maxWeight: 60m, weightUQ: "KG", availableVolume: 33m, maxVolume: 36m, volumeUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(5m);
			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");
			inv1.Setup(inv => inv.ProductVolume).Returns(5m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(7m);
			inv2.Setup(inv => inv.ProductWeight).Returns(900m);
			inv2.Setup(inv => inv.ProductWeightUQ).Returns("G");
			inv2.Setup(inv => inv.ProductVolume).Returns(999999m);
			inv2.Setup(inv => inv.ProductVolumeUQ).Returns("CC");

			AssertEquals(nameof(IPutawayLocationFact.CanAllStockBePutaway), true, putawayLocationFact.CanAllStockBePutaway(new[] { inv1.Object, inv2.Object }));
		}

		public void TestCanAllStockBePutaway_UnitsLimited_MultipleInventories()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableUnits: 13m, maxUnits: 20m);

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(7m);
			AssertEquals("Precondition: Single inventory can be putaway.", true, putawayLocationFact.CanAllStockBePutaway(new[] { inv1.Object }));

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(7m);
			AssertEquals("Precondition: Single inventory can be putaway.", true, putawayLocationFact.CanAllStockBePutaway(new[] { inv2.Object }));

			AssertEquals("Should *not* be able to putaway the two inventory records together.", false, putawayLocationFact.CanAllStockBePutaway(new[] { inv1.Object, inv2.Object }));

			inv2.Setup(inv => inv.Quantity).Returns(6m);
			AssertEquals("Should be able to putaway the two inventory records together.", true, putawayLocationFact.CanAllStockBePutaway(new[] { inv1.Object, inv2.Object }));
		}

		public void TestCanAllStockBePutaway_WeightLimited_MultipleInventories()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableWeight: 11m, maxWeight: 30m, weightUQ: "KG");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(1m);
			inv1.Setup(inv => inv.ProductWeight).Returns(10m);
			inv1.Setup(inv => inv.ProductWeightUQ).Returns("KG");
			AssertEquals("Precondition: Single inventory can be putaway.", true, putawayLocationFact.CanAllStockBePutaway(new[] { inv1.Object }));

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(1m);
			inv2.Setup(inv => inv.ProductWeight).Returns(1100m);
			inv2.Setup(inv => inv.ProductWeightUQ).Returns("G");
			AssertEquals("Precondition: Single inventory can be putaway.", true, putawayLocationFact.CanAllStockBePutaway(new[] { inv2.Object }));

			AssertEquals("Should *not* be able to putaway the two inventory records together.", false, putawayLocationFact.CanAllStockBePutaway(new[] { inv1.Object, inv2.Object }));

			inv2.Setup(inv => inv.ProductWeight).Returns(1000m);
			AssertEquals("Should be able to putaway the two inventory records together.", true, putawayLocationFact.CanAllStockBePutaway(new[] { inv1.Object, inv2.Object }));
		}

		public void TestCanAllStockBePutaway_VolumeLimited_MultipleInventories()
		{
			var putawayLocationFact = PutawayLocationFactTest.GetPutawayLocationFact(availableVolume: 11m, maxVolume: 30m, weightUQ: "M3");

			var inv1 = new Mock<IInventoryFact>();
			inv1.Setup(inv => inv.Quantity).Returns(1m);
			inv1.Setup(inv => inv.ProductVolume).Returns(10m);
			inv1.Setup(inv => inv.ProductVolumeUQ).Returns("M3");
			AssertEquals("Precondition: Single inventory can be putaway.", true, putawayLocationFact.CanAllStockBePutaway(new[] { inv1.Object }));

			var inv2 = new Mock<IInventoryFact>();
			inv2.Setup(inv => inv.Quantity).Returns(1m);
			inv2.Setup(inv => inv.ProductVolume).Returns(1100000m);
			inv2.Setup(inv => inv.ProductVolumeUQ).Returns("CC");
			AssertEquals("Precondition: Single inventory can be putaway.", true, putawayLocationFact.CanAllStockBePutaway(new[] { inv2.Object }));

			AssertEquals("Should *not* be able to putaway the two inventory records together.", false, putawayLocationFact.CanAllStockBePutaway(new[] { inv1.Object, inv2.Object }));

			inv2.Setup(inv => inv.ProductVolume).Returns(1000000m);
			AssertEquals("Should be able to putaway the two inventory records together.", true, putawayLocationFact.CanAllStockBePutaway(new[] { inv1.Object, inv2.Object }));
		}

		// Testing it in this way ensures broad coverage of the different branches/edge cases for the sad case (cannot putaway) with a single inventory.
		// This minimises the amount of edge cases that need to be tested above.
		protected override void AssertCanPutawayFunction(decimal putawayQuantity, IPutawayLocationFact location, IInventoryFact inventory)
		{
			AssertEquals(nameof(IPutawayLocationFact.CanAllStockBePutaway), inventory.Quantity <= putawayQuantity, location.CanAllStockBePutaway(new[] { inventory }));
		}
	}

	abstract class PutawayLocationFactAttributeTest<T> : TestCase
	{
		public void TestContainsOtherPartAttribute_False_NoAttributes()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = default, NumberOfAttributes = 0 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();
			attributeData.Add(productPK, productAttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact = new Mock<IInventoryFact>();
			inventoryFact.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact, Value1);

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, new[] { inventoryFact.Object }));
		}

		public void TestContainsOtherPartAttribute_False_NoAttributes_Empty()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = default, NumberOfAttributes = 0 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();
			attributeData.Add(productPK, productAttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact = new Mock<IInventoryFact>();
			inventoryFact.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact, Empty);

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, new[] { inventoryFact.Object }));
		}

		public void TestContainsOtherPartAttribute_False_SameAttribute()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = Value1, NumberOfAttributes = 1 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();
			attributeData.Add(productPK, productAttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact = new Mock<IInventoryFact>();
			inventoryFact.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact, Value1);

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, new[] { inventoryFact.Object }));
		}

		public void TestContainsOtherPartAttribute_False_DifferentProduct()
		{
			var product1PK = Guid.NewGuid();
			var product2PK = Guid.NewGuid();

			var product1AttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = Value1, NumberOfAttributes = 1 };
			var product2AttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = default, NumberOfAttributes = 0 };

			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();
			attributeData.Add(product1PK, product1AttributeData);
			attributeData.Add(product2PK, product2AttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact = new Mock<IInventoryFact>();
			inventoryFact.Setup(i => i.Product).Returns(CreateProductJoin(product2PK));
			SetAttributeValueOnMock(inventoryFact, Value2);

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, new[] { inventoryFact.Object }));
		}

		public void TestContainsOtherPartAttribute_True_OtherAttribute()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = Value1, NumberOfAttributes = 1 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();
			attributeData.Add(productPK, productAttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact = new Mock<IInventoryFact>();
			inventoryFact.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact, Value2);

			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, new[] { inventoryFact.Object }));
		}

		public void TestContainsOtherPartAttribute_True_ManyAttributes()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = default, NumberOfAttributes = 2 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();
			attributeData.Add(productPK, productAttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact = new Mock<IInventoryFact>();
			inventoryFact.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact, Value2);

			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, new[] { inventoryFact.Object }));
		}

		public void TestContainsOtherPartAttribute_True_Empty()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = Value1, NumberOfAttributes = 1 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();
			attributeData.Add(productPK, productAttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact = new Mock<IInventoryFact>();
			inventoryFact.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact, Empty);

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, new[] { inventoryFact.Object }));
		}

		public void TestContainsOtherPartAttribute_MultipleInventories_False() => TestContainsOtherPartAttribute_MultipleInventories(false);
		public void TestContainsOtherPartAttribute_MultipleInventories_True() => TestContainsOtherPartAttribute_MultipleInventories(true);
		void TestContainsOtherPartAttribute_MultipleInventories(bool containsOther)
		{
			var product1PK = Guid.NewGuid();
			var product2PK = Guid.NewGuid();

			var product1AttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = Value1, NumberOfAttributes = 1 };
			var product2AttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = Value2, NumberOfAttributes = 1 };

			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();
			attributeData.Add(product1PK, product1AttributeData);
			attributeData.Add(product2PK, product2AttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(i => i.Product).Returns(CreateProductJoin(product1PK));
			SetAttributeValueOnMock(inventoryFact1, Value1);

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(i => i.Product).Returns(CreateProductJoin(product2PK));
			SetAttributeValueOnMock(inventoryFact2, containsOther ? Value1 : Value2);

			AssertEquals("ContainsOtherPartAttributes", containsOther, ContainsOtherPartAttributes(locationFact, new[] { inventoryFact1.Object, inventoryFact2.Object }));
		}

		public void TestContainsOtherPartAttribute_MultipleInventories_CheckConsistencyOnPallet_False() => TestContainsOtherPartAttribute_MultipleInventories_CheckConsistencyOnPallet(false);
		public void TestContainsOtherPartAttribute_MultipleInventories_CheckConsistencyOnPallet_True() => TestContainsOtherPartAttribute_MultipleInventories_CheckConsistencyOnPallet(true);
		void TestContainsOtherPartAttribute_MultipleInventories_CheckConsistencyOnPallet(bool containsOther)
		{
			var product1PK = Guid.NewGuid();
			var locationFact = CreatePutawayLocationFact("{}");

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(i => i.Product).Returns(CreateProductJoin(product1PK));
			SetAttributeValueOnMock(inventoryFact1, Value1);

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(i => i.Product).Returns(CreateProductJoin(product1PK));
			SetAttributeValueOnMock(inventoryFact2, containsOther ? Value2 : Value1);

			AssertEquals("ContainsOtherPartAttributes", containsOther, ContainsOtherPartAttributes(locationFact, new[] { inventoryFact1.Object, inventoryFact2.Object }));
		}

		public void TestContainsOtherPartAttribute_MultipleInventories_CheckConsistencyOnPallet_DifferentProduct()
		{
			var product1PK = Guid.NewGuid();
			var product2PK = Guid.NewGuid();
			var locationFact = CreatePutawayLocationFact("{}");

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(i => i.Product).Returns(CreateProductJoin(product1PK));
			SetAttributeValueOnMock(inventoryFact1, Value1);

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(i => i.Product).Returns(CreateProductJoin(product2PK));
			SetAttributeValueOnMock(inventoryFact2, Value2);

			AssertEquals("ContainsOtherPartAttributes", false, ContainsOtherPartAttributes(locationFact, new[] { inventoryFact1.Object, inventoryFact2.Object }));
		}

		public void TestContainsOtherPartAttribute_MultipleInventories_CheckConsistencyOnPallet_Empty()
		{
			var product1PK = Guid.NewGuid();
			var locationFact = CreatePutawayLocationFact("{}");

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(i => i.Product).Returns(CreateProductJoin(product1PK));
			SetAttributeValueOnMock(inventoryFact1, Value1);

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(i => i.Product).Returns(CreateProductJoin(product1PK));
			SetAttributeValueOnMock(inventoryFact2, Empty);

			AssertEquals("ContainsOtherPartAttributes", false, ContainsOtherPartAttributes(locationFact, new[] { inventoryFact1.Object, inventoryFact2.Object }));
		}

		public void TestUpdateDataAfterPutawayAllocation()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = Value2, NumberOfAttributes = 1 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();
			attributeData.Add(productPK, productAttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact1, Value1);

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact2, Value2);

			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));

			locationFact.UpdateDataAfterPutawayAllocation(inventoryFact1.Object, 5m);
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));
		}

		public void TestUpdateDataAfterPutawayAllocation_WithParentLocation()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = Value2, NumberOfAttributes = 1 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();
			attributeData.Add(productPK, productAttributeData);

			var parentLocationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));
			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData), parentLocation: parentLocationFact);

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact1, Value1);

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact2, Value2);

			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(parentLocationFact, inventoryFact1.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(parentLocationFact, inventoryFact2.Object));

			locationFact.UpdateDataAfterPutawayAllocation(inventoryFact1.Object, 5m);
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(parentLocationFact, inventoryFact1.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(parentLocationFact, inventoryFact2.Object));
		}

		public void TestUpdateDataAfterPutawayAllocation_Empty()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = default, NumberOfAttributes = 0 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();
			attributeData.Add(productPK, productAttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact1, Empty);

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact2, Value2);

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));

			locationFact.UpdateDataAfterPutawayAllocation(inventoryFact1.Object, 5m);
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));
		}

		public void TestUpdateDataAfterPutawayAllocation_WithoutProductData()
		{
			var productPK = Guid.NewGuid();
			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>()));

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact1, Value1);

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact2, Value2);

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));

			locationFact.UpdateDataAfterPutawayAllocation(inventoryFact1.Object, 5m);
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));
		}

		public void TestUpdateDataAfterPutawayAllocation_Enumerable_WithParentLocation()
		{
			var productPK = Guid.NewGuid();

			var parentLocationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>()));
			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>()), parentLocation: parentLocationFact);

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(inv => inv.Quantity).Returns(5m);
			inventoryFact1.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact1, Value1);

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(inv => inv.Quantity).Returns(5m);
			inventoryFact2.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact2, Value2);

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(parentLocationFact, inventoryFact1.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(parentLocationFact, inventoryFact2.Object));

			locationFact.UpdateDataAfterPutawayAllocation(new[] { inventoryFact1.Object, inventoryFact2.Object });
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(parentLocationFact, inventoryFact1.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(parentLocationFact, inventoryFact2.Object));
		}

		public void TestUpdateDataAfterPutawayAllocation_Enumerable_WithoutProductData()
		{
			var productPK = Guid.NewGuid();
			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>()));

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			inventoryFact1.Setup(inv => inv.Quantity).Returns(1m);
			SetAttributeValueOnMock(inventoryFact1, Value1);

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			inventoryFact2.Setup(inv => inv.Quantity).Returns(1m);
			SetAttributeValueOnMock(inventoryFact2, Value2);

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));

			locationFact.UpdateDataAfterPutawayAllocation(new[] { inventoryFact1.Object, inventoryFact2.Object });
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));
		}

		public void TestUpdateDataAfterPutawayAllocation_Enumerable_OneEmpty()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<T> { Attribute = default, NumberOfAttributes = 0 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<T>>();
			attributeData.Add(productPK, productAttributeData);
			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			inventoryFact1.Setup(inv => inv.Quantity).Returns(1m);
			SetAttributeValueOnMock(inventoryFact1, Value1);

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			inventoryFact2.Setup(inv => inv.Quantity).Returns(1m);
			SetAttributeValueOnMock(inventoryFact2, Empty);

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));

			locationFact.UpdateDataAfterPutawayAllocation(new[] { inventoryFact1.Object, inventoryFact2.Object });
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));
		}

		protected abstract bool ContainsOtherPartAttributes(PutawayLocationFact locationFact, IInventoryFact inventory);
		protected abstract bool ContainsOtherPartAttributes(PutawayLocationFact locationFact, IEnumerable<IInventoryFact> inventories);
		protected abstract void SetAttributeValueOnMock(Mock<IInventoryFact> mock, T value);
		protected abstract PutawayLocationFact CreatePutawayLocationFact(string attributeData, PutawayLocationFact parentLocation = null);

		protected FactJoin<IPutawayProductFact> CreateProductJoin(Guid productPK)
		{
			var productMock = new Mock<IPutawayProductFact>();
			productMock.Setup(p => p.PK).Returns(productPK);
			return new FactJoin<IPutawayProductFact>(productMock.Object);
		}

		protected abstract T Value1 { get; }
		protected abstract T Value2 { get; }
		protected abstract T Empty { get; }
	}

	abstract class PutawayLocationFactPartAttributeTest : PutawayLocationFactAttributeTest<string>
	{
		public void TestContainsOtherPartAttribute_False_CaseInsensitive()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<string> { Attribute = Value1, NumberOfAttributes = 1 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<string>>();
			attributeData.Add(productPK, productAttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact = new Mock<IInventoryFact>();
			inventoryFact.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact, Value1.ToLower());

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact.Object));
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, new[] { inventoryFact.Object }));
		}

		public void TestUpdateDataAfterPutawayAllocation_CaseInsensitive()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<string> { Attribute = Value1, NumberOfAttributes = 1 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<string>>();
			attributeData.Add(productPK, productAttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact1, Value1.ToLower());

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			SetAttributeValueOnMock(inventoryFact2, Value2);

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));

			locationFact.UpdateDataAfterPutawayAllocation(inventoryFact1.Object, 5m);
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));
		}

		public void TestUpdateDataAfterPutawayAllocation_Enumerable_CaseInsensitive()
		{
			var productPK = Guid.NewGuid();
			var productAttributeData = new WhsPutawayLocationCacheAttributeData<string> { Attribute = Value1, NumberOfAttributes = 1 };
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData<string>>();
			attributeData.Add(productPK, productAttributeData);

			var locationFact = CreatePutawayLocationFact(JsonConvert.SerializeObject(attributeData));

			var inventoryFact1 = new Mock<IInventoryFact>();
			inventoryFact1.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			inventoryFact1.Setup(inv => inv.Quantity).Returns(1m);
			SetAttributeValueOnMock(inventoryFact1, Value1.ToLower());

			var inventoryFact2 = new Mock<IInventoryFact>();
			inventoryFact2.Setup(inv => inv.Product).Returns(CreateProductJoin(productPK));
			inventoryFact2.Setup(inv => inv.Quantity).Returns(1m);
			SetAttributeValueOnMock(inventoryFact2, Value2);

			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));

			locationFact.UpdateDataAfterPutawayAllocation(new[] { inventoryFact1.Object });
			AssertEquals("Should *not* contain other part attributes.", false, ContainsOtherPartAttributes(locationFact, inventoryFact1.Object));
			AssertEquals("Should contain other part attributes.", true, ContainsOtherPartAttributes(locationFact, inventoryFact2.Object));
		}

		protected override string Value1 => "ABC";
		protected override string Value2 => "XYZ";
		protected override string Empty => string.Empty;
	}

	class PutawayLocationFactPartAttribute1Test : PutawayLocationFactPartAttributeTest
	{
		protected override bool ContainsOtherPartAttributes(PutawayLocationFact locationFact, IInventoryFact inventory) => locationFact.ContainsOtherPartAttribute1s(inventory);

		protected override bool ContainsOtherPartAttributes(PutawayLocationFact locationFact, IEnumerable<IInventoryFact> inventories) => locationFact.ContainsOtherPartAttribute1s(inventories);

		protected override void SetAttributeValueOnMock(Mock<IInventoryFact> mock, string value) => mock.Setup(inv => inv.PartAttribute1).Returns(value);

		protected override PutawayLocationFact CreatePutawayLocationFact(string attributeData, PutawayLocationFact parentLocation) => PutawayLocationFactTest.GetPutawayLocationFact(pa1Data: attributeData, parentLocation: parentLocation);
	}

	class PutawayLocationFactPartAttribute2Test : PutawayLocationFactPartAttributeTest
	{
		protected override bool ContainsOtherPartAttributes(PutawayLocationFact locationFact, IInventoryFact inventory) => locationFact.ContainsOtherPartAttribute2s(inventory);

		protected override bool ContainsOtherPartAttributes(PutawayLocationFact locationFact, IEnumerable<IInventoryFact> inventories) => locationFact.ContainsOtherPartAttribute2s(inventories);

		protected override void SetAttributeValueOnMock(Mock<IInventoryFact> mock, string value) => mock.Setup(inv => inv.PartAttribute2).Returns(value);

		protected override PutawayLocationFact CreatePutawayLocationFact(string attributeData, PutawayLocationFact parentLocation) => PutawayLocationFactTest.GetPutawayLocationFact(pa2Data: attributeData, parentLocation: parentLocation);
	}

	class PutawayLocationFactPartAttribute3Test : PutawayLocationFactPartAttributeTest
	{
		protected override bool ContainsOtherPartAttributes(PutawayLocationFact locationFact, IInventoryFact inventory) => locationFact.ContainsOtherPartAttribute3s(inventory);

		protected override bool ContainsOtherPartAttributes(PutawayLocationFact locationFact, IEnumerable<IInventoryFact> inventories) => locationFact.ContainsOtherPartAttribute3s(inventories);

		protected override void SetAttributeValueOnMock(Mock<IInventoryFact> mock, string value) => mock.Setup(inv => inv.PartAttribute3).Returns(value);

		protected override PutawayLocationFact CreatePutawayLocationFact(string attributeData, PutawayLocationFact parentLocation) => PutawayLocationFactTest.GetPutawayLocationFact(pa3Data: attributeData, parentLocation: parentLocation);
	}

	abstract class PutawayLocationFactDateAttributeTest : PutawayLocationFactAttributeTest<DateTime?>
	{
		protected override DateTime? Value1 => ZDateTime.BrettsBirthday.ToDateTime();
		protected override DateTime? Value2 => new DateTime(2020, 02, 02);
		protected override DateTime? Empty => null;
	}

	class PutawayLocationFactPackingDateTest : PutawayLocationFactDateAttributeTest
	{
		protected override bool ContainsOtherPartAttributes(PutawayLocationFact locationFact, IInventoryFact inventory) => locationFact.ContainsOtherPackingDates(inventory);

		protected override bool ContainsOtherPartAttributes(PutawayLocationFact locationFact, IEnumerable<IInventoryFact> inventories) => locationFact.ContainsOtherPackingDates(inventories);

		protected override void SetAttributeValueOnMock(Mock<IInventoryFact> mock, DateTime? value) => mock.Setup(inv => inv.PackingDate).Returns(value);

		protected override PutawayLocationFact CreatePutawayLocationFact(string attributeData, PutawayLocationFact parentLocation) => PutawayLocationFactTest.GetPutawayLocationFact(packingData: attributeData, parentLocation: parentLocation);
	}

	class PutawayLocationFactExpiryDateTest : PutawayLocationFactDateAttributeTest
	{
		protected override bool ContainsOtherPartAttributes(PutawayLocationFact locationFact, IInventoryFact inventory) => locationFact.ContainsOtherExpiryDates(inventory);

		protected override bool ContainsOtherPartAttributes(PutawayLocationFact locationFact, IEnumerable<IInventoryFact> inventories) => locationFact.ContainsOtherExpiryDates(inventories);

		protected override void SetAttributeValueOnMock(Mock<IInventoryFact> mock, DateTime? value) => mock.Setup(inv => inv.ExpiryDate).Returns(value);

		protected override PutawayLocationFact CreatePutawayLocationFact(string attributeData, PutawayLocationFact parentLocation) => PutawayLocationFactTest.GetPutawayLocationFact(expiryData: attributeData, parentLocation: parentLocation);
	}

	internal class WhsPutawayLocationCacheAttributeData<T>
	{
		public T Attribute { get; set; }
		public int NumberOfAttributes { get; set; }
	}
}
