using CargoWise.MobileServices.Common.Messages;
using CargoWise.Types;

namespace Enterprise.Telematics.Business
{
	public static class GlbDeviceKindCodes
	{
		public static readonly ZString Android = "AND";
		public static readonly ZString AppleMobile = "IOS";
		public static readonly ZString WindowsMobileLegacy = "WIN";
		public static readonly ZString WTGEmbedded = "EMB";
		public static readonly ZString Unknown = "UNK";

		public static ZString Get(DeviceKind kind)
		{
			switch (kind)
			{
				case DeviceKind.Android:
					return Android;

				case DeviceKind.AppleMobile:
					return AppleMobile;

				case DeviceKind.WindowsMobileLegacy:
					return WindowsMobileLegacy;

				case DeviceKind.WiseTechVehicularPlatform:
					return WTGEmbedded;

				case DeviceKind.InvalidKind:
				default:
					return Unknown;
			}
		}
	}
}
