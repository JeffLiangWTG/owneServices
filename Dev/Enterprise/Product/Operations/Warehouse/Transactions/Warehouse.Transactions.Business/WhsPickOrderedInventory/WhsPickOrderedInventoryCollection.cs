using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickOrderedInventoryCollection : NonPersistentBusinessObjectCollection<WhsPickOrderedInventory>, IEnumerable<BusinessObject>, IEnumerable
	{
		#region Constructors

		public WhsPickOrderedInventoryCollection(BusinessObjectFactory factory, WhsPick pick)
			: base(factory)
		{
			Pick = pick;
		}

		readonly WhsPick Pick;

		#endregion

		#region Adding / Removing From Order

		#region OnAdded

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			if (!AddingOrderedInventorySempahore.IsSuspended)
			{
				throw new InvalidOperationException("Should not add Ordered Inventory to the collection manually, use 'AddNewFromOrder()' instead.");
			}

			base.OnAdded(bizOAdded);
		}

		Semaphore AddingOrderedInventorySempahore
		{
			get { return addingOrderedInventorySempahore ?? (addingOrderedInventorySempahore = new Semaphore()); }
		}

		Semaphore addingOrderedInventorySempahore;

		#region Test
#if DEBUG
		internal static IDisposable SuspendAddingOrderedInventorySempahoreForTesting(WhsPickOrderedInventoryCollection collection)
		{
			return new SemaphoreManager(collection.AddingOrderedInventorySempahore);
		}
