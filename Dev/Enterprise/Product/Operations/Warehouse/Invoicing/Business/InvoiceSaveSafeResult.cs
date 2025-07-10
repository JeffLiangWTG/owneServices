namespace Enterprise.Warehouse.Invoicing.Business
{
	#region InvoiceSaveSafeResult

	public enum InvoiceSaveSafeResult
	{
		SaveSuccessful,
		SaveFailedRetry,
		SaveFailedNoRetry,
	}

	#endregion
}
