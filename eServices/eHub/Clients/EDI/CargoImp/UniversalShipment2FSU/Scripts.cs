using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.Clients.EDI.Transforms.CargoImp.UniversalShipment2FSU
{
	public class Scripts
	{
		#region QuantityDetails

		public string GetQuantityDetails()
		{
			return QuantityDetails;
		}

		public string SetAndGetQuantityDetails(string input)
		{
			QuantityDetails = input;
			return QuantityDetails;
		}

		public string QuantityDetails;

		#endregion

		#region FormatNumber

		public string FormatNumber(string input, int noOfDigits)
		{
			return FormatNumberRaw(input, noOfDigits);
		}

		public string FormatNumberRaw(string input, int noOfDigits)
		{
			string result = string.Empty;

			int indexOfDot = input.IndexOf('.');
			if (indexOfDot > 0)
			{
				string integerPart = input.Substring(0, indexOfDot);

				if (integerPart.Length >= noOfDigits) result = integerPart.Substring(0, Math.Min(noOfDigits, input.Length));
				else result = input.Substring(0, Math.Min(noOfDigits + 1, input.Length));
			}
			else
			{
				result = input.Substring(0, Math.Min(noOfDigits, input.Length));
			}

			return result;
		}

		#endregion
	}
}
