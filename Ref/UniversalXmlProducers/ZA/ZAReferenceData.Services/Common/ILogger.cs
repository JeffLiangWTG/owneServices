namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Common
{
	public interface ILogger
	{
		void LogInfo(string message);
		void LogError(string message);
	}
}
