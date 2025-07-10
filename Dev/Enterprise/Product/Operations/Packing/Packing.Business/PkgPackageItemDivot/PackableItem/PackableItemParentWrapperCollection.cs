using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public class PackableItemParentWrapperCollection : NonPersistentBusinessObjectCollection<PackableItemParentWrapper>, IEnumerable<PackableItemParentWrapper>, ICollection
	{
		#region Construction

		public PackableItemParentWrapperCollection(IPackingParentWithPackableItems parentJob)
			: this(parentJob.Factory)
		{
			ParentJob = parentJob;
		}

		public PackableItemParentWrapperCollection(BusinessObjectFactory factory, IEnumerable<PackableItemParentWrapper> packableItemParents = null)
			: base(Argument.NotNull(factory, nameof(factory)))
		{
			if (packableItemParents != null)
			{
				AddRange(packableItemParents);
			}
		}

		readonly IPackingParentWithPackableItems ParentJob;

		#endregion

		#region AllowNew / CreateNonPersistentBusinessObject

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		#endregion

		#region FindByPackableItemParent / FindByPackedItem
#if DEBUG

		/// <summary>
		/// Only used in tests. do not make this available in release code because it returns the first found (and an item may be wrapped multiple times)
		/// </summary>
		public PackableItemParentWrapper FindByPackableItemParent(IPackableItemParent packableItemParent)
		{
			Argument.NotNull(packableItemParent, nameof(packableItemParent));
			return this.FirstOrDefault<PackableItemParentWrapper>(wrapper => wrapper.PackableItemParent == packableItemParent);
		}

		/// <summary>
		/// Only used in tests. do not make this available in release code because it returns the first found (and an item may be wrapped multiple times)
		/// </summary>
		public PackableItemParentWrapper FindByPackedItem(PkgPackageItemDivotsWrapper packedItem)
		{
			Argument.NotNull(packedItem, nameof(packedItem));
			return this.FirstOrDefault<PackableItemParentWrapper>(wrapper => wrapper.GroupedPackedItems.Contains(packedItem));
		}

#endif
		#endregion

		#region GetPackableItemFromKey

		public IPackableItemParent GetPackableItemParentFromKey(GroupingKey key)
		{
			Argument.NotNull(key, nameof(key));

			if (IsInvalidated)
			{
				RebuildWrappersFromParentCollection();
			}

			return PackableItemParentsByGroup.TryGetValue(key, out var packableItemParent) ? packableItemParent : null;
		}

		#endregion

		#region RebuildWrappersFromParentCollection

		void RebuildWrappersFromParentCollection()
		{
			using (SuspendListChanged())
			{
				var currentKeys = new HashSet<GroupingKey>();

				// add new items
				foreach (var packableItemParent in ParentJob.PackableItemParents)
				{
					if (packableItemParent.TotalQty > 0m && packableItemParent.PackableItems.Any())
					{
						var wrapper = new PackableItemParentWrapper(Factory, packableItemParent, ParentJob);
						currentKeys.Add(wrapper.Key);

						if (!PackableItemParentsByGroup.TryGetValue(wrapper.Key, out var result) || result.IsDeleted)
						{
							PackableItemParentsByGroup[wrapper.Key] = packableItemParent;
							Add(wrapper);
						}
					}
				}

				// remove deleted items
				for (var index = base.Count - 1; index >= 0; index--)
				{
					var wrapper = this[index];
					if (wrapper.PackableItemParent == null || !currentKeys.Contains(wrapper.Key))
					{
						Remove(wrapper);

						if (!currentKeys.Contains(wrapper.Key))
						{
							PackableItemParentsByGroup.Remove(wrapper.Key);
						}
					}
				}
			}

			IsInvalidated = false;
		}

		Dictionary<GroupingKey, IPackableItemParent> PackableItemParentsByGroup
		{
			get { return packableItemParentsByGroup ?? (packableItemParentsByGroup = new Dictionary<GroupingKey, IPackableItemParent>()); }
		}

		Dictionary<GroupingKey, IPackableItemParent> packableItemParentsByGroup;

		bool IsInvalidated { get; set; }

		#endregion

		#region WrapPackableItemParents

		internal void WrapPackableItemParents()
		{
			ParentJob.PackableItemParentsCountChanged += PackableItemParents_CountChanged;
			RebuildWrappersFromParentCollection();
		}

		void PackableItemParents_CountChanged(object sender, EventArgs e)
		{
			IsInvalidated = true;
		}

		#endregion

		#region IEnumerator<T>

		public IEnumerator<PackableItemParentWrapper> GetEnumerator()
		{
			return Enumerable().GetEnumerator();
		}

		IEnumerable<PackableItemParentWrapper> Enumerable()
		{
			foreach (PackableItemParentWrapper wrapper in (IEnumerable)this)
			{
				yield return wrapper;
			}
		}

		#endregion

		#region ICollection

		int ICollection.Count => Count;

		public new int Count
		{
			get
			{
				if (IsInvalidated)
				{
					RebuildWrappersFromParentCollection();
				}

				return base.Count;
			}
		}

		#endregion

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (IsInvalidated)
			{
				RebuildWrappersFromParentCollection();
			}

			return new BusinessObjectCollectionEnumerator(this);
		}

		#endregion
	}
}
