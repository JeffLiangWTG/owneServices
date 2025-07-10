namespace Enterprise.MasterFiles.Business.Accounting.ProcessLogging
{
	/// <summary>
	/// Responsible for saving error info as logs to a storage (e.g., a DB table, a file, forward to another logger)
	/// </summary>
	public interface IAccProcessLogger
	{
		void Log(ISupportAccProcessLogging parent, ILogableError error);
	}
}
