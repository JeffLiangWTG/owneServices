using CargoWise.Types;

namespace Enterprise.MasterFiles.Business;

public static class IsraelInvoiceAmountBoundaryRetriever
{
	public static ZDecimal GetAmount(ZDateTime date)
	{
		foreach (InvoiceAmountBoundary boundary in AccountingMasterFilesRegistry.Instance.IsraelInvoiceAmountBoundaries.Value)
		{
			if (boundary.IsWithinDateAmount(date))
			{
				return boundary.Amount;
			}
		}
		return ZDecimal.Zero;
	}
}
