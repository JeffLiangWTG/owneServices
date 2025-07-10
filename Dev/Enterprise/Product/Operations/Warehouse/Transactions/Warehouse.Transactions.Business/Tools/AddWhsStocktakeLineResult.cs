namespace Enterprise.Warehouse.Transactions.Business.Tools
{
	public class AddWhsStocktakeLineResult
	{
		public AddWhsStocktakeLineResult(WhsStocktakeLine stocktakeLine, string errorMessage)
		{
			StocktakeLine = stocktakeLine;
			ErrorMessage = errorMessage;
		}

		public WhsStocktakeLine StocktakeLine { get; }
		public string ErrorMessage { get; }
	}
}
