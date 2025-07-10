using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbSecurity
	{
		ZBool GU_SecurityItemIsAllowed { get; set; }
		ZString GU_SecurityRight { get; set; }
		ZGuid GU_GG { get; set; }
		ZGuid GU_GS { get; set; }
		ZGuid GU_GB { get; set; }
		ZGuid GU_GE { get; set; }
		ZGuid GU_GC { get; set; }
	}
}
