namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IDPSSecurityProvider
	{
		bool ValidateDPS(WhsDocket docket);
	}
}
