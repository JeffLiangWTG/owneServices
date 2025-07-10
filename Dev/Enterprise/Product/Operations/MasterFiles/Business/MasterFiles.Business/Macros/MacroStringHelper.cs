using System;
using System.Collections.Generic;
using System.Globalization;

namespace Enterprise.MasterFiles.Business
{
	static class MacroStringHelper
	{
		internal static string WrapWithQuotes(string s)
		{
			return s.StartsWith("\"", StringComparison.Ordinal)
				? s
				: string.Format(CultureInfo.InvariantCulture, "\"{0}\"", s);
		}

		internal static (List<KeyValuePair<string, string>> pairs, List<string> nonPair) GetParameters(string reference)
		{
			var pairs = new List<KeyValuePair<string, string>>();
			var singles = new List<string>();
			var splitParameters = reference.Split(',');

			foreach (var parameter in splitParameters)
			{
				var splitParameter = parameter.Split('=');
				if (splitParameter.Length == 2 && !string.IsNullOrEmpty(splitParameter[0]))
				{
					pairs.Add(new KeyValuePair<string, string>(splitParameter[0], splitParameter[1]));
				}
				else if (!string.IsNullOrEmpty(parameter))
				{
					singles.Add(parameter);
				}
			}
			return (pairs, singles);
		}
	}
}
