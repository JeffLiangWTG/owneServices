using System;
using System.Text.RegularExpressions;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.Business.Testing
{
	static class AddressValidationServiceExtensionsTest
	{
		public static CleanseAction ExtractCleanseAction(this Uri url)
		{
			var regex = new Regex("action=(?<action>[^&]+)", RegexOptions.IgnoreCase);
			var match = regex.Match(url.AbsoluteUri);

			// NOTE: We don't have 'Unknown' value for this enum, will pick less likely use value to indicate 'Unknown' value.
			var cleanseAction = CleanseAction.ReverseGeocode;

			if (match.Success)
			{
				cleanseAction = (CleanseAction)Enum.Parse(typeof(CleanseAction), match.Groups["action"].Value, true);
			}

			return cleanseAction;
		}
	}
}
