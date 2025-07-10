using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

sealed class IsraelInvoiceAmountBoundaryRetrieverTest : TestCase
{
	public void TestGetAmount()
	{
		var collection = AccountingMasterFilesRegistry.Instance.IsraelInvoiceAmountBoundaries.Value;
		var first = collection[0];
		var second = collection[1];

		AssertEquals("Empty date", ZDecimal.Zero, IsraelInvoiceAmountBoundaryRetriever.GetAmount(ZDateTime.Empty));

		AssertEquals("Before start date (no period)", ZDecimal.Zero, IsraelInvoiceAmountBoundaryRetriever.GetAmount(first.StartDate.AddDays(-1)));
		AssertEquals("Start date", first.Amount, IsraelInvoiceAmountBoundaryRetriever.GetAmount(first.StartDate));
		AssertEquals("After start date", first.Amount, IsraelInvoiceAmountBoundaryRetriever.GetAmount(first.StartDate.AddDays(1)));

		AssertEquals("Before end date", first.Amount, IsraelInvoiceAmountBoundaryRetriever.GetAmount(first.EndDate.AddDays(-1)));
		AssertEquals("End date", first.Amount, IsraelInvoiceAmountBoundaryRetriever.GetAmount(first.EndDate));
		AssertEquals("After end date (next period)", second.Amount, IsraelInvoiceAmountBoundaryRetriever.GetAmount(first.EndDate.AddDays(1)));

		AssertEquals("Much earlier than the first period", ZDecimal.Zero, IsraelInvoiceAmountBoundaryRetriever.GetAmount(new ZDateTime(1980, 1, 1)));
		AssertEquals("Much later than the last period", ZDecimal.Zero, IsraelInvoiceAmountBoundaryRetriever.GetAmount(new ZDateTime(2030, 1, 1)));
	}
}
