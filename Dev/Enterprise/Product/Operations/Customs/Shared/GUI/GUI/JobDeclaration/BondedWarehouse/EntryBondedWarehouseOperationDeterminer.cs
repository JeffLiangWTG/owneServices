using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GUI
{
	public class EntryBondedWarehouseOperationDeterminer : BondedWarehouseOperationDeterminer
	{
		public EntryBondedWarehouseOperationDeterminer(CusEntryHeader supporter)
			: base(supporter)
		{
		}

		protected new CusEntryHeader supporter
		{
			get { return (CusEntryHeader)base.supporter; }
		}

		#region Inventory Management Data

		bool IsWaitingForResponse
		{
			get { return supporter.IsWaitingForResponse; }
		}

		#endregion

		protected override bool CanCancelBondedWarehouseChangeOfOwnershipCheckCore()
		{
			var result = base.CanCancelBondedWarehouseChangeOfOwnershipCheckCore();
			if (IsWaitingForResponse)
			{
				result = false;
				Globals.Message.ShowError(ResString.GetMultilingualString("{C943A55C-0979-4E99-A9B0-1B1FD058F7CF}", "Cannot cancel Bonded Warehouse Change of Ownership while waiting for a response."));
			}
			return result;
		}

		protected override bool CanCancelInventoryChangeOfRegimeCheckCore()
		{
			var result = base.CanCancelInventoryChangeOfRegimeCheckCore();
			if (IsWaitingForResponse)
			{
				result = false;
				Globals.Message.ShowError(ResString.GetMultilingualString("{9D1FA1B9-CB4F-4029-96F0-043B2C88BCCF}", "Cannot cancel Inventory Change of Regime while waiting for a response."));
			}
			return result;
		}

		protected override bool CanCancelBondedWarehouseOutwardCheckCore()
		{
			var result = base.CanCancelBondedWarehouseOutwardCheckCore();
			if (IsWaitingForResponse)
			{
				result = false;
				Globals.Message.ShowError(ResString.GetMultilingualString("{8429C8A7-3642-4696-8265-3890BF4F4C7B}", "Cannot cancel Inventory stock release while waiting for a response."));
			}
			return result;
		}

		protected override bool CanCancelUpdateBondedWarehouseInwardCheckCore()
		{
			var result = base.CanCancelUpdateBondedWarehouseInwardCheckCore();

			if (IsWaitingForResponse)
			{
				result = false;
				Globals.Message.ShowError(ResString.GetMultilingualString("{6164340B-59A5-4EEA-B897-F777C308872D}", "Cannot cancel Inventory stock levels update while waiting for a response."));
			}
			return result;
		}

		protected override bool CanUpdateBondedWarehouseInwardCheckCore()
		{
			var result = base.CanUpdateBondedWarehouseInwardCheckCore();
			if (IsWaitingForResponse)
			{
				result = false;
				Globals.Message.ShowError(ResString.GetMultilingualString("{F6C650EF-23F2-4DE9-8D83-E6DB53814AD7}", "Cannot update Inventory stock levels while waiting for a response."));
			}
			return result;
		}

		protected override bool CanUpdateBondedWarehouseOutwardCheckCore()
		{
			var result = base.CanUpdateBondedWarehouseOutwardCheckCore();
			if (IsWaitingForResponse)
			{
				result = false;
				Globals.Message.ShowError(ResString.GetMultilingualString("{70E1FFD3-F890-4919-BA9D-D4EB3D2C5A86}", "Cannot update Inventory stock release while waiting for a response."));
			}
			return result;
		}

		protected override bool CanUpdateBondedWarehouseChangeOfOwnershipCheckCore()
		{
			var result = base.CanUpdateBondedWarehouseChangeOfOwnershipCheckCore();
			if (IsWaitingForResponse)
			{
				result = false;
				Globals.Message.ShowError(ResString.GetMultilingualString("{0FC81595-243A-4BDB-A0BF-A485D22F1FF3}", "Cannot update Bonded Warehouse Change of Ownership while waiting for a response."));
			}
			return result;
		}

		protected override bool CanUpdateInventoryChangeOfRegimeCheckCore()
		{
			var result = base.CanUpdateInventoryChangeOfRegimeCheckCore();
			if (IsWaitingForResponse)
			{
				result = false;
				Globals.Message.ShowError(ResString.GetMultilingualString("{3AA6545C-3009-49EA-9434-EDE9DB2353E5}", "Cannot update Inventory Change of Regime while waiting for a response."));
			}
			return result;
		}
	}
}
