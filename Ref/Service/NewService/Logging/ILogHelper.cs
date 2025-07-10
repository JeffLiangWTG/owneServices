namespace CargoWise.RefDbRepo.NewService
{
	public interface ILogHelper
	{
		void LogInfo(string userId, string message, string requestUri = null);
	}
}
