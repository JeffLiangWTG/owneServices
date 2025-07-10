using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class InventoryAllocationsUserControl : ZUserControl
	{
		public InventoryAllocationsUserControl()
		{
			InitializeComponent();
		}

		#region OnCurrentDataItemChanged

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var docket = DocketLine != null ? DocketLine.Docket : null;
			PartAttributeColumnManager.SetColumns(docket != null ? docket.Client : null);
		}

		#endregion

		#region PartAttributeColumnManager

		PartAttributeColumnManager PartAttributeColumnManager
		{
			get
			{
				if (partAttributeColumnManager == null)
				{
					partAttributeColumnManager = new PartAttributeColumnManager(crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid,
						"DocketLine+" + WhsDocketLineSchema.WE_ExpiryDate.Name,
						"DocketLine+" + WhsDocketLineSchema.WE_PackingDate.Name,
						"DocketLine+" + WhsDocketLineSchema.WE_PartAttrib1.Name,
						"DocketLine+" + WhsDocketLineSchema.WE_PartAttrib2.Name,
						"DocketLine+" + WhsDocketLineSchema.WE_PartAttrib3.Name,
						"DocketLine+" + WhsDocketLineSchema.WE_SerialNumber.Name);
				}
				return partAttributeColumnManager;
			}
		}
		PartAttributeColumnManager partAttributeColumnManager;

		#endregion

		#region DocketLine

		WhsDocketLine DocketLine
		{
			get { return (WhsDocketLine)CurrentDataItem; }
		}

		#endregion
	}
}

#if DEBUG

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class InventoryAllocationsUserControl
	{
		public CrossDockedOrderLineAttachedToInventoryGrid GetCrossDockedOrderLineAttachedToInventoryGrid() => crossDockedOrderLineAttachedToInventoryGrid1;
	}
}

#endif
