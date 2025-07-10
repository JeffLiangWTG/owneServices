using System;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Common
{
	public static class BooleanHelper
	{
		public static bool? ParseBool(string booleanAsString)
		{
			bool? value = null;
			if (!string.IsNullOrEmpty(booleanAsString))
			{
				if (booleanAsString == "0" || booleanAsString == "1")
				{
					value = booleanAsString != "0";
				}
				else
				{
					throw new FormatException($"Parse bool Error: {booleanAsString}");
				}
			}
			return value;
		}
	}
}
