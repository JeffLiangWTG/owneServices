using System;

namespace Enterprise.Freight.Agency.Business
{
	[Flags]
	public enum PortDirections
	{
		None = 0, // 0x000
		Load = 1, // 0x001
		Transit = 2, // 0x010
		LoadOrTransit = 3, // 0x011
		Discharge = 4, // 0x100
		DischargeOrTransit = 6, // 0x110
		All = Load | Transit | Discharge // 0x111
	}
}
