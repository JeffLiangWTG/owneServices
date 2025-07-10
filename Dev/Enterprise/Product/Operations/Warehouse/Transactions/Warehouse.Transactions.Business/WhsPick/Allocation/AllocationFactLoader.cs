using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProductionRules.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.Facts;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	class AllocationFactLoader : IAllocationFactLoader
	{
		public AllocationFactLoader(IAvailableInventoryFactManagerFactory inventoryFactManagerFactory)
		{
			InventoryFactManagerFactory = Argument.NotNull(inventoryFactManagerFactory, nameof(inventoryFactManagerFactory));
		}

		IAvailableInventoryFactManagerFactory InventoryFactManagerFactory { get; }

		public IEnumerable<IEnumerable<IInputFact>> GetAllocationFacts(IEnumerable<WhsPickOrderedInventory> orderedInventories, IPickStrategy pickStrategy)
		{
			Argument.NotNull(orderedInventories, nameof(orderedInventories));
			Argument.NotNull(pickStrategy, nameof(pickStrategy));

			var inventoryFactManager = InventoryFactManagerFactory.GetNewManager();

			var orderedInventoriesArray = orderedInventories.ToArray();
			var facts = new List<IEnumerable<IInputFact>>(orderedInventoriesArray.Length);
			var organisationFacts = new Dictionary<ZGuid, OrganisationFact>();
			var productFacts = new Dictionary<(ZGuid ProductPK, ZGuid ClientPK), (AllocationProductFact ProductFact, decimal PalletSize)>();
			var locationFacts = new Dictionary<ZGuid, AllocationLocationFact>();

			AddFetchHints(orderedInventoriesArray);
			foreach (var orderedInv in orderedInventoriesArray.OrderBy(ordInv => ordInv.SerialNumber.IsEmpty))
			{
				var factsForThisOrderedInv = new List<IInputFact>();
				var dynamicPickAreaOverride = orderedInv.Pick.WP_WA_DynamicPickAreaOverride;

				var clientPK = orderedInv.GetClientPKFast(orderedInv.Owners.FirstOrDefault());
				if (clientPK.IsEmpty)
				{
					throw new ArgumentException("Ordered Inventories require at least a valid Owner.", nameof(orderedInv.Owners));
				}

				var (productFact, palletSize) = GetOrCreateProductFact(productFacts, orderedInv, clientPK, () => orderedInv.SupplierPart);

				var groupedOrderLinesByDocket = orderedInv.Owners.Where(ol => ol.QuantityNotPicked > 0).GroupBy(o => o.WE_WD);
				foreach (var docketOrderLines in groupedOrderLinesByDocket)
				{
					factsForThisOrderedInv.AddRange(CreateOrderLineFactsForDocket(docketOrderLines, organisationFacts, orderedInv, productFact));
				}

				var hasOrderLinesToAllocate = factsForThisOrderedInv.Count > 0;
				var hasInventoryLinesToAllocate = false;

				if (hasOrderLinesToAllocate)
				{
					foreach (var availInv in orderedInv.AvailableInventories.Cast<WhsPickAvailableInventory>().Where(ai => !ai.IsPickByBOMKitInventory))
					{
						var locationFact = GetOrCreateLocationFact(locationFacts, dynamicPickAreaOverride, availInv.LocationPK, () => availInv.Location);
						if (locationFact != null)
						{
							locationFact.IsAllocatedOnThisPick = locationFact.IsAllocatedOnThisPick || availInv.QuantityCommitted > 0;

							var quantity = availInv.AvailableForAllocationAlgorithm ? pickStrategy.GetQuantityUnPicked(availInv) : ZDecimal.Zero;
							if (quantity > 0)
							{
								hasInventoryLinesToAllocate = true;

								var key = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(availInv.Inventory[0], orderedInv);
								var keyWithoutOrderedInv = WhsPickAvailableInventoryCollection.GetInventoryHashCodeIncludingOrderedSerial(availInv.Inventory[0], null);
								var inventoryFact = new AvailableInventoryFact(
									availInv,
									locationFact,
									orderedInv.PK,
									pickStrategy.CanAllocateInQuantitiesDifferentToAutoAllocateQuantity(availInv),
									quantity,
									palletSize,
									qty => pickStrategy.GetAutoAllocateQuantity(qty, availInv),
									qtyAllocated =>
									{
										var inventoryUpdated = inventoryFactManager.Update(key, availInv.PK.ToGuid(), qtyAllocated);

										// Updating Quantity on Available Inventory will in turn find its sibling Available Inventories
										// via InventoryFactManager and Update their Quantities. During this recursion InventoryFactManager will
										// return an empty Collection and so we know not to do anything for these sibling Inventories.
										if (inventoryUpdated.Any())
										{
											pickStrategy.OnInventoryPicked(orderedInv, availInv, qtyAllocated);

											// If these keys differ, it means there is an ordered attribute neutral serial number.
											// In this case, we may have to update inventory without the attribute specified, but not other ordered serials.
											// It is not necessary to handle the reverse case (which would require tracking serials) as we process ordered serial numbers first.
											if (key != keyWithoutOrderedInv && inventoryFactManager.HasKeyRegistered(keyWithoutOrderedInv))
											{
												inventoryFactManager.Update(keyWithoutOrderedInv, availInv.PK.ToGuid(), qtyAllocated);
											}
										}
									});

								factsForThisOrderedInv.Add(inventoryFact);
								inventoryFactManager.Register(key, inventoryFact);
							}
						}
					}
				}

				if (hasInventoryLinesToAllocate)
				{
					facts.Add(factsForThisOrderedInv);
				}
			}

			return facts;
		}

		static void AddFetchHints(WhsPickOrderedInventory[] orderedInventories)
		{
			var firstOrderInventory = orderedInventories.FirstOrDefault();
			if (firstOrderInventory != null)
			{
				var factory = firstOrderInventory.Factory;
				var orderLinesWithUnpickedQty = orderedInventories
					.SelectMany(orderedInv => orderedInv.Owners)
					.Where(orderLine => orderLine.QuantityNotPicked > 0);

				AddJobDocAddressFetchHints(factory, orderLinesWithUnpickedQty);
				AddOrgRelatedFetchHints(factory, orderLinesWithUnpickedQty);
				AddPickableDocketFetchHints(orderedInventories);
			}
		}

		static void AddJobDocAddressFetchHints(BusinessObjectFactory factory, IEnumerable<WhsPickableDocketLine> orderLines)
		{
			var orderPKs = orderLines
				.Select(orderLine => orderLine.WE_WD)
				.Distinct();

			foreach (var orderPK in orderPKs)
			{
				factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, orderPK);
			}
		}

		static void AddOrgRelatedFetchHints(BusinessObjectFactory factory, IEnumerable<WhsPickableDocketLine> orderLines)
		{
			var orders = orderLines
				.GroupBy(orderLine => orderLine.WE_WD)
				.Select(orderLineGroup => orderLineGroup.First().PickableDocket)
				.ToArray();

			var clientPKs = orders.Select(order => order.WD_OH_Client);
			var consigneeAddresses = GetAddresses(orders, DocAddressType.ConsigneeAddress);
			var transportCoAddresses = GetAddresses(orders, DocAddressType.TransportCompanyDocumentaryAddress);
			var distributionCenterAddresses = GetAddresses(orders, DocAddressType.DistributionCentreAddress);

			AddOrgAddressFetchHints(factory,
				consigneeAddresses.Select(address => address.E2_OA_Address),
				transportCoAddresses.Select(address => address.E2_OA_Address),
				distributionCenterAddresses.Select(address => address.E2_OA_Address));

			var orgPKs = clientPKs
				.Union(consigneeAddresses.Select(address => address.OrganisationPK))
				.Union(transportCoAddresses.Select(address => address.OrganisationPK))
				.Union(distributionCenterAddresses.Select(address => address.OrganisationPK));

			FetchHintsHelper.AddFetchHintsForGlbCompanyOrgProxy(factory, orgPKs);
			FetchHintsHelper.AddFetchHintsForGlbBranchOrgProxy(factory, orgPKs);
		}

		static IEnumerable<JobDocAddress> GetAddresses(IEnumerable<WhsPickableDocket> orders, DocAddressType addressType)
		{
			return orders
				.Select(order => order.LoadJobDocAddressQuickly(addressType))
				.Where(address => address != null);
		}

		static void AddOrgAddressFetchHints(BusinessObjectFactory factory,
			IEnumerable<ZGuid> consigneeAddressPKs,
			IEnumerable<ZGuid> transportCoAddressPKs,
			IEnumerable<ZGuid> distributionCenterAddressPKs)
		{
			var addressPKs = new HashSet<ZGuid>(consigneeAddressPKs.Union(transportCoAddressPKs).Union(distributionCenterAddressPKs));
			foreach (var addressPK in addressPKs)
			{
				factory.AddFetchHint(OrgAddressSchema.PK, addressPK);
			}
		}

		static void AddPickableDocketFetchHints(IEnumerable<WhsPickOrderedInventory> orderedInventories)
		{
			foreach (WhsInventoryView inventory in orderedInventories.SelectMany(orderedInv => orderedInv.AvailableInventories.Cast<WhsPickAvailableInventory>()).SelectMany(availInv => availInv.Inventory))
			{
				inventory.LoadFetchHintsForPickLines();
			}
		}

		static AllocationLocationFact GetOrCreateLocationFact(
			Dictionary<ZGuid, AllocationLocationFact> locationFacts,
			ZGuid dynamicPickAreaOverridePk,
			ZGuid locationPk,
			Func<WhsLocation> getLocation)
		{
			if (!locationFacts.TryGetValue(locationPk, out var locationFact))
			{
				var location = getLocation();

				if (dynamicPickAreaOverridePk.IsEmpty || location.WLV_WA_PickingArea == dynamicPickAreaOverridePk)
				{
					locationFacts[locationPk] = locationFact = new AllocationLocationFact(location);
				}
				else
				{
					locationFacts[locationPk] = locationFact = null;
				}
			}

			return locationFact;
		}

		static (AllocationProductFact ProductFact, decimal PalletSize) GetOrCreateProductFact(
			Dictionary<(ZGuid ProductPK, ZGuid ClientPK), (AllocationProductFact ProductFact, decimal PalletSize)> productFacts,
			WhsPickOrderedInventory orderedInv,
			ZGuid clientPk,
			Func<OrgSupplierPart> getProduct)
		{
			var productPk = orderedInv.SupplierPartPK;
			var partRelationKey = (productPk, clientPk);

			if (!productFacts.TryGetValue(partRelationKey, out var productFactAndPalletSize))
			{
				var part = getProduct();
				var product = WhsProduct.GetWhsProduct(part);
				var partRelation = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(clientPk, OrgPartRelation.RelationshipTypes.Owner);
				var warehousePk = orderedInv.Pick.WP_WW_Whs;

				var paramsByWhsAndClient = product.GetParamsByWhsAndClient(warehousePk, clientPk);
				var isDynamic = (paramsByWhsAndClient?.W3_WA_DynamicPickFaceArea.IsValid ?? false) && orderedInv.OrderedHeldCode.IsEmpty;
				var hasPickFace = product.PickFaces.HasPickFace(clientPk, productPk, warehousePk) && orderedInv.OrderedHeldCode.IsEmpty;

				var styleSize = product.ProductStyleSize;
				var styleColor = product.ProductStyleColour;
				var styleClassification = product.ProductStyleClassification;

				var palletSize = part.OP_StockKeepingUnitPerPallet;

				productFacts[partRelationKey] = productFactAndPalletSize = (new AllocationProductFact(part, partRelation, isDynamic, hasPickFace, styleSize?.ProductStyle?.WST_Code ?? string.Empty, styleClassification?.WSS_Code ?? string.Empty, styleColor?.WSC_Code ?? string.Empty, styleSize?.WSZ_Size ?? string.Empty), palletSize);
			}

			return productFactAndPalletSize;
		}

		static IEnumerable<OrderLineFact> CreateOrderLineFactsForDocket(
			IEnumerable<WhsPickableDocketLine> docketOrderLines,
			Dictionary<ZGuid, OrganisationFact> organisationFacts,
			WhsPickOrderedInventory orderedInv,
			AllocationProductFact productFact)
		{
			var docket = (WhsPickableDocket)docketOrderLines.First().Docket;

			if (!docket.WD_RequiredDate.IsValid)
			{
				throw new FactLoadingException(Res.GetString("0dac181b-407e-4b42-ae96-e5f76ed88ca4", "Order No. {0} cannot be allocated, Required Date is not valid.", docket.WD_ExternalReference));
			}

			var clientFact = WarehouseFactsHelper.GetOrCreateOrganisationFact(organisationFacts, docket.WD_OH_Client, () => docket.Client);
			var consigneeFact = WarehouseFactsHelper.GetOrCreateOrganisationFact(organisationFacts, docket.ConsigneePK, () => docket.Consignee);
			var transportCompanyFact = WarehouseFactsHelper.GetOrCreateOrganisationFact(organisationFacts, docket.TransportCoPK, () => docket.TransportCo);
			var distributionCentreFact = WarehouseFactsHelper.GetOrCreateOrganisationFact(organisationFacts, docket.DistributionCentreDocAddress.OrganisationPK, () => docket.DistributionCentreDocAddress.Organisation);

			foreach (var orderLine in docketOrderLines)
			{
				yield return new OrderLineFact(orderLine, docket, clientFact, productFact, consigneeFact, orderedInv.PK, transportCompanyFact, distributionCentreFact);
			}
		}
	}
}
