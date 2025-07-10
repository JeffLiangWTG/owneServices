using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Messaging.Business
{
	public static class DateTimeParser
	{
		/// <param name="actionTime">can be HHMM or HHMMSS where HH can be 9 or 13</param>
		public static ZDateTime GetDateTimeFromZDateAndStringTime(ZDate actionDate, ZString actionTime)
		{
			ZDateTime result = actionDate;

			const int LengthOfMinutes = 2;

			int lengthOfHour = actionTime.Length == 4 || actionTime.Length == 6 ? 2 : 1;
			bool hasSeconds = actionTime.Length >= 5;

			ZInt hH = ZInt.ParseSafe(actionTime.Left(lengthOfHour), 0);
			ZInt mm = ZInt.ParseSafe(actionTime.SubstringSafe(lengthOfHour, LengthOfMinutes), 0);
			ZInt ss = hasSeconds ? ZInt.ParseSafe(actionTime.SubstringSafe(lengthOfHour + LengthOfMinutes), 0) : ZInt.Zero;

			return result.IsValid ? result.AddHours(hH).AddMinutes(mm).AddSeconds(ss) : ZDateTime.Empty;
		}

		public static ZDateTimeOffset GetFromJobBranchCurrentTime(GlbBranch branch)
		{
			var result = ZDateTimeOffset.Now;
			if (branch != null && ((IGlbBranch)branch).HomeTimeZone is ITimeZone homeTimeZone)
			{
				result = homeTimeZone.ToLocalTime(ZDateTimeOffset.UtcNow.ToDateTime());
			}
			return result;
		}
	}
}
