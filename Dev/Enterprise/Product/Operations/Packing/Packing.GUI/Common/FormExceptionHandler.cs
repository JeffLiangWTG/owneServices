using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Packing.GUI.Common
{
	public static class FormExceptionHandler
	{
		public static bool HandleSaveExceptionForTriggers(Exception ex)
		{
			var result = true;
			if (ex is ZSaveException)
			{
				if (ex.InnerException?.InnerException?.Message == WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID)      // Exception message is not translatable
				{
					Globals.Message.ShowError(WarehouseErrorMessages.TransactionAndPickedQtyIsCorrectMsgForUser);
				}
			}
			else
			{
				result = false;
			}

			return result;
		}
	}
}
