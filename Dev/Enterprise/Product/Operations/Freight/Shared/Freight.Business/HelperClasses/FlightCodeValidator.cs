using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public static class FlightCodeValidator
	{
		/// <summary>
		/// Validation rule:
		///	   - The Flight No. must start with the following combinations:
		///		 AA, AN, or NA (where A is alpha, N is numeric) followed by 1-4 numeric characters.
		///		 and optional alpha character
		/// </summary>
		public static bool IsValid(string flightCode)
		{
			bool isValid = false;

			if (flightCode.Length >= 2 && flightCode.Length <= 7)
			{
				string pattern = (NoResString)"^(([A-Za-z]{2}|[A-Za-z][0-9]|[0-9][A-Za-z])[0-9]{1,4}[A-Za-z]?)$";
				isValid = Regex.IsMatch(flightCode, pattern);
			}

			return isValid;
		}
	}
}
