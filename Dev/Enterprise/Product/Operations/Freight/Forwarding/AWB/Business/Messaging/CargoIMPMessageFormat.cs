using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	public enum CIMPFieldFormats
	{
		Alpha,
		AlphaNumeric,
		Numeric,
		Text
	}

	public static class CargoIMPFormatExtensions
	{
		public static bool IsCargoIMPEmpty(this IZType value, CIMPFieldFormats format = CIMPFieldFormats.Text)
		{
			bool isEmpty = value.IsEmpty;
			if (!isEmpty)
			{
				string regexPattern = string.Empty;
				switch (format)
				{
					case CIMPFieldFormats.Alpha:
						regexPattern = (NoResString)@"[a-zA-Z]"; // Regex pattern
						break;
					case CIMPFieldFormats.AlphaNumeric:
						regexPattern = (NoResString)@"[a-zA-Z0-9]"; // Regex pattern
						break;
					case CIMPFieldFormats.Numeric:
						regexPattern = @"[0-9]"; // Regex pattern
						break;
					case CIMPFieldFormats.Text:
						regexPattern = (NoResString)@"[a-zA-Z0-9\-\.]"; // Regex pattern
						break;
				}

				isEmpty = !Regex.IsMatch(value.ToString(), regexPattern);
			}

			return isEmpty;
		}
	}
}
