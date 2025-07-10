using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IQueuedLogDelayer
	{
		ZDateTime EventTime { set; }

		ZBool IsDelayFired { get; set; }
	}
}
