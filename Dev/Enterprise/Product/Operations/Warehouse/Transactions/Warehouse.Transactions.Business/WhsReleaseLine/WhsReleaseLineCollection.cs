using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReleaseLineCollection : NonPersistentBusinessObjectCollection<WhsReleaseLine>, IBusiness, ICollection, IEnumerable<BusinessObject>, IEnumerable, IPickLineUpdatesRefreshable
	{
		const string AttributesSeparator = "|";

		WhsReleaseLineCollection(WhsPickableDocketLine orderLine)
			: base(orderLine.Factory)
		{
			OrderLine = orderLine;
			IsInvalidated = true;
			RegisterDataRefresh();
		}

		readonly WhsPickableDocketLine OrderLine;

		#region DataRefreshSubscriber

		PickLinesDataRefreshBusSubscriber RegisterDataRefresh()
		{
			return new PickLinesDataRefreshBusSubscriber(Factory, this);
		}

		// Data refresh subscriber is needed as CollectionCountChanged event for ReleaseLinesCollection
		// does not get fired for new picklines added on data refresh
		// Tested in WhsOrderTest.TestPackableItemParents_ReleaseLinesUpdatedWhenOrderLinesChanged_OrderLineDeleted_DataRefreshDoesNotThrowException
		void IPickLineUpdatesRefreshable.Refresh(IEnumerable<WhsPickLine> pickLines)
		{
			var pickLinesToMerge = pickLines.Where(pickLine => pickLine.WZ_WE_TransactionLine == OrderLine.PK);
			foreach (var pickLine in pickLinesToMerge)
			{
				MergePickLine(pickLine);
			}
		}

		#endregion

		static BusinessObjectFactory GetFactoryWithNullCheck(WhsPickableDocketLine orderLine)
		{
			return Argument.NotNull(orderLine, nameof(orderLine)).Factory;
		}

		#region Properties

		public ZGuid OrderPK => OrderLine.WE_WD;

		#endregion

		//

		#region GetNewReleaseLinesCollection

		public static WhsReleaseLineCollection GetNewReleaseLinesCollection(WhsPickableDocketLine orderLine)
		{
			var factory = GetFactoryWithNullCheck(orderLine);
			var releaseLines = factory.GetCachedValue("WhsReleaseLineCollection|GetNewReleaseLinesCollection|" + orderLine.PK, () => new WhsReleaseLineCollection(orderLine));

			if (releaseLines.IsInvalidated && !orderLine.IsDeleted)
			{
				ReleaseLinesWrapper.SetReleaseLines(orderLine, releaseLines);
				BuildReleaseLinesCollection(orderLine, releaseLines);
			}

			return releaseLines;
		}

		sealed class ReleaseLinesWrapper : WhsPickableDocketLine.ReleaseLinesWrapper
		{
			internal static void SetReleaseLines(WhsPickableDocketLine orderLine, WhsReleaseLineCollection releaseLines) => SetReleaseLineField(orderLine, releaseLines);
		}

		static void BuildReleaseLinesCollection(WhsPickableDocketLine orderLine, WhsReleaseLineCollection releaseLines)
		{
			if (!orderLine.IsDeleted && orderLine.WE_OP.IsValid && !orderLine.IsComponentLineOnSalesOrder)
			{
				using (releaseLines.SuspendListChanged())
				{
					var pick = GetPick(orderLine);
					if (pick != null)
					{
						using (orderLine.GetValidationSuspender()) // suspend shortfall calculation since we are just building release lines
						{
							ReleaseLinesOnPickManager.AddInventoryFetchHintsIfRequired(pick);

							foreach (var pickLine in orderLine.PickLinesWithNonZeroUnits.OrderByDescending(l => !l.IsPickByBOMKitPickLine()).ToArray())
							{
								releaseLines.MergePickLine(pickLine, shouldCreateReleaseLines: true);
							}
						}
					}

					releaseLines.HookCollections();
					releaseLines.IsInvalidated = false;

					ReleaseLinesOnPickManager.RegisterReleaseLines(orderLine.Factory, releaseLines, orderLine);
					orderLine.RegisterEditableChildObject(releaseLines);

					HandleUnreleasedReleaseCapturedAttribs(orderLine);

					if (pick != null)
					{
						ReleaseLinesOnPickManager.PokeOtherReleaseLineCollections(pick, orderLine);
					}
				}
			}
			else
			{
				releaseLines.IsInvalidated = false;
			}
		}

		static void HandleUnreleasedReleaseCapturedAttribs(WhsPickableDocketLine orderLine)
		{
			if (ReleaseCapturedHelper.IsReleaseCaptured(orderLine))
			{
				foreach (var pickLine in orderLine.PickLinesWithNonZeroUnits.Where(pl => pl.HasReleaseCapturedAttribs))
				{
					var unreleaseCapturedQty = pickLine.UnreleaseCapturedQty;
					if (unreleaseCapturedQty < 0m)
					{
						ReleaseLinesOnPickManager.NotifyChange(pickLine.Factory, pickLine, unreleaseCapturedQty);
					}
				}
			}
		}

		static WhsPick GetPick(WhsPickableDocketLine orderLine) => orderLine?.PickableDocket?.Pick;

		bool IsInvalidated
		{
			get { return !IsLoaded; }
			set { IsLoaded = !value; }
		}

		#region MergePickLine

		internal void MergePickLine(WhsPickLine pickLine)
		{
			MergePickLine(pickLine, shouldCreateReleaseLines: false);
		}

		void MergePickLine(WhsPickLine pickLine, bool shouldCreateReleaseLines)
		{
			var inventory = pickLine.InventoryLine;

			if (ReleaseCapturedHelper.IsReleaseCaptured(OrderLine) && pickLine.HasReleaseCapturedAttribs)
			{
				if (shouldCreateReleaseLines)
				{
					MergeReleaseCapturedAttribs(pickLine, inventory);
				}
				else
				{
					AddPickLineToCollection(pickLine, GetKey(inventory, pickLine));
				}
			}
			else
			{
				MergeSinglePickLine(pickLine, inventory);

				if (shouldCreateReleaseLines)
				{
					CreateReleaseLineIfNeccessaryAndUpdateCaches(GetKey(inventory), pickLine, inventory);
				}
			}
		}

		#region AddPickLineToCollection

		void AddPickLineToCollection(WhsPickLine pickLine, string key)
		{
			if (!PickLinesByAttributes.TryGetValue(key, out var pickLines))
			{
				PickLinesByAttributes[key] = pickLines = new HashSet<WhsPickLine>();
			}

			pickLines.Add(pickLine);
		}

		#endregion

		#region MergeReleaseCapturedAttribs

		void MergeReleaseCapturedAttribs(WhsPickLine pickLine, WhsDocketLine inventory)
		{
			var key = GetKey(inventory, pickLine);
			AddPickLineToCollection(pickLine, key);
			CreateReleaseLineIfNeccessaryAndUpdateCaches(key, pickLine, inventory);

			if (pickLine.UnreleaseCapturedQty > 0)
			{
				CreateReleaseLineIfNeccessaryAndUpdateCaches(GetKey(inventory), pickLine, inventory);
			}
		}

		public static string GetKey(WhsDocketLine inventory, WhsPickLine pickLine)
		{
			string key = null;

			if (inventory != null && pickLine != null)
			{
				var attributes = new[]
				{
					(inventory.WE_PartAttrib1.IsEmpty ? pickLine.WZ_ReleaseCapturedPartAttrib1 : inventory.WE_PartAttrib1).ToUpper(),
					(inventory.WE_PartAttrib2.IsEmpty ? pickLine.WZ_ReleaseCapturedPartAttrib2 : inventory.WE_PartAttrib2).ToUpper(),
					(inventory.WE_PartAttrib3.IsEmpty ? pickLine.WZ_ReleaseCapturedPartAttrib3 : inventory.WE_PartAttrib3).ToUpper(),
					(inventory.WE_SerialNumber.IsEmpty ? pickLine.WZ_ReleaseCapturedSerialNumber : inventory.WE_SerialNumber).ToUpper(),
					GetDateKey(inventory.WE_ExpiryDate),
					GetDateKey(inventory.WE_PackingDate)
				};

				key = string.Join(AttributesSeparator, attributes);
			}

			return key ?? "";
		}

		#endregion

		#region MergeSinglePickLine

		void MergeSinglePickLine(WhsPickLine pickLine, WhsDocketLine inventory)
		{
			var key = GetKey(inventory);
			AddPickLineToCollection(pickLine, key);
		}

		#endregion

		#endregion

		#endregion

		#region Hook / Unhook Collections

		void HookCollections()
		{
			OrderLine.PickLinesWithNonZeroUnits.CollectionCountChange += PickLinesWithoutChildLinePickLines_CollectionCountChange;
		}

		void UnhookCollections()
		{
			OrderLine.PickLinesWithNonZeroUnits.CollectionCountChange -= PickLinesWithoutChildLinePickLines_CollectionCountChange;
		}

		#endregion

		#region ClearCollection

		public void ClearCollection()
		{
			if (OrderLine.IsDeleted)
			{
				throw new InvalidOperationException("Should not clear Release Lines on Deleted Order Line twice.");
			}

			if (!OrderLine.IsComponentLineOnSalesOrder && IsLoaded)
			{
				UnhookCollections();

				var order = OrderLine.PickableDocket;
				try
				{
					using (order.SuspendPackableItemParentsCountChanged())
					{
						RemoveAll(); // we need to remove all elements to update the ReleaseLinesCaches before Unregistering.
					}

					ReleaseLinesOnPickManager.UnRegisterReleaseLines(Factory, this, OrderLine);

					pickLinesByAttributes = null;
					releaseLinesByAttributes = null;
					releaseLinesCache = null;
					SumOfUnitsMet = 0m;

					// we don't want methods that enumerate child objects of the OrderLine to repopulate this collection.
					OrderLine.UnRegisterEditableChildObject(this);
				}
				// in case an exception is thrown we always want to mark this collection as invalidated once the Collections are unhooked.
				finally
				{
					IsInvalidated = true;
					order.RegisterOrderLineWithClearedReleaseLines(OrderLine);
				}
			}
		}

		#endregion

		#region RefreshCollection

		void RefreshCollection()
		{
			BuildReleaseLinesCollection(OrderLine, this);
			RefreshBinding();
		}

		#endregion

		#region SuspendRebuild

		/// <summary>
		/// Do NOT use this method as this will prevent the Collection from updating from enumeration or checking the Count.
		/// This is only used in the GUI for when the Grid Control is Disposed.
		/// </summary>
		public IDisposable SuspendRebuild()
		{
			bool previousState = IsInvalidated;
			return new DisposableAction(() => IsInvalidated = false, () => IsInvalidated = previousState);
		}

		#endregion

		//

		#region Allow New / Remove

		protected override bool AllowNewCore
		{
			get { return base.AllowNewCore && AllowRowManipulation && SumOfUnitsMet < OrderLine.WE_TransactionQuantity && HasUnreleasedQty; }
		}

		bool HasUnreleasedQty
		{
			get
			{
				var releaseLines = this.Cast<WhsReleaseLine>();
				return releaseLines.All(rl => rl.Quantity > 0) && releaseLines.Any(rl => rl.UnreleasedQty > 0);
			}
		}

		protected override bool AllowRemoveCore
		{
			get { return base.AllowRemoveCore && AllowRowManipulation; }
		}

		bool AllowRowManipulation => !IsPickFinalised && ReleaseCapturedHelper.IsReleaseCaptured(OrderLine)
			&& !OrderLine.PickLines.All(pl => pl.IsPickedFromPutawayLocation);

		bool IsPickFinalised => !OrderLine.IsDeleted && (GetPick(OrderLine)?.IsFinalised ?? false);

		#endregion

		#region SetDefaultsForNewChild

		protected override void SetDefaultsForNewChild(BusinessObject childBizO)
		{
			var releaseLine = (WhsReleaseLine)childBizO;
			SetAttributesToUnReleasedPickLines(releaseLine);
			SetUnitsToRemainingOrderedQuantity(releaseLine);

			base.SetDefaultsForNewChild(releaseLine);
		}

		void SetUnitsToRemainingOrderedQuantity(WhsReleaseLine releaseLine)
		{
			// During splitting of Serial Number Release Lines we may add 1000's of lines to this collection. Each Add() would try
			// to default the Qty Sent. This is completely unnecessary as Splitting serial numbers sets the Qty Sent when it splits.
			// Therefore, we suspend setting a default Quantity here.
			if (!SuspendSettingDefaultsSemaphore.IsSuspended && releaseLine.IsSerialisedProduct())
			{
				using (releaseLine.GetValidationSuspender())
				{
					releaseLine.Quantity = (Count == 0) ? OrderLine.WE_TransactionQuantity : (ZDecimal)Math.Max(0m, OrderLine.WE_TransactionQuantity - SumOfUnitsMet);
				}
			}
		}

		/// <summary>
		/// When using Release Captured Attributes we want to ease the Entering of Attributes for the user by
		/// defaulting the Attribute Combination of the first Release Line that has not been fully Allocated.
		/// </summary>
		void SetAttributesToUnReleasedPickLines(WhsReleaseLine releaseLine)
		{
			if ((IsNonCommittedCollectionElement(releaseLine) || SuspendAdditionallyForImportSemaphore.IsSuspended) && SumOfUnitsMet < OrderLine.WE_TransactionQuantity && ReleaseCapturedHelper.IsReleaseCaptured(OrderLine))
			{
				var firstUnreleasedLine = this.Cast<WhsReleaseLine>().FirstOrDefault(rl => rl.UnreleasedQty > 0m);
				if (firstUnreleasedLine != null)
				{
					CopyNonReleaseCapturedAttributesCore(releaseLine, firstUnreleasedLine, OrderLine);
				}
			}
		}

		public static void CopyNonReleaseCapturedAttributes(WhsReleaseLine releaseLineToSet, WhsReleaseLine releaseLineToCopy)
		{
			Argument.NotNull(releaseLineToSet, nameof(releaseLineToSet));
			Argument.NotNull(releaseLineToCopy, nameof(releaseLineToCopy));
			CopyNonReleaseCapturedAttributesCore(releaseLineToSet, releaseLineToCopy, releaseLineToSet.ParentCollection.OrderLine);
		}

		static void CopyNonReleaseCapturedAttributesCore(WhsReleaseLine releaseLineToSet, WhsReleaseLine releaseLineToCopy, WhsPickableDocketLine orderLine)
		{
			releaseLineToSet.SetupReleaseLine(
				GetNonReleaseCapturedAttributeCore(orderLine, releaseLineToCopy.PartAttribute1, PartAttributeNumber.One),
				GetNonReleaseCapturedAttributeCore(orderLine, releaseLineToCopy.PartAttribute2, PartAttributeNumber.Two),
				GetNonReleaseCapturedAttributeCore(orderLine, releaseLineToCopy.PartAttribute3, PartAttributeNumber.Three),
				releaseLineToCopy.ExpiryDate,
				releaseLineToCopy.PackingDate,
				GetNonReleaseCapturedSerialNumberCore(orderLine, releaseLineToCopy.SerialNumber));

			releaseLineToSet.UnreleasedQty = releaseLineToCopy.UnreleasedQty;
		}

		#endregion

		#region SuspendSettingDefaults

		public IDisposable SuspendSettingDefaults()
		{
			return new SemaphoreManager(SuspendSettingDefaultsSemaphore);
		}

		Semaphore SuspendSettingDefaultsSemaphore
		{
			get { return suspendSettingDefaultsSemaphore ?? (suspendSettingDefaultsSemaphore = new Semaphore()); }
		}

		Semaphore suspendSettingDefaultsSemaphore;

		#endregion

		#region SuspendAdditionallyForImport

		public override IDisposable SuspendAdditionallyForImport()
		{
			return new SemaphoreManager(SuspendAdditionallyForImportSemaphore);
		}

		internal Semaphore SuspendAdditionallyForImportSemaphore
		{
			get { return additionallyForImportSemaphore ?? (additionallyForImportSemaphore = new Semaphore()); }
		}

		Semaphore additionallyForImportSemaphore;

		#endregion

		#region Add

		#region AddNew

		public WhsReleaseLine AddNew(ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate expiry, ZDate packing)
		{
			return AddNew(partAttrib1, partAttrib2, partAttrib3, serialNumber, expiry, packing, ZDecimal.Zero);
		}

		public WhsReleaseLine AddNew(ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate expiry, ZDate packing, ZDecimal quantity)
		{
			if (quantity < 0m)
			{
				throw new ArgumentException("Do not attempt to Create a Release Line with a Negative Quantity.", nameof(quantity));
			}

			var result = (WhsReleaseLine)GetNew(CreateNonPersistentBusinessObject);
			result.SetupReleaseLine(partAttrib1, partAttrib2, partAttrib3, expiry, packing, serialNumber);
			this.Add(result);

			if (quantity > 0m)
			{
				result.Quantity = quantity;
				FireReleaseLineAdded(result);
			}

			return result;
		}

		/// <summary>
		/// This is only used when Splitting Serial Number Lines. We do not want to update PackableItemParents until they enter a Serial.
		/// </summary>
		protected override BusinessObject AddNewCore()
		{
			return GetNew(base.AddNewCore);
		}

		BusinessObject GetNew(Func<BusinessObject> getNew)
		{
			if (IsInAddNew)
			{
				throw new InvalidOperationException("Should not call AddNew() recursively.");
			}

			if (IsInvalidated)
			{
				throw new InvalidOperationException("Should not call AddNew() when Release Lines is Invalidated.");
			}

			try
			{
				IsInAddNew = true;
				return getNew();
			}
			finally
			{
				IsInAddNew = false;
			}
		}

		bool IsInAddNew;

		#endregion

		#region CreateNonPersistentBusinessObject

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var result = new WhsReleaseLine(OrderLine);
			ReleaseLinesCache.Add(result);

			return result;
		}

		#endregion

		#region CreateReleaseLineIfNeccessaryAndUpdateCaches

		void CreateReleaseLineIfNeccessaryAndUpdateCaches(string key, WhsPickLine pickLine, WhsDocketLine inventoryLine)
		{
			var qty = pickLine.HasReleaseCapturedAttribs ? pickLine.ReleaseCapturedQty : pickLine.UnreleaseCapturedQty;
			CreateReleaseLineIfNeccessaryAndUpdateCachesCore(key, inventoryLine, pickLine, qty, isForComponentLines: pickLine.IsPickByBOMKitPickLine());
		}

		void CreateReleaseLineIfNeccessaryAndUpdateCachesCore(string key, IPartAttributes attributesLine, WhsPickLine pickLine, ZDecimal qty, bool isForComponentLines = false)
		{
			if (!ReleaseLinesByAttributes.TryGetValue(key, out var releaseLine))
			{
				var partAttrib1 = getAttribute(a => a.PartAttrib1, pl => pl.WZ_ReleaseCapturedPartAttrib1);
				var partAttrib2 = getAttribute(a => a.PartAttrib2, pl => pl.WZ_ReleaseCapturedPartAttrib2);
				var partAttrib3 = getAttribute(a => a.PartAttrib3, pl => pl.WZ_ReleaseCapturedPartAttrib3);
				var serialNumber = getAttribute(a => a.SerialNumber, pl => pl.WZ_ReleaseCapturedSerialNumber);
				var expiry = attributesLine.ExpiryDate;
				var packing = attributesLine.PackingDate;

				releaseLine = new WhsReleaseLine(OrderLine, isForComponentLines);
				releaseLine.SetupReleaseLine(partAttrib1, partAttrib2, partAttrib3, expiry, packing, serialNumber);

				ReleaseLinesCache.Add(releaseLine);

				Add(releaseLine);

				var unreleasedQty = ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(Factory, releaseLine, OrderLine);
				if (unreleasedQty != 0m)
				{
					releaseLine.UnreleasedQty = unreleasedQty;
				}

				string getAttribute(Func<IPartAttributes, ZString> getPartAttribute, Func<WhsPickLine, ZString> getPickLineAttribute)
				{
					string partAttrib;
					var pickLineAttribute = pickLine != null ? getPickLineAttribute(pickLine) : ZString.Empty;
					if (!pickLineAttribute.IsEmpty)
					{
						partAttrib = pickLineAttribute;
					}
					else
					{
						partAttrib = getPartAttribute(attributesLine);
					}

					return partAttrib;
				}
			}

			using (releaseLine.GetValidationSuspender())
			using (releaseLine.SuspendUpdatingSumOfUnitsMetAndOrderFields())
			using (releaseLine.SuspendSettingHasChanges())
			{
				releaseLine.Quantity += qty;
				SumOfUnitsMet += qty;
			}
		}

		#endregion

		#region OnAdded

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			var releaseLine = (WhsReleaseLine)bizOAdded;
			HookAndAddReleaseLineToAttributeCache(releaseLine);

			base.OnAdded(bizOAdded);
		}

		#region HookAndAddReleaseLineToAttributeCache

		void HookAndAddReleaseLineToAttributeCache(WhsReleaseLine releaseLine)
		{
			HookReleaseLine(releaseLine);

			AddReleaseLineToAttributeCacheCore(releaseLine);
		}

		void HookReleaseLine(WhsReleaseLine releaseLine)
		{
			releaseLine.AttributesChanged += ReleaseLine_AttributesChanged;
		}

		/// <summary>
		/// Adds the Release Line to the Attributes Dictionary and returns the key it was added with.
		/// </summary>
		string AddReleaseLineToAttributeCacheCore(WhsReleaseLine releaseLine)
		{
			var key = GetKey(releaseLine);
			if (!ReleaseLinesByAttributes.ContainsKey(key))
			{
				ReleaseLinesByAttributes[key] = releaseLine;
			}

			return key;
		}

		static internal string GetKey(WhsReleaseLine releaseLine)
		{
			var attributes = new[]
			{
				releaseLine.PartAttribute1.ToUpper(),
				releaseLine.PartAttribute2.ToUpper(),
				releaseLine.PartAttribute3.ToUpper(),
				releaseLine.SerialNumber.ToUpper(),
				GetDateKey(releaseLine.ExpiryDate),
				GetDateKey(releaseLine.PackingDate)
			};

			return string.Join(AttributesSeparator, attributes);
		}

		static ZString GetDateKey(ZDate date) => date.ToShortDateString();

		#endregion

		#endregion

		#region OnCommitted

		protected override void OnNonCommittedAdded(BusinessObject bizO)
		{
			base.OnNonCommittedAdded(bizO);

			var releaseLine = (WhsReleaseLine)bizO;
			UpdateReleaseCapturedQuantity(releaseLine);
			FireReleaseLineAdded(releaseLine);
			RefreshBindingIfItWasDeferred();
		}

		void RefreshBindingIfItWasDeferred()
		{
			if (NewElementRequiresDeferredRefreshBinding)
			{
				NewElementRequiresDeferredRefreshBinding = false;
				RefreshBinding();
			}
		}

		#endregion

		#endregion

		#region CountChanged

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);

			// we will handle *adding* manually as CountChanged is not fired at a good time when adding new Release lines.
			if (!IsInvalidated && e.ItemRemoved)
			{
				FireReleaseLineAddedOrRemoved((WhsReleaseLine)e.BizObject, releaseLineWasAdded: false);
			}
		}

		void FireReleaseLineAdded(WhsReleaseLine releaseLine)
		{
			if (!IsNonCommittedCollectionElement(releaseLine) && !IsDuplicateReleaseLine(releaseLine))
			{
				// If there are no Pick Lines for this attribute combination there are no PackableItems to update the Packing Tab with
				var key = GetKey(releaseLine);
				if (PickLinesByAttributes.TryGetValue(key, out var pickLines) && pickLines.Count > 0 && GetNonDeletedPickLines(pickLines).Any())
				{
					FireReleaseLineAddedOrRemoved(releaseLine, releaseLineWasAdded: true);
				}
			}
		}

		void FireReleaseLineAddedOrRemoved(WhsReleaseLine releaseLine, bool releaseLineWasAdded)
		{
			OrderLine.PickableDocket.FireReleaseLineAddedOrRemoved(releaseLine, releaseLineWasAdded);
		}

		#endregion

		#region Delete / Remove

		#region DeleteReleaseLine

		public void DeleteReleaseLine(WhsReleaseLine releaseLine)
		{
			var doesReleaseLineBelongToThisCollection = releaseLine != null && releaseLine.ParentCollection == this;
			if (!doesReleaseLineBelongToThisCollection)
			{
				throw new InvalidOperationException("Release Lines should be deleted with the correct Collection.");
			}
			else if (ReleaseLinesCache.Contains(releaseLine))
			{
				DeleteReleaseLineCore(releaseLine);
			}
		}

		void DeleteReleaseLineCore(WhsReleaseLine releaseLine)
		{
			if (releaseLine.Quantity != 0m)
			{
				using (releaseLine.GetValidationSuspender())
				{
					releaseLine.Quantity = 0m; // Must be done before unhooking to update SumOfUnitsMet
				}
			}

			RemoveKeyFromAttributeCache(releaseLine, GetKey(releaseLine));
			UnhookReleaseLine(releaseLine);

			try
			{
				RemoveAndDeleteElement(releaseLine);
			}
			finally
			{
				LineToAutoRemoveOnDelete = null;
			}
		}

		void UnhookReleaseLine(WhsReleaseLine releaseLine)
		{
			releaseLine.AttributesChanged -= ReleaseLine_AttributesChanged;
			ReleaseLinesCache.Remove(releaseLine);
		}

		void RemoveKeyFromAttributeCache(WhsReleaseLine releaseLine, string key)
		{
			if (ReleaseLinesByAttributes.TryGetValue(key, out var releaseLineInDictionary) && releaseLineInDictionary == releaseLine)
			{
				ReleaseLinesByAttributes.Remove(key);
			}
		}

		#region RemoveAndDelete

		void RemoveAndDeleteElement(WhsReleaseLine releaseLine)
		{
			if (IsNonCommittedCollectionElement(releaseLine))
			{
				// if the Non-Committed Element is being deleted Manually, we need to remove it from the Collection
				if (releaseLine.IsDeleted && Elements.IndexOfOptimisedForHashByPK(releaseLine) != -1)
				{
					Remove(releaseLine);
				}

				// if for some reason the Non-Committed Persistent Release Line is being deleted directly we need to discard the Non-Persistent Release Line.
				if (!releaseLine.IsDeleted)
				{
					((ICancelAddNew)this).CancelNew(Elements.IndexOfOptimisedForHashByPK(releaseLine));
				}
			}
			else
			{
				Delete(releaseLine);
			}
		}

		void Delete(WhsReleaseLine releaseLine)
		{
			// if someone manually Deletes the Release Line, it should be removed from the Collection
			if (releaseLine.IsDeleted && LineToAutoRemoveOnDelete != releaseLine)
			{
				Remove(releaseLine);
			}

			if (!releaseLine.IsDeleted)
			{
				using (SuspendListChanged())
				{
					RemoveAndDelete(releaseLine);
				}
			}
		}

		#endregion

		#endregion

		#region OnRemoved

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			// Deletes and Uncommitting is already handled.
			if (!bizO.IsDeleted && !IsNonCommittedCollectionElement(bizO))
			{
				var releaseLine = (WhsReleaseLine)bizO;
				if (ReleaseLinesCache.Contains(releaseLine))
				{
					UnhookReleaseLine(releaseLine);
					releaseLine.DeleteObjectOnRemove_DoNotUse();
				}
			}
		}

		#endregion

		#region Remove

		public override void Remove(BusinessObject elementToRemove)
		{
			if (IsNonCommittedCollectionElement(elementToRemove))
			{
				// RemoveAndDelete will call List Reset event, so no need to care about deferring RefreshBinding()
				NewElementRequiresDeferredRefreshBinding = false;

				if (!InRemoveAndDelete)
				{
					// This will call Delete() on the Element before it is removed from the collection.
					RemoveAndDelete(elementToRemove);
				}
			}
			else
			{
				base.Remove(elementToRemove);
			}
		}

		#endregion

		#region RemoveCollectionRelationships

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			base.RemoveCollectionRelationshipsCore(child, forDelete);

			if (forDelete)
			{
				LineToAutoRemoveOnDelete = (WhsReleaseLine)child;
			}
		}

		WhsReleaseLine LineToAutoRemoveOnDelete;

		#endregion

		#endregion

		#region GetKey

		public static string GetKey(IPartAttributes attributesLine)
		{
			string key = null;

			if (attributesLine != null)
			{
				var attributes = new[]
				{
					attributesLine.PartAttrib1.ToUpper(),
					attributesLine.PartAttrib2.ToUpper(),
					attributesLine.PartAttrib3.ToUpper(),
					attributesLine.SerialNumber.ToUpper(),
					GetDateKey(attributesLine.ExpiryDate),
					GetDateKey(attributesLine.PackingDate)
				};

				key = string.Join(AttributesSeparator, attributes);
			}

			return key ?? string.Empty;
		}

		#endregion

		#region GetKeyForLineWithoutReleaseCapturedAttribs

		public static string GetKeyForLineWithoutReleaseCapturedAttribs(IPartAttributes attributesLine, WhsPickableDocketLine orderLine)
		{
			Argument.NotNull(attributesLine, nameof(attributesLine));
			Argument.NotNull(orderLine, nameof(orderLine));

			return GetKeyForLineWithoutReleaseCapturedAttribsCore(attributesLine, orderLine);
		}

		static string GetKeyForLineWithoutReleaseCapturedAttribsCore(IPartAttributes attributesLine, WhsPickableDocketLine orderLine)
		{
			var attributes = new[]
			{
				GetNonReleaseCapturedAttributeCore(orderLine, attributesLine.PartAttrib1, PartAttributeNumber.One),
				GetNonReleaseCapturedAttributeCore(orderLine, attributesLine.PartAttrib2, PartAttributeNumber.Two),
				GetNonReleaseCapturedAttributeCore(orderLine, attributesLine.PartAttrib3, PartAttributeNumber.Three),
				GetNonReleaseCapturedSerialNumberCore(orderLine, attributesLine.SerialNumber),
				GetDateKey(attributesLine.ExpiryDate),
				GetDateKey(attributesLine.PackingDate)
			};

			return string.Join(AttributesSeparator, attributes);
		}

		static ZString GetNonReleaseCapturedAttributeCore(WhsPickableDocketLine orderLine, ZString partAttribute, PartAttributeNumber attribNo)
		{
			return ReleaseCapturedHelper.IsAttributeReleaseCaptured(orderLine, attribNo) ? ZString.Empty : partAttribute.ToUpper();
		}

		static ZString GetNonReleaseCapturedSerialNumberCore(WhsPickableDocketLine orderLine, ZString serialNumber)
		{
			return ReleaseCapturedHelper.IsSerialNumberReleaseCaptured(orderLine) ? ZString.Empty : serialNumber.ToUpper();
		}

		public string GetKeyForLineWithoutReleaseCapturedAttribs(WhsReleaseLine releaseLine)
		{
			Argument.NotNull(releaseLine, nameof(releaseLine));
			return GetKeyForLineWithoutReleaseCapturedAttribsCore(releaseLine, OrderLine);
		}

		public string GetKeyForLineWithoutReleaseCapturedAttribs(AttributeParts oldAttributes)
		{
			Argument.NotNull(oldAttributes, nameof(oldAttributes));
			return GetKeyForLineWithoutReleaseCapturedAttribsCore(oldAttributes, OrderLine);
		}

		#endregion

		#region GetNonReleaseCapturedAttribute

		public static ZString GetNonReleaseCapturedAttribute(WhsPickableDocketLine orderLine, ZString partAttribute, PartAttributeNumber attribNo)
		{
			Argument.NotNull(orderLine, nameof(orderLine));
			return GetNonReleaseCapturedAttributeCore(orderLine, partAttribute, attribNo);
		}

		#endregion

		#region GetPackableItems

		public IEnumerable<IPackableItem> GetPackableItems(WhsReleaseLine releaseLine)
		{
			IEnumerable<IPackableItem> result = null;

			if (!ReleaseLinesCache.Contains(releaseLine))
			{
				throw new InvalidOperationException("Only pass in a Release Line that is Related to this Collection.");
			}

			var key = GetKey(releaseLine);
			if (PickLinesByAttributes.TryGetValue(key, out var pickLines))
			{
				result = GetNonDeletedPickLines(pickLines).ToArray();
			}

			return result ?? Enumerable.Empty<IPackableItem>();
		}

		#endregion

		#region HasChanges

		// Has Changes in base will enumerate this collection to know if there
		// are changes, however this is a Non-Persistent BizO collection and we
		// don't want to enumerate the collection when the collection has been cleared.
		protected override bool HasChangesCore
		{
			get { return !IsInvalidated && base.HasChangesCore; }
			set { base.HasChangesCore = value; }
		}

		#endregion

		#region IsAnyReleaseCapturedQuantityBelowPickedQuantity

		public bool IsAnyReleaseCapturedQuantityBelowPickedQuantity(WhsReleaseLine releaseLine)
		{
			var result = false;

			if (IsValidToCheckReleaseLine(releaseLine))
			{
				if (IsNonCommittedCollectionElement(releaseLine))
				{
					if (releaseLine.Quantity > 0m)
					{
						var key = GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine);
						result = PickLinesByAttributes.TryGetValue(key, out var pickLines) && GetNonDeletedPickLinesExcludingPickByBOMLines(pickLines).Where(p => p.IsPickedFromPutawayLocation).Sum(p => Math.Max(0m, p.UnreleaseCapturedQty)) > releaseLine.Quantity;
					}
				}
				else
				{
					var key = GetKey(releaseLine);
					result = PickLinesByAttributes.TryGetValue(key, out var pickLines) && GetNonDeletedPickLinesExcludingPickByBOMLines(pickLines).Sum(p => p.WZ_Units) > releaseLine.Quantity;
				}
			}

			return result;
		}

		#endregion

		#region IsDuplicateReleaseLine

		public bool IsDuplicateReleaseLine(WhsReleaseLine releaseLine)
		{
			WhsReleaseLine releaseLineInCache;
			return releaseLine != null
				&& !IsNonCommittedCollectionElement(releaseLine)
				&& ReleaseLinesCache.Contains(releaseLine)
				&& ReleaseLinesByAttributes.TryGetValue(GetKey(releaseLine), out releaseLineInCache)
				&& releaseLineInCache != releaseLine;
		}

		#endregion

		#region IsForOrderLine

		internal bool IsForOrderLine(WhsPickableDocketLine orderLine) => OrderLine == orderLine;

		#endregion

		#region IsOrderLineOverReleased

		public bool IsOrderLineOverReleased
		{
			get { return !OrderLine.IsDeleted && SumOfUnitsMet > OrderLine.WE_TransactionQuantity; }
		}

		#endregion

		#region IsReleaseCapturedAttributeUnallocated

		public bool IsReleaseCapturedAttributeUnallocated(WhsReleaseLine releaseLine)
		{
			var result = false;

			if (IsValidToCheckReleaseLine(releaseLine))
			{
				if (IsNonCommittedCollectionElement(releaseLine))
				{
					var key = GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine);
					result = !PickLinesByAttributes.TryGetValue(key, out var pickLines) || !GetNonDeletedPickLinesExcludingPickByBOMLines(pickLines).Any(p => p.UnreleaseCapturedQty > 0m);
				}
				else
				{
					var key = GetKey(releaseLine);
					result = !PickLinesByAttributes.TryGetValue(key, out var pickLines) || pickLines.Count == 0 || pickLines.All(pl => pl.IsDeleted) || !HasReleaseCapturedAttrib(pickLines, OrderLine, releaseLine);
				}
			}

			return result;
		}

		static bool HasReleaseCapturedAttrib(IEnumerable<WhsPickLine> pickLines, WhsPickableDocketLine orderLine, WhsReleaseLine releaseLine)
		{
			var rca1 = Lazy.Create(() => GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute1, PartAttributeNumber.One));
			var rca2 = Lazy.Create(() => GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute2, PartAttributeNumber.Two));
			var rca3 = Lazy.Create(() => GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute3, PartAttributeNumber.Three));
			var serial = Lazy.Create(() => GetReleaseCapturedSerialNumber(orderLine, releaseLine.SerialNumber));
			return GetNonDeletedPickLines(pickLines).Any(pl =>
				pl.WZ_ReleaseCapturedSerialNumber.EqualsIgnoringCase(serial.Value)
				&& pl.WZ_ReleaseCapturedPartAttrib1.EqualsIgnoringCase(rca1.Value)
				&& pl.WZ_ReleaseCapturedPartAttrib2.EqualsIgnoringCase(rca2.Value)
				&& pl.WZ_ReleaseCapturedPartAttrib3.EqualsIgnoringCase(rca3.Value));
		}

		#endregion

		#region IsReleaseLinePickedFromPutawayLocation

		public bool IsReleaseLinePickedFromPutawayLocation(WhsReleaseLine releaseLine)
		{
			var result = false;

			if (IsValidToCheckReleaseLine(releaseLine) && !IsNonCommittedCollectionElement(releaseLine))
			{
				var key = GetKey(releaseLine);
				var releaseCapturedQty = GetCurrentReleaseCapturedQty(key, releaseLine, onlyPickedPickLines: true);
				if (releaseCapturedQty > 0m)
				{
					var releasedQty = GetCurrentReleasedQty(key);
					result = releasedQty - releaseCapturedQty < releaseLine.Quantity;
				}
			}

			return result;
		}

		#endregion

		#region IsReleaseLineUnderCaptured

		public bool IsReleaseLineUnderCaptured(WhsReleaseLine releaseLine)
		{
			var result = false;

			if (IsValidToCheckReleaseLine(releaseLine))
			{
				if (IsNonCommittedCollectionElement(releaseLine))
				{
					if (releaseLine.Quantity > 0m)
					{
						var key = GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine);
						result = !PickLinesByAttributes.TryGetValue(key, out var pickLines) || GetNonDeletedPickLinesExcludingPickByBOMLines(pickLines).Sum(p => Math.Max(0m, p.UnreleaseCapturedQty)) < releaseLine.Quantity;
					}
				}
				else
				{
					var key = GetKey(releaseLine);
					var releaseCapturedQty = GetCurrentReleaseCapturedQty(key, releaseLine);
					var releasedQty = GetCurrentReleasedQty(key);
					result = releasedQty > releaseCapturedQty;
				}
			}

			return result;
		}

		#endregion

		#region IsMatchingAttributes

		public bool IsMatchingAttributes(ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate expiry, ZDate packing)
		{
			return (OrderLine.WE_PartAttrib1.IsEmpty || OrderLine.WE_PartAttrib1.EqualsIgnoringCase(partAttrib1))
				&& (OrderLine.WE_PartAttrib2.IsEmpty || OrderLine.WE_PartAttrib2.EqualsIgnoringCase(partAttrib2))
				&& (OrderLine.WE_PartAttrib3.IsEmpty || OrderLine.WE_PartAttrib3.EqualsIgnoringCase(partAttrib3))
				&& (OrderLine.WE_SerialNumber.IsEmpty || OrderLine.WE_SerialNumber.EqualsIgnoringCase(serialNumber))
				&& (OrderLine.WE_ExpiryDate.IsEmpty || OrderLine.WE_ExpiryDate == expiry)
				&& (OrderLine.WE_PackingDate.IsEmpty || OrderLine.WE_PackingDate == packing);
		}

		#endregion

		#region IsValidToCheckReleaseLine

		bool IsValidToCheckReleaseLine(WhsReleaseLine releaseLine)
		{
			return releaseLine != null
				&& ReleaseLinesCache.Contains(releaseLine)
				&& releaseLine.Quantity >= 0m
				&& ReleaseCapturedHelper.IsReleaseCaptured(OrderLine)
				&& IsAnyAttributeReleaseCaptured(releaseLine);
		}

		bool IsAnyAttributeReleaseCaptured(WhsReleaseLine releaseLine)
		{
			return
				(ReleaseCapturedHelper.IsAttributeReleaseCaptured(OrderLine, PartAttributeNumber.One) && !releaseLine.PartAttribute1.IsEmpty) ||
				(ReleaseCapturedHelper.IsAttributeReleaseCaptured(OrderLine, PartAttributeNumber.Two) && !releaseLine.PartAttribute2.IsEmpty) ||
				(ReleaseCapturedHelper.IsAttributeReleaseCaptured(OrderLine, PartAttributeNumber.Three) && !releaseLine.PartAttribute3.IsEmpty) ||
				(ReleaseCapturedHelper.IsSerialNumberReleaseCaptured(OrderLine) && !releaseLine.SerialNumber.IsEmpty);
		}

		#endregion

		#region SuspendUnreleasedQtyChange

		internal IDisposable SuspendUnreleasedQtyChange() => new SemaphoreManager(SuspendUnreleasedQtyChangeSemaphore);

		Semaphore SuspendUnreleasedQtyChangeSemaphore => suspendUnreleasedQtyChangeSemaphore ?? (suspendUnreleasedQtyChangeSemaphore = new Semaphore());
		Semaphore suspendUnreleasedQtyChangeSemaphore;

		#endregion

		#region SumOfUnitsMet

		public ZDecimal SumOfUnitsMet { get; private set; }

		public void UpdateSumOfUnitsMet(WhsReleaseLine releaseLine, ZDecimal previousQuantity)
		{
			if (ReleaseLinesCache.Contains(releaseLine))
			{
				var difference = (releaseLine.Quantity - previousQuantity);
				SumOfUnitsMet += difference;

				if (!SuspendUnreleasedQtyChangeSemaphore.IsSuspended)
				{
					ReleaseLinesOnPickManager.NotifyChange(Factory, releaseLine, OrderLine, difference);
				}

				UpdateReleaseCapturedQuantity(releaseLine);

				// Negative or Zero Release Lines can't be Packed.
				if (releaseLine.Quantity <= 0m)
				{
					FireReleaseLineAddedOrRemoved(releaseLine, releaseLineWasAdded: false);
				}
				// Only Add this Release Line if it was previously a Zero or negative quantity Release Line.
				else if (previousQuantity <= 0m)
				{
					FireReleaseLineAdded(releaseLine);
				}

				if (!IsNonCommittedCollectionElement(releaseLine))
				{
					RefreshBinding(); // We need to update AllowNew and AllowRemove on the Collection in the GUI
				}
				else
				{
					// For non committed lines we don't want refresh binding to change the position of the line, so instead
					// of calling Refreshing Binding immediately, we delay it until the line is committed or deleted.
					NewElementRequiresDeferredRefreshBinding = true;
				}
			}
		}

		bool NewElementRequiresDeferredRefreshBinding;

		void UpdateReleaseCapturedQuantity(WhsReleaseLine releaseLine)
		{
			// we don't want to update Release Captured Attributes until the Release Line is committed
			if (!IsNonCommittedCollectionElement(releaseLine) && releaseLine.Quantity >= 0m && ReleaseCapturedHelper.IsReleaseCaptured(OrderLine))
			{
				var key = GetKey(releaseLine);
				var keyWithoutAttribs = GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine);

				if (keyWithoutAttribs != key // we will not add / remove empty release captured attributes
					&& (!ReleaseLinesByAttributes.TryGetValue(key, out var releaseLineByAttribute) || releaseLineByAttribute == releaseLine)) // only update RCAs for Non-Duplicated Release Lines
				{
					var releaseCapturedQty = GetCurrentReleaseCapturedQty(key, releaseLine);
					var releasedQty = GetCurrentReleasedQty(key);
					UpdateReleasedQuantityCore(releaseLine, releasedQty - releaseCapturedQty, key, keyWithoutAttribs);
				}
			}
		}

		void UpdateReleasedQuantityCore(WhsReleaseLine releaseLine, decimal changeInQuantity, string key, string keyWithoutAttribs)
		{
			// we need to remove Release Captured Attributes
			if (changeInQuantity < 0m)
			{
				if (PickLinesByAttributes.TryGetValue(key, out var pickLines) && pickLines.Count > 0)
				{
					var nonDeletedPickLines = GetNonDeletedPickLinesExcludingPickByBOMLines(pickLines).ToArray();
					if (nonDeletedPickLines.Length > 0)
					{
						var args = new UpdateAttributesEventArgs(AttributeParts.New(releaseLine), PartAttributeNumber.None);
						new ReleaseAttributesRemover(releaseLine, args).RemoveReleaseCapturedAttributes(nonDeletedPickLines, Math.Abs(changeInQuantity));
					}
				}
			}
			// we need to add Release Captured Attributes
			else if (changeInQuantity > 0m)
			{
				// intitialise Cache here so we don't have to check the key exists when we want to add to the Cache.
				if (!PickLinesByAttributes.ContainsKey(key))
				{
					PickLinesByAttributes[key] = new HashSet<WhsPickLine>();
				}

				AddReleaseCapturedAttributesToPickLines(releaseLine, PartAttributeNumber.None, key, keyWithoutAttribs, changeInQuantity);
			}
		}

		decimal GetCurrentReleaseCapturedQty(string key, WhsReleaseLine releaseLine, bool onlyPickedPickLines = false)
		{
			return GetCurrentReleaseCapturedQtyCore(key, (WhsPickLine pickLine) => CheckPickLineWithNewAttributes(pickLine, OrderLine, releaseLine), onlyPickedPickLines);
		}

		decimal GetCurrentReleaseCapturedQty(string key, AttributeParts oldAttributeParts)
		{
			return GetCurrentReleaseCapturedQtyCore(key, (WhsPickLine pickLine) => CheckPickLineWithOldAttribs(pickLine, OrderLine, oldAttributeParts));
		}

		decimal GetCurrentReleaseCapturedQtyCore(string key, Func<WhsPickLine, bool> getAttributePredicate, bool onlyPickedPickLines = false)
		{
			var result = 0m;

			if (PickLinesByAttributes.TryGetValue(key, out var pickLines) && pickLines.Count > 0)
			{
				var nonDeletedPickLines = GetNonDeletedPickLinesExcludingPickByBOMLines(pickLines).ToArray();
				if (nonDeletedPickLines.Length > 0)
				{
					var pickLinesToConsider = onlyPickedPickLines ? nonDeletedPickLines.Where(p => p.IsPickedFromPutawayLocation).ToArray() : nonDeletedPickLines;
					if (pickLinesToConsider.Length > 0)
					{
						result = pickLinesToConsider.Where(getAttributePredicate).Sum(r => r.WZ_Units);
					}
				}
			}

			return result;
		}

		decimal GetCurrentReleasedQty(string key)
		{
			return ReleaseLinesByAttributes.TryGetValue(key, out var releaseLine) && releaseLine.Quantity > 0 ? releaseLine.Quantity : ZDecimal.Zero;
		}

		#endregion

		#region GetTotalUnitsMet

		public decimal GetTotalUnitsMet()
		{
			return ReleaseLinesCache.Sum(rl => rl.Quantity);
		}

		#endregion

		#region Events

		#region PickLinesForBinding_CollectionCountChange

		void PickLinesWithoutChildLinePickLines_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				var pickLine = (WhsPickLine)e.BizObject;
				if (pickLine != null)
				{
					MergePickLine(pickLine);
				}
				else
				{
					MergeCollectionIfNecessary(OrderLine.PickLinesWithNonZeroUnits, MergePickLine);
				}
			}
			else if (e.BizObject == null)
			{
				MergeCollectionIfNecessary(OrderLine.PickLinesWithNonZeroUnits, MergePickLine, ClearPickLinesCacheForReMerge);
			}
			else if (!e.BizObject.IsDeleted) // deletes are automatically handled by ActiveBizOCollections
			{
				var pickLine = (WhsPickLine)e.BizObject;

				// remove Pick Line as a key both with and without Release Captured Attributes
				var inventory = pickLine.InventoryLine;
				RemovePickLineFromCache(pickLine, GetKey(inventory));

				if (ReleaseCapturedHelper.IsReleaseCaptured(OrderLine))
				{
					RemovePickLineFromCache(pickLine, GetKey(inventory, pickLine));
				}
			}
		}

		// the only way to rebuild the picklines cache correctly when an element is removed and we
		// don't know which, is to clear the old dictionary and rebuild it from scratch.
		void ClearPickLinesCacheForReMerge()
		{
			pickLinesByAttributes?.Clear();
		}

		void MergeCollectionIfNecessary<T>(ActiveBusinessObjectCollection<T> collection, Action<T> mergeElement, Action beforeMerge = null)
			where T : BusinessObject
		{
			IActiveBusinessObjectCollection activeCollection = collection;

			// If for some reason the Cache is not populated even though the Collection is loaded we need to enumerate all Elements to merge any missing ones.
			if (activeCollection.IsLoaded && activeCollection.IsRebuildPending)
			{
				beforeMerge?.Invoke();

				foreach (var element in collection)
				{
					mergeElement(element);
				}
			}
		}

		void RemovePickLineFromCache(WhsPickLine pickLine, string key)
		{
			if (PickLinesByAttributes.TryGetValue(key, out var pickLines) && pickLines.Contains(pickLine))
			{
				RemovePickLinesFromCache(pickLines, key, new[] { pickLine });
			}
		}

		#endregion

		#region ReleaseLine_AttributesChanged

		void ReleaseLine_AttributesChanged(object sender, UpdateAttributesEventArgs e)
		{
			var releaseLine = (WhsReleaseLine)sender;
			var newKey = UpdateReleaseLinesAttributeCache(releaseLine, e);
			UpdateReleaseCapturedAttributes(releaseLine, e, newKey);
		}

		#region UpdateReleaseLinesAttributeCache

		string UpdateReleaseLinesAttributeCache(WhsReleaseLine releaseLine, UpdateAttributesEventArgs e)
		{
			RemoveKeyFromAttributeCache(releaseLine, e.OldKey);

			return AddReleaseLineToAttributeCacheCore(releaseLine);
		}

		#endregion

		#region UpdateReleaseCapturedAttributes

		void UpdateReleaseCapturedAttributes(WhsReleaseLine releaseLine, UpdateAttributesEventArgs e, string newKey)
		{
			if (ReleaseCapturedHelper.IsReleaseCaptured(OrderLine))
			{
				// Only remove if we are changing a non-Release Captured attribute or the old Release Attribute was not empty. If the old
				// Release Attribute was empty, we will likely want to update the existing Release Captures Attributes on the PickLine instead.
				var oldAttributeValue = e.OldAttributes.GetPartAttribute(e.PartAttribChanged);
				if (!ReleaseCapturedHelper.IsAttributeReleaseCaptured(OrderLine, e.PartAttribChanged)
					|| !oldAttributeValue.IsEmpty)
				{
					RemoveOldReleaseCapturedAttributes(releaseLine, e);
				}

				FireReleaseLineAddedOrRemoved(releaseLine, releaseLineWasAdded: false);

				// only add Release Captured Attributes if release Line is not Duplicated
				if (ReleaseLinesByAttributes[newKey] == releaseLine)
				{
					var keyWithoutAttribs = GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine);
					if (keyWithoutAttribs != newKey) // if the new attributes have all Release Captured Attributes empty, we should not add Release Captured Attributes Rows to the PickLines.
					{
						AddReleaseCapturedAttributes(releaseLine, e, keyWithoutAttribs, newKey);
					}

					FireReleaseLineAdded(releaseLine);
				}
			}
		}

		#region RemoveOldReleaseCapturedAttributes

		void RemoveOldReleaseCapturedAttributes(WhsReleaseLine releaseLine, UpdateAttributesEventArgs e)
		{
			if (PickLinesByAttributes.TryGetValue(e.OldKey, out var pickLines) && pickLines.Count > 0)
			{
				var nonDeletedPickLines = GetNonDeletedPickLinesExcludingPickByBOMLines(pickLines).ToArray();
				if (nonDeletedPickLines.Length > 0)
				{
					var qtyReleased = GetCurrentReleasedQty(e.OldKey);
					var qtyReleaseCaptured = GetCurrentReleaseCapturedQty(e.OldKey, e.OldAttributes);
					var qtyToRemove = qtyReleaseCaptured - qtyReleased;
					if (qtyToRemove > 0m)
					{
						new ReleaseAttributesRemover(releaseLine, e).RemoveReleaseCapturedAttributes(nonDeletedPickLines, qtyToRemove);
					}
				}
			}
		}

		void RemovePickLinesFromCache(HashSet<WhsPickLine> pickLines, string key, IEnumerable<WhsPickLine> pickLinesToRemove)
		{
			pickLines.ExceptWith(pickLinesToRemove);

			if (pickLines.Count == 0 || pickLines.All(pl => pl.IsDeleted))
			{
				PickLinesByAttributes.Remove(key);
			}
		}

		static bool CheckPickLineWithOldAttribs(WhsPickLine pl, WhsPickableDocketLine orderLine, AttributeParts oldAttributes)
		{
			return pl.WZ_ReleaseCapturedPartAttrib1.EqualsIgnoringCase(GetReleaseCapturedAttribute(orderLine, oldAttributes.PartAttribute1, PartAttributeNumber.One))
				&& pl.WZ_ReleaseCapturedPartAttrib2.EqualsIgnoringCase(GetReleaseCapturedAttribute(orderLine, oldAttributes.PartAttribute2, PartAttributeNumber.Two))
				&& pl.WZ_ReleaseCapturedPartAttrib3.EqualsIgnoringCase(GetReleaseCapturedAttribute(orderLine, oldAttributes.PartAttribute3, PartAttributeNumber.Three))
				&& pl.WZ_ReleaseCapturedSerialNumber.EqualsIgnoringCase(GetReleaseCapturedAttribute(orderLine, oldAttributes.SerialNumber, PartAttributeNumber.SerialNumber));
		}

		public static ZString GetReleaseCapturedAttribute(WhsPickableDocketLine orderLine, ZString partAttribute, PartAttributeNumber partAttribNo)
		{
			return ReleaseCapturedHelper.IsAttributeReleaseCaptured(orderLine, partAttribNo) ? partAttribute : ZString.Empty;
		}

		static ZString GetReleaseCapturedSerialNumber(WhsPickableDocketLine orderLine, ZString serialNumber)
		{
			return ReleaseCapturedHelper.IsSerialNumberReleaseCaptured(orderLine) ? serialNumber : ZString.Empty;
		}

		class ReleaseAttributesRemover
		{
			public ReleaseAttributesRemover(WhsReleaseLine releaseLine, UpdateAttributesEventArgs args)
			{
				Args = Argument.NotNull(args, "args");
				ReleaseLine = Argument.NotNull(releaseLine, "releaseLine");
			}

			protected UpdateAttributesEventArgs Args { get; }

			protected WhsPickableDocketLine OrderLine
			{
				get { return ReleaseLine.ParentCollection.OrderLine; }
			}

			protected WhsReleaseLine ReleaseLine { get; }

			public ZDecimal RemoveReleaseCapturedAttributes(WhsPickLine[] nonDeletedPickLines, ZDecimal qtyToRemove)
			{
				var qtyStillToRemove = qtyToRemove;
				var newOrValidLineToBeRemoved =
					!ReleaseLine.ParentCollection.ReleaseLinesByAttributes.TryGetValue(Args.OldKey, out var res)
					|| res == ReleaseLine;

				if (newOrValidLineToBeRemoved)
				{
					var pickLinesToRemoveFromCache = new List<WhsPickLine>();
					var pickLinesWithEmptyAttributes = new List<WhsPickLine>();
					var keyWithoutRCAs = GetKeyForLineWithoutReleaseCapturedAttribs(ReleaseLine, ReleaseLine.OrderLine);

					foreach (var pickLine in nonDeletedPickLines.OrderByDescending(pl => pl.WZ_Units == qtyStillToRemove).ThenBy(pl => pl.WZ_Units))
					{
						if (pickLine.IsUnpacked(pickLine.Factory) && !pickLine.IsPickedFromPutawayLocation)
						{
							if (pickLine.WZ_Units <= qtyStillToRemove)
							{
								qtyStillToRemove -= pickLine.WZ_Units;
							}
							else
							{
								var newPickLine = GetNewWhsPickLineReleaseCapturedAttribute(OrderLine, pickLine, ReleaseLine);
								newPickLine.WZ_Units = pickLine.WZ_Units - qtyStillToRemove;
								pickLine.WZ_Units = qtyStillToRemove;
								qtyStillToRemove = 0m;

								AddNewPickLineToCache(newPickLine);
							}

							pickLine.ClearReleaseCapturedAttributes();

							pickLinesToRemoveFromCache.Add(pickLine);
							pickLinesWithEmptyAttributes.Add(pickLine);
						}

						if (qtyStillToRemove == 0m)
						{
							break;
						}
					}

					var pickLineCache = ReleaseLine.ParentCollection.PickLinesByAttributes[Args.OldKey];
					ReleaseLine.ParentCollection.RemovePickLinesFromCache(pickLineCache, Args.OldKey, pickLinesToRemoveFromCache);
					ReleaseLine.ParentCollection.RemergeUpdatedPickLines(keyWithoutRCAs, pickLinesWithEmptyAttributes);
					AfterReleaseCapturedAttributesRemoved();
				}

				return qtyStillToRemove;
			}

			protected virtual void AddNewPickLineToCache(WhsPickLine newPickLine)
			{
			}

			#region AfterReleaseCapturedAttributesRemoved

			protected virtual void AfterReleaseCapturedAttributesRemoved()
			{
			}

			#endregion
		}

		#endregion

		#region AddReleaseCapturedAttributes

		void AddReleaseCapturedAttributes(WhsReleaseLine releaseLine, UpdateAttributesEventArgs e, string keyWithoutAttribs, string newKey)
		{
			var qtyStillToAdd = releaseLine.Quantity;
			if (qtyStillToAdd > 0m)
			{
				// intitialise Cache here so we don't have to check the key exists when we want to add to the Cache.
				if (!PickLinesByAttributes.ContainsKey(newKey))
				{
					PickLinesByAttributes[newKey] = new HashSet<WhsPickLine>();
				}

				if (ReleaseCapturedHelper.IsAttributeReleaseCaptured(OrderLine, e.PartAttribChanged))
				{
					// this will try to clear RCAs on certain pick lines and update them with new RCAs later.
					UpdateExistingReleaseCapturedAttributeValues(releaseLine, e, qtyStillToAdd, keyWithoutAttribs);
				}

				AddReleaseCapturedAttributesToPickLines(releaseLine, e.PartAttribChanged, newKey, keyWithoutAttribs, qtyStillToAdd);
			}
		}

		#region UpdateExistingReleaseCapturedAttributeValues

		void UpdateExistingReleaseCapturedAttributeValues(WhsReleaseLine releaseLine, UpdateAttributesEventArgs e, ZDecimal qtyToRemove, string keyWithoutAttribs)
		{
			// Look for Release Lines with an empty Release Attribute for the Attribute that was changed. If we changed the
			// Release Line that was empty, we can simply add/update the Release Captured Attributes already made for these Release Lines.
			var oldAttributeValue = e.OldAttributes.GetPartAttribute(e.PartAttribChanged);
			if (oldAttributeValue.IsEmpty)
			{
				// no point trying to Update Release Captured Attributes with all values empty, they won't exist.
				var keyWithChangedReleaseAttribEmpty = GetKeyForLineWithEmptyChangedReleaseAttribute(releaseLine, e.PartAttribChanged);
				if (keyWithChangedReleaseAttribEmpty != keyWithoutAttribs)
				{
					if (PickLinesByAttributes.TryGetValue(keyWithChangedReleaseAttribEmpty, out var pickLines) && pickLines.Count > 0)
					{
						var nonDeletedPickLines = GetNonDeletedPickLinesExcludingPickByBOMLines(pickLines).ToArray();
						if (nonDeletedPickLines.Length > 0)
						{
							new ReleaseAttributesUpdater(releaseLine, e).RemoveReleaseCapturedAttributes(nonDeletedPickLines, qtyToRemove);
						}
					}
				}
			}
		}

		static string GetKeyForLineWithEmptyChangedReleaseAttribute(WhsReleaseLine releaseLine, PartAttributeNumber partAttribChanged)
		{
			var attributes = new[]
			{
				partAttribChanged == PartAttributeNumber.One ? ZString.Empty : releaseLine.PartAttribute1.ToUpper(),
				partAttribChanged == PartAttributeNumber.Two ? ZString.Empty : releaseLine.PartAttribute2.ToUpper(),
				partAttribChanged == PartAttributeNumber.Three ? ZString.Empty : releaseLine.PartAttribute3.ToUpper(),
				partAttribChanged == PartAttributeNumber.SerialNumber ? ZString.Empty : releaseLine.SerialNumber.ToUpper(),
				GetDateKey(releaseLine.ExpiryDate),
				GetDateKey(releaseLine.PackingDate)
			};

			return string.Join(AttributesSeparator, attributes);
		}

		#region ReleaseAttributesUpdater

		sealed class ReleaseAttributesUpdater : ReleaseAttributesRemover
		{
			public ReleaseAttributesUpdater(WhsReleaseLine releaseLine, UpdateAttributesEventArgs args)
				: base(releaseLine, args)
			{
				PickLinesWithNewAttributes = new List<WhsPickLine>();
				var orderLine = releaseLine.ParentCollection.OrderLine;
				NewAttributesQuery = GetQueryForNewAttributes(orderLine, releaseLine);
			}

			readonly List<WhsPickLine> PickLinesWithNewAttributes;
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in ReleaseAttributesUpdater")]
			readonly ZQuery NewAttributesQuery;

			protected override void AddNewPickLineToCache(WhsPickLine newPickLine)
			{
				PickLinesWithNewAttributes.Add(newPickLine);
			}

			protected override void AfterReleaseCapturedAttributesRemoved()
			{
				base.AfterReleaseCapturedAttributesRemoved();

				var newKey = GetKey(ReleaseLine);
				ReleaseLine.ParentCollection.RemergeUpdatedPickLines(newKey, PickLinesWithNewAttributes);
			}
		}

		#endregion

		#endregion

		#region AddReleaseCapturedAttributesToPickLines

		void AddReleaseCapturedAttributesToPickLines(WhsReleaseLine releaseLine, PartAttributeNumber partAttribChanged, string key, string keyWithoutAttribs, ZDecimal qtyToAdd)
		{
			var pickLinesFullyReleased = new HashSet<WhsPickLine>();

			// try to increase *Units* on Existing Release Captured Attributes first
			var qtyStillToAdd = AddReleaseCapturedAttributesToPickLines(releaseLine, pickLinesFullyReleased, key, qtyToAdd);
			if (qtyStillToAdd > 0m)
			{
				// Only try to update PickLines where the changed Attribute is empty if the new Release Captured Attribute Value is *not* empty.
				var newAttributeValue = GetReleaseCapturedAttribute(OrderLine, releaseLine.GetPartAttribute(partAttribChanged), partAttribChanged);
				if (!newAttributeValue.IsEmpty)
				{
					var keyWithChangedReleaseAttribEmpty = GetKeyForLineWithEmptyChangedReleaseAttribute(releaseLine, partAttribChanged);
					qtyStillToAdd = AddReleaseCapturedAttributesToPickLines(releaseLine, pickLinesFullyReleased, keyWithChangedReleaseAttribEmpty, key, qtyStillToAdd);
				}

				// finally try to Add Release Captured Attributes to Pick Lines that might still have unreleased Quantity available.
				AddReleaseCapturedAttributesToPickLines(releaseLine, pickLinesFullyReleased, keyWithoutAttribs, key, qtyStillToAdd);
			}
		}

		ZDecimal AddReleaseCapturedAttributesToPickLines(WhsReleaseLine releaseLine, HashSet<WhsPickLine> pickLinesFullyReleased, string newKey, ZDecimal qtyToAdd)
		{
			return AddReleaseCapturedAttributesToPickLines(releaseLine, pickLinesFullyReleased, newKey, newKey, qtyToAdd);
		}

		ZDecimal AddReleaseCapturedAttributesToPickLines(WhsReleaseLine releaseLine, HashSet<WhsPickLine> pickLinesFullyReleased, string keyForPickLinesToConsider, string newKey, ZDecimal qtyToAdd)
		{
			var qtyStillToAdd = qtyToAdd;

			if (qtyStillToAdd > 0m)
			{
				if (PickLinesByAttributes.TryGetValue(keyForPickLinesToConsider, out var pickLines) && pickLines.Count > 0)
				{
					var nonDeletedPickLines = GetNonDeletedPickLinesExcludingPickByBOMLines(pickLines).ToArray();
					if (nonDeletedPickLines.Length > 0)
					{
						qtyStillToAdd = AddReleaseCapturedAttributesToPickLinesCore(releaseLine, nonDeletedPickLines, pickLinesFullyReleased, qtyStillToAdd, newKey, keyForPickLinesToConsider);
					}
				}
			}

			return qtyStillToAdd;
		}

		#region AddReleaseCapturedAttributesToPickLinesCore

		ZDecimal AddReleaseCapturedAttributesToPickLinesCore(WhsReleaseLine releaseLine, WhsPickLine[] nonDeletedPickLines, HashSet<WhsPickLine> pickLinesFullyReleased, ZDecimal qtyToAdd, string newKey, string oldKey)
		{
			var qtyStillToAdd = qtyToAdd;
			var pickLinesWithNewAttributes = new List<WhsPickLine>();

			foreach (var pickLine in nonDeletedPickLines.OrderByDescending(pl => pl.WZ_Units == qtyStillToAdd).ThenBy(pl => pl.WZ_Units))
			{
				if (!pickLinesFullyReleased.Contains(pickLine))
				{
					if (!pickLine.HasReleaseCapturedAttribs && pickLine.IsUnpacked(Factory))
					{
						if (pickLine.WZ_Units <= qtyStillToAdd)
						{
							qtyStillToAdd -= pickLine.WZ_Units;
						}
						else
						{
							var args = new BusinessObjectCloneArgs(Array.Empty<string>(), performRowCopyWithoutTriggeringValidationAndSetter: true);
							var newPickLine = (WhsPickLine)pickLine.Clone(args);
							newPickLine.WZ_Units = pickLine.WZ_Units - qtyStillToAdd;
							pickLine.WZ_Units = qtyStillToAdd;
							qtyStillToAdd = 0m;
						}

						// Because we are changing RCAs of pickLine, need to remove it from PickLinesByAttributes with the old key and add it back with new key later.
						RemovePickLineFromCache(pickLine, oldKey);

						SetWhsPickLineReleaseCapturedAttribute(releaseLine.OrderLine, pickLine, releaseLine);
						pickLinesWithNewAttributes.Add(pickLine);

						if (qtyStillToAdd == 0m)
						{
							break;
						}
					}
					else
					{
						pickLinesFullyReleased.Add(pickLine);
					}
				}
			}

			RemergeUpdatedPickLines(newKey, pickLinesWithNewAttributes);

			return qtyStillToAdd;
		}

		static ZQuery GetQueryForNewAttributes(WhsPickableDocketLine orderLine, WhsReleaseLine releaseLine)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib1, GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute1, PartAttributeNumber.One));
			query.AddToFilter(WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib2, GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute2, PartAttributeNumber.Two));
			query.AddToFilter(WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib3, GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute3, PartAttributeNumber.Three));
			query.AddToFilter(WhsPickLineSchema.WZ_ReleaseCapturedSerialNumber, GetReleaseCapturedSerialNumber(orderLine, releaseLine.SerialNumber));
			return query;
		}

		static bool CheckPickLineWithNewAttributes(WhsPickLine pl, WhsPickableDocketLine orderLine, WhsReleaseLine releaseLine)
		{
			return pl.WZ_ReleaseCapturedPartAttrib1 == GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute1, PartAttributeNumber.One)
				&& pl.WZ_ReleaseCapturedPartAttrib2 == GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute2, PartAttributeNumber.Two)
				&& pl.WZ_ReleaseCapturedPartAttrib3 == GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute3, PartAttributeNumber.Three)
				&& pl.WZ_ReleaseCapturedSerialNumber == GetReleaseCapturedSerialNumber(orderLine, releaseLine.SerialNumber);
		}

		static WhsPickLine GetNewWhsPickLineReleaseCapturedAttribute(WhsPickableDocketLine orderLine, WhsPickLine pickLine, WhsReleaseLine releaseLine)
		{
			return GetNewWhsPickLineReleaseCapturedAttribute(orderLine, pickLine, GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute1, PartAttributeNumber.One), GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute2, PartAttributeNumber.Two), GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute3, PartAttributeNumber.Three), GetReleaseCapturedSerialNumber(orderLine, releaseLine.SerialNumber));
		}

		static WhsPickLine GetNewWhsPickLineReleaseCapturedAttribute(WhsPickableDocketLine orderLine, WhsPickLine pickLine, ZString partAttribute1, ZString partAttribute2, ZString partAttribute3, ZString serialNumber)
		{
			var excludedColumns = new[] { WhsPickLineSchema.Constants.WZ_ReleaseCapturedPartAttrib1, WhsPickLineSchema.Constants.WZ_ReleaseCapturedPartAttrib2, WhsPickLineSchema.Constants.WZ_ReleaseCapturedPartAttrib3, WhsPickLineSchema.Constants.WZ_ReleaseCapturedSerialNumber, WhsPickLineSchema.Constants.WZ_Units };
			var args = new BusinessObjectCloneArgs(excludedColumns, performRowCopyWithoutTriggeringValidationAndSetter: true);
			var newPickLine = (WhsPickLine)pickLine.Clone(args);

			newPickLine.WZ_ReleaseCapturedPartAttrib1 = GetReleaseCapturedAttribute(orderLine, partAttribute1, PartAttributeNumber.One);
			newPickLine.WZ_ReleaseCapturedPartAttrib2 = GetReleaseCapturedAttribute(orderLine, partAttribute2, PartAttributeNumber.Two);
			newPickLine.WZ_ReleaseCapturedPartAttrib3 = GetReleaseCapturedAttribute(orderLine, partAttribute3, PartAttributeNumber.Three);
			newPickLine.WZ_ReleaseCapturedSerialNumber = GetReleaseCapturedSerialNumber(orderLine, serialNumber);

			return newPickLine;
		}

		static void SetWhsPickLineReleaseCapturedAttribute(WhsPickableDocketLine orderLine, WhsPickLine pickLine, WhsReleaseLine releaseLine)
		{
			pickLine.WZ_ReleaseCapturedPartAttrib1 = GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute1, PartAttributeNumber.One);
			pickLine.WZ_ReleaseCapturedPartAttrib2 = GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute2, PartAttributeNumber.Two);
			pickLine.WZ_ReleaseCapturedPartAttrib3 = GetReleaseCapturedAttribute(orderLine, releaseLine.PartAttribute3, PartAttributeNumber.Three);
			pickLine.WZ_ReleaseCapturedSerialNumber = GetReleaseCapturedSerialNumber(orderLine, releaseLine.SerialNumber);
		}

		void RemergeUpdatedPickLines(string newKey, IEnumerable<WhsPickLine> pickLinesWithNewAttributes)
		{
			var pickLinesToAdd = new List<WhsPickLine>();
			foreach (var pickLine in pickLinesWithNewAttributes)
			{
				((IPackableItem)pickLine).ReMerge();
				if (!pickLine.IsDeleted)
				{
					pickLinesToAdd.Add(pickLine);
				}
			}

			AddUpdatedPickLinesToCache(newKey, pickLinesToAdd);
		}

		void AddUpdatedPickLinesToCache(string newKey, IEnumerable<WhsPickLine> pickLinesWithNewAttributes)
		{
			if (!PickLinesByAttributes.TryGetValue(newKey, out var pickLines))
			{
				PickLinesByAttributes[newKey] = pickLines = new HashSet<WhsPickLine>();
			}

			pickLines.UnionWith(pickLinesWithNewAttributes);
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#endregion

		#endregion

		#region ReleaseCapturedHelper

		static class ReleaseCapturedHelper
		{
			public static bool IsReleaseCaptured(WhsPickableDocketLine orderLine)
			{
				return !orderLine.IsDeleted && orderLine.Factory.GetCachedValue("ReleaseCapturedHelper|" + orderLine.WE_OP + "|" + orderLine.WE_WD, () => GetIsReleaseCaptured(orderLine), CacheStalenessPolicy.StaleOnFactorySave);
			}

			public static bool IsAttributeReleaseCaptured(WhsPickableDocketLine orderLine, PartAttributeNumber partAttrib)
			{
				return partAttrib != PartAttributeNumber.SerialNumber
					? IsPartAttributeNumberReleaseCaptured(orderLine, partAttrib)
					: IsSerialNumberReleaseCaptured(orderLine);
			}

			static bool IsPartAttributeNumberReleaseCaptured(WhsPickableDocketLine orderLine, PartAttributeNumber partAttrib)
			{
				return partAttrib != PartAttributeNumber.None && !orderLine.IsDeleted
					&& orderLine.Factory.GetCachedValue("ReleaseCapturedHelper|" + orderLine.WE_OP + "|" + orderLine.WE_WD + "|" + (int)partAttrib, () => GetIsReleaseCaptured(orderLine, partAttrib), CacheStalenessPolicy.StaleOnFactorySave);
			}

			public static ZString GetPartAttributeName(WhsPickableDocketLine orderLine, PartAttributeNumber partAttrib)
			{
				return partAttrib != PartAttributeNumber.None
					? orderLine.Factory.GetCachedValue("ReleaseCapturedHelper|" + orderLine.WE_WD + "|" + (int)partAttrib, () => GetPartAttributeNameCore(orderLine, partAttrib), CacheStalenessPolicy.StaleOnFactorySave)
					: ZString.Empty;
			}

			public static bool IsSerialNumberReleaseCaptured(WhsPickableDocketLine orderLine)
			{
				return !orderLine.IsDeleted && orderLine.Factory.GetCachedValue((NoResString)"ReleaseCapturedHelper|" + orderLine.WE_OP + (NoResString)"|" + orderLine.WE_WD + (NoResString)"|Serialnumber", () => GetIsSerialNumberReleaseCaptured(orderLine), CacheStalenessPolicy.StaleOnFactorySave); // Cache value key
			}

			static bool GetIsSerialNumberReleaseCaptured(WhsPickableDocketLine orderLine)
			{
				var pickableDocket = orderLine.PickableDocket;
				var client = pickableDocket != null ? pickableDocket.Client : null;
				var product = orderLine.Product;
				return product != null && product.IsSerialNumberReleaseCaptured(client);
			}

			static bool GetIsReleaseCaptured(WhsPickableDocketLine orderLine, PartAttributeNumber partAttrib)
			{
				var client = orderLine.PickableDocket?.Client;
				var product = orderLine.Product;
				return product != null && product.IsPartAttribReleaseCaptured(client, (int)partAttrib + 1);
			}

			static bool GetIsReleaseCaptured(WhsPickableDocketLine orderLine)
			{
				bool result = false;

				var pickableDocket = orderLine.PickableDocket;
				if (pickableDocket?.WD_DocketType.EqualsIgnoringCase(DocketType.Codes.Order) ?? false)
				{
					var client = pickableDocket.Client;
					var product = orderLine.Product;
					result = product != null && product.IsAnyPartAttribReleaseCaptured(client);
				}

				return result;
			}

			static ZString GetPartAttributeNameCore(WhsPickableDocketLine orderLine, PartAttributeNumber partAttrib)
			{
				var pickableDocket = orderLine.PickableDocket;
				var client = pickableDocket != null ? pickableDocket.Client : null;
				return client != null ? client.PartAttributeManager.PartAttributeName((int)partAttrib + 1) : ZString.Empty;
			}
		}

		#endregion

		#region Caches

		#region ReleaseLinesCache

		HashSet<WhsReleaseLine> ReleaseLinesCache
		{
			get { return releaseLinesCache ?? (releaseLinesCache = new HashSet<WhsReleaseLine>()); }
		}

		HashSet<WhsReleaseLine> releaseLinesCache;

		#endregion

		#region PickLinesByAttributes

		Dictionary<string, HashSet<WhsPickLine>> PickLinesByAttributes
		{
			get { return pickLinesByAttributes ?? (pickLinesByAttributes = new Dictionary<string, HashSet<WhsPickLine>>()); }
		}

		Dictionary<string, HashSet<WhsPickLine>> pickLinesByAttributes;

		static IEnumerable<WhsPickLine> GetNonDeletedPickLines(IEnumerable<WhsPickLine> pickLines) => pickLines.Where(p => !p.IsDeleted);

		static IEnumerable<WhsPickLine> GetNonDeletedPickLinesExcludingPickByBOMLines(IEnumerable<WhsPickLine> pickLines)
			=> GetNonDeletedPickLines(pickLines).Where(p => !p.IsPickByBOMKitPickLine());

		#endregion

		#region ReleaseLinesByAttributes

		Dictionary<string, WhsReleaseLine> ReleaseLinesByAttributes
		{
			get { return releaseLinesByAttributes ?? (releaseLinesByAttributes = new Dictionary<string, WhsReleaseLine>()); }
		}

		Dictionary<string, WhsReleaseLine> releaseLinesByAttributes;

		#endregion

		#endregion

		// interfaces

		#region IBusiness

		void IBusiness.RunPreSaveValidation()
		{
			RunPreSaveValidation();
		}

		public new void RunPreSaveValidation()
		{
			if (ReleaseCapturedHelper.IsReleaseCaptured(OrderLine))
			{
				AddReleaseCapturedAttributesForNonCapturedReleaseLines();
				AddErrorsIfThereAreReleaseCapturedAttribsOnPickLinesNotReflectedByReleaseLines();
			}

			base.RunPreSaveValidation();
		}

		void AddReleaseCapturedAttributesForNonCapturedReleaseLines()
		{
			foreach (var releaseGroup in ReleaseLinesByAttributes)
			{
				var releaseLine = releaseGroup.Value;
				if (releaseLine != null && IsAnyAttributeReleaseCaptured(releaseLine))
				{
					var releasedQty = GetCurrentReleasedQty(releaseGroup.Key);
					if (releasedQty > 0m)
					{
						var uncapturedQty = releasedQty - GetCurrentReleaseCapturedQty(releaseGroup.Key, releaseLine);
						if (uncapturedQty > 0m) // we don't want to remove Release Captured Attributes automatically, that should be done by the user.
						{
							var keyWithoutAttribs = GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine);
							UpdateReleasedQuantityCore(releaseLine, uncapturedQty, releaseGroup.Key, keyWithoutAttribs);
						}
					}
				}
			}
		}

		#region AddErrorsIfThereAreReleaseCapturedAttribsOnPickLinesNotReflectedByReleaseLines

		void AddErrorsIfThereAreReleaseCapturedAttribsOnPickLinesNotReflectedByReleaseLines()
		{
			var errorMessageSuffix = Res.GetString("a36cd20f-4f31-49e1-b38a-22e7181523a0", "This may have been caused by entering Release Captured Attributes in {0} before Release Capturing Attributes in RF. You must update the Release Lines for this Order line to match the Release Captured Quantity.", Core.Constants.ProductName);
			OrderLine.ClearRowNotificationsContaining(errorMessageSuffix);

			foreach (var pickLineGroup in PickLinesByAttributes)
			{
				var pickLines = GetNonDeletedPickLinesExcludingPickByBOMLines(pickLineGroup.Value).ToArray();
				var pickLine = pickLines.FirstOrDefault();
				if (pickLine != null)
				{
					var key = pickLineGroup.Key;
					AddRowErrorToOrderLineIfNecessary(pickLine, key, pickLines.Sum(pl => pl.WZ_Units), errorMessageSuffix);
				}
			}
		}

		void AddRowErrorToOrderLineIfNecessary(WhsPickLine pickLine, string key, decimal releaseCapturedQty, ZString errorMessageSuffix)
		{
			var releasedQty = GetCurrentReleasedQty(key);
			AddRowErrorToOrderLineIfNecessaryCore(
				pickLine.WZ_ReleaseCapturedPartAttrib1,
				pickLine.WZ_ReleaseCapturedPartAttrib2,
				pickLine.WZ_ReleaseCapturedPartAttrib3,
				pickLine.WZ_ReleaseCapturedSerialNumber,
				releaseCapturedQty, releasedQty, errorMessageSuffix);
		}

		void AddRowErrorToOrderLineIfNecessaryCore(ZString attrib1, ZString attrib2, ZString attrib3, ZString serialNumber, decimal releaseCapturedQty, decimal releasedQty, ZString errorMessageSuffix)
		{
			if (releaseCapturedQty > releasedQty)
			{
				var attribs = new[]
				{
					new { Attribute = attrib1, AttribNo = PartAttributeNumber.One },
					new { Attribute = attrib2, AttribNo = PartAttributeNumber.Two },
					new { Attribute = attrib3, AttribNo = PartAttributeNumber.Three },
					new { Attribute = serialNumber, AttribNo = PartAttributeNumber.SerialNumber }
				};

				var attribInfo = string.Join(", ", attribs.Where(a => !a.Attribute.IsEmpty).Select(a => string.Format(Culture.Current, "{0}: {1}", ReleaseCapturedHelper.GetPartAttributeName(OrderLine, a.AttribNo), a.Attribute)));
				OrderLine.AddRowError(Res.GetString("faf64550-1738-4fee-a336-4e1692a759e2", @"There are {0}x [{1}] Release Captured Attributes for Product {2} but only {3} released on this Order Line.
{4}",
					((ZDecimal)releaseCapturedQty).ToStringTrimZeros(), attribInfo, OrderLine.ProductDesc, ((ZDecimal)releasedQty).ToStringTrimZeros(), errorMessageSuffix));
			}
		}

		#endregion

		#endregion

		#region ICollection

		int ICollection.Count
		{
			get { return Count; }
		}

		public new int Count
		{
			get
			{
				if (IsInvalidated)
				{
					RefreshCollection();
				}

				return base.Count;
			}
		}

		#endregion

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<BusinessObject> IEnumerable<BusinessObject>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<BusinessObject> GetEnumerator()
		{
			if (IsInvalidated)
			{
				RefreshCollection();
			}

			return new BusinessObjectCollectionEnumerator(this);
		}

		#endregion
	}
}
