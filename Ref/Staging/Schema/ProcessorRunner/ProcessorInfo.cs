using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.ProcessorRunner
{
	public class ProcessorInfo
	{
		public ProcessorInfo(string jobName, string jobGroup, string schedName)
		{
			Argument.NotNullOrEmpty(jobName, nameof(jobName));
			Argument.NotNullOrEmpty(jobGroup, nameof(jobGroup));
			Argument.NotNullOrEmpty(schedName, nameof(schedName));
			JobName = jobName;
			JobGroup = jobGroup;
			SchedName = schedName;
		}

		public string JobName { get; private set; }
		public string JobGroup { get; private set; }
		public string SchedName { get; private set; }
	}
}