#endif
		#endregion

		#endregion

		public void AddNewFromOrder(WhsPickableDocket docket)
		{
			Merge(docket.GetLinesToPick().Cast<WhsPickableDocketLine>());

			ValidationItemsAfterAddOrRemove();
		}

		public void RemoveFromOrder(WhsPickableDocket order)
		{
			if (order != null)
			{
				var branchPK = GetBranchPK();
				foreach (var line in order.GetLinesToPick().Cast<WhsPickableDocketLine>())
				{
					Unmerge(branchPK, line);
				}

				if (order.Pick != null)
				{
					ValidationItemsAfterAddOrRemove();
				}
			}
		}

		public void AddNewFromOrderLines(IEnumerable<WhsPickableDocketLine> orderLines)
		{
			Merge(orderLines);
			ValidationItemsAfterAddOrRemove();
		}

		void ValidationItemsAfterAddOrRemove()
		{
			if (!DeferValidationItemsSemaphore.IsSuspended)
			{
				foreach (WhsPickOrderedInventory orderedInventory in this)
				{
					using (new SemaphoreManager(orderedInventory.AvailableInventoryValidationSuspendedSemaphore))
					{
						orderedInventory.Validation.ValidatePickLineQuantity();
						orderedInventory.Validation.ValidateQuantityShort();
					}
				}

				shouldRunValidationItemsAfterSuspension = false;
			}
			else
			{
				shouldRunValidationItemsAfterSuspension = true;
			}
		}

		internal IDisposable DeferValidationItemsOnAddOrRemove() =>
			new DisposableList(new IDisposable[] { new SemaphoreManager(DeferValidationItemsSemaphore), new DisposableAction(RunValidationItemsIfNecessary) });

		Semaphore DeferValidationItemsSemaphore
		{
			get { return deferValidationItemsSemaphore ?? (deferValidationItemsSemaphore = new Semaphore()); }
		}

		void RunValidationItemsIfNecessary()
		{
			if (shouldRunValidationItemsAfterSuspension)
			{
				ValidationItemsAfterAddOrRemove();
			}
		}

		Semaphore deferValidationItemsSemaphore;
		bool shouldRunValidationItemsAfterSuspension;

		#endregion

		#region GetOrderedInventoryForLine

		public WhsPickOrderedInventory GetOrderedInventoryForLine(WhsPickableDocketLine line) => this.Cast<WhsPickOrderedInventory>().FirstOrDefault(pickOrderedInventory => pickOrderedInventory.Owners.Contains(line));

		#endregion

		#region Merging

		void Merge(IEnumerable<WhsPickableDocketLine> orderLines)
		{
			var ranges = new Dictionary<WhsPickOrderedInventory, List<WhsPickableDocketLine>>();
			var branchPK = GetBranchPK();
			foreach (var orderLine in orderLines)
			{
				var orderLineAttribHashCode = GetAttributesHashCode(branchPK, orderLine);
				if (OrderedInventoryLookup.TryGetValue(orderLineAttribHashCode, out var orderedInventory))
				{
					if (!ranges.TryGetValue(orderedInventory, out var range))
					{
						ranges[orderedInventory] = range = new List<WhsPickableDocketLine>();
					}

					range.Add(orderLine);
				}
				else
				{
					using (new SemaphoreManager(AddingOrderedInventorySempahore))
					{
						orderedInventory = AddNew();
					}

					((IWhsPickOrderedInventoryInternals)orderedInventory).SetAllProperties(Pick, orderLine);
					OrderedInventoryLookup.Add(orderLineAttribHashCode, orderedInventory);
				}
			}

			foreach (var entry in ranges)
			{
				entry.Key.Owners.AddRange(entry.Value);
			}
		}

		Guid GetBranchPK()
		{
			var branchPK = Pick?.Warehouse?.WW_GB_RelatedCompanyBranch;
			return branchPK != null && branchPK.Value.IsValid ? branchPK.Value.ToGuid() : Guid.Empty;
		}

		void Unmerge(Guid branchPK, WhsPickableDocketLine line)
		{
			WhsPickOrderedInventory orderedInventory;
			var orderLineAttribHashCode = GetAttributesHashCode(branchPK, line);

			if (OrderedInventoryLookup.TryGetValue(orderLineAttribHashCode, out orderedInventory))
			{
				if (orderedInventory.Owners.Contains(line))
				{
					orderedInventory.Owners.RemoveFromRelationship(line);

					if (orderedInventory.QuantityOrdered <= 0m)
					{
						orderedInventory.ClearAvailableInventoriesCache(); // since we are removing the ordered inventory, its available inventories need to be cleared as well
						Remove(orderedInventory);
						OrderedInventoryLookup.Remove(orderLineAttribHashCode);
					}
				}
			}
		}

		string GetAttributesHashCode(Guid branchPK, WhsPickableDocketLine line)
		{
			var hashCode = new StringBuilder();
			var docket = line.Docket;
			hashCode.Append(docket.WD_OH_Client.ToString());
			hashCode.Append(docket.IsCustomsTransaction);
			hashCode.Append(docket.WD_IsInwardsProcessingJob);
			hashCode.Append(line.WE_OP.ToString());
			hashCode.Append(line.WE_PalletID.ToString());
			hashCode.Append(line.WE_BondedEntryKey);
			if (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				hashCode.Append(line.WE_SerialNumber);
			}
			hashCode.Append(AttributeComparer.GetHashCodeForConsolidation(line));
			AddCustomAttributeHash(branchPK, line, hashCode);
			hashCode.Append(line.MinimumShelfLife);
			hashCode.Append(line.WE_PackageGroupId); // no need to add PerPackageQty as it is readonly for order.

			if (line.IsComponentLineOnSalesOrder)
			{
				var parentLine = line.ParentLine;
				if (parentLine != null)
				{
					hashCode.Append(parentLine.WE_OP.ToString());
				}
			}

			return hashCode.ToString();
		}

		void AddCustomAttributeHash(Guid branchPK, WhsPickableDocketLine line, StringBuilder hashCode)
		{
			if (WarehouseDataRegistry.Instance.GroupOrderedInventoryByCustomAttributes.GetFallBackValueAtAllLevels(
				Guid.Empty,
				branchPK,
				Guid.Empty))
			{
				hashCode.Append(CustomAttributeComparer.GetHashCodeForConsolidation(line));
			}
		}

		Dictionary<string, WhsPickOrderedInventory> OrderedInventoryLookup
		{
			get { return orderedInventoryLookup ?? (orderedInventoryLookup = new Dictionary<string, WhsPickOrderedInventory>()); }
		}

		CustomAttributeComparer CustomAttributeComparer
		{
			get { return customAttributeComparer ?? (customAttributeComparer = new CustomAttributeComparer()); }
		}

		Dictionary<string, WhsPickOrderedInventory> orderedInventoryLookup;
		CustomAttributeComparer customAttributeComparer;

		#endregion

		#region NonPersistentBusinessObjectCollection Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WhsPickOrderedInventory(Factory);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region ClearCollectionAndRelatedCache

		internal void ClearCollectionAndRelatedCache()
		{
			using (SuspendRefreshingCollection())
			{
				foreach (WhsPickOrderedInventory orderedInventory in this)
				{
					orderedInventory.ClearAvailableInventoriesCache();
				}

				RemoveAll();
				OrderedInventoryLookup.Clear();
			}
		}

		#endregion

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumeratorCore();
		}

		IEnumerator<BusinessObject> IEnumerable<BusinessObject>.GetEnumerator()
		{
			return GetEnumeratorCore();
		}

		IEnumerator<BusinessObject> GetEnumeratorCore()
		{
			if (CanCollectionBeRefreshed)
			{
				RefreshCollection();
			}

			return new BusinessObjectCollectionEnumerator(this);
		}

		#endregion

		#region RefreshCollection

		void RefreshCollection()
		{
			using (SuspendRefreshingCollection())
			{
				Pick.RebuildOrderedInventoriesIfNecessary();
			}
		}

		internal IDisposable SuspendRefreshingCollection() => new SemaphoreManager(RefreshingCollection);

		bool CanCollectionBeRefreshed => Pick != null && Pick.OrderedInventoriesNeedRefresh && !RefreshingCollection.IsSuspended;

		Semaphore RefreshingCollection => refreshingCollection ?? (refreshingCollection = new Semaphore());
		Semaphore refreshingCollection;

		#endregion
	}
}
