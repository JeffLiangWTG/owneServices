using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgFreeWaitingTime
	{
		ZDateTime CNE { get; set; }
		ZDateTime CFS { get; set; }
		ZDateTime CNR { get; set; }
		ZDateTime CTO { get; set; }
		ZDateTime CYD { get; set; }
		ZDateTime Other { get; set; }
		ZGuid CNTType { get; set; }
		ZString DropMode { get; set; }
	}
}
