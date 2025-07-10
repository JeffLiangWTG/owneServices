using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.US;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Facts;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehousePutaway;
using WTG.ProductionRules.Core;
using static Enterprise.Core.Constants;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Warehouse.Transactions.Business
{
	abstract class PutawayEngineManager<TJob, TLineToPutaway>
		where TJob : ILineToPutawayParent
		where TLineToPutaway : ILineToPutaway
	{
		public PutawayEngineManager(
			IUserHaltableProductionRulesEngineService engine,
			IPutawayLocationFactLoader locationFactLoader,
			IPutawayLocationCacheUpdater putawayLocationCacheUpdater)
		{
			Engine = Argument.NotNull(engine, nameof(engine));
			LocationFactLoader = Argument.NotNull(locationFactLoader, nameof(locationFactLoader));
			PutawayLocationCacheUpdater = Argument.NotNull(putawayLocationCacheUpdater, nameof(putawayLocationCacheUpdater));
		}

		IUserHaltableProductionRulesEngineService Engine { get; }
		IPutawayLocationFactLoader LocationFactLoader { get; }
		IPutawayLocationCacheUpdater PutawayLocationCacheUpdater { get; }

		public void Putaway(IEnumerable<TJob> parents, IEnumerable<TLineToPutaway> linesToPutaway, INotifications notifications, RefEquipment equipment = null, IEnumerable<ZGuid> skipLocationPKs = null, bool useLocationConcurrencyHandling = false, bool needRebuildLocationCache = true)
		{
			Argument.NotNull(parents, nameof(parents));
			Argument.NotNull(linesToPutaway, nameof(linesToPutaway));

			var factory = parents.Select(p => p.Factory).Distinct().SingleOrDefault();
			var warehousePK = parents.Select(p => p.WarehousePK).Distinct().SingleOrDefault();
			var warehouse = parents.FirstOrDefault()?.Warehouse;

			if (factory == null || warehousePK == ZGuid.Empty)
			{
				throw new ArgumentException("All Jobs must share a single Factory and a single Warehouse during Putaway.");
			}
			else if (!needRebuildLocationCache || PutawayLocationCacheUpdater.UpdateWarehousePutawayLocationCacheWithProgressForm(factory, warehouse.PK, notifications))
			{
				using (GetCacheManagersToActivate(factory, linesToPutaway))
				{
					var filter = ProductionRuleSetFilter.WithWarehouse(warehouse.PK.ToGuid());
					Engine.RunRulesEngine(factory, notifications, RulesContextType.InventoryPutaway, filter, () => GetInventoryAndLocationFacts(factory, parents, linesToPutaway, warehouse, equipment, skipLocationPKs), r => ProcessResults(parents, factory, linesToPutaway, r, useLocationConcurrencyHandling));
				}
			}
		}

		IDisposable GetCacheManagersToActivate(BusinessObjectFactory factory, IEnumerable<TLineToPutaway> linesToPutaway)
		{
			return new DisposableList(new[] {
				PalletIDLocationValidationCacheManager.UseLocationCache(factory),
				PalletIDPutawayTransferCacheManager.UseHasPutawayTransferCache(factory),
				PendingOrdersCacheManager.UseInventoryPendingOrdersCache(factory, () => linesToPutaway.Select(line => line.ProductPK))
			});
		}

		IEnumerable<IInputFact> GetInventoryAndLocationFacts(
			BusinessObjectFactory factory,
			IEnumerable<TJob> parents,
			IEnumerable<TLineToPutaway> lines,
			WhsWarehouse warehouse, RefEquipment equipment,
			IEnumerable<ZGuid> skipLocationPKs = null)
		{
			AddFetchHintsForGetInvAndLocFacts(factory, parents, lines, warehouse.PK);
			var groupedLinesToPutaway = GetGroupedLinesToPutaway(factory, parents, lines, warehouse);
			return PrepareInputFacts(factory, parents, warehouse, equipment, groupedLinesToPutaway, skipLocationPKs);
		}

		void AddFetchHintsForGetInvAndLocFacts(BusinessObjectFactory factory, IEnumerable<TJob> parents, IEnumerable<TLineToPutaway> lines, ZGuid warehousePK)
		{
			var products = new HashSet<ZGuid>();
			foreach (var line in lines)
			{
				if (products.Add(line.ProductPK))
				{
					factory.AddFetchHint(WhsProductParamsByWhsAndClientSchema.W3_OP, line.ProductPK);
				}

				factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, line.PK);
			}

			var undgItems = lines.SelectMany(line => line.Product.UNDGs);
			foreach (var undgItem in undgItems)
			{
				factory.AddFetchHint(UNDGSubstanceSchema.PK, undgItem.DI_DG);
			}

			var clientPKs = parents.Select(p => p.ClientPK).Distinct().ToArray();
			foreach (var productPK in products)
			{
				factory.AddFetchHint(WhsABCCategorySchema.Instance, WhsABCCategory.GetABCCategoriesQuery(productPK, clientPKs, warehousePK));
			}

			var paramsQuery = new ZQuery(WhsProductParamsByWhsAndClientSchema.W3_OP, products);
			paramsQuery.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_OH, clientPKs);
			paramsQuery.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_WW, warehousePK);
			foreach (var param in factory.Load<WhsProductParamsByWhsAndClient>(paramsQuery).Where(p => !p.W3_WPG_PutawayGroup.IsEmpty))
			{
				factory.AddFetchHint(WhsPutawayGroupSchema.PK, param.W3_WPG_PutawayGroup);
			}

			AddFetchHintsForGetInvAndLocFactsCore(factory, lines);
		}

		protected virtual void AddFetchHintsForGetInvAndLocFactsCore(BusinessObjectFactory factory, IEnumerable<TLineToPutaway> lines)
		{
		}

		protected abstract IEnumerable<(TJob Parent, IEnumerable<TLineToPutaway> Lines)> GetGroupedLinesToPutaway(BusinessObjectFactory factory, IEnumerable<TJob> parents, IEnumerable<TLineToPutaway> lines, WhsWarehouse warehouse);

		IEnumerable<IInputFact> PrepareInputFacts(
			BusinessObjectFactory factory,
			IEnumerable<TJob> originalParents,
			WhsWarehouse warehouse,
			RefEquipment equipment,
			IEnumerable<(TJob Parent, IEnumerable<TLineToPutaway> Lines)> linesToPutawayByDocket,
			IEnumerable<ZGuid> skipLocationPKs = null)
		{
			var facts = new List<IInputFact>();

			var organisations = new Dictionary<ZGuid, OrganisationFact>();
			var products = new Dictionary<ZGuid, PutawayProductFact>();
			var processedPallets = new HashSet<string>();
			var clients = new HashSet<ZGuid>();
			var productPKs = new HashSet<ZGuid>();

			EquipmentFact equipmentFact = null;
			if (equipment != null)
			{
				equipmentFact = new EquipmentFact(equipment, equipment.RoadContainerType);
			}

			var parentsToPutaway = originalParents.Select(op => op.PK).ToHashSet();

			foreach (var docketWithLinesPair in linesToPutawayByDocket)
			{
				if (docketWithLinesPair.Lines.Any())
				{
					var parent = docketWithLinesPair.Parent;

					if (parentsToPutaway.Contains(parent.PK))
					{
						// Some lines are not actually being putaway, and just have matching pallet IDs
						// Either the lines share the product + client, or are for different product/client pairs
						// In the latter case, the pallet is mixed and can't be put to a pick face or partial pallet
						var clientPK = parent.ClientPK;
						clients.Add(clientPK);
						productPKs.UnionWith(docketWithLinesPair.Lines.Select(i => i.ProductPK));
					}

					var palletIds =
						docketWithLinesPair.Lines
						.Select(i => (string)i.PalletID)
						.Distinct(StringComparer.OrdinalIgnoreCase)
						.Where(pid => !string.IsNullOrEmpty(pid));

					foreach (var palletId in palletIds)
					{
						if (processedPallets.Add(palletId))
						{
							facts.Add(new PalletFact(palletId));
						}
					}

					facts.AddRange(AddInputFactsFromJob(docketWithLinesPair.Lines, parent, warehouse, equipmentFact, organisations, products));
				}
			}

			if (productPKs.Count > 0)
			{
				facts.AddRange(LocationFactLoader.GetPutawayLocationFacts(
					factory,
					warehouse.PK,
					clients.ToArray(),
					productPKs,
					skipLocationPKs));
			}

			return facts;
		}

		IEnumerable<IInputFact> AddInputFactsFromJob(
			IEnumerable<TLineToPutaway> linesToPutaway,
			TJob parent,
			WhsWarehouse warehouse,
			EquipmentFact equipment,
			Dictionary<ZGuid, OrganisationFact> organisations,
			Dictionary<ZGuid, PutawayProductFact> products)
		{
			var client = parent.Client;
			var isTsaKnownClient = TSAInfo.IsOrgHeaderTSAKnown(client);
			var isTsaPolicyRequired = warehouse.CountryCode == CountryCodes.UnitedStates && warehouse.IsApprovedKnown;

			if (!organisations.TryGetValue(client.PK, out var clientFact))
			{
				organisations[client.PK] = clientFact = WarehouseFactsHelper.GetOrganisationFact(client);
			}

			foreach (var line in linesToPutaway)
			{
				var product = line.Product;
				var whsProduct = WhsProduct.GetWhsProduct(product);
				var relation = product.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);

				if (!products.TryGetValue(relation.PK, out var productFact))
				{
					var abcCategory = WhsABCCategory.GetABCCategory(warehouse.Factory, product.PK, client.PK, warehouse.PK)?.WJ_Category;

					var paramsByWhsAndClient = WhsProduct.GetWhsProduct(product).GetParamsByWhsAndClient(warehouse, client);
					var putawayGroupCode = paramsByWhsAndClient?.PutawayGroup?.WPG_Code ?? "";

					var styleSize = whsProduct.ProductStyleSize;
					var styleColor = whsProduct.ProductStyleColour;
					var styleClassification = whsProduct.ProductStyleClassification;

					var (firstDangerousGoodsFact, hasMultipleDangerousGoods) = GetDangerousGoodsFactDetails(product);
					products[relation.PK] = productFact = new PutawayProductFact(product, relation, abcCategory, putawayGroupCode, styleSize?.ProductStyle?.WST_Code ?? string.Empty, styleClassification?.WSS_Code ?? string.Empty, styleColor?.WSC_Code ?? string.Empty, styleSize?.WSZ_Size ?? string.Empty, firstDangerousGoodsFact, hasMultipleDangerousGoods);
				}

				yield return AddInputFactFromLineCore(parent, line, clientFact, product, productFact, equipment, organisations, isTsaKnownClient, isTsaPolicyRequired);
			}
		}

		(IDangerousGoodsFact firstDangerousGoodsFact, bool hasMultipleDangerousGoods) GetDangerousGoodsFactDetails(OrgSupplierPart product)
		{
			var hasMultipleDangerousGoods = false;
			IDangerousGoodsFact firstDangerousGoodsFact = null;
			var firstDangerousGood = product.UNDGs.OrderBy(dg => dg.DI_IMOClass).ThenBy(dg => dg.UNDGSubstance?.DG_Identifier).FirstOrDefault();
			if (firstDangerousGood != null)
			{
				firstDangerousGoodsFact = new DangerousGoodsFact(firstDangerousGood.PK.ToGuid(), product.PK.ToGuid(), firstDangerousGood.UNDGSubstance?.DG_Identifier ?? string.Empty, firstDangerousGood.DI_IMOClass);
				hasMultipleDangerousGoods = product.UNDGs.Count > 1;
			}

			return (firstDangerousGoodsFact, hasMultipleDangerousGoods);
		}

		protected abstract IInputFact AddInputFactFromLineCore(
			TJob parent,
			TLineToPutaway line,
			OrganisationFact client,
			OrgSupplierPart product,
			PutawayProductFact productFact,
			EquipmentFact equipmentFact,
			Dictionary<ZGuid, OrganisationFact> organisations,
			bool isTsaKnownClient,
			bool isTsaPolicyRequired);

		INotification ProcessResults(IEnumerable<TJob> parents, BusinessObjectFactory factory, IEnumerable<TLineToPutaway> lines, ProductionRulesEngineResult result, bool useLocationConcurrencyHandling)
		{
			var putawayInstructions = result.Facts.OfType<PutawayResultFact>().ToArray();
			AddFetchHintsForProcessResults(factory, lines, putawayInstructions);

			var linesLookup = lines.ToDictionary(l => l.PK);

			using (SuspendForProcessResults(parents))
			{
				foreach (var instruction in putawayInstructions)
				{
					linesLookup.TryGetValue(instruction.InventoryPK, out var line);

					if (line != null)
					{
						if (instruction.StockQuantity < line.QuantityToPutaway)
						{
							line = (TLineToPutaway)line.Split(instruction.StockQuantity);
						}

						if (useLocationConcurrencyHandling)
						{
							var location = factory.Load<WhsLocation>(instruction.LocationPK);
							var changeIDInCache = instruction.LastAllocatedOrChangedID == null ? ZGuid.Empty : new ZGuid(instruction.LastAllocatedOrChangedID);
							if (location.WLV_LastAllocatedOrChangedID != changeIDInCache)
							{
								throw new PutawayAllocateLocationConcurrencyException();
							}
							ConcurrencyInfo.SetConcurrencyPolicy(location, nameof(WhsLocationViewSchema.WLV_LastAllocatedOrChangedID), ConcurrencyPolicy.Strict);
						}

						line.LocationPK = instruction.LocationPK;
						if (!string.IsNullOrEmpty(instruction.PalletID))
						{
							line.PalletID = instruction.PalletID;
						}
					}
				}
			}

			return
				lines.Any(i => i.QuantityToPutaway > 0 && i.LocationPK.IsEmpty) ?
				new WarningNotification(Res.GetString("43a01465-3326-4484-a1f4-66f9bbc098bd", "Some inventory did not have locations allocated, this might be due to full locations or an incomplete rule setup.")) :
				null;
		}

		protected virtual IDisposable SuspendForProcessResults(IEnumerable<TJob> parents) => null;

		void AddFetchHintsForProcessResults(BusinessObjectFactory factory, IEnumerable<TLineToPutaway> lines, IEnumerable<PutawayResultFact> putawayInstructions)
		{
			var locations = new HashSet<Guid>();
			var locationPalletIDPairs = new HashSet<(Guid, string)>();
			var currentPalletIDsOnLines = lines.ToDictionary(l => l.PK, l => l.PalletID);

			foreach (var instruction in putawayInstructions)
			{
				var locationPk = instruction.LocationPK;

				currentPalletIDsOnLines.TryGetValue(instruction.InventoryPK, out var palletIDOnInventory);
				var palletID = string.IsNullOrEmpty(instruction.PalletID) ? (string)palletIDOnInventory : instruction.PalletID;

				if (!string.IsNullOrEmpty(palletID) && locationPalletIDPairs.Add((locationPk, palletID.ToUpperInvariant())))
				{
					factory.AddFetchHint(WhsDocketLineSchema.Instance, WhsValidationHelper.GetLocationsWithStockIncludingNotYetFinalisedQuery(palletID));
				}

				if (locations.Add(locationPk))
				{
					factory.AddFetchHint(WhsLocationViewSchema.Constants.TableName, locationPk);
				}
			}

			AddFetchHintsForProcessResultsCore(factory, lines, putawayInstructions);
		}

		protected virtual void AddFetchHintsForProcessResultsCore(BusinessObjectFactory factory, IEnumerable<TLineToPutaway> lines, IEnumerable<PutawayResultFact> putawayInstructions)
		{
		}
	}
}
