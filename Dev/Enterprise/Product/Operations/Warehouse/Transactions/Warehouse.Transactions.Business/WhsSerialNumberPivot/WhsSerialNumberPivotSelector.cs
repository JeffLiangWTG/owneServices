using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsSerialNumberPivotSelector : NonPersistentBusinessObject, ISerialNumberPivotAssigner
	{
		#region Constractor

		public WhsSerialNumberPivotSelector(WhsSerialNumberPivot serialNumberPivot, Func<ZGuid, bool> readOnlyForChangingAllocationsProvider)
			: base()
		{
			Argument.NotNull(serialNumberPivot, nameof(serialNumberPivot));
			Argument.NotNull(readOnlyForChangingAllocationsProvider, nameof(readOnlyForChangingAllocationsProvider));

			SerialNumberPivot = serialNumberPivot;
			ReadOnlyForChangingAllocationsProvider = readOnlyForChangingAllocationsProvider;
		}

		public WhsSerialNumberPivotSelector(WhsSerialNumberPivot serialNumberPivot, WhsPickLine pickLineforPicking)
			: this(serialNumberPivot, (_) => true)
		{
			Argument.NotNull(pickLineforPicking, nameof(pickLineforPicking));

			SerialNumberPivot = serialNumberPivot;
			PickLineforPicking = pickLineforPicking;
		}

		public abstract class Schema
		{
			public const string SerialNumberValue = nameof(SerialNumberValue);
			public const string Selected = nameof(Selected);
		}

		#endregion

		#region Properties

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(SelectedReadOnly))]
		public ZBool Selected
		{
			get => SerialNumberPivot.WSV_WZ_PickingLine.IsValid;
			set
			{
				if (Selected != value)
				{
					OnSelectionChanged?.Invoke(this, EventArgs.Empty);
					SelectedInfo.RefreshBinding();
				}
			}
		}

		public event EventHandler OnSelectionChanged;

		bool SelectedReadOnly => ReadOnly || ReadOnlyForChangingAllocationsProvider(PickingLinePK);

		public ZPropertyInfo SelectedInfo => GetZPropertyInfo(Schema.Selected);

		public ZString SerialNumberValue => SerialNumberPivot.SerialNumberValue;

		public ZGuid InventoryPK => SerialNumberPivot.WSV_ParentID;

		public ZGuid PickingLinePK => SerialNumberPivot.WSV_WZ_PickingLine;

		public WhsPickLine PickLineforPicking { get; }

		readonly WhsSerialNumberPivot SerialNumberPivot;

		readonly Func<ZGuid, bool> ReadOnlyForChangingAllocationsProvider;

		#endregion

		#region Methods

		public void SelectSerialNumber(ZGuid pickLinePK) => SerialNumberPivot.WSV_WZ_PickingLine = pickLinePK;

		public void SelectSerialNumber()
		{
			if (PickLineforPicking == null)
			{
				throw new InvalidOperationException("Only call SelectSerialNumber when pickline is already taken from this serial number.");
			}

			SerialNumberPivot.WSV_WZ_PickingLine = PickLineforPicking.PK;
		}

		public void RemoveSerialNumberSelection()
		{
			if (SerialNumberPivot.WSV_WZ_PickingLine != ZGuid.Empty)
			{
				SerialNumberPivot.WSV_WZ_PickingLine = ZGuid.Empty;
			}
		}

		#endregion
	}
}
