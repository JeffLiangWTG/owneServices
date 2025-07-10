using System;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	[Flags]
	public enum PortPairTypes
	{
		None = 0,
		All = Domestic | Import | Export,
		Domestic = 0x1,
		Import = 0x2,
		Export = 0x4,
	}
}
