using System;

namespace Enterprise.Rating.Integration
{
	[Flags]
	public enum ChargeCodeFilter
	{
		AutorateNothing = 0x0,
		AutorateConsolLevelOnly = 0x1,
		AutorateNonConsolLevelOnly = 0x2,
		AutorateAll = AutorateConsolLevelOnly | AutorateNonConsolLevelOnly
	}
}