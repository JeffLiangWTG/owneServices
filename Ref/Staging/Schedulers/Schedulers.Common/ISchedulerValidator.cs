using Microsoft.AspNetCore.Http;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public interface ISchedulerValidator
	{
		bool IsNewScheduleJob(string jobName, string groupName);
		bool Validate(IFormCollectionService formCollectionService, out string errorMessage);
	}
}
