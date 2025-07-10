namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IGeneratePickLogger
	{
		void LogWarning(string message);
		void LogError(string message);
		void LogError(WhsPick pick, string message);
		void LogSaveConcurrencyException(WhsPick pick, string docketID);
	}
}