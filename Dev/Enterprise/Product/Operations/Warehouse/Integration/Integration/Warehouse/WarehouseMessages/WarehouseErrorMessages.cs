namespace Enterprise.Warehouse.Integration
{
	public static class WarehouseErrorMessages
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventTransactionAndPickQuantityDesyncTriggerID = "Transaction and Pick quantity is not correct.";

		public static string TransactionAndPickedQtyIsCorrectMsgForUser
		{
			get { return Res.GetString("6B098B96-33F7-47D4-88B2-DFF0BB1A92A8", @"Attempt to set Transaction and Pick qty out of sync. Try to close and re-open the form."); }
		}
	}
}
