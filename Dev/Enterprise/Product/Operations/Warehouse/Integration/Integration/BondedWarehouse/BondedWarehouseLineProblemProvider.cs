namespace Enterprise.Warehouse.Integration.BondedWarehouse
{
	public class BondedWarehouseLineProblemProvider
	{
		public NotificationCollection WarehouseProblems
		{
			get
			{
				if (warehouseProblems == null)
				{
					warehouseProblems = new NotificationCollection();
				}
				return warehouseProblems;
			}
		}
		NotificationCollection warehouseProblems;

		public NotificationCollection QuantityProblems
		{
			get
			{
				if (quantityProblems == null)
				{
					quantityProblems = new NotificationCollection();
				}
				return quantityProblems;
			}
		}
		NotificationCollection quantityProblems;

		public NotificationCollection EntryKeyProblems
		{
			get
			{
				if (entryKeyProblems == null)
				{
					entryKeyProblems = new NotificationCollection();
				}
				return entryKeyProblems;
			}
		}
		NotificationCollection entryKeyProblems;

		public NotificationCollection PartAttrib1Problems
		{
			get
			{
				if (partAttrib1Problems == null)
				{
					partAttrib1Problems = new NotificationCollection();
				}
				return partAttrib1Problems;
			}
		}
		NotificationCollection partAttrib1Problems;

		public NotificationCollection PartAttrib2Problems
		{
			get
			{
				if (partAttrib2Problems == null)
				{
					partAttrib2Problems = new NotificationCollection();
				}
				return partAttrib2Problems;
			}
		}
		NotificationCollection partAttrib2Problems;

		public NotificationCollection PartAttrib3Problems
		{
			get
			{
				if (partAttrib3Problems == null)
				{
					partAttrib3Problems = new NotificationCollection();
				}
				return partAttrib3Problems;
			}
		}
		NotificationCollection partAttrib3Problems;

		public NotificationCollection SerialNumberProblems
		{
			get
			{
				if (serialNumberProblems == null)
				{
					serialNumberProblems = new NotificationCollection();
				}
				return serialNumberProblems;
			}
		}
		NotificationCollection serialNumberProblems;

		public NotificationCollection BondedWarehouseQuantityProblems
		{
			get
			{
				if (bondedWarehouseQuantityProblems == null)
				{
					bondedWarehouseQuantityProblems = new NotificationCollection();
				}
				return bondedWarehouseQuantityProblems;
			}
		}
		NotificationCollection bondedWarehouseQuantityProblems;

		public virtual bool HasErrors
		{
			get
			{
				return WarehouseProblems.HasErrors ||
					QuantityProblems.HasErrors ||
					EntryKeyProblems.HasErrors ||
					PartAttrib1Problems.HasErrors ||
					PartAttrib2Problems.HasErrors ||
					PartAttrib3Problems.HasErrors ||
					SerialNumberProblems.HasErrors ||
					BondedWarehouseQuantityProblems.HasErrors;
			}
		}

		public virtual bool HasWarnings
		{
			get
			{
				return WarehouseProblems.HasWarnings ||
					QuantityProblems.HasWarnings ||
					EntryKeyProblems.HasWarnings ||
					PartAttrib1Problems.HasWarnings ||
					PartAttrib2Problems.HasWarnings ||
					PartAttrib3Problems.HasWarnings ||
					SerialNumberProblems.HasWarnings ||
					BondedWarehouseQuantityProblems.HasWarnings;
			}
		}

		#region Clear Notifications

		public virtual void ClearAllNotifications()
		{
			WarehouseProblems.ErrorList.Clear();
			WarehouseProblems.WarningList.Clear();
			QuantityProblems.ErrorList.Clear();
			QuantityProblems.WarningList.Clear();
			EntryKeyProblems.ErrorList.Clear();
			EntryKeyProblems.WarningList.Clear();
			PartAttrib1Problems.ErrorList.Clear();
			PartAttrib1Problems.WarningList.Clear();
			PartAttrib2Problems.ErrorList.Clear();
			PartAttrib2Problems.WarningList.Clear();
			PartAttrib3Problems.ErrorList.Clear();
			PartAttrib3Problems.WarningList.Clear();
			SerialNumberProblems.ErrorList.Clear();
			SerialNumberProblems.WarningList.Clear();
			BondedWarehouseQuantityProblems.ErrorList.Clear();
			BondedWarehouseQuantityProblems.WarningList.Clear();
		}

		#endregion
	}
}

