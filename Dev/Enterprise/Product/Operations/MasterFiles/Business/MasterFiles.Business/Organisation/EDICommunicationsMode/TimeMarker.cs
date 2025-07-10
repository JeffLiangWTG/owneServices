using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.MessageDelivery.Utils
{
	public static class TimeMarker
	{
		static ZDateTime last = ZDateTime.Now;

		public static void Mark()
		{
			last = ZDateTime.Now;
		}

		public static ZDateTime LastMarkedTime
		{
			get
			{
				return last;
			}
		}

		public static bool IsNow(this ZDateTime time)
		{
			return time == ZDateTime.Now
#if DEBUG
 && !TestDateAttribute.IsActive
#endif
;
		}
	}

	public static class StringExtension
	{
		public static string Substitute(this ZString nameSeed, Dictionary<string, string> replacementDict)
		{
			foreach (var replacement in replacementDict)
			{
				nameSeed = nameSeed.ReplaceIgnoringCase(replacement.Key, replacement.Value);
			}
			return nameSeed;
		}
	}
}
