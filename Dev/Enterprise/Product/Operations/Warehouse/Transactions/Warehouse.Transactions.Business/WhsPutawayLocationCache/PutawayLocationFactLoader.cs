using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Facts;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Core;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Warehouse.Transactions.Business
{
	class PutawayLocationFactLoader : IPutawayLocationFactLoader
	{
		public PutawayLocationFactLoader(IWhsPutawayLocationCacheManager cacheManager)
		{
			CacheManager = Argument.NotNull(cacheManager, nameof(cacheManager));
		}

		IWhsPutawayLocationCacheManager CacheManager { get; }

		public IEnumerable<IInputFact> GetPutawayLocationFacts(BusinessObjectFactory factory, ZGuid warehousePK, IEnumerable<ZGuid> clientPKs, IEnumerable<ZGuid> partPKs, IEnumerable<ZGuid> skipLocationPKs = null)
		{
			var cacheData = CacheManager.GetCache(factory, warehousePK, clientPKs, partPKs, skipLocationPKs);

			var facts = new List<IInputFact>();

			if (cacheData.Any())
			{
				var columns = cacheData.First().Table.Columns;

				// It's faster to use index and do a single name lookup at the start
				var locationIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_WL_Location.Name);
				var areaNameIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_AreaName.Name);
				var areaTypeIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_AreaType.Name);
				var locationCacheTypeIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name);
				var availableQuantityIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name);
				var maxQuantityIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name);
				var stockOnHandIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_StockOnHand.Name);
				var pkIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.PK.Name);
				var warehouseIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_WW_Warehouse.Name);
				var locationTypeCodeIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_LocationTypeCode.Name);
				var locationClassIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_LocationClass.Name);
				var rowNameIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_RowName.Name);
				var columnIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_Column.Name);
				var levelIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_Level.Name);
				var trayIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_Tray.Name);
				var putawaySequenceIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_PutawaySequence.Name);
				var locationStatusIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_LocationStatus.Name);
				var availableWeightIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name);
				var maxWeightIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name);
				var weightUnitIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_WeightUnit.Name);
				var availableVolumeIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name);
				var maxVolumeIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name);
				var volumeUnitIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_VolumeUnit.Name);
				var partialPalletIDIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name);
				var fixedPickFaceFullIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_IsFixedPickFaceFull.Name);
				var productIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_OP_Product.Name);
				var clientIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_OH_Client.Name);
				var approvedKnownIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_IsTSAApprovedKnown.Name);
				var productDataIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_ProductData.Name);
				var partAttribute1DataIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_PartAttribute1Data.Name);
				var partAttribute2DataIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_PartAttribute2Data.Name);
				var partAttribute3DataIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_PartAttribute3Data.Name);
				var expiryDateDataIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_ExpiryDateData.Name);
				var packingDateDataIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_PackingDateData.Name);
				var palletSpacesIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_PalletSpaces.Name);
				var palletQuantityIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_PalletQuantity.Name);
				var lastAllocatedOrChangedIDIndex = columns.IndexOf(WhsPutawayLocationCacheSchema.WPC_LastAllocatedOrChangedID.Name);

				const string partialPalletCacheType = "PLT";
				const string locationCacheType = "LOC";

				var locations = new Dictionary<Guid, PutawayLocationFact>();
				foreach (var dataAndCacheType in cacheData
							.Select(data => new { Data = data, CacheType = ((string)data[locationCacheTypeIndex]) })
							.OrderBy(dataAndCacheType => dataAndCacheType.CacheType == partialPalletCacheType))
				{
					var data = dataAndCacheType.Data;
					var cacheType = dataAndCacheType.CacheType;
					var locationPk = ((Guid)data[locationIndex]);

					PutawayLocationFact parentLocationFact = null;
					if (cacheType != partialPalletCacheType || locations.TryGetValue(locationPk, out parentLocationFact))
					{
						var areaName = ((string)data[areaNameIndex]);
						var areaType = ((string)data[areaTypeIndex]);

						var locationFact = new PutawayLocationFact(
								((Guid)data[pkIndex]),
								locationPk,
								((Guid)data[warehouseIndex]),
								((string)data[locationTypeCodeIndex]),
								((string)data[locationClassIndex]),
								areaName,
								((string)data[rowNameIndex]),
								((short)data[columnIndex]),
								((short)data[levelIndex]),
								((short)data[trayIndex]),
								((int)data[putawaySequenceIndex]),
								((string)data[locationStatusIndex]),
								areaType,
								((decimal)data[availableWeightIndex]),
								((decimal)data[maxWeightIndex]),
								((string)data[weightUnitIndex]),
								((decimal)data[availableVolumeIndex]),
								((decimal)data[maxVolumeIndex]),
								((string)data[volumeUnitIndex]),
								((decimal)data[availableQuantityIndex]),
								((decimal)data[maxQuantityIndex]),
								((decimal)data[stockOnHandIndex]),
								cacheType == partialPalletCacheType,
								((string)data[partialPalletIDIndex]),
								cacheType == "DYN",
								cacheType == "FIX",
								((bool)data[fixedPickFaceFullIndex]),
								DataRowLoader.GetNullableGuid(data[productIndex]),
								DataRowLoader.GetNullableGuid(data[clientIndex]),
								((bool)data[approvedKnownIndex]),
								((string)data[productDataIndex]),
								((string)data[partAttribute1DataIndex]),
								((string)data[partAttribute2DataIndex]),
								((string)data[partAttribute3DataIndex]),
								((string)data[expiryDateDataIndex]),
								((string)data[packingDateDataIndex]),
								((short)data[palletSpacesIndex]),
								((short)data[palletQuantityIndex]),
								parentLocationFact,
								DataRowLoader.GetNullableGuid(data[lastAllocatedOrChangedIDIndex]));

						facts.Add(locationFact);

						if (cacheType == locationCacheType)
						{
							locations.Add(locationPk, locationFact);
						}
					}
				}
			}

			return facts;
		}
	}
}
