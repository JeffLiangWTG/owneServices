using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsPickAvailableInventorySplitBaseCollection<T> : NonPersistentBusinessObjectCollection<T>
		where T : WhsPickAvailableInventorySplitBase
	{
		protected WhsPickAvailableInventorySplitBaseCollection(BusinessObjectFactory factory, WhsPickAvailableInventory availableInventory)
			: base(factory)
		{
			AvailableInventory = Argument.NotNull(availableInventory, nameof(availableInventory));
		}

		protected readonly WhsPickAvailableInventory AvailableInventory;

		#region RebuildCollection

		public void RebuildCollection()
		{
			using (SuspendListChanged())
			using (SuspendSettingHasChanges())
			{
				var pickLines = AvailableInventory.PickLines.ToArray();
				foreach (var pickLine in pickLines)
				{
					pickLine.HasChangesChanged -= HandleDeleteOfPickLine;
				}

				RemoveAndDeleteAll();

				var part = AvailableInventory.SupplierPart;
				if (part != null)
				{
					var pickLinesToUse = new List<IPickLinePair>(pickLines.Length);
					foreach (var pickLine in pickLines)
					{
						pickLine.HasChangesChanged += HandleDeleteOfPickLine;
						pickLinesToUse.Add(PickLinePair.New(pickLine));
					}

					RebuildCollectionCore(part, pickLinesToUse);

					NeedsRefresh = false;
				}
			}
			RefreshBinding();
		}

		void HandleDeleteOfPickLine(object o, HasChangesChangedEventArgs e)
		{
			var bisO = (BusinessObject)o;
			if (bisO.IsDeleted && e.ObjectJustWasChanged)
			{
				NeedsRefresh = true;
				AvailableInventory.AllocateInfo.RefreshBinding();
				AvailableInventory.PickLineQuantityInfo.RefreshBinding();
			}
		}

		protected abstract void RebuildCollectionCore(OrgSupplierPart part, IReadOnlyCollection<IPickLinePair> pickLinesToUse);

		#endregion

		internal bool NeedsRefresh { get; set; }

		#region AllowNew

		protected override bool AllowNewCore => false;

		#endregion

		#region AllowRemoveCore

		protected override bool AllowRemoveCore => false;

		#endregion
	}
}
