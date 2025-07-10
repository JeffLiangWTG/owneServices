using System;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	[Flags]
	public enum RefDbRepoMachineCapabilityRequirements
	{
		None,
		CanConnectToOdbc = 1 << 1
	}
}
