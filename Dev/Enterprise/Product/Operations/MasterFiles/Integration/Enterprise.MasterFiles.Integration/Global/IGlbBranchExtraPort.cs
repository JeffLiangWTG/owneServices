using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGlbBranchExtraPort
	{
		ZGuid PK { get; }

		ZBool GY_IsValid { get; set; }

		ZString GY_RL_NKAdditionalBranchRelatedPort { get; set; }

		ZGuid GY_GB { get; set; }
	}
}
