using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class InventorySelectionLineCollection<TInventorySelectionLine> :
		NonPersistentBusinessObjectCollection<TInventorySelectionLine>,
		IInventorySelectionLineCollection<TInventorySelectionLine>
		where TInventorySelectionLine : InventorySelectionLine
	{
		public InventorySelectionLineCollection(InventorySelectionHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}

		public bool HasALeastOneLineWithDrawQty
		{
			get
			{
				if (hasALeastOneLineWithDrawQtyCached == null)
				{
					hasALeastOneLineWithDrawQtyCached = new CachedProperty<bool>(Factory, () => this.OfType<InventorySelectionLine>().Any(x => x.HasDrawQty));
				}
				return hasALeastOneLineWithDrawQtyCached.Value;
			}
		}
		CachedProperty<bool> hasALeastOneLineWithDrawQtyCached;

		public bool WillDrawLineFromDifferentWarehouses
		{
			get
			{
				if (willDrawLineFromDifferentWarehousesCached == null)
				{
					willDrawLineFromDifferentWarehousesCached = new CachedProperty<bool>(Factory, () =>
						{
							ZString? warehouse = null;
							foreach (var line in this.OfType<InventorySelectionLine>().Where(x => x.HasDrawQty))
							{
								if (warehouse.HasValue)
								{
									if (warehouse.Value != line.US_Warehouse)
									{
										return true;
									}
								}
								else
								{
									warehouse = line.US_Warehouse;
								}
							}
							return false;
						});
				}
				return willDrawLineFromDifferentWarehousesCached.Value;
			}
		}
		CachedProperty<bool> willDrawLineFromDifferentWarehousesCached;

		public OrgAddress GetFirstWarehouseAddress()
		{
			OrgAddress result = null;
			foreach (var line in this.OfType<InventorySelectionLine>())
			{
				result = line.GetWarehouseAddress();
				if (result != null)
				{
					break;
				}
			}
			return result;
		}

		#region Implementation
		readonly InventorySelectionHeader header;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InventorySelectionLine(header);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
		#endregion

		public IEnumerator<TInventorySelectionLine> GetEnumerator() => Elements.Cast<TInventorySelectionLine>().GetEnumerator();
	}
}
