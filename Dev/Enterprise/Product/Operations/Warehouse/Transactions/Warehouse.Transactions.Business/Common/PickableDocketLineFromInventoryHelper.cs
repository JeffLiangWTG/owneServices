using System;
using CargoWise.ComponentModel;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PickableDocketLineFromInventoryHelper<T> : DocketLineFromInventoryHelper<T>
		where T : WhsPickableDocketLine
	{
		public PickableDocketLineFromInventoryHelper(INotifications notifications, WhsPickableDocket docket)
			: base(notifications, docket)
		{
		}

		// tested in WhsOrderLineFromInventoryTestCase
		protected override IDisposable SuspendProcessWhileAcceptingInventoryLines() => ((WhsPickableDocket)Docket).ShortfallManager.DeferMarkingLinesAsShortfallPropertiesChanged();

		protected override void SetDocketLineFromInventoryCore(T transactionLine, WhsDocketLine inventoryLine, ExcludeFromCopy exclude)
		{
			using (transactionLine.DeferSettingHasProductUnitsOrAttribsChanged())
			{
				base.SetDocketLineFromInventoryCore(transactionLine, inventoryLine, exclude);

				if (!exclude.HasFlag(ExcludeFromCopy.Qty))
				{
					transactionLine.WE_TransactionQuantity = inventoryLine.AvailableToPickQuantity;
				}
			}
		}

		protected override void SetLocationDataFromInventory(T transactionLine, WhsDocketLine inventoryLine)
		{
			// Location not used by order
		}
	}
}
