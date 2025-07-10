using System;

namespace Enterprise.MasterFiles.Integration
{
	[Flags]
	public enum ViewLocationType
	{
		None = 0,
		UNLOCO = 1,
		Country = 2,
		State = 4,
		City = 8,
		InternationalZone = 16,
		TransportZone = 32,

		All = UNLOCO | Country | State | City | InternationalZone | TransportZone
	}
}
