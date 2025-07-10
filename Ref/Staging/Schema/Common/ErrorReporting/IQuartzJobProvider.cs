namespace CargoWise.RefDbRepo.Staging.Common.ErrorReporting
{
	public interface IQuartzJobProvider
	{
		string GetQuartzJobName(string programExePath, string programArgs);
		int GetTriggeredTimesPerDay(string jobName);
	}
}
