namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsDocketCreationLogger
	{
		void LogSuccess(string message);
		void LogFailure(string message);
		void LogWarning(string message);
		void LogHyperLinkSuccess(WhsDocket docket, string message);
	}
}