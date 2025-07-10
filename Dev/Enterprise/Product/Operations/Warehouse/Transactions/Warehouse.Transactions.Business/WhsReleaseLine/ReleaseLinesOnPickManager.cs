using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class ReleaseLinesOnPickManager
	{
		#region Constructor

		ReleaseLinesOnPickManager()
		{
		}

		static ReleaseLinesOnPickManager GetCache(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ReleaseLinesOnPickManager|GetCache", () => new ReleaseLinesOnPickManager());
		}

		#endregion

		#region GetUnreleasedQuantityAndBuildCacheIfNeeded

		public static ZDecimal GetUnreleasedQuantityAndBuildCacheIfNeeded(BusinessObjectFactory factory, WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine)
		{
			ValidateArguments(releaseLine, orderLine, factory);

			var attributesKey = WhsReleaseLineCollection.GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine, orderLine);
			return GetUnreleasedQuantityAndBuildCacheIfNeeded(orderLine, attributesKey);
		}

		public static ZDecimal GetUnreleasedQuantityAndBuildCacheIfNeeded(WhsPickableDocketLine orderLine, string attributesKey)
		{
			Argument.NotNull(orderLine, nameof(orderLine));
			Argument.NotNull(attributesKey, nameof(attributesKey));

			var factory = orderLine.Factory;
			var cache = GetCache(factory);
			return cache.GetUnreleasedQuantityAndBuildCacheIfNeededFromCache(orderLine, attributesKey);
		}

		ZDecimal GetUnreleasedQuantityAndBuildCacheIfNeededFromCache(WhsPickableDocketLine orderLine, string attributesKey)
		{
			var pickAndProductKey = PickProductAndClientKey.GetKey(orderLine);

			Dictionary<string, ZDecimal> unreleasedQuantities;
			if (!PickLinesUnreleasedByAttributes.TryGetValue(pickAndProductKey, out unreleasedQuantities))
			{
				PickLinesUnreleasedByAttributes[pickAndProductKey] = unreleasedQuantities = new Dictionary<string, ZDecimal>();
			}

			ZDecimal unReleasedQty;
			if (!unreleasedQuantities.TryGetValue(attributesKey, out unReleasedQty))
			{
				unreleasedQuantities[attributesKey] = unReleasedQty = 0m;
			}

			return unReleasedQty;
		}

		static void ValidateArguments(WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine, BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			ValidateArguments(releaseLine, orderLine);
		}

		static void ValidateArguments(WhsReleaseLineCollection releaseLines, WhsPickableDocketLine orderLine, BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(releaseLines, nameof(releaseLines));
			Argument.NotNull(orderLine, nameof(orderLine));

			if (!releaseLines.IsForOrderLine(orderLine))
			{
				throw new InvalidOperationException("Should not pass in a Release Line Collection that is not related to the Order Line.");
			}
		}

		static void ValidateArguments(WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine)
		{
			Argument.NotNull(releaseLine, nameof(releaseLine));
			Argument.NotNull(orderLine, nameof(orderLine));

			if (!releaseLine.IsForOrderLine(orderLine))
			{
				throw new InvalidOperationException("Should not pass in a Release Line that is not related to the Order Line.");
			}
		}

		#endregion

		#region PokeOtherReleaseLineCollections

		public static void PokeOtherReleaseLineCollections(WhsPick pick, WhsPickableDocketLine orderLine)
		{
			Argument.NotNull(pick, nameof(pick));
			Argument.NotNull(orderLine, nameof(orderLine));

			if (!pick.IsBuildingAllReleaseLinesForPickSuspended)
			{
				var cache = GetCache(pick.Factory);
				cache.PokeOtherReleaseLineCollectionsCore(pick, orderLine);
			}
		}

		void PokeOtherReleaseLineCollectionsCore(WhsPick pick, WhsPickableDocketLine orderLine)
		{
			if (PicksAlreadyPokedOtherReleaseLinesFor.Add(pick))
			{
				// We build all Release Lines Collections on the Pick because otherwise the Unreleased Qty on RCA Lines may increase if extra collections were built later instead
				foreach (WhsPickableDocketLine line in pick.Orders.Cast<WhsPickableDocket>().SelectMany(o => o.Lines.Where(l => l != orderLine)))
				{
					PokeReleaseLines(line);
				}
			}
		}

		WhsReleaseLineCollection PokeReleaseLines(WhsPickableDocketLine line) => line.ReleaseLines;

		HashSet<WhsPick> PicksAlreadyPokedOtherReleaseLinesFor => picksAlreadyPokedOtherReleaseLinesFor ?? (picksAlreadyPokedOtherReleaseLinesFor = new HashSet<WhsPick>());
		HashSet<WhsPick> picksAlreadyPokedOtherReleaseLinesFor;

		#endregion

		#region AddInventoryFetchHintsIfRequired

		public static void AddInventoryFetchHintsIfRequired(WhsPick pick)
		{
			Argument.NotNull(pick, nameof(pick));

			var cache = GetCache(pick.Factory);
			cache.AddInventoryFetchHintsIfRequiredCore(pick);
		}

		void AddInventoryFetchHintsIfRequiredCore(WhsPick pick)
		{
			if (PicksAlreadyAddedInventoryDocketFetchHintsFor.Add(pick))
			{
				// tested in ReleaseEntryFormTest.FinaliseOrderAndPickButtonPerformance_Click_WithStockCommittedToAdjustments, and ReleaseEntryFormTest.FinaliseOrderButtonPerformance_Click_WithStockCommittedToTransfers
				// since we are going to Load Pick lines for all Orders on the Pick we must fetch hint the pick lines so they are loaded in one DB Hit. Thankfully using OrderedInventory does this for us. After which we
				// can load the Pick Lines to get the Inventory Lines to add a fetch hint to load their Dockets.
				var inventoryLinePKs = pick.GetAllPickLines().Select(pl => pl.WZ_WE_InventoryLine)
						.Distinct();

				var inventoryLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
				inventoryLineQuery.AddToFilter(WhsDocketLineSchema.PK, inventoryLinePKs);

				var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
				docketQuery.AddSubQuery(inventoryLineQuery, JoinCondition.And);

				pick.Factory.AddFetchHint(WhsDocketSchema.Instance, docketQuery);
			}
		}

		HashSet<WhsPick> PicksAlreadyAddedInventoryDocketFetchHintsFor => picksAlreadyAddedInventoryDocketFetchHintsFor ?? (picksAlreadyAddedInventoryDocketFetchHintsFor = new HashSet<WhsPick>());
		HashSet<WhsPick> picksAlreadyAddedInventoryDocketFetchHintsFor;

		#endregion

		#region NotifyAttributeChange

		// Tested where consumed in WhsReleaseLine.cs (On Attribute Change)
		public static void NotifyAttributeChange(BusinessObjectFactory factory, WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine, AttributeParts oldAttributes)
		{
			Argument.NotNull(oldAttributes, nameof(oldAttributes));
			ValidateArguments(releaseLine, orderLine, factory);

			bool lineWithAttributeChangeHasUnits = releaseLine.Quantity != 0m;
			if (lineWithAttributeChangeHasUnits)
			{
				var oldAttributesKey = WhsReleaseLineCollection.GetKeyForLineWithoutReleaseCapturedAttribs(oldAttributes, orderLine);
				var newAttributesKey = WhsReleaseLineCollection.GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine, orderLine);

				if (oldAttributesKey != newAttributesKey) // no need to reduce and increase the exact same attribute combination
				{
					var cache = GetCache(factory);
					cache.NotifyChange(releaseLine, orderLine, oldAttributes.PartAttribute1, oldAttributes.PartAttribute2, oldAttributes.PartAttribute3, oldAttributes.SerialNumber, oldAttributes.ExpiryDate, oldAttributes.PackingDate, oldAttributesKey, -releaseLine.Quantity);
					cache.NotifyChange(releaseLine, orderLine, releaseLine.PartAttribute1, releaseLine.PartAttribute2, releaseLine.PartAttribute3, releaseLine.SerialNumber, releaseLine.ExpiryDate, releaseLine.PackingDate, newAttributesKey, releaseLine.Quantity);
				}
			}
		}

		#endregion

		#region NotifyChange

		// Tested where consumed in WhsReleaseLine.cs (On Quantity Changed)
		public static void NotifyChange(BusinessObjectFactory factory, WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine, ZDecimal difference)
		{
			if (difference != 0m)
			{
				ValidateArguments(releaseLine, orderLine, factory);

				var attributesKey = WhsReleaseLineCollection.GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine, orderLine);
				var cache = GetCache(factory);
				cache.NotifyChange(releaseLine, orderLine, releaseLine.PartAttribute1, releaseLine.PartAttribute2, releaseLine.PartAttribute3, releaseLine.SerialNumber, releaseLine.ExpiryDate, releaseLine.PackingDate, attributesKey, difference);
			}
		}

		// Tested where consumed in WhsPickAvailableInventory.cs (On PickLineQuantity Changed)
		public static void NotifyChange(BusinessObjectFactory factory, WhsPickLine pickLine, ZDecimal difference)
		{
			Argument.NotNull(pickLine, nameof(pickLine));
			var inventory = Argument.NotNull(pickLine.InventoryLine, nameof(pickLine.InventoryLine));

			var orderLine = (WhsPickableDocketLine)pickLine.DocketLine;
			NotifyChange(factory, orderLine, AttributeParts.New(inventory), difference);
		}

		// Tested where consumed in WhsPickAvailableInventory.cs (On PickLineQuantity Changed)
		// Called directly in the Pick By BOM
		public static void NotifyChange(BusinessObjectFactory factory, WhsPickableDocketLine orderLine, AttributeParts attributesWithNoRCAs, ZDecimal difference)
		{
			if (difference != 0m)
			{
				Argument.NotNull(factory, nameof(factory));
				Argument.NotNull(orderLine, nameof(orderLine));
				Argument.NotNull(attributesWithNoRCAs, nameof(attributesWithNoRCAs));

				var key = PickProductAndClientKey.GetKey(orderLine);
				var cache = GetCache(factory);
				var pickableDocket = orderLine.PickableDocket;

				if (orderLine.IsReleaseLineCollectionBuilt)
				{
					cache.GetUnreleasedQuantityAndBuildCacheIfNeededFromCache(orderLine, attributesWithNoRCAs.Key);
					var unReleasedQuantities =
								GetUnReleasedQuantitiesWithExpandedErrorLog(
									key, cache.PickLinesUnreleasedByAttributes,
									pickableDocket?.WD_DocketID ?? (NoResString)"No WhsOrder Number found.", orderLine.PK,
									attributesWithNoRCAs.PartAttribute1, attributesWithNoRCAs.PartAttribute2, attributesWithNoRCAs.PartAttribute3, attributesWithNoRCAs.SerialNumber,
									attributesWithNoRCAs.ExpiryDate, attributesWithNoRCAs.PackingDate, difference,
									attributesWithNoRCAs.Key,
									null, cache.ReleaseLinesByProductAndClient); // should always exist if you're calling NotifyChange()

					HashSet<WhsReleaseLineCollection> groupedReleaseLines;
					if (cache.ReleaseLinesByProductAndClient.TryGetValue(key, out groupedReleaseLines))
					{
						// In case PackableItemParents collection is hooked by packing we need to suspend all changes until the end.
						using (pickableDocket.SuspendPackableItemParentsCountChanged())
						{
							// If only one Order has this attribute combo, we can reduce the quantity released on one of the order lines for that order
							// If there are Release Captured Attributes we may still have to perform the step below - either for the whole quantity or part of it
							var quantityCouldNotUpdate = cache.UpdateOrderIfOnlyOneMatching(pickableDocket, orderLine, attributesWithNoRCAs, difference, unReleasedQuantities, groupedReleaseLines);
							if (quantityCouldNotUpdate != 0m)
							{
								// If not, we cannot know where to reduce quantity from/where to add it to, so we instead alter the Unreleased quantity and allow the user to decide
								cache.UpdateUnreleasedQty(quantityCouldNotUpdate, orderLine, unReleasedQuantities, attributesWithNoRCAs, groupedReleaseLines);
							}
						}
					}
					else
					{
						unReleasedQuantities[attributesWithNoRCAs.Key] += difference;
					}
				}
				else
				{
					var pick = pickableDocket.Pick;
					if (pick != null && pick.IsAutoAllocatingItemsSemaphore.IsSuspended)
					{
						pick.AddAutoAllocatedPickableDocketLine(orderLine, difference);
					}
					else
					{
						WhsReleaseLine.UpdateOrderTotalsAndCalculateExtendedLinePrice(orderLine, difference);
					}
				}
			}
		}

		#region UpdateOrderIfOnlyOneMatching

		ZDecimal UpdateOrderIfOnlyOneMatching(WhsPickableDocket order, WhsPickableDocketLine orderLine, AttributeParts attributesWithNoRCAs, ZDecimal difference, Dictionary<string, ZDecimal> unReleasedQuantities, HashSet<WhsReleaseLineCollection> groupedReleaseLines)
		{
			var attributesKey = attributesWithNoRCAs.Key;
			var onlyOneMatchingOrder =
				unReleasedQuantities[attributesKey] >= 0m
				&& groupedReleaseLines.All(r => r.OrderPK == orderLine.WE_WD) // look through cache first
				&& !OtherLinesOnThisPickContainThisProduct(order, orderLine.WE_OP); // look through whole Pick

			var quantityCouldNotUpdate = difference;
			if (onlyOneMatchingOrder)
			{
				using (orderLine.ReleaseLines.SuspendUnreleasedQtyChange())
				{
					if (difference > 0m)
					{
						AddOrUpdateUnreleasedQtyOnReleaseLines(quantityCouldNotUpdate, orderLine, attributesWithNoRCAs, attributesKey);
						quantityCouldNotUpdate = 0m;
					}
					else
					{
						quantityCouldNotUpdate = RemoveUnreleasedQtyOnReleaseLines(quantityCouldNotUpdate, orderLine, attributesWithNoRCAs, attributesKey);
					}
				}
			}

			return quantityCouldNotUpdate;
		}

		bool OtherLinesOnThisPickContainThisProduct(WhsPickableDocket order, ZGuid productPK)
		{
			return order.Pick?.Orders.Cast<WhsPickableDocket>().Any(
				o => o.PK != order.PK &&
				o.WD_OH_Client == order.WD_OH_Client &&
				o.Lines.Cast<WhsPickableDocketLine>().Any(l => !l.IsComponentLineOnSalesOrder && l.WE_OP == productPK)) ?? false;
		}

		void AddOrUpdateUnreleasedQtyOnReleaseLines(ZDecimal difference, WhsPickableDocketLine orderLine, AttributeParts attributesWithNoRCAs, string attributesKey)
		{
			Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>> releaseLineCollectionsWithSameAttribs;
			if (!ReleaseLinesByAttributesExcludingReleaseCapturedAttributes.TryGetValue(attributesKey, out releaseLineCollectionsWithSameAttribs))
			{
				ReleaseLinesByAttributesExcludingReleaseCapturedAttributes[attributesKey] = releaseLineCollectionsWithSameAttribs = new Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>>();
			}

			HashSet<WhsReleaseLine> releaseLinesInCollection;
			if (!releaseLineCollectionsWithSameAttribs.TryGetValue(orderLine.ReleaseLines, out releaseLinesInCollection))
			{
				releaseLineCollectionsWithSameAttribs[orderLine.ReleaseLines] = releaseLinesInCollection = new HashSet<WhsReleaseLine>();
			}

			var releaseLineWithNoRCAttribs = GetReleaseLineWithNoRCAttribs(attributesWithNoRCAs, releaseLinesInCollection);
			if (releaseLineWithNoRCAttribs != null)
			{
				releaseLineWithNoRCAttribs.Quantity += difference;
			}
			else
			{
				using (orderLine.ReleaseLines.SuspendSettingDefaults())
				using (orderLine.ReleaseLines.SuspendListChanged())
				{
					var newReleaseLine = orderLine.ReleaseLines.AddNew(
						attributesWithNoRCAs.PartAttribute1,
						attributesWithNoRCAs.PartAttribute2,
						attributesWithNoRCAs.PartAttribute3,
						attributesWithNoRCAs.SerialNumber,
						attributesWithNoRCAs.ExpiryDate,
						attributesWithNoRCAs.PackingDate,
						difference);

					newReleaseLine.UnreleasedQty = GetUnreleasedQuantityAndBuildCacheIfNeeded(orderLine, attributesKey);
				}
			}
		}

		ZDecimal RemoveUnreleasedQtyOnReleaseLines(ZDecimal difference, WhsPickableDocketLine orderLine, AttributeParts attributesWithNoRCAs, string attributesKey)
		{
			const WhsReleaseLine NullReleaseLine = null;

			if (difference >= 0m)
			{
				throw new ArgumentException(string.Format(Culture.Invariant, "{0} must be less than 0", nameof(difference)));
			}

			HashSet<WhsReleaseLine> releaseLinesInCollection;
			ReleaseLinesByAttributesExcludingReleaseCapturedAttributes[attributesKey].TryGetValue(orderLine.ReleaseLines, out releaseLinesInCollection);

			var quantityCouldNotUpdate = difference;
			var releaseLineWithNoRCAttribs = releaseLinesInCollection != null ? GetReleaseLineWithNoRCAttribs(attributesWithNoRCAs, releaseLinesInCollection) : null;
			if (releaseLineWithNoRCAttribs != null)
			{
				if (releaseLineWithNoRCAttribs.UnreleasedQty > 0m)
				{
					var qtyToAddToUnreleasedQty = Math.Min(Math.Abs(quantityCouldNotUpdate), releaseLineWithNoRCAttribs.UnreleasedQty);
					NotifyChange(NullReleaseLine,
						orderLine,
						attributesWithNoRCAs.PartAttribute1,
						attributesWithNoRCAs.PartAttribute2,
						attributesWithNoRCAs.PartAttribute3,
						attributesWithNoRCAs.SerialNumber,
						attributesWithNoRCAs.ExpiryDate,
						attributesWithNoRCAs.PackingDate,
						attributesWithNoRCAs.Key,
						qtyToAddToUnreleasedQty);

					quantityCouldNotUpdate += qtyToAddToUnreleasedQty;
				}

				// quantityCouldNotUpdate is negative so the below is actually subtracting
				if (releaseLineWithNoRCAttribs.Quantity + quantityCouldNotUpdate <= 0)
				{
					if (!releaseLineWithNoRCAttribs.IsDeleted && !releaseLineWithNoRCAttribs.IsPacked)
					{
						// we are over released by more than the release line quantity, so we just delete it.
						quantityCouldNotUpdate += releaseLineWithNoRCAttribs.Quantity;
						releaseLineWithNoRCAttribs.Delete();
					}
				}
				else if (quantityCouldNotUpdate < 0m)
				{
					if (!releaseLineWithNoRCAttribs.IsPacked)
					{
						// there is enough qty on the Release Line to cancel out the unreleasedqty.
						releaseLineWithNoRCAttribs.Quantity += quantityCouldNotUpdate;
						quantityCouldNotUpdate = 0m;
					}
				}
			}

			return quantityCouldNotUpdate;
		}

		static WhsReleaseLine GetReleaseLineWithNoRCAttribs(AttributeParts attributes, HashSet<WhsReleaseLine> releaseLinesInCollection)
		{
			return releaseLinesInCollection.FirstOrDefault(r =>
				r.PartAttribute1.EqualsIgnoringCase(attributes.PartAttribute1) &&
				r.PartAttribute2.EqualsIgnoringCase(attributes.PartAttribute2) &&
				r.PartAttribute3.EqualsIgnoringCase(attributes.PartAttribute3) &&
				// This check for serial number isn't actually necessary because the Dictionary that stores the hashset above is keyed by
				// Non Release Captured Serials. As serials are unique, there can only ever be one in the collection, but will the check add for consistency
				r.SerialNumber.EqualsIgnoringCase(attributes.SerialNumber) &&
				r.ExpiryDate == attributes.ExpiryDate &&
				r.PackingDate == attributes.PackingDate);
		}

		#endregion

		#region UpdateUnreleasedQty

		void UpdateUnreleasedQty(ZDecimal quantityCouldNotUpdate, WhsPickableDocketLine orderLine, Dictionary<string, ZDecimal> unReleasedQuantities, AttributeParts attributesWithNoRCAs, HashSet<WhsReleaseLineCollection> groupedReleaseLines)
		{
			const WhsReleaseLine NullReleaseLine = null;

			NotifyChange(NullReleaseLine,
				orderLine,
				attributesWithNoRCAs.PartAttribute1,
				attributesWithNoRCAs.PartAttribute2,
				attributesWithNoRCAs.PartAttribute3,
				attributesWithNoRCAs.SerialNumber,
				attributesWithNoRCAs.ExpiryDate,
				attributesWithNoRCAs.PackingDate,
				attributesWithNoRCAs.Key,
				-quantityCouldNotUpdate); // since quantityCouldNotUpdate will have the opposite sign of the increase or decrease for Unreleased Qty, we swap the sign.

			// When you de-allocate all of Product X from the Available Inventory. Product X should not be release-able for any Order, so all Release Lines for Product X should be deleted.
			// To check this is the case when the unreleased quantity is negative we can look at the sum of all release lines and
			// if this sum is less than or equal to the inverse it means we can delete all of the release lines as there are no units able to be released.
			Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>> releaseLinesCollectionsByAttributeKeys;
			if (ReleaseLinesByAttributesExcludingReleaseCapturedAttributes.TryGetValue(attributesWithNoRCAs.Key, out releaseLinesCollectionsByAttributeKeys))
			{
				var releaseLinesForThisAttributeKey = groupedReleaseLines.SelectMany(r => GetReleaseLines(releaseLinesCollectionsByAttributeKeys, r)).ToArray();
				if (releaseLinesForThisAttributeKey.Sum(rl => rl.Quantity) <= -unReleasedQuantities[attributesWithNoRCAs.Key])
				{
					// In case PackableItemParents collection is hooked by packing we need to suspend all changes until the end.
					using (SuspendPackableItemsCountChangedForAllOrders(orderLine, releaseLinesForThisAttributeKey))
					{
						foreach (var releaseLine in releaseLinesForThisAttributeKey)
						{
							releaseLine.Delete();
						}
					}

					unReleasedQuantities.Remove(attributesWithNoRCAs.Key);
				}
			}
		}

		static IEnumerable<WhsReleaseLine> GetReleaseLines(IReadOnlyDictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>> releaseLinesCollectionsByAttributeKeys, WhsReleaseLineCollection releaseLinesCollection)
		{
			HashSet<WhsReleaseLine> result;
			return releaseLinesCollectionsByAttributeKeys.TryGetValue(releaseLinesCollection, out result) ? result : Enumerable.Empty<WhsReleaseLine>();
		}

		static IDisposable SuspendPackableItemsCountChangedForAllOrders(WhsPickableDocketLine orderLine, WhsReleaseLine[] releaseLinesForThisAttributeKey)
		{
			IDisposable result = null;

			if (orderLine is WhsOrderLine)
			{
				// only suspend each order once, no need to suspend the order for the current order line as it has already been done.
				result = new DisposableList(releaseLinesForThisAttributeKey
					.Where(r => r.OrderPK != orderLine.WE_WD)
					.Distinct(new ReleaseLineByOrderComparer())
					.Select(r => r.PickableDocket.SuspendPackableItemParentsCountChanged()));
			}

			return result;
		}

		class ReleaseLineByOrderComparer : IEqualityComparer<WhsReleaseLine>
		{
			bool IEqualityComparer<WhsReleaseLine>.Equals(WhsReleaseLine x, WhsReleaseLine y) => x.OrderPK == y.OrderPK;
			int IEqualityComparer<WhsReleaseLine>.GetHashCode(WhsReleaseLine obj) => obj.OrderPK.GetHashCode();
		}

		#endregion

		void NotifyChange(WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate expiry, ZDate packing, string attributesKey, ZDecimal difference)
		{
			if (difference == 0m)
			{
				throw new ArgumentException("Should not call NotifyChange() if difference is Zero.", nameof(difference));
			}

			var key = PickProductAndClientKey.GetKey(orderLine);
			var unReleasedQuantities =
				GetUnReleasedQuantitiesWithExpandedErrorLog(
					key, PickLinesUnreleasedByAttributes,
					orderLine?.Docket?.WD_DocketID ?? (NoResString)"No WhsOrder Number found.", orderLine.PK,
					partAttrib1, partAttrib2, partAttrib3, serialNumber, expiry, packing, difference, attributesKey,
					releaseLine, ReleaseLinesByProductAndClient); // should always exist if you're calling NotifyChange()

			if (unReleasedQuantities.TryGetValue(attributesKey, out ZDecimal currentUnreleasedQtyValue))
			{
				currentUnreleasedQtyValue -= difference;
				unReleasedQuantities[attributesKey] = currentUnreleasedQtyValue;
			}
			else
			{
				currentUnreleasedQtyValue = ZDecimal.Zero;
			}

			Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>> releaseLinesToCheck;
			if (!ReleaseLinesByAttributesExcludingReleaseCapturedAttributes.TryGetValue(attributesKey, out releaseLinesToCheck))
			{
				ReleaseLinesByAttributesExcludingReleaseCapturedAttributes[attributesKey] = releaseLinesToCheck = new Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>>();
			}

			var groupedReleaseLines = ReleaseLinesByProductAndClient[key];
			foreach (var releaseLines in groupedReleaseLines)
			{
				if (releaseLinesToCheck.TryGetValue(releaseLines, out HashSet<WhsReleaseLine> releaseLinesToEdit))
				{
					foreach (var line in releaseLinesToEdit.ToArray())
					{
						line.UnreleasedQty = currentUnreleasedQtyValue;
						DeleteRedundantReleaseLinesOrClearValidationErrors(releaseLine, line, releaseLines, attributesKey, key);
					}
				}
				else if (currentUnreleasedQtyValue > 0m)
				{
					// Add zero quantity release line for the under released attribute combo
					AddZeroQuantityReleaseLine(releaseLines, partAttrib1, partAttrib2, partAttrib3, serialNumber, expiry, packing, currentUnreleasedQtyValue);
				}
			}

			// if the element was not updated because it was not in the cache we need to make the change.
			if (releaseLine != null && releaseLine.UnreleasedQty != currentUnreleasedQtyValue)
			{
				releaseLine.UnreleasedQty = currentUnreleasedQtyValue;
			}

			ReconcilePickLines(orderLine, key, attributesKey, releaseLine, difference);

			if (releaseLine != null && releaseLine.Quantity == 0 && currentUnreleasedQtyValue == 0 && !releaseLine.ParentCollection.AllowRemove)
			{
				// Delete this line as it is invalid and the user cant delete it
				releaseLine.Delete();
			}
		}

		static Dictionary<string, ZDecimal> GetUnReleasedQuantitiesWithExpandedErrorLog(
			PickProductAndClientKey key, Dictionary<PickProductAndClientKey, Dictionary<string, ZDecimal>> dictionary,
			string orderNum, ZGuid orderlinePK,
			string partAttrib1, string partAttrib2, string partAttrib3, string serialNumber, ZDate packingDate, ZDate expiryDate,
			ZDecimal difference, string attribsKey, WhsReleaseLine releaseLine,
			Dictionary<PickProductAndClientKey, HashSet<WhsReleaseLineCollection>> releaseLinesByProductAndClient)
		{
			try
			{
				return dictionary[key];
			}
			catch (KeyNotFoundException)
			{
				var errorMessage = $@"The given key was not present in the dictionary.
Current State Information:
Key: [{key.ToLogString()}]
Dictionary:
{string.Join(System.Environment.NewLine, DictionaryToString())}.

Order Number: {orderNum}
OrderLinePK: {orderlinePK}
Part Attribute 1: {partAttrib1}
Part Attribute 2: {partAttrib2}
Part Attribute 3: {partAttrib3}
Serial Number: {serialNumber}
Packing Date: {packingDate}
Expiry Date: {expiryDate}
Difference: {difference}
AttributesKey: {attribsKey}

ReleaseLine.IsDeleted: {releaseLine?.IsDeleted ?? false}
ReleaseLinesByProductAndClient contains Key: {releaseLinesByProductAndClient.ContainsKey(key)}
Does returned ReleaseLinesByProductAndClient value contain ReleaseLine: {releaseLinesByProductAndClient[key].Any(r => r.Contains(releaseLine))}";
				ErrorReporter.ReportOnce("ReleaseLinesOnPickManager_PickLinesUnreleasedByAttributes_KeyNotFound", errorMessage);
				throw;
			}

			string DictionaryToString()
			{
				var result = string.Empty;
				var index = 0;
				foreach (var kvp in dictionary)
				{
					result += $"{index++}: [{kvp.Key.ToLogString()}] <=> {string.Join(",", kvp.Value)}";
				}

				return result;
			}
		}

		void DeleteRedundantReleaseLinesOrClearValidationErrors(WhsReleaseLine lineChanged, WhsReleaseLine lineToCheck, WhsReleaseLineCollection releaseLines, string attributesKey, PickProductAndClientKey key)
		{
			if (lineToCheck != lineChanged && lineToCheck.Quantity == 0 && lineToCheck.UnreleasedQty <= 0m)
			{
				var unReleasedQuantities = PickLinesUnreleasedByAttributes[key]; // this needs to be done before deleting the Release Line
				var wasDuplicate = lineChanged != null && releaseLines.IsDuplicateReleaseLine(lineChanged);
				lineToCheck.Delete();

				if (wasDuplicate && !releaseLines.IsDuplicateReleaseLine(lineChanged))
				{
					// Line which used to be a duplicate is no longer one; add to cache and rebuild caches if neccessary (i.e. if only one order line had the old attribute combo)
					AddReleaseLineToCache(releaseLines, lineChanged, attributesKey);

					if (!unReleasedQuantities.ContainsKey(attributesKey))
					{
						unReleasedQuantities[attributesKey] = lineToCheck.UnreleasedQty;
					}

					if (!PickLinesUnreleasedByAttributes.ContainsKey(key))
					{
						PickLinesUnreleasedByAttributes[key] = unReleasedQuantities;
					}
				}
			}
			else
			{
				// Clear any Errors on this field
				lineToCheck.Validation.ValidateUnreleasedQty();
			}
		}

		static void AddZeroQuantityReleaseLine(WhsReleaseLineCollection releaseLines, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate expiry, ZDate packing, ZDecimal unreleasedQty)
		{
			// only add this Release Line if the Ordered Attributes allow it to be valid.
			if (releaseLines.IsMatchingAttributes(partAttrib1, partAttrib2, partAttrib3, serialNumber, expiry, packing))
			{
				using (releaseLines.SuspendSettingDefaults())
				using (releaseLines.SuspendListChanged())
				{
					var newReleaseLine = releaseLines.AddNew(partAttrib1, partAttrib2, partAttrib3, serialNumber, expiry, packing);
					newReleaseLine.UnreleasedQty = unreleasedQty;
				}
			}
		}

		#endregion

		#region ReconcilePickLines

		void ReconcilePickLines(WhsPickableDocketLine orderLine, PickProductAndClientKey key, string attributesKey, WhsReleaseLine releaseLine, ZDecimal difference)
		{
			var qtyToReconcile = difference;
			var isFindingPickLinesForThisOrderLine = qtyToReconcile > 0; // negative means we are are removing them from this order line
			var orderLinesOutOfSyncForThisAttributeKey = GetOrBuildOrderLinesOutOfSync(key, attributesKey);

			// 1. Try to reconcile the changes on this line with its own outstanding changes, e.g. when reducing then increasing the same release line.
			var currentOutOfSyncQty = ZDecimal.Zero;
			orderLinesOutOfSyncForThisAttributeKey.TryGetValue(orderLine, out currentOutOfSyncQty);

			var canReconcileFromItsOwnChanges = isFindingPickLinesForThisOrderLine ? currentOutOfSyncQty > 0m : currentOutOfSyncQty < 0m;
			if (canReconcileFromItsOwnChanges)
			{
				var qtyToChangeForOrderLine = Math.Min(Math.Abs(currentOutOfSyncQty), Math.Abs(qtyToReconcile));
				var qtyToAddToCache = isFindingPickLinesForThisOrderLine ? -qtyToChangeForOrderLine : qtyToChangeForOrderLine;
				orderLinesOutOfSyncForThisAttributeKey[orderLine] += qtyToAddToCache;
				qtyToReconcile += qtyToAddToCache;
			}

			// 2. Try to reconcile this order line's changes with any outstanding changes on other order lines
			if (qtyToReconcile != 0)
			{
				ReconcilePickLinesWithOutOfSyncQtysOnOtherOrderLines(orderLine, orderLinesOutOfSyncForThisAttributeKey, attributesKey, releaseLine, ref qtyToReconcile);

				// 3. Populate the cache with the outstanding quantity, to be reconciled when another line is changed
				if (qtyToReconcile != 0)
				{
					if (orderLinesOutOfSyncForThisAttributeKey.ContainsKey(orderLine))
					{
						orderLinesOutOfSyncForThisAttributeKey[orderLine] -= qtyToReconcile;
					}
					else
					{
						orderLinesOutOfSyncForThisAttributeKey[orderLine] = -qtyToReconcile;
					}
				}
			}

			CleanUpOrderLinesOutOfSyncCachesIfNeeded(key, attributesKey);
		}

		Dictionary<WhsPickableDocketLine, ZDecimal> GetOrBuildOrderLinesOutOfSync(PickProductAndClientKey key, string attributesKey)
		{
			Dictionary<string, Dictionary<WhsPickableDocketLine, ZDecimal>> orderLineOutOfSyncByAttributes;
			if (!OrderLinesOutOfSyncWithPickLines.TryGetValue(key, out orderLineOutOfSyncByAttributes))
			{
				OrderLinesOutOfSyncWithPickLines[key] = orderLineOutOfSyncByAttributes = new Dictionary<string, Dictionary<WhsPickableDocketLine, ZDecimal>>();
			}

			Dictionary<WhsPickableDocketLine, ZDecimal> orderLinesOutOfSyncForThisAttributeKey;
			if (!orderLineOutOfSyncByAttributes.TryGetValue(attributesKey, out orderLinesOutOfSyncForThisAttributeKey))
			{
				orderLineOutOfSyncByAttributes[attributesKey] = orderLinesOutOfSyncForThisAttributeKey = new Dictionary<WhsPickableDocketLine, ZDecimal>();
			}

			return orderLinesOutOfSyncForThisAttributeKey;
		}

		#region ReconcilePickLinesWithOutOfSyncQtysOnOtherOrderLines

		void ReconcilePickLinesWithOutOfSyncQtysOnOtherOrderLines(WhsPickableDocketLine orderLine, Dictionary<WhsPickableDocketLine, ZDecimal> orderLinesOutOfSyncForThisAttributeKey, string attributesKey, WhsReleaseLine releaseLine, ref ZDecimal qtyToReconcile)
		{
			var findingPickLinesForThisOrderLine = qtyToReconcile > 0; // negative means we are are removing them from this order line

			// We only want to look at order lines that are out of sync in a way that complements the order line we are trying to synchronise
			// i.e. if released quantity was increased, we want to find order lines with excess picked quantity
			bool isQtySameSign(ZDecimal outOfSyncQty) => findingPickLinesForThisOrderLine ? outOfSyncQty > 0 : outOfSyncQty < 0;
			foreach (var kvp in orderLinesOutOfSyncForThisAttributeKey.Where(o => o.Key.PK != orderLine.PK && isQtySameSign(o.Value)).ToArray())
			{
				var outOfSyncQty = kvp.Value;
				var qtyToMoveInitial = Math.Min(Math.Abs(outOfSyncQty), Math.Abs(qtyToReconcile));
				var qtyToMoveRunningTotal = qtyToMoveInitial;
				var qtyMovedFromAssembledKits = 0m;

				var orderLineToCheck = kvp.Key;
				var orderLineFrom = findingPickLinesForThisOrderLine ? orderLineToCheck : orderLine;
				var orderLineTo = findingPickLinesForThisOrderLine ? orderLine : orderLineToCheck;

				var validPickLinesToMove = GetPickLinesToMove(orderLineFrom, attributesKey, releaseLine, findingPickLinesForThisOrderLine);

				var originalKitReceiveLinesFrom = new Lazy<WhsDocketLine[]>(() => GetPickByBOMInventoryLines(orderLineFrom));
				var originalKitReceiveLinesTo = new Lazy<WhsDocketLine[]>(() => GetPickByBOMInventoryLines(orderLineTo));

				foreach (var pickLine in validPickLinesToMove)
				{
					var isPickByBOMKitPickLine = pickLine.IsPickByBOMKitPickLine();
					if (isPickByBOMKitPickLine)
					{
						_ = originalKitReceiveLinesFrom.Value;
						_ = originalKitReceiveLinesTo.Value;
					}

					var qtyToMove = Math.Min(pickLine.WZ_Units, qtyToMoveRunningTotal);
					MovePickLineToAnotherOrderLine(orderLineTo, pickLine, qtyToMove, isMovingPickedByBOMComponentLines: false);

					qtyMovedFromAssembledKits += isPickByBOMKitPickLine ? qtyToMove : 0m;
					qtyToMoveRunningTotal -= qtyToMove;
					if (qtyToMoveRunningTotal == 0)
					{
						break;
					}
				}

				if (qtyMovedFromAssembledKits > 0m)
				{
					// Move component lines + their pick lines if we have moved assembled kit pick lines
					var bomPartMap = MovePickedByBOMComponentLines(orderLineFrom, orderLineTo, qtyMovedFromAssembledKits);
					RecreateKitReceiveLines(orderLineFrom, orderLineTo, originalKitReceiveLinesFrom, originalKitReceiveLinesTo, qtyMovedFromAssembledKits, bomPartMap);
				}

				RefreshReleaseCapturedAttributes(attributesKey, orderLineToCheck);

				// update cache with new out of sync amount.
				var qtyThatCouldNotBeReconciled = (qtyToMoveInitial - qtyToMoveRunningTotal);
				var qtyToAddToCache = findingPickLinesForThisOrderLine ? -qtyThatCouldNotBeReconciled : qtyThatCouldNotBeReconciled;
				orderLinesOutOfSyncForThisAttributeKey[orderLineToCheck] += qtyToAddToCache;
				qtyToReconcile += qtyToAddToCache;

				if (qtyToReconcile == 0)
				{
					break;
				}
			}
		}

		WhsDocketLine[] GetPickByBOMInventoryLines(WhsPickableDocketLine orderLine) => orderLine.PickLines.Where(l => l.IsPickByBOMKitPickLine()).Select(l => l.InventoryLineForAvailableInventory).ToArray();

		WhsPickLine[] GetPickLinesToMove(WhsPickableDocketLine orderLineFrom, string attributesKey, WhsReleaseLine releaseLine, bool findingPickLinesForThisOrderLine)
		{
			var pickLinesToMove = orderLineFrom.PickLines.Where(pl => hasSameInventoryAttribAndNotPacked(pl)
				&& (findingPickLinesForThisOrderLine ? !pl.HasReleaseCapturedAttribs : (releaseLine == null || WhsReleaseLineCollection.GetKey(pl.InventoryLine, pl) == WhsReleaseLineCollection.GetKey(releaseLine))));

			bool hasSameInventoryAttribAndNotPacked(WhsPickLine pickLine) =>
				pickLine.WZ_Units > 0 && WhsReleaseLineCollection.GetKey(pickLine.InventoryLine) == attributesKey && IsUnpacked(pickLine); // has the same attributes and is not packed

			return pickLinesToMove.OrderByDescending(pl => !pl.IsPickedFromPutawayLocation && pl.WZ_GS_NKAssignedTo.IsEmpty).ToArray();
		}

		static bool IsUnpacked(WhsPickLine pickLine) => pickLine.IsUnpacked(pickLine.Factory);

		#region MovePickLineToAnotherOrderLine

		void MovePickLineToAnotherOrderLine(WhsPickableDocketLine orderLineToMoveTo, WhsPickLine pickLineToMove, ZDecimal qtyToMove, bool isMovingPickedByBOMComponentLines)
		{
			if (orderLineToMoveTo.PK == pickLineToMove.WZ_WE_TransactionLine)
			{
				throw new ArgumentException("Trying to move pick line to the orderline it is attached to.");
			}
			else if (qtyToMove <= 0)
			{
				throw new ArgumentException("Difference should always be greater than 0.");
			}

			if (isMovingPickedByBOMComponentLines)
			{
				MovePickLineToAnotherOrderLineWithoutRCAs(orderLineToMoveTo, pickLineToMove, qtyToMove);
			}
			else
			{
				MovePickLineToAnotherOrderLineWithRCAs(orderLineToMoveTo, pickLineToMove, qtyToMove);
			}
		}

		void MovePickLineToAnotherOrderLineWithRCAs(WhsPickableDocketLine orderLineToMoveTo, WhsPickLine pickLineToMove, ZDecimal qtyToMove)
		{
			// If there are ReleaseLines with same attributes, we have to exclude the PickLines that have been used by previous ReleaseLines.
			var pickLinesCounted = new HashSet<WhsPickLine>();
			foreach (var releaseLine in orderLineToMoveTo.ReleaseLines.Cast<WhsReleaseLine>())
			{
				// Find Pick Lines with the same Release Captured Attribs then check and merge/move depend on Quantity not met
				var pickLinesByAttributes = orderLineToMoveTo.ReleaseLines.GetPackableItems(releaseLine).Cast<WhsPickLine>();
				var pickLinesToCount = pickLinesByAttributes.Where(pl => !pickLinesCounted.Contains(pl)).ToArray();
				var sumOfPickLineUnits = pickLinesToCount.Sum(pl => pl.WZ_Units);
				pickLinesCounted.UnionWith(pickLinesByAttributes);
				var qtyNotMet = releaseLine.Quantity - sumOfPickLineUnits;

				if (qtyNotMet > 0)
				{
					var existingPickLine = pickLinesToCount.FirstOrDefault(pl => PickLineMergeablePredicate(pl, pickLineToMove));

					var qtyToMoveForThisReleaseLine = Math.Min(qtyToMove, qtyNotMet);

					if (existingPickLine != null)
					{
						pickLineToMove = MergePickLine(pickLineToMove, existingPickLine, qtyToMoveForThisReleaseLine);
						existingPickLine.Pick.OrderedInventories.GetOrderedInventoryForLine(orderLineToMoveTo).ClearPickLinesCache();
					}
					else if (pickLineToMove.WZ_Units == qtyToMoveForThisReleaseLine)
					{
						MovePickLine(orderLineToMoveTo, pickLineToMove);
					}
					else
					{
						SplitPickLine(orderLineToMoveTo, pickLineToMove, qtyToMoveForThisReleaseLine);
					}
					qtyToMove -= qtyToMoveForThisReleaseLine;
				}

				if (qtyToMove == 0)
				{
					break;
				}
			}
		}

		readonly Func<WhsPickLine, WhsPickLine, bool> PickLineMergeablePredicate = (WhsPickLine pickLine1, WhsPickLine pickLine2)
			=> pickLine1.WZ_WE_InventoryLine == pickLine2.WZ_WE_InventoryLine
				&& pickLine1.WZ_WE_OriginalPickedInventoryLine == pickLine2.WZ_WE_OriginalPickedInventoryLine
				&& pickLine1.WZ_PickedDateTime == pickLine2.WZ_PickedDateTime
				&& pickLine1.WZ_GS_NKAssignedTo == pickLine2.WZ_GS_NKAssignedTo
				&& pickLine1.IsFinalised == pickLine2.IsFinalised; // can only merge if both pick lines are Finalised or not Finalised

		void MovePickLineToAnotherOrderLineWithoutRCAs(WhsPickableDocketLine orderLineToMoveTo, WhsPickLine pickLineToMove, ZDecimal qtyToMove)
		{
			var existingPickLine = orderLineToMoveTo.PickLines.FirstOrDefault(pl => PickLineMergeablePredicate(pl, pickLineToMove));

			if (existingPickLine != null)
			{
				MergePickLine(pickLineToMove, existingPickLine, qtyToMove);
				existingPickLine.Pick.OrderedInventories.GetOrderedInventoryForLine(orderLineToMoveTo).ClearPickLinesCache();
			}
			else if (pickLineToMove.WZ_Units == qtyToMove)
			{
				MovePickLine(orderLineToMoveTo, pickLineToMove);
			}
			else
			{
				SplitPickLine(orderLineToMoveTo, pickLineToMove, qtyToMove);
			}
		}

		static WhsPickLine MergePickLine(WhsPickLine pickLineToMove, WhsPickLine existingPickLine, ZDecimal qtyToMove)
		{
			var newPickLineToMove = pickLineToMove;
			var orderLineToMoveFrom = (WhsPickableDocketLine)pickLineToMove.DocketLine;

			// We may have to recreate pick lines to get around the over pick trigger and ZSQLSaver's post order
			// This only has to be done if the changes will result in two updates (i.e. if we delete one line, it is fine)
			if (!pickLineToMove.IsInDatabase || (pickLineToMove.WZ_Units - qtyToMove == 0 && !pickLineToMove.IsReserveLine))
			{
				WhsPickLine.TransferQtyAcrossPickLines(existingPickLine, pickLineToMove, qtyToMove);

				if (pickLineToMove.WZ_Units == 0m)
				{
					pickLineToMove.Delete();
				}
			}
			else
			{
				var clonedPickLine = (WhsPickLine)pickLineToMove.Clone();
				newPickLineToMove = clonedPickLine;

				// the Original Reserved Qty is excluded from the cloning process so we must manually set it.
				((IBusinessObjectInternals)clonedPickLine).Row[WhsPickLineSchema.Constants.WZ_OriginalReservedQty] = (decimal)pickLineToMove.WZ_OriginalReservedQty;

				// we cannot delete the line if it has the Originally Picked Inventory Line, however since we have replaced the pickline there is no problem with clearing it here
				pickLineToMove.WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;
				// since the pickline's Picked Qty has been replaced with a Clone, we Delete the original PickLine
				Unpick(pickLineToMove, pl => pl.Delete());
				WhsPickLine.TransferQtyAcrossPickLines(existingPickLine, clonedPickLine, qtyToMove);
			}

			// if moving to the same ordered inventory, only need to update the cache once on the same ordered inventory
			var orderedInventoryFrom = existingPickLine.Pick.OrderedInventories.GetOrderedInventoryForLine(orderLineToMoveFrom);
			if (orderedInventoryFrom.Owners.FindByPK(existingPickLine.WZ_WE_TransactionLine) == null)
			{
				orderedInventoryFrom.ClearPickLinesCache();
			}

			return newPickLineToMove;
		}

		static void MovePickLine(WhsPickableDocketLine orderLineToMoveTo, WhsPickLine pickLine)
		{
			var orderLineFrom = (WhsPickableDocketLine)pickLine.DocketLine;
			var pick = orderLineFrom.PickableDocket.Pick;
			var orderedInventoryFrom = pick.OrderedInventories.GetOrderedInventoryForLine(orderLineFrom);

			var pickLineToMove = pickLine;
			if (pickLine.IsReserveLine)
			{
				// Must retain WZ_OriginalReservedQty
				pickLineToMove = (WhsPickLine)pickLine.Clone(); // clone will not have WZ_OriginalReservedQty

				// We cannot normally Unpick a PickLine but in this case we are transferring the entire picked Amount
				// to another Order Line and the Original Pick Line is kept merely to retain WZ_OriginalReservedQty. The
				// Original Pickline will have its Picked Qty set to Zero.
				Unpick(pickLine, pl => pl.ClearOutPickingValuesForReservedLine());
			}

			pickLineToMove.ClearReleaseCapturedAttributes();
			pickLineToMove.WZ_WE_TransactionLine = orderLineToMoveTo.PK;

			orderedInventoryFrom.ClearPickLinesCache();
			// if moving to the same ordered inventory, only need to update the cache once on the same ordered inventory
			if (!orderedInventoryFrom.Owners.Contains(orderLineToMoveTo))
			{
				pick.OrderedInventories.GetOrderedInventoryForLine(orderLineToMoveTo).ClearPickLinesCache();
			}
		}

		static void SplitPickLine(WhsPickableDocketLine orderLineToMoveTo, WhsPickLine pickLineToMove, ZDecimal qtyToMove)
		{
			var pick = orderLineToMoveTo.PickableDocket.Pick;
			var orderLineFrom = (WhsPickableDocketLine)pickLineToMove.DocketLine;
			var orderedInventoryFrom = pick.OrderedInventories.GetOrderedInventoryForLine(orderLineFrom);

			var splitLine = pickLineToMove.Split(qtyToMove);

			// remove split pick line from its order line as it was cloned
			orderLineFrom.PickLines.RemoveFromRelationship(splitLine);

			splitLine.ClearReleaseCapturedAttributes();
			splitLine.WZ_WE_TransactionLine = orderLineToMoveTo.PK;

			orderedInventoryFrom.ClearPickLinesCache();
			// if moving to the same ordered inventory, only need to update the cache once on the same ordered inventory
			if (!orderedInventoryFrom.Owners.Contains(orderLineToMoveTo))
			{
				pick.OrderedInventories.GetOrderedInventoryForLine(orderLineToMoveTo).ClearPickLinesCache();
			}
		}

		static void Unpick<T>(T pickLineWithNoQuantityLeft, Action<T> unpick)
			where T : WhsPickLine, IWhsPickLineInternals
		{
			using (pickLineWithNoQuantityLeft.IsPicked ? pickLineWithNoQuantityLeft.TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse() : null)
			{
				unpick(pickLineWithNoQuantityLeft);
			}
		}

		#endregion

		#region MovePickedByBOMComponentLines

		Dictionary<(ZGuid, ZString), OrgPartBOM> MovePickedByBOMComponentLines(WhsPickableDocketLine orderLineFrom, WhsPickableDocketLine orderLineTo, decimal pickedUnitsFromComponents)
		{
			var bomPartMap = new Dictionary<(ZGuid, ZString), OrgPartBOM>();
			if (pickedUnitsFromComponents > 0)
			{
				var bomParts = orderLineFrom.Product.Parent.BillOfMaterials.ToArray();
				foreach (var bomPart in bomParts)
				{
					bomPartMap[(bomPart.OE_OP_Component, bomPart.OE_F3_NKPackType)] = bomPart;
					var componentQtyToMove = BOMComponentQuantityHelper.GetComponentsQuantityToBuildKits(bomPart, pickedUnitsFromComponents);
					var childComponentLineFrom = orderLineFrom.ChildComponentLines.OrderByDescending(ccl => ccl.WE_TransactionQuantity).First(ccl => ccl.WE_OP == bomPart.OE_OP_Component && ccl.WE_F3_NKPackType == bomPart.OE_F3_NKPackType);
					var childComponentLineTo = orderLineTo.ChildComponentLines.FirstOrDefault(ccl => ccl.WE_OP == bomPart.OE_OP_Component && ccl.WE_F3_NKPackType == bomPart.OE_F3_NKPackType);

					if (childComponentLineTo == null && childComponentLineFrom.WE_TransactionQuantity == componentQtyToMove)
					{
						// Can move component lines + pick lines straight over
						childComponentLineFrom.WE_WD = orderLineTo.WE_WD;
						childComponentLineFrom.WE_WE_ParentDocketLine = orderLineTo.PK;
					}
					else if (childComponentLineFrom.WE_TransactionQuantity >= componentQtyToMove)
					{
						// Have to split and/or merge component lines
						MovePickedByBOMComponentLines(orderLineTo, childComponentLineFrom, childComponentLineTo, bomPart, componentQtyToMove);
					}
					else
					{
						throw new NotSupportedException("Component lines are not in the expected datashape.");
					}
				}
			}

			return bomPartMap;
		}

		void MovePickedByBOMComponentLines(WhsPickableDocketLine orderLineTo, WhsPickableDocketLine childComponentLineFrom, WhsPickableDocketLine childComponentLineTo, OrgPartBOM bomPart, ZDecimal componentQtyToMove)
		{
			if (componentQtyToMove <= 0)
			{
				throw new ArgumentException("componentQtyToMove should always be greater than 0.");
			}

			// if there is no destination Child Component Line we need to add one to transfer the qty.
			if (childComponentLineTo == null)
			{
				var order = ((WhsPickableDocket)orderLineTo.Docket);
				childComponentLineTo = order.AllLines.AddNew();
				childComponentLineTo.WE_F3_NKPackType = bomPart.OE_F3_NKPackType;
				childComponentLineTo.WE_OP = bomPart.OE_OP_Component;
				childComponentLineTo.WE_WE_ParentDocketLine = orderLineTo.PK;

				var owners = order.Pick.OrderedInventories.GetOrderedInventoryForLine(childComponentLineFrom).Owners;
				if (!owners.Contains(childComponentLineTo))
				{
					owners.Add(childComponentLineTo);
				}
			}

			childComponentLineFrom.WE_TransactionQuantity -= componentQtyToMove;
			childComponentLineTo.WE_TransactionQuantity += componentQtyToMove;

			foreach (var pickLine in childComponentLineFrom.PickLines.ToArray())
			{
				var qtyThatCanBeMoved = Math.Min(pickLine.WZ_Units, componentQtyToMove);
				MovePickLineToAnotherOrderLine(childComponentLineTo, pickLine, qtyThatCanBeMoved, isMovingPickedByBOMComponentLines: true);

				componentQtyToMove -= qtyThatCanBeMoved;
				if (componentQtyToMove == 0)
				{
					break;
				}
			}

			// if childComponentLineTo was null we might still be moving the whole component line across.
			if (childComponentLineFrom.WE_TransactionQuantity == 0m)
			{
				var query = new ZQuery(WhsPickLineSchema.WZ_WE_OriginalOrderLine, childComponentLineFrom.PK);
				var pickLinesToRecreate = childComponentLineFrom.Factory.Load<WhsPickLine>(query);
				foreach (var pickLine in pickLinesToRecreate)
				{
					var args = new BusinessObjectCloneArgs([WhsPickLineSchema.Constants.WZ_WE_OriginalOrderLine], performRowCopyWithoutTriggeringValidationAndSetter: true);
					var newPickLine = (WhsPickLine)pickLine.Clone(args);
					newPickLine.WZ_WE_OriginalOrderLine = childComponentLineTo.PK;
					
					using (((IWhsPickLineInternals)pickLine).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
					{
						pickLine.Delete();
					}
				}

				childComponentLineFrom.Delete();
			}
		}

		#endregion

		#region RecreateKitReceiveLines

		void RecreateKitReceiveLines(WhsPickableDocketLine orderLineFrom, WhsPickableDocketLine orderLineTo, Lazy<WhsDocketLine[]> originalKitReceiveLinesFrom, Lazy<WhsDocketLine[]> originalKitReceiveLinesTo, decimal qtyMovedFromAssembledKits, Dictionary<(ZGuid, ZString), OrgPartBOM> bomPartMap)
		{
			var factory = orderLineFrom.Factory;
			var pickLinesFrom = orderLineFrom.PickLines.Where(l => l.IsPickByBOMKitPickLine()).ToArray();
			var pickLinesTo = orderLineTo.PickLines.Where(l => l.IsPickByBOMKitPickLine()).ToArray();

			var bomLinksFrom = GetLinksLookup(factory, originalKitReceiveLinesFrom.Value, Array.Empty<WhsDocketLine>());

			var unpickedLinesFrom = pickLinesFrom.Where(l => !l.IsPickedFromPutawayLocation).ToArray();
			var unpickedLinesTo = pickLinesTo.Where(l => !l.IsPickedFromPutawayLocation).ToArray();
			var unpickedQtyFrom = unpickedLinesFrom.Sum(l => l.WZ_Units);
			var unpickedQtyTo = unpickedLinesTo.Sum(l => l.WZ_Units);
			var unpickedReceiveKitLineFrom = originalKitReceiveLinesFrom.Value.SingleOrDefault(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Pending);
			var unpickedReceiveKitLineTo = originalKitReceiveLinesTo.Value.SingleOrDefault(l => l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Pending);

			if (unpickedReceiveKitLineFrom != null)
			{
				CreateOrUpdateKitReceiveLine();
			}
			UpdateOrMergeKitPickLines();
			RecreateBOMLinks(factory, orderLineFrom, orderLineTo, originalKitReceiveLinesFrom, bomLinksFrom, bomPartMap);

			if (unpickedReceiveKitLineFrom != null && unpickedReceiveKitLineFrom.WE_TransactionQuantity == 0m)
			{
				unpickedReceiveKitLineFrom.Delete();
				var order = orderLineFrom.PickableDocket;
				order.Pick.RemoveDeletedPickByBOMInventoryFromCaches(unpickedReceiveKitLineFrom, orderLineFrom.WE_OP, order.WD_OH_Client);
			}

			void CreateOrUpdateKitReceiveLine()
			{
				if (unpickedReceiveKitLineTo == null)
				{
					if (qtyMovedFromAssembledKits >= unpickedReceiveKitLineFrom.WE_TransactionQuantity)
					{
						unpickedReceiveKitLineTo = unpickedReceiveKitLineFrom;
					}
					else
					{
						unpickedReceiveKitLineTo = WhsPickByBOMHelper.NewKitReceiveLine(factory, unpickedReceiveKitLineFrom.WE_WD, orderLineFrom.WE_OP, unpickedQtyTo);
						unpickedReceiveKitLineFrom.WE_ClientOrderedUnits = unpickedQtyFrom;
						unpickedReceiveKitLineFrom.WE_StockOnHand = unpickedQtyFrom;
					}
				}
				else
				{
					unpickedReceiveKitLineTo.WE_ClientOrderedUnits = unpickedQtyTo;
					unpickedReceiveKitLineTo.WE_StockOnHand = unpickedQtyTo;
					unpickedReceiveKitLineFrom.WE_ClientOrderedUnits = unpickedQtyFrom;
					unpickedReceiveKitLineFrom.WE_StockOnHand = unpickedQtyFrom;
				}
			}

			void UpdateOrMergeKitPickLines()
			{
				if (unpickedLinesTo.Length == 1)
				{
					unpickedLinesTo[0].WZ_WE_InventoryLine = unpickedReceiveKitLineTo.PK;
				}
				else if (unpickedLinesTo.Length > 1)
				{
					var firstPickLine = unpickedLinesTo.First(l => l.WZ_WE_InventoryLine == unpickedReceiveKitLineTo.PK);
					foreach (var unpickedLine in unpickedLinesTo.Where(l => l.PK != firstPickLine.PK))
					{
						firstPickLine.WZ_Units += unpickedLine.WZ_Units;
						unpickedLine.Delete();
					}
				}
			}
		}

		Dictionary<(ZGuid, ZGuid), WhsBOMInventoryPivot> GetLinksLookup(BusinessObjectFactory factory, WhsDocketLine[] kitReceiveLines, WhsDocketLine[] componentLines)
		{
			var query = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_InventoryLine, kitReceiveLines.Select(l => l.PK));
			if (componentLines.Length > 0)
			{
				query.AddToFilter(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, componentLines.Select(l => l.PK));
			}
			return factory.Load<WhsBOMInventoryPivot>(query).ToDictionary(l => (l.WIP_WE_InventoryLine, l.WIP_WE_ComponentLine));
		}

		void RecreateBOMLinks(
			BusinessObjectFactory factory,
			WhsPickableDocketLine orderLineFrom,
			WhsPickableDocketLine orderLineTo,
			Lazy<WhsDocketLine[]> originalKitReceiveLinesFrom,
			Dictionary<(ZGuid, ZGuid), WhsBOMInventoryPivot> bomLinksFrom,
			Dictionary<(ZGuid, ZString), OrgPartBOM> bomPartMap)
		{
			RecreateBomLinksCore(factory, orderLineFrom, originalKitReceiveLinesFrom.Value, bomLinksFrom, bomPartMap, isForToLine: false);

			var kitReceiveLinesTo = GetPickByBOMInventoryLines(orderLineTo);
			var bomLinksTo = GetLinksLookup(factory, kitReceiveLinesTo, orderLineTo.ChildComponentLines.ToArray());
			RecreateBomLinksCore(factory, orderLineTo, kitReceiveLinesTo, bomLinksTo, bomPartMap, isForToLine: true);
		}

		void RecreateBomLinksCore(
			BusinessObjectFactory factory,
			WhsPickableDocketLine orderLine,
			WhsDocketLine[] kitReceiveLines,
			Dictionary<(ZGuid, ZGuid), WhsBOMInventoryPivot> bomLinks,
			Dictionary<(ZGuid, ZString), OrgPartBOM> bomPartMap,
			bool isForToLine)
		{
			var componentLines = orderLine.ChildComponentLines;
			if (componentLines.Count > 0)
			{
				var kitPickLines = orderLine.PickLines;
				foreach (var receiveLine in kitReceiveLines)
				{
					var kitQuantityOnReceiveLine = kitPickLines.Where(l => l.InventoryLinePKForAvailableInventory == receiveLine.PK).Sum(l => l.WZ_Units);
					foreach (var componentLine in componentLines)
					{
						var bomPart = bomPartMap[(componentLine.WE_OP, componentLine.WE_F3_NKPackType)];
						var linkQty = BOMComponentQuantityHelper.GetComponentsQuantityToBuildKits(bomPart, kitQuantityOnReceiveLine);
						if (bomLinks.TryGetValue((receiveLine.PK, componentLine.PK), out var existingLink))
						{
							if (existingLink.WIP_ComponentQuantity != linkQty)
							{
								existingLink.Delete();

								var newLink = factory.New<WhsBOMInventoryPivot>();
								newLink.WIP_ComponentQuantity = linkQty;
								newLink.WIP_WE_InventoryLine = receiveLine.PK;
								newLink.WIP_WE_ComponentLine = componentLine.PK;
							}
						}
						else if (isForToLine)
						{
							var newLink = factory.New<WhsBOMInventoryPivot>();
							newLink.WIP_ComponentQuantity = linkQty;
							newLink.WIP_WE_InventoryLine = receiveLine.PK;
							newLink.WIP_WE_ComponentLine = componentLine.PK;
						}
					}
				}
			}
			else
			{
				foreach (var link in bomLinks.Values)
				{
					var componentLine = link.ComponentLine;
					if (componentLine == null || componentLine.IsDeleted)
					{
						link.Delete();
					}
				}
			}
		}

		#endregion

		#region RefreshReleaseCapturedAttributes

		void RefreshReleaseCapturedAttributes(string attributesKey, WhsPickableDocketLine orderLine)
		{
			Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>> releaseLinesInCollectionByAttributes;
			HashSet<WhsReleaseLine> releaseLinesToUpdate;

			if (ReleaseLinesByAttributesExcludingReleaseCapturedAttributes.TryGetValue(attributesKey, out releaseLinesInCollectionByAttributes)
				&& releaseLinesInCollectionByAttributes.TryGetValue(orderLine.ReleaseLines, out releaseLinesToUpdate))
			{
				using (orderLine.ReleaseLines.SuspendUnreleasedQtyChange())
				{
					foreach (var releaseLine in releaseLinesToUpdate)
					{
						// This will Update Release Captured Attributes, we pass in the release Line's Current Qty
						// to prevent any change to SumOfUnitsMet.
						orderLine.ReleaseLines.UpdateSumOfUnitsMet(releaseLine, releaseLine.Quantity);
					}
				}
			}
		}

		#endregion

		#endregion

		#region CleanUpOrderLinesOutOfSyncCachesIfNeeded

		void CleanUpOrderLinesOutOfSyncCachesIfNeeded(PickProductAndClientKey key, string attributesKey)
		{
			Dictionary<string, Dictionary<WhsPickableDocketLine, ZDecimal>> orderLineOutOfSyncByAttributes;
			Dictionary<WhsPickableDocketLine, ZDecimal> orderLinesOutOfSyncForThisAttributeKey;

			if (OrderLinesOutOfSyncWithPickLines.TryGetValue(key, out orderLineOutOfSyncByAttributes)
				&& orderLineOutOfSyncByAttributes.TryGetValue(attributesKey, out orderLinesOutOfSyncForThisAttributeKey))
			{
				if (orderLinesOutOfSyncForThisAttributeKey.All(o => o.Value == 0m))
				{
					if (orderLineOutOfSyncByAttributes.Count <= 1)
					{
						OrderLinesOutOfSyncWithPickLines.Remove(key);
					}
					else
					{
						orderLineOutOfSyncByAttributes.Remove(attributesKey);
					}
				}
				else
				{
					foreach (var orderLineKVP in orderLinesOutOfSyncForThisAttributeKey.Where(o => o.Value == 0m).ToArray())
					{
						orderLinesOutOfSyncForThisAttributeKey.Remove(orderLineKVP.Key);
					}
				}
			}
		}

		#endregion

		#endregion

		#region RegisterReleaseLines

		public static void RegisterReleaseLines(BusinessObjectFactory factory, WhsReleaseLineCollection releaseLines, WhsPickableDocketLine orderLine)
		{
			ValidateArguments(releaseLines, orderLine, factory);

			var cache = GetCache(factory);
			cache.RegisterReleaseLines(releaseLines, orderLine);
		}

		void RegisterReleaseLines(WhsReleaseLineCollection releaseLines, WhsPickableDocketLine orderLine)
		{
			HashSet<WhsReleaseLineCollection> groupedReleaseLines;

			var key = PickProductAndClientKey.GetKey(orderLine);
			if (!ReleaseLinesByProductAndClient.TryGetValue(key, out groupedReleaseLines))
			{
				ReleaseLinesByProductAndClient[key] = groupedReleaseLines = new HashSet<WhsReleaseLineCollection>();
			}

			if (groupedReleaseLines.Add(releaseLines))
			{
				AddReleaseLinesToCache(releaseLines, orderLine);
				releaseLines.CountChanged += ReleaseLines_CountChanged;

				AddUnreleasedQtyReleaseLineIfAttributeCombinationNotPresent(releaseLines, orderLine, key);
			}
		}

		void AddUnreleasedQtyReleaseLineIfAttributeCombinationNotPresent(WhsReleaseLineCollection releaseLines, WhsPickableDocketLine orderLine, PickProductAndClientKey key)
		{
			Dictionary<string, ZDecimal> unreleasedQuantities;
			if (PickLinesUnreleasedByAttributes.TryGetValue(key, out unreleasedQuantities))
			{
				foreach (var unreleasedQty in unreleasedQuantities.Where(o => o.Value > 0m))
				{
					var releaseLineCollectionWithSameAttributes = ReleaseLinesByAttributesExcludingReleaseCapturedAttributes[unreleasedQty.Key];
					if (!releaseLineCollectionWithSameAttributes.ContainsKey(releaseLines)) // look for unreleased attribute combinations that are not contained in 'releaseLines' Collection
					{
						var firstReleaseLineInCollection = releaseLineCollectionWithSameAttributes.First().Value.FirstOrDefault();
						if (firstReleaseLineInCollection != null)
						{
							var partAttrib1 = WhsReleaseLineCollection.GetNonReleaseCapturedAttribute(orderLine, firstReleaseLineInCollection.PartAttribute1, PartAttributeNumber.One);
							var partAttrib2 = WhsReleaseLineCollection.GetNonReleaseCapturedAttribute(orderLine, firstReleaseLineInCollection.PartAttribute2, PartAttributeNumber.Two);
							var partAttrib3 = WhsReleaseLineCollection.GetNonReleaseCapturedAttribute(orderLine, firstReleaseLineInCollection.PartAttribute3, PartAttributeNumber.Three);
							var serial = WhsReleaseLineCollection.GetNonReleaseCapturedAttribute(orderLine, firstReleaseLineInCollection.SerialNumber, PartAttributeNumber.SerialNumber);
							var expiry = firstReleaseLineInCollection.ExpiryDate;
							var packing = firstReleaseLineInCollection.PackingDate;

							AddZeroQuantityReleaseLine(releaseLines, partAttrib1, partAttrib2, partAttrib3, serial, expiry, packing, unreleasedQty.Value);
						}
					}
				}
			}
		}

		#region AddReleaseLinesToCache

		void AddReleaseLinesToCache(WhsReleaseLineCollection releaseLines, WhsPickableDocketLine orderLine)
		{
			foreach (WhsReleaseLine releaseLine in releaseLines)
			{
				var attributesKey = WhsReleaseLineCollection.GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine, orderLine);

				AddReleaseLineToCache(releaseLines, releaseLine, attributesKey);
			}
		}

		void AddReleaseLineToCache(WhsReleaseLineCollection releaseLines, WhsReleaseLine releaseLine, string attributesKey)
		{
			Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>> releaseLinesWithSameAttribs;
			if (!ReleaseLinesByAttributesExcludingReleaseCapturedAttributes.TryGetValue(attributesKey, out releaseLinesWithSameAttribs))
			{
				ReleaseLinesByAttributesExcludingReleaseCapturedAttributes[attributesKey] = releaseLinesWithSameAttribs = new Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>>();
			}

			HashSet<WhsReleaseLine> releaseLinesInCollection;
			if (!releaseLinesWithSameAttribs.TryGetValue(releaseLines, out releaseLinesInCollection))
			{
				releaseLinesWithSameAttribs[releaseLines] = releaseLinesInCollection = new HashSet<WhsReleaseLine>();
			}

			if (releaseLinesInCollection.Add(releaseLine))
			{
				releaseLine.AttributesChanged += ReleaseLine_AttributesChanged;
			}
		}

		#endregion

		#region ReleaseLines_CountChanged

		void ReleaseLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var releaseLine = (WhsReleaseLine)e.BizObject;
			var releaseLines = (WhsReleaseLineCollection)sender;
			var attributesKey = releaseLines.GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine);

			if (e.ItemAdded)
			{
				if (!releaseLines.IsDuplicateReleaseLine(releaseLine))
				{
					AddReleaseLineToCache(releaseLines, releaseLine, attributesKey);
				}
				// we still need to hook AttributesChanged on duplicate Release Lines in case they change the Attribute Combination to be unique
				else
				{
					// prevent double hooking by always re-hooking
					releaseLine.AttributesChanged -= ReleaseLine_AttributesChanged;
					releaseLine.AttributesChanged += ReleaseLine_AttributesChanged;

					Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>> releaseLinesWithSameAttribs;
					if (!ReleaseLinesByAttributesExcludingReleaseCapturedAttributes.TryGetValue(attributesKey, out releaseLinesWithSameAttribs))
					{
						ReleaseLinesByAttributesExcludingReleaseCapturedAttributes[attributesKey] = releaseLinesWithSameAttribs = new Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>>();
					}
					HashSet<WhsReleaseLine> releaseLinesInCollection;
					if (!releaseLinesWithSameAttribs.TryGetValue(releaseLines, out releaseLinesInCollection))
					{
						releaseLinesWithSameAttribs[releaseLines] = releaseLinesInCollection = new HashSet<WhsReleaseLine>();
					}
					releaseLinesInCollection.Add(releaseLine);
				}
			}
			else
			{
				RemoveReleaseLineFromCache(releaseLine, releaseLines, attributesKey);
			}
		}

		void RemoveReleaseLineFromCache(WhsReleaseLine releaseLine, WhsReleaseLineCollection releaseLines, string attributesKey)
		{
			releaseLine.AttributesChanged -= ReleaseLine_AttributesChanged;

			HashSet<WhsReleaseLine> releaseLinesInCollection;
			Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>> releaseLineCollectionsWithSameAttribs;
			if (ReleaseLinesByAttributesExcludingReleaseCapturedAttributes.TryGetValue(attributesKey, out releaseLineCollectionsWithSameAttribs)
				&& releaseLineCollectionsWithSameAttribs.TryGetValue(releaseLines, out releaseLinesInCollection)
				&& releaseLinesInCollection.Contains(releaseLine))
			{
				if (releaseLinesInCollection.Count == 1)
				{
					if (releaseLineCollectionsWithSameAttribs.Count == 1)
					{
						ReleaseLinesByAttributesExcludingReleaseCapturedAttributes.Remove(attributesKey);

						var key = PickProductAndClientKey.GetKey(releaseLine);

						if (PickLinesUnreleasedByAttributes.TryGetValue(key, out var unreleasedQuantities))
						{
							if (unreleasedQuantities.Count == 1)
							{
								if (unreleasedQuantities.ContainsKey(attributesKey))
								{
									PickLinesUnreleasedByAttributes.Remove(key);
								}
							}
							else
							{
								unreleasedQuantities.Remove(attributesKey);
							}
						}
					}
					else
					{
						releaseLineCollectionsWithSameAttribs.Remove(releaseLines);
					}
				}
				else
				{
					releaseLinesInCollection.Remove(releaseLine);
				}
			}
		}

		void ReleaseLine_AttributesChanged(object sender, UpdateAttributesEventArgs e)
		{
			var releaseLine = (WhsReleaseLine)sender;
			var releaseLines = releaseLine.ParentCollection;
			var oldAttributesKey = releaseLines.GetKeyForLineWithoutReleaseCapturedAttribs(e.OldAttributes);
			var newAttributesKey = releaseLines.GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine);

			HashSet<WhsReleaseLine> releaseLinesInCollectionForOldAttributes;
			Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>> groupedReleaseLinesForOldAttributes;

			if (oldAttributesKey != newAttributesKey // no need to remove if we're adding the same releaseLine
				&& ReleaseLinesByAttributesExcludingReleaseCapturedAttributes.TryGetValue(oldAttributesKey, out groupedReleaseLinesForOldAttributes)
				&& groupedReleaseLinesForOldAttributes.TryGetValue(releaseLines, out releaseLinesInCollectionForOldAttributes)
				&& releaseLinesInCollectionForOldAttributes.Contains(releaseLine))
			{
				if (releaseLinesInCollectionForOldAttributes.Count == 1)
				{
					if (groupedReleaseLinesForOldAttributes.Count == 1)
					{
						ReleaseLinesByAttributesExcludingReleaseCapturedAttributes.Remove(oldAttributesKey);
					}
					else
					{
						groupedReleaseLinesForOldAttributes.Remove(releaseLines);
					}
				}
				else
				{
					releaseLinesInCollectionForOldAttributes.Remove(releaseLine);
				}
			}

			HashSet<WhsReleaseLine> releaseLinesInCollectionForNewAttributes;
			Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>> groupedReleaseLinesForNewAttributes;

			if (!releaseLines.IsDuplicateReleaseLine(releaseLine))
			{
				if (!ReleaseLinesByAttributesExcludingReleaseCapturedAttributes.TryGetValue(newAttributesKey, out groupedReleaseLinesForNewAttributes))
				{
					groupedReleaseLinesForNewAttributes = new Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>>();
					ReleaseLinesByAttributesExcludingReleaseCapturedAttributes[newAttributesKey] = groupedReleaseLinesForNewAttributes;
				}

				if (!groupedReleaseLinesForNewAttributes.TryGetValue(releaseLines, out releaseLinesInCollectionForNewAttributes))
				{
					releaseLinesInCollectionForNewAttributes = new HashSet<WhsReleaseLine>();
					groupedReleaseLinesForNewAttributes[releaseLines] = releaseLinesInCollectionForNewAttributes;
				}

				releaseLinesInCollectionForNewAttributes.Add(releaseLine);
			}
		}

		#endregion

		#endregion

		#region UnRegisterReleaseLines

		public static void UnRegisterReleaseLines(BusinessObjectFactory factory, WhsReleaseLineCollection releaseLines, WhsPickableDocketLine orderLine)
		{
			ValidateArguments(releaseLines, orderLine, factory);

			var cache = GetCache(factory);
			cache.UnRegisterReleaseLines(releaseLines, orderLine);
		}

		void UnRegisterReleaseLines(WhsReleaseLineCollection releaseLines, WhsPickableDocketLine orderLine)
		{
			HashSet<WhsReleaseLineCollection> groupedReleaseLines;
			var key = PickProductAndClientKey.GetKey(orderLine);
			if (ReleaseLinesByProductAndClient.TryGetValue(key, out groupedReleaseLines) && groupedReleaseLines.Contains(releaseLines))
			{
				releaseLines.CountChanged -= ReleaseLines_CountChanged;

				if (groupedReleaseLines.Count == 1)
				{
					ReleaseLinesByProductAndClient.Remove(key);
				}
				else
				{
					groupedReleaseLines.Remove(releaseLines);
				}
			}
		}

		#endregion

		#region ReleaseLineHasDistinctNonRCAttributes

		/// <summary>
		/// So that the user always knows what Stock is Currently Unreleased (i.e. has not been re-allocated to an Order yet), we only allow
		/// deleting the Release Line if it's not the Only one of its Non-Release Captured combination.
		/// </summary>
		public static bool ReleaseLineHasDistinctNonRCAttributes(BusinessObjectFactory factory, WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine)
		{
			ValidateArguments(releaseLine, orderLine, factory);

			var cache = GetCache(factory);
			var releaseLines = releaseLine.ParentCollection;
			var nonReleaseCapturedAttributesKey = WhsReleaseLineCollection.GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine, orderLine);
			var releaseLinesToCheck = cache.ReleaseLinesByAttributesExcludingReleaseCapturedAttributes[nonReleaseCapturedAttributesKey][releaseLines];
			return releaseLinesToCheck.Count == 1 && releaseLinesToCheck.Single() == releaseLine;
		}

		#endregion

		#region PickProductAndClientKey

		class PickProductAndClientKey
		{
			#region Constructors

			PickProductAndClientKey()
			{
			}

			PickProductAndClientKey(ZGuid pick, ZGuid product, ZGuid client)
			{
				Pick = pick;
				Product = product;
				Client = client;
			}

			readonly ZGuid Pick;
			readonly ZGuid Product;
			readonly ZGuid Client;

			#endregion

			#region GetKey

			public static PickProductAndClientKey GetKey(WhsPickableDocketLine orderLine)
			{
				var order = orderLine.PickableDocket;
				return order != null
					? new PickProductAndClientKey((order is WhsOrder o) ? o.CurrentPickPKOrPickPKJustBeforeDataRefreshForClearingCache : order.WD_WP, orderLine.WE_OP, order.WD_OH_Client)
					: new PickProductAndClientKey();
			}

			public static PickProductAndClientKey GetKey(WhsReleaseLine releaseLine)
			{
				var order = releaseLine.PickableDocket;
				return order != null
					? new PickProductAndClientKey((order is WhsOrder o) ? o.CurrentPickPKOrPickPKJustBeforeDataRefreshForClearingCache : order.WD_WP, releaseLine.ProductPK, order.WD_OH_Client)
					: new PickProductAndClientKey();
			}

			#endregion

			#region Operator Overloads

			public override bool Equals(object obj) => obj != null && (obj as PickProductAndClientKey) == this;
			public static bool operator ==(PickProductAndClientKey x, PickProductAndClientKey y) => x.Pick == y.Pick && x.Product == y.Product && x.Client == y.Client;
			public static bool operator !=(PickProductAndClientKey x, PickProductAndClientKey y) => !(x == y);
			public override int GetHashCode() => Pick.GetHashCode() ^ Product.GetHashCode() ^ Client.GetHashCode();
			public string ToLogString() => string.Format((NoResString)"Pick PK: {0} | Product PK: {1} | Client PK: {2}", Pick, Product, Client);

			#endregion
		}

		#endregion

		#region Dictionaries

		Dictionary<PickProductAndClientKey, Dictionary<string, ZDecimal>> PickLinesUnreleasedByAttributes
		{
			get { return pickLinesUnreleasedByAttributes ?? (pickLinesUnreleasedByAttributes = new Dictionary<PickProductAndClientKey, Dictionary<string, ZDecimal>>()); }
		}

		// Decimal stored in the innermost dictionary is how much the OrderLine is Out of Sync by.
		Dictionary<PickProductAndClientKey, Dictionary<string, Dictionary<WhsPickableDocketLine, ZDecimal>>> OrderLinesOutOfSyncWithPickLines
		{
			get { return orderLinesOutOfSyncWithPickLines ?? (orderLinesOutOfSyncWithPickLines = new Dictionary<PickProductAndClientKey, Dictionary<string, Dictionary<WhsPickableDocketLine, ZDecimal>>>()); }
		}

		Dictionary<string, Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>>> ReleaseLinesByAttributesExcludingReleaseCapturedAttributes
		{
			get { return releaseLinesByAttributesExcludingReleaseCapturedAttributes ?? (releaseLinesByAttributesExcludingReleaseCapturedAttributes = new Dictionary<string, Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>>>()); }
		}

		Dictionary<PickProductAndClientKey, HashSet<WhsReleaseLineCollection>> ReleaseLinesByProductAndClient
		{
			get { return releaseLinesByProductAndClient ?? (releaseLinesByProductAndClient = new Dictionary<PickProductAndClientKey, HashSet<WhsReleaseLineCollection>>()); }
		}

		Dictionary<string, Dictionary<WhsReleaseLineCollection, HashSet<WhsReleaseLine>>> releaseLinesByAttributesExcludingReleaseCapturedAttributes;
		Dictionary<PickProductAndClientKey, Dictionary<string, ZDecimal>> pickLinesUnreleasedByAttributes;
		Dictionary<PickProductAndClientKey, HashSet<WhsReleaseLineCollection>> releaseLinesByProductAndClient;
		Dictionary<PickProductAndClientKey, Dictionary<string, Dictionary<WhsPickableDocketLine, ZDecimal>>> orderLinesOutOfSyncWithPickLines;

		#endregion
	}
}
