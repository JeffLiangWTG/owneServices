namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public interface ILoggerConfiguration
	{
		TransferrerLogLevel LogLevel { get; set; }
		string LogDir { get; set; }
		string StructuredLogDir { get; set; }
		int? LogMaxSize { get; set; }
		int? LogMaxCount { get; set; }
		TransferrerLogFormat LogFormat { get; set; }
	}
}
