namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public interface ILogger
	{
		void Log(LogType logType, string message, object value = null);
	}

	public enum LogType
	{
		Info,
		Warning,
		ReviewRequired
	}
}
