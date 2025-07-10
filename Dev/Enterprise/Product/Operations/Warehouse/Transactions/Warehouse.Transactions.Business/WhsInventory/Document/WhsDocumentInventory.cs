using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocumentInventory : NonPersistentBusinessObject
	{
		public WhsDocumentInventory(WhsInventoryView inventory)
			: base(inventory.Factory)
		{
			Inventory = inventory;
		}

		internal WhsDocumentInventory() : base() { }

		#region Inventory

		public WhsInventoryView Inventory
		{
			get { return inventory; }
			set
			{
				inventory = value;
				LabelsToPrint = TotalLabelsToPrintFrom;
			}
		}

		WhsInventoryView inventory;

		#endregion

		#region LabelsToPrint

		public ZDecimal LabelsToPrint
		{
			get { return labelsToPrint; }
			set
			{
				SetNonPersistentPropertyValue(LabelsToPrintInfo, ref labelsToPrint, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateLabelsToPrint();
				}
			}
		}

		ZDecimal labelsToPrint;

		public ZPropertyInfo LabelsToPrintInfo
		{
			get { return GetZPropertyInfo(nameof(LabelsToPrint)); }
		}

		#endregion

		#region TotalLabelsToPrintFrom

		public ZDecimal TotalLabelsToPrintFrom
		{
			get
			{
				return Inventory != null ? decimal.Ceiling(Inventory.WI_TotalUnits) : 0m;
			}
		}

		#endregion

		#region Validation

		public WhsDocumentInventoryValidation Validation
		{
			get { return new WhsDocumentInventoryValidation(this); }
		}

		#endregion
	}
}
