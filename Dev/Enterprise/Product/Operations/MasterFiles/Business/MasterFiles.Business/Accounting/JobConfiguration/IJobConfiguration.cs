using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IJobConfiguration
	{
		ZString JobType { get; }
		ZString ServiceDirection { get; }
		ZString TransportMode { get; }
		bool IncludeOptionsForAllJobTypes { get; }
	}
}
