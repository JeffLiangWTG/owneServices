using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	// tested in WhsOrderDataObjectReader - Test_CustomsSource_RejectsImportIfShortfall_*

	public static class WhsOrderCustomsErrorMessageAdvisorUS
	{
		#region GetQuantitiesRequired

		public static Results GetQuantitiesRequired(WhsPickableDocket order)
		{
			var result = new Results(order);
			result.PopulateQuantities();
			if (result.HasEnoughStock)
			{
				TryToFullfill(result);
			}
			return result;
		}

		#region TryToFullfill

		static bool TryToFullfill(Results result)
		{
			var stillHasRequiredItems = true;
			foreach (var quantity in CloneDictionaryCloningValues(result.OriginalShortFallQuants))
			{
				while (stillHasRequiredItems)
				{
					FullfillRequirement(quantity, result);
					UpdateOriginalShortfallQuantities(result.OriginalShortFallQuants, result.ProductsToOrder);
					stillHasRequiredItems = result.OriginalShortFallQuants.Any(q => q.Value > 0m);

					if (!stillHasRequiredItems || result.ShouldBreakLoop())
					{
						break;
					}
				}
				if (!stillHasRequiredItems)
				{
					break;
				}
			}
			return !stillHasRequiredItems;
		}

		static void UpdateOriginalShortfallQuantities(Dictionary<ProductWithAttributes, ZDecimal> originalShortFallQuants, Dictionary<ProductWithAttributes, ZDecimal> shortfallQuantities)
		{
			var tempDictionary = CloneDictionaryCloningValues(originalShortFallQuants);
			foreach (var tempItem in tempDictionary)
			{
				foreach (var item in shortfallQuantities)
				{
					if (ProductsMatches(tempItem.Key, item.Key))
					{
						originalShortFallQuants[tempItem.Key] = shortfallQuantities[item.Key];
						break;
					}
				}
			}
		}

		#region FullfillRequirement

		static void FullfillRequirement(KeyValuePair<ProductWithAttributes, ZDecimal> shortfallItem, Results result)
		{
			var packsToAdd = new Dictionary<ProductWithAttributes, int>();
			var matchingPacks = MatchPacks(result.PackageGroups, shortfallItem);
			var viablePacks = DetermineViablePacksToAdd(result.ProductsToOrder, matchingPacks);
			if (viablePacks.Count != 0)
			{
				var bestPack = DetermineBestPackToUseFromViablePacks(viablePacks, result);

				if (bestPack.Value != 0)
				{
					var packsAdded = ChangeQuantityRequiredBasedOnBestPack(result.ProductsToOrder, packsToAdd, bestPack);
					bestPack.Key.ChangeOnHand(packsAdded);
				}
			}
		}

		static KeyValuePair<Pack, int> DetermineBestPackToUseFromViablePacks(Dictionary<Pack, int> viablePacks, Results result)
		{
			var gradedPacks = GradePacks(viablePacks, result);
			var bestPack = gradedPacks.OrderByDescending(p => p.Value);

			return bestPack.First().Key;
		}

		static Dictionary<KeyValuePair<Pack, int>, ZDecimal> GradePacks(Dictionary<Pack, int> viablePacks, Results result)
		{
			var gradedPacks = new Dictionary<KeyValuePair<Pack, int>, ZDecimal>();
			var shortfallQuantsToUse = result.OriginalShortFallQuants.Where(q => q.Value > 0).ToArray();
			foreach (var pack in viablePacks)
			{
				var qtyOfShortfallsFixed = 0m;
				var qtyRequired = 0m;
				var qtyToOrder = 0m;
				var grade = 0m;
				foreach (var item in pack.Key.Products)
				{
					qtyToOrder += item.PerPackageQty;
					foreach (var qr in shortfallQuantsToUse)
					{
						if (ProductsMatches(qr.Key, item))
						{
							qtyOfShortfallsFixed++;
							var required = qr.Value > 0m ? qr.Value : (ZDecimal)0m;
							qtyRequired += Math.Min(item.PerPackageQty, required);
							break;
						}
					}
				}

				if (qtyToOrder != 0)
				{
					grade = (qtyRequired / qtyToOrder) * (qtyOfShortfallsFixed / shortfallQuantsToUse.Length);
				}
				gradedPacks.Add(pack, grade);
			}
			return gradedPacks;
		}

		#region ChangeQuantityRequiredBasedOnBestPack

		static int ChangeQuantityRequiredBasedOnBestPack(Dictionary<ProductWithAttributes, ZDecimal> quantitiesRequired, Dictionary<ProductWithAttributes, int> packsToAdd, KeyValuePair<Pack, int> bestPack)
		{
			var packsToAddOrRemove = HowManyPacksShouldWeAddOrRemove(quantitiesRequired, bestPack);
			foreach (var item in bestPack.Key.Products)
			{
				var productInRequired = false;
				foreach (var qr in quantitiesRequired)
				{
					if (ProductsMatches(item, qr.Key))
					{
						quantitiesRequired[qr.Key] += item.PerPackageQty * packsToAddOrRemove;
						productInRequired = true;
						var packExists = false;
						foreach (var pta in packsToAdd)
						{
							if (ProductsMatches(pta.Key, item))
							{
								packExists = true;
								packsToAdd[pta.Key] += packsToAddOrRemove;
								break;
							}
						}
						if (!packExists)
						{
							packsToAdd.Add(item, packsToAddOrRemove);
						}

						break;
					}
				}

				if (!productInRequired)
				{
					quantitiesRequired.Add(item, (item.PerPackageQty * packsToAddOrRemove));
				}
			}

			return packsToAddOrRemove;
		}

		static int HowManyPacksShouldWeAddOrRemove(Dictionary<ProductWithAttributes, ZDecimal> quantitiesRequired, KeyValuePair<Pack, int> bestPack)
		{
			var result = 1;
			foreach (var quant in quantitiesRequired)
			{
				var matchingPack = bestPack.Key.Products.Where(p => ProductsMatches(quant.Key, p)).ToArray();
				if (matchingPack.Length > 0)
				{
					var minForPack = (int)Math.Min(Math.Abs(quant.Value / matchingPack.Sum(p => p.PerPackageQty)), bestPack.Key.QtyOnHand);
					minForPack = minForPack == 0 ? 1 : minForPack;
					result = Math.Min(minForPack, result);
				}
			}

			result *= bestPack.Key.AddItem ? -1 : 1;
			return result;
		}

		#endregion

		#endregion

		#region DetermineViablePacksToAdd

		static Dictionary<Pack, int> DetermineViablePacksToAdd(Dictionary<ProductWithAttributes, ZDecimal> quantitiesRequired, List<Pack> matchingPacks)
		{
			var viablePacks = new Dictionary<Pack, int>();
			foreach (var pack in matchingPacks)
			{
				AddToViable(quantitiesRequired, viablePacks, pack);
			}

			return viablePacks;
		}

		#endregion

		#region AddToViable

		static bool AddToViable(Dictionary<ProductWithAttributes, ZDecimal> quantitiesRequired, Dictionary<Pack, int> viablePacks, Pack pack)
		{
			var perfectMatch = true;
			var itemsMatched = 0;
			var addPacks = true;
			if (pack.QtyOnHand != 0)
			{
				foreach (var product in pack.Products)
				{
					var matched = false;
					foreach (var qr in quantitiesRequired)
					{
						if (ProductsMatches(product, qr.Key)
							&& (qr.Value >= 0))
						{
							matched = true;
							itemsMatched++;
							break;
						}
					}
					perfectMatch &= matched && addPacks;
				}
				pack.AddItem = addPacks;

				if (itemsMatched >= 1)
				{
					viablePacks.Add(pack, itemsMatched);
				}
			}
			return perfectMatch;
		}

		#endregion

		#region MatchPacks

		static List<Pack> MatchPacks(List<Pack> availableStock, KeyValuePair<ProductWithAttributes, ZDecimal> shortfallItem)
		{
			var matchingPacks = new List<Pack>();
			foreach (var availablePack in availableStock)
			{
				foreach (var item in availablePack.Products)
				{
					if (ProductsMatches(shortfallItem.Key, item))
					{
						matchingPacks.Add(availablePack);
						break;
					}
				}
			}
			return matchingPacks;
		}

		#endregion

		#endregion

		#region Pack Class

		internal class Pack
		{
			public Pack()
			{
				Products = new List<ProductWithAttributes>();
			}
			public ZString GroupId;
			public ZDecimal QtyOnHand;
			public List<ProductWithAttributes> Products;
			public bool AddItem;

			public void AddPack(ProductWithAttributes product)
			{
				Products.Add(product);
			}

			public void ChangeOnHand(int packsChanged)
			{
				QtyOnHand += packsChanged;
			}
		}

		#endregion

		#endregion

		#region Results Class

		public class Results
		{
			public Results(WhsPickableDocket order)
			{
				Order = order;
				Factory = order.Factory;
				Warehouse = order.Warehouse;

				ProductsToOrder = new Dictionary<ProductWithAttributes, ZDecimal>();
				DictionaryForLoops = new Dictionary<ProductWithAttributes, ZDecimal>();
				DictionaryForPreviousLoop = new Dictionary<ProductWithAttributes, ZDecimal>();
				DictionaryForLoopBreaking = new Dictionary<ProductWithAttributes, ZDecimal>();
				QtyShortInWhs = new Dictionary<ProductWithAttributes, ZDecimal>();
				HasEnoughStock = true;
			}
			readonly WhsPickableDocket Order;
			readonly BusinessObjectFactory Factory;
			readonly WhsWarehouse Warehouse;

			public Dictionary<ProductWithAttributes, ZDecimal> ProductsToOrder;
			public bool HasEnoughStock;
			public Dictionary<ProductWithAttributes, ZDecimal> QtyShortInWhs;
			internal Dictionary<ProductWithAttributes, ZDecimal> OriginalShortFallQuants;
			internal Dictionary<ProductWithAttributes, ZDecimal> DictionaryForLoops;
			internal Dictionary<ProductWithAttributes, ZDecimal> DictionaryForPreviousLoop;
			internal Dictionary<ProductWithAttributes, ZDecimal> DictionaryForLoopBreaking;
			internal List<Pack> PackageGroups;

			#region PopulateQuantities

			public void PopulateQuantities()
			{
				var quantitiesRequired = new Dictionary<ProductWithAttributes, ZDecimal>();
				foreach (WhsPickOrderedInventory inv in Order.Pick.OrderedInventories)
				{
					if (inv.QuantityShort > 0)
					{
						var item = ProductWithAttributes.GetProductWithAttributes(inv);
						if (quantitiesRequired.ContainsKey(item))
						{
							quantitiesRequired[item] += inv.QuantityShort;
						}
						else
						{
							quantitiesRequired.Add(item, inv.QuantityShort);
						}
					}
				}
				ProductsToOrder = OriginalShortFallQuants = DictionaryForLoops = quantitiesRequired;
				PopulatePackageGroups();
			}

			#region PopulatePackageGroups

			void PopulatePackageGroups()
			{
				PackageGroups = new List<Pack>();
				foreach (var line in Order.Lines.OrderBy(o => o.WE_LineNo))
				{
					var ai = GetAllAvailableInventories(line);
					foreach (var inv in ai)
					{
						var packHeader = new Pack();
						packHeader.QtyOnHand = -1;
						var alreadyExists = PackageGroups.Find(p => (inv.PackageGroupId.IsEmpty && p.GroupId == (inv.WI_OP_PartNum + inv.PerPackageQty)) || inv.PackageGroupId == p.GroupId) != null;
						if (inv.PackageGroupId.IsEmpty && !alreadyExists)
						{
							var packFooter = new Pack();
							var product = new ProductWithAttributes(inv.Client.PK, inv.WI_OP, inv.WI_PartAttrib1, inv.WI_PartAttrib2, inv.WI_PartAttrib3, inv.WI_SerialNumber,
								inv.WI_ExpiryDate, inv.WI_PackingDate, inv.WI_BondedEntryKey, inv.WI_AllocationKey, inv.PerPackageQty);

							packHeader.GroupId = inv.WI_OP_PartNum;
							var perPackage = inv.PerPackageQty != 0 ? inv.PerPackageQty : 1;
							packFooter.QtyOnHand = (int)(inv.WI_AvailableToPickQuantity / perPackage);

							if (packHeader.QtyOnHand == -1 || packHeader.QtyOnHand > packFooter.QtyOnHand)
							{
								packHeader.QtyOnHand = packFooter.QtyOnHand;
							}
							else if (packHeader.QtyOnHand == 0)
							{
								packHeader.QtyOnHand = packFooter.QtyOnHand;
							}
							else if (packHeader.QtyOnHand != packFooter.QtyOnHand)
							{
								throw new InvalidOperationException("If you got in here - Good luck debugging.");
							}
							packHeader.AddPack(product);
							PackageGroups.Add(packHeader);
						}
						else if (!alreadyExists)
						{
							var packageGroupIDContent = USBondedHelper.GetDistinctPackageGroupIDInventories(Factory, Warehouse, inv.PackageGroupId);
							var inventoryMatchingPackageGroupID = USBondedHelper.GetInventoryMatchingPackageGroupID(Factory, Warehouse, inv.PackageGroupId);
							packHeader.GroupId = inv.PackageGroupId;
							foreach (var product in packageGroupIDContent)
							{
								var matchingInv = inventoryMatchingPackageGroupID.First(p => p.WI_OP == product.Key.ProductPK);
								var qtyOnHand = matchingInv.WI_AvailableToPickQuantity / matchingInv.PerPackageQty;
								if (packHeader.QtyOnHand == -1 || packHeader.QtyOnHand > qtyOnHand)
								{
									packHeader.QtyOnHand = qtyOnHand;
								}
								packHeader.AddPack(product.Key);
							}
							PackageGroups.Add(packHeader);
						}
					}
				}
				CheckPackageGroupsHaveEnoughStock(PackageGroups);
			}

			#region GetAllAvailableInventories

			WhsInventoryView[] GetAllAvailableInventories(WhsDocketLine line)
			{
				var inventoryQuery = new ZQuery(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);
				inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_OP, line.WE_OP);
				inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_WW_Whs, Order.WD_WW_Whs);
				inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_BondedEntryKey, line.WE_BondedEntryKey);

				return Order.Factory.Load<WhsInventoryView>(inventoryQuery);
			}

			#endregion

			#region CheckPackageGroupsHaveEnoughStock

			void CheckPackageGroupsHaveEnoughStock(List<Pack> packageGroupsUsableByOrderedItems)
			{
				foreach (var item in OriginalShortFallQuants)
				{
					var orderedItems = Order.Pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Where(oi =>
						oi.Product.Parent.PK == item.Key.ProductPK &&
						(oi.BondedEntryKey.IsEmpty || oi.BondedEntryKey.EqualsIgnoringCase(item.Key.BondedEntryKey)));
					var shortfall = orderedItems.Sum(oi => oi.QuantityShort);
					var packsContainingItem = packageGroupsUsableByOrderedItems.Where(pack => pack.Products.Any(product => ProductsMatches(item.Key, product)));
					var sumOfItems = packsContainingItem.Sum(pack => pack.QtyOnHand * pack.Products.First(product => ProductsMatches(item.Key, product)).PerPackageQty);

					if (shortfall > sumOfItems)
					{
						HasEnoughStock = false;
						QtyShortInWhs.Add(item.Key, (sumOfItems - shortfall));
					}
				}
			}

			#endregion

			#endregion

			#endregion

			#region ShouldBreakLoop

			public bool ShouldBreakLoop()
			{
				var result = ShortFallLoopsAreTheSame();
				if (!result)
				{
					DictionaryForLoopBreaking = CloneDictionaryCloningValues(DictionaryForPreviousLoop);
					DictionaryForPreviousLoop = CloneDictionaryCloningValues(DictionaryForLoops);
				}
				return result;
			}

			bool ShortFallLoopsAreTheSame()
			{
				var isEqual = DictionaryForLoopBreaking.Count != 0;
				foreach (var sq in DictionaryForLoops)
				{
					foreach (var pq in DictionaryForLoopBreaking)
					{
						if (ProductsMatches(sq.Key, pq.Key))
						{
							isEqual = sq.Value == pq.Value;
							break;
						}
					}
					if (!isEqual)
					{
						break;
					}
				}

				return isEqual;
			}

			#endregion
		}

		#endregion

		#region CloneDictionaryCloningValues

		static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>
			(Dictionary<TKey, TValue> original)
		{
			var newDictionary = new Dictionary<TKey, TValue>();
			foreach (var entry in original)
			{
				newDictionary.Add(entry.Key, entry.Value);
			}
			return newDictionary;
		}

		#endregion

		#region ProductsMatches

		public static bool ProductsMatches(ProductWithAttributes product1, ProductWithAttributes product2)
		{
			return (product1.ProductPK == product2.ProductPK)
				&& (product1.BondedEntryKey.IsEmpty || product1.BondedEntryKey.EqualsIgnoringCase(product2.BondedEntryKey))
				&& (product1.AllocationKey.IsEmpty || product1.AllocationKey.EqualsIgnoringCase(product2.AllocationKey))
				&& (product1.SerialNumber.IsEmpty || product1.SerialNumber.EqualsIgnoringCase(product2.SerialNumber))
				&& (product1.ExpiryDate.IsEmpty || product1.ExpiryDate == product2.ExpiryDate)
				&& (product1.PackingDate.IsEmpty || product1.PackingDate == product2.PackingDate)
				&& (product1.PartAttrib1.IsEmpty || product1.PartAttrib1.EqualsIgnoringCase(product2.PartAttrib1))
				&& (product1.PartAttrib2.IsEmpty || product1.PartAttrib2.EqualsIgnoringCase(product2.PartAttrib2))
				&& (product1.PartAttrib3.IsEmpty || product1.PartAttrib3.EqualsIgnoringCase(product2.PartAttrib3));
		}

		#endregion	
	}
}
