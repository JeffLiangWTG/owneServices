using System;
using CargoWise.Types;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Util
{
	static class UnixTimestampProvider
	{
		static readonly ZDateTime UnixEpoch = new ZDateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static string GetCurrentUnixTimestampMilliseconds()
		{
			return ((UInt64)(ZDateTime.UtcNow - UnixEpoch).TotalMilliseconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
		}

		public static string GetCurrentUnixTimestampMicroseconds()
		{
			return ((UInt64)((ZDateTime.UtcNow - UnixEpoch).Ticks / (TimeSpan.TicksPerMillisecond / 1000))).ToString(System.Globalization.CultureInfo.InvariantCulture);
		}
	}
}
