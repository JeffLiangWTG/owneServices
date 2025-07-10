using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Warehouse.Environment.Business;
using Newtonsoft.Json;
using WTG.ProductionRules.Business.ProductWarehousePutaway;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class PutawayLocationFact : LocationFact, IPutawayLocationFact
	{
		public PutawayLocationFact(
			Guid pk,
			Guid locationPk,
			Guid warehousePK,
			string locationTypeCode,
			string locationClass,
			string areaName,
			string rowName,
			int column,
			int level,
			int tray,
			int putawaySequence,
			string locationStatus,
			string areaTypeCode,
			decimal availableWeight,
			decimal maxWeight,
			string maxWeightUnit,
			decimal availableVolume,
			decimal maxVolume,
			string maxVolumeUnit,
			decimal availableUnits,
			decimal maxUnits,
			decimal currentAndIncomingStock,
			bool isPartialPallet,
			string partialPalletID,
			bool isDynamicPickFace,
			bool isFixedPickFace,
			bool isFixedPickFaceFull,
			Guid? productPK,
			Guid? clientPK,
			bool isTsaKnownLocation,
			string productDataJson,
			string pa1DataJson,
			string pa2DataJson,
			string pa3DataJson,
			string expiryDateDataJson,
			string packingDateDataJson,
			int palletSpaces,
			int palletQuantity,
			PutawayLocationFact parentLocation = null,
			Guid? lastAllocatedOrChangedID = null)
			: base(pk, locationTypeCode, locationClass, areaName, rowName, column, level, tray, locationStatus, isFixedPickFace, isDynamicPickFace)
		{
			LocationPK = locationPk;
			WarehousePK = warehousePK;
			PutawaySequence = putawaySequence;
			AreaTypeCode = areaTypeCode;
			AvailableWeight = availableWeight;
			this.maxWeightUnit = maxWeightUnit;
			AvailableVolume = availableVolume;
			this.maxVolumeUnit = maxVolumeUnit;
			AvailableUnits = availableUnits;
			CurrentAndIncomingStock = currentAndIncomingStock;
			IsPartialPallet = isPartialPallet;
			PartialPalletID = partialPalletID;
			IsFixedPickFaceFull = isFixedPickFaceFull;
			ProductPK = productPK;
			ClientPK = clientPK;
			IsTSAKnownLocation = isTsaKnownLocation;

			ProductDataJson = productDataJson;
			PA1DataJson = pa1DataJson;
			PA2DataJson = pa2DataJson;
			PA3DataJson = pa3DataJson;
			PackingDateDataJson = packingDateDataJson;
			ExpiryDateDataJson = expiryDateDataJson;

			PalletSpaces = palletSpaces;
			PalletQuantity = palletQuantity;

			hasUnitCapacity = maxUnits > 0;
			hasWeightCapacity = maxWeight > 0 && !string.IsNullOrEmpty(MaxWeightUnit);
			hasVolumeCapacity = maxVolume > 0 && !string.IsNullOrEmpty(MaxVolumeUnit);

			ParentLocation = parentLocation;
			LastAllocatedOrChangedID = lastAllocatedOrChangedID;
		}

		public Guid LocationPK { get; }

		public Guid WarehousePK { get; }

		public bool IsPartialPallet { get; }

		public bool IsFixedPickFaceFull { get; private set; }

		public Guid? ProductPK { get; }

		public Guid? ClientPK { get; }

		public int PutawaySequence { get; }

		public decimal CurrentAndIncomingStock { get; private set; }

		public bool IsTSAKnownLocation { get; }

		public string AreaTypeCode { get; }

		public string PartialPalletID { get; }

		public decimal AvailableWeight
		{
			get => ParentLocation?.AvailableWeight ?? availableWeight;
			private set => availableWeight = value;
		}
		decimal availableWeight;

		public decimal AvailableVolume
		{
			get => ParentLocation?.AvailableVolume ?? availableVolume;
			private set => availableVolume = value;
		}
		decimal availableVolume;

		public decimal AvailableUnits
		{
			get
			{
				decimal value;

				if (ParentLocation?.HasUnitCapacity ?? false)
				{
					value = hasUnitCapacity ? Math.Min(ParentLocation.AvailableUnits, availableUnits) : ParentLocation.AvailableUnits;
				}
				else
				{
					value = availableUnits;
				}

				return value;
			}

			private set => availableUnits = value;
		}
		decimal availableUnits;

		public int PalletSpaces { get; }

		public int PalletQuantity
		{
			get => ParentLocation?.PalletQuantity ?? palletQuantity;
			private set => palletQuantity = value;
		}
		int palletQuantity;

		public bool HasUnitCapacity => hasUnitCapacity || (ParentLocation?.HasUnitCapacity ?? false);
		readonly bool hasUnitCapacity;

		public bool HasWeightCapacity => hasWeightCapacity || (ParentLocation?.HasWeightCapacity ?? false);
		readonly bool hasWeightCapacity;

		public bool HasVolumeCapacity => hasVolumeCapacity || (ParentLocation?.HasVolumeCapacity ?? false);
		readonly bool hasVolumeCapacity;

		public string MaxWeightUnit => ParentLocation?.MaxWeightUnit ?? maxWeightUnit;
		readonly string maxWeightUnit;

		public string MaxVolumeUnit => ParentLocation?.MaxVolumeUnit ?? maxVolumeUnit;
		readonly string maxVolumeUnit;

		#region Inventory Specific Methods

		public bool LocationStatusMatchesInventory(IInventoryFact inventory) => LocationStatusMatchesInventory(new[] { inventory });

		public bool LocationStatusMatchesInventory(IEnumerable<IInventoryFact> inventory)
		{
			const string damagedLocationStatus = "DAM";
			const string heldLocationStatus = "HEL";
			const string normalLocationStatus = "NOR";

			var locationStatusesOrdered = new[] { damagedLocationStatus, heldLocationStatus, normalLocationStatus };

			var resultIndex = 0;
			foreach (var lineToPutaway in inventory)
			{
				string currentStatus = CalculatePreferredLocationStatus(lineToPutaway.HoldCode);
				int currentIndex = Array.IndexOf(locationStatusesOrdered, currentStatus);
				if (resultIndex < currentIndex)
				{
					resultIndex = currentIndex;
				}

				if (locationStatusesOrdered[resultIndex] == normalLocationStatus)
				{
					break;
				}
			}

			return string.Equals(locationStatusesOrdered[resultIndex], LocationStatus, StringComparison.OrdinalIgnoreCase);

			string CalculatePreferredLocationStatus(string currentHoldCode)
			{
				switch (currentHoldCode)
				{
					case "":
						return normalLocationStatus;

					case "DAM":
						return damagedLocationStatus;

					default:
						return heldLocationStatus;
				}
			}
		}

		public bool ContainsOtherAttributeCombinations(IInventoryFact inventory)
		{
			return
				ContainsOtherPartAttribute1s(inventory) || ContainsOtherPartAttribute2s(inventory) || ContainsOtherPartAttribute3s(inventory) ||
				ContainsOtherExpiryDates(inventory) || ContainsOtherPackingDates(inventory);
		}

		public bool ContainsOtherPartAttribute1s(IInventoryFact inventory) => ContainsOtherPartAttributes(PartAttribute1Data, inventory.Product.Fact.PK, inventory.PartAttribute1);

		public bool ContainsOtherPartAttribute2s(IInventoryFact inventory) => ContainsOtherPartAttributes(PartAttribute2Data, inventory.Product.Fact.PK, inventory.PartAttribute2);

		public bool ContainsOtherPartAttribute3s(IInventoryFact inventory) => ContainsOtherPartAttributes(PartAttribute3Data, inventory.Product.Fact.PK, inventory.PartAttribute3);

		bool ContainsOtherPartAttributes(Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<string>> attributeData, Guid partRelationFK, string value)
		{
			return ContainsOtherAttributes(attributeData, partRelationFK, value, PAHelper);
		}

		public bool ContainsOtherExpiryDates(IInventoryFact inventory) => ContainsOtherDateAttributes(ExpiryDateData, inventory.Product.Fact.PK, inventory.ExpiryDate);

		public bool ContainsOtherPackingDates(IInventoryFact inventory) => ContainsOtherDateAttributes(PackingDateData, inventory.Product.Fact.PK, inventory.PackingDate);

		bool ContainsOtherDateAttributes(Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<DateTime?>> attributeData, Guid partRelationFK, DateTime? value)
		{
			return ContainsOtherAttributes(attributeData, partRelationFK, value, DatePAHelper);
		}

		bool ContainsOtherAttributes<T>(Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<T>> attributeData, Guid partRelationFK, T value, AttributeHelper<T> attributeHelper)
		{
			return attributeData.TryGetValue(partRelationFK, out var productAttributeData) &&
				(productAttributeData.NumberOfAttributes > 1 || (productAttributeData.Attribute != null && !attributeHelper.Equals(value, attributeHelper.EmptyValue) && !attributeHelper.Equals(productAttributeData.Attribute, value)));
		}

		public bool ContainsThisProduct(IInventoryFact inventory) => ProductRelations.Contains(inventory.Product.Fact.PK);

		public int NumberOfOtherProducts(IInventoryFact inventory)
		{
			var containsThisProductCount = ProductRelations.Contains(inventory.Product.Fact.PK) ? 1 : 0;
			return ProductRelations.Count - containsThisProductCount;
		}

		public bool CanTransferToOrFromVASServiceArea(IInventoryFact inventory) => WhsLocation.CanTransferToOrFromAreaType(AreaTypeCode, inventory.VASServiceAreaTypeCode);

		public bool CanAllStockBePutaway(IInventoryFact inventory) => StockQuantityThatCanBePutaway(inventory) == inventory.Quantity;

		public decimal StockQuantityThatCanBePutaway(IInventoryFact inventory)
		{
			var maxFitByQuantity = HasUnitCapacity ? AvailableUnits : inventory.Quantity;
			var maxFitByWeight = ShouldCheckWeight(inventory) ? AvailableWeight / GetConvertedProductWeight(inventory) : maxFitByQuantity;
			var maxFitByVolume = ShouldCheckVolume(inventory) ? AvailableVolume / GetConvertedProductVolume(inventory) : maxFitByQuantity;

			return decimal.Floor(Math.Min(inventory.Quantity, Math.Min(maxFitByQuantity, Math.Min(maxFitByVolume, maxFitByWeight))));
		}

		bool ShouldCheckWeight(IInventoryFact inventory) => HasWeightCapacity && inventory.ProductWeight > 0 && !string.IsNullOrEmpty(inventory.ProductWeightUQ);
		bool ShouldCheckVolume(IInventoryFact inventory) => HasVolumeCapacity && inventory.ProductVolume > 0 && !string.IsNullOrEmpty(inventory.ProductVolumeUQ);
		decimal GetConvertedProductWeight(IInventoryFact inventory) => Constants.Weight.Convert(inventory.ProductWeight, inventory.ProductWeightUQ, MaxWeightUnit);
		decimal GetConvertedProductVolume(IInventoryFact inventory) => Constants.Volume.Convert(inventory.ProductVolume, inventory.ProductVolumeUQ, MaxVolumeUnit);

		#endregion

		#region Inventory Specific Methods -- IEnumerable

		public bool ContainsOtherAttributeCombinations(IEnumerable<IInventoryFact> inventory)
		{
			return
				ContainsOtherPartAttribute1s(inventory) || ContainsOtherPartAttribute2s(inventory) || ContainsOtherPartAttribute3s(inventory) ||
				ContainsOtherExpiryDates(inventory) || ContainsOtherPackingDates(inventory);
		}

		public bool ContainsOtherPartAttribute1s(IEnumerable<IInventoryFact> inventory) => ContainsOtherPartAttributesOnThisPallet(inventory, i => i.PartAttribute1) || inventory.Any(ContainsOtherPartAttribute1s);

		public bool ContainsOtherPartAttribute2s(IEnumerable<IInventoryFact> inventory) => ContainsOtherPartAttributesOnThisPallet(inventory, i => i.PartAttribute2) || inventory.Any(ContainsOtherPartAttribute2s);

		public bool ContainsOtherPartAttribute3s(IEnumerable<IInventoryFact> inventory) => ContainsOtherPartAttributesOnThisPallet(inventory, i => i.PartAttribute3) || inventory.Any(ContainsOtherPartAttribute3s);

		public bool ContainsOtherExpiryDates(IEnumerable<IInventoryFact> inventory) => ContainsOtherDateAttributesOnThisPallet(inventory, i => i.ExpiryDate) || inventory.Any(ContainsOtherExpiryDates);

		public bool ContainsOtherPackingDates(IEnumerable<IInventoryFact> inventory) => ContainsOtherDateAttributesOnThisPallet(inventory, i => i.PackingDate) || inventory.Any(ContainsOtherPackingDates);

		bool ContainsOtherPartAttributesOnThisPallet(IEnumerable<IInventoryFact> inventories, Func<IInventoryFact, string> getPA)
		{
			return ContainsOtherAttributesOnThisPallet(inventories, getPA, PAHelper);
		}

		bool ContainsOtherDateAttributesOnThisPallet(IEnumerable<IInventoryFact> inventories, Func<IInventoryFact, DateTime?> getPA)
		{
			return ContainsOtherAttributesOnThisPallet(inventories, getPA, DatePAHelper);
		}

		bool ContainsOtherAttributesOnThisPallet<T>(IEnumerable<IInventoryFact> inventories, Func<IInventoryFact, T> getPA, AttributeHelper<T> paHelper)
		{
			return inventories
				.GroupBy(i => i.Product.Fact.PK)
				.Any(g =>
					g.Select(getPA)
					.Where(pa => !paHelper.Equals(pa, paHelper.EmptyValue))
					.Distinct()
					.IsCountMoreThan(1));
		}

		public bool ContainsThisProduct(IEnumerable<IInventoryFact> inventory) => inventory.All(ContainsThisProduct);

		public int NumberOfOtherProducts(IEnumerable<IInventoryFact> inventory)
		{
			var numberOfOtherProductsOffset = inventory.Select(i => i.Product.Fact.PK).Distinct().Sum(prfk => ProductRelations.Contains(prfk) ? 1 : 0);
			return ProductRelations.Count - numberOfOtherProductsOffset;
		}

		public bool CanAllStockBePutaway(IEnumerable<IInventoryFact> inventories)
		{
			var units = 0m;
			var weight = 0m;
			var volume = 0m;

			foreach (var inventory in inventories)
			{
				units += inventory.Quantity;

				if (ShouldCheckWeight(inventory))
				{
					weight += inventory.Quantity * GetConvertedProductWeight(inventory);
				}

				if (ShouldCheckVolume(inventory))
				{
					volume += inventory.Quantity * GetConvertedProductVolume(inventory);
				}
			}

			return (!HasUnitCapacity || units <= AvailableUnits) && (!HasWeightCapacity || weight <= AvailableWeight) && (!HasVolumeCapacity || volume <= AvailableVolume);
		}

		#endregion

		#region Inventory Data

		public void UpdateDataAfterPutawayAllocation(IEnumerable<IInventoryFact> inventories)
		{
			if (ParentLocation == null)
			{
				if (!IsPartialPallet)
				{
					PalletQuantity++;
				}
			}
			else
			{
				if (!ParentLocation.IsPartialPallet)
				{
					ParentLocation.PalletQuantity++;
				}
			}

			foreach (var inventory in inventories)
			{
				UpdateDataAfterPutawayAllocation(inventory, inventory.Quantity);
			}
		}

		public void UpdateDataAfterPutawayAllocation(IInventoryFact inventory, decimal putawayQty)
		{
			if (putawayQty <= 0)
			{
				throw new ArgumentException("Putaway Quantity must be greater than zero.", nameof(putawayQty));
			}

			var partRelationFK = inventory.Product.Fact.PK;
			ProductRelations.Add(partRelationFK);
			UpdatePartAttributeData(PartAttribute1Data, partRelationFK, inventory.PartAttribute1);
			UpdatePartAttributeData(PartAttribute2Data, partRelationFK, inventory.PartAttribute2);
			UpdatePartAttributeData(PartAttribute3Data, partRelationFK, inventory.PartAttribute3);
			UpdateDateAttributeData(ExpiryDateData, partRelationFK, inventory.ExpiryDate);
			UpdateDateAttributeData(PackingDateData, partRelationFK, inventory.PackingDate);

			CurrentAndIncomingStock += putawayQty;

			if (HasUnitCapacity)
			{
				AvailableUnits -= putawayQty;
			}

			if (IsFixedPickFace && AvailableUnits <= 0m)
			{
				IsFixedPickFaceFull = true;
			}

			if (ParentLocation == null)
			{
				if (ShouldCheckWeight(inventory))
				{
					AvailableWeight -= putawayQty * GetConvertedProductWeight(inventory);
				}

				if (ShouldCheckVolume(inventory))
				{
					AvailableVolume -= putawayQty * GetConvertedProductVolume(inventory);
				}
			}
			else
			{
				ParentLocation.UpdateDataAfterPutawayAllocation(inventory, putawayQty);
			}
		}

		void UpdateDateAttributeData(Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<DateTime?>> attributeData, Guid partRelationFK, DateTime? value)
		{
			UpdateAttributeData(attributeData, partRelationFK, value, DatePAHelper);
		}

		void UpdatePartAttributeData(Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<string>> attributeData, Guid partRelationFK, string value)
		{
			UpdateAttributeData(attributeData, partRelationFK, value, PAHelper);
		}

		void UpdateAttributeData<T>(Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<T>> attributeData, Guid partRelationFK, T value, AttributeHelper<T> paHelper)
		{
			if (!paHelper.Equals(value, paHelper.EmptyValue))
			{
				if (!attributeData.TryGetValue(partRelationFK, out var productAttributeData))
				{
					attributeData[partRelationFK] = productAttributeData = new WhsPutawayLocationCachePartAttributeData<T>();
				}

				if (productAttributeData.Attribute != null && !paHelper.Equals(productAttributeData.Attribute, value))
				{
					productAttributeData.Attribute = default;
					productAttributeData.NumberOfAttributes = 2;
				}
				else if (productAttributeData.NumberOfAttributes == 0)
				{
					productAttributeData.Attribute = value;
					productAttributeData.NumberOfAttributes = 1;
				}
			}
		}

		HashSet<Guid> ProductRelations => productRelations ?? (productRelations = JsonConvert.DeserializeObject<HashSet<Guid>>(ProductDataJson));
		HashSet<Guid> productRelations;
		string ProductDataJson { get; }

		Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<string>> PartAttribute1Data =>
			partAttribute1Data ?? (partAttribute1Data = JsonConvert.DeserializeObject<Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<string>>>(PA1DataJson));

		Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<string>> partAttribute1Data;
		string PA1DataJson { get; }

		Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<string>> PartAttribute2Data =>
			partAttribute2Data ?? (partAttribute2Data = JsonConvert.DeserializeObject<Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<string>>>(PA2DataJson));

		Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<string>> partAttribute2Data;
		string PA2DataJson { get; }

		Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<string>> PartAttribute3Data =>
			partAttribute3Data ?? (partAttribute3Data = JsonConvert.DeserializeObject<Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<string>>>(PA3DataJson));

		Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<string>> partAttribute3Data;
		string PA3DataJson { get; }

		Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<DateTime?>> ExpiryDateData =>
			expiryDateData ?? (expiryDateData = JsonConvert.DeserializeObject<Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<DateTime?>>>(ExpiryDateDataJson));

		Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<DateTime?>> expiryDateData;
		string ExpiryDateDataJson { get; }

		Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<DateTime?>> PackingDateData =>
			packingDateData ?? (packingDateData = JsonConvert.DeserializeObject<Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<DateTime?>>>(PackingDateDataJson));

		Dictionary<Guid, WhsPutawayLocationCachePartAttributeData<DateTime?>> packingDateData;
		string PackingDateDataJson { get; }

		PutawayLocationFact ParentLocation { get; }

		public Guid? LastAllocatedOrChangedID { get; }

		#endregion

		readonly static PartAttributeHelper PAHelper = new PartAttributeHelper();
		readonly static DateAttributeHelper DatePAHelper = new DateAttributeHelper();

		[ThreadSafe]
		class PartAttributeHelper : AttributeHelper<string>
		{
			public override bool Equals(string s1, string s2) => string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
			public override string EmptyValue => string.Empty;
		}

		[ThreadSafe]
		class DateAttributeHelper : AttributeHelper<DateTime?>
		{
			public override bool Equals(DateTime? d1, DateTime? d2) => d1 == d2;
			public override DateTime? EmptyValue => null;
		}

		abstract class AttributeHelper<T>
		{
			public abstract bool Equals(T t1, T t2);

			public abstract T EmptyValue { get; }
		}
	}

	class WhsPutawayLocationCachePartAttributeData<T>
	{
		public T Attribute { get; set; }
		public int NumberOfAttributes { get; set; }
	}
}
