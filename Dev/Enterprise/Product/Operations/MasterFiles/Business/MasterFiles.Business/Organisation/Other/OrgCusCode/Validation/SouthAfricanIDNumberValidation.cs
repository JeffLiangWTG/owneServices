using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Summary description for SouthAfricanIDNumberValidation.
	/// </summary>
	public class SouthAfricanIDNumberValidation
	{
		public void Validate(ZPropertyInfo iDNumberInfo)
		{
			ZString iDNumber = ((ZString)iDNumberInfo.Value);
			if (iDNumber.Length == 13 && iDNumber.IsNumbersOnlyOrEmpty)
			{
				int calculatedCheckDigit = CalculateCheckDigit(iDNumber);
				int checkDigit = Convert.ToInt32(iDNumber.Substring(12, 1), CultureInfo.InvariantCulture);
				if (calculatedCheckDigit != checkDigit)
				{
					iDNumberInfo.AddError(Res.GetString("AE527403-E925-4379-969A-F9B27D033C55", "The ID Number does not have a valid check-digit. The check-digit should be {0:G}.", calculatedCheckDigit));
				}
			}
			else
			{
				iDNumberInfo.AddError(Res.GetString("2165F5EF-B3C6-48ED-A084-A034C740EA42", "The ID Number must be 12 digits and a 13th check-digit."));
			}
		}

		#region Implementation

		protected int CalculateCheckDigit(ZString iDNumber)
		{
			int checkDigit = 0;
			int oddIndexedDigits = 0;
			string evenIndexedDigits = "";
			int doubledEvenDigits = 0;

			for (int pos = 0; pos < 12; pos++)
			{
				if (pos % 2 == 1)
				{
					evenIndexedDigits += iDNumber.Substring(pos, 1);
				}
				else
				{
					oddIndexedDigits += Convert.ToInt32(iDNumber.Substring(pos, 1), CultureInfo.InvariantCulture);
				}
			}

			string tempC = String.Format(CultureInfo.InvariantCulture, "{0}", 2 * Convert.ToInt32(evenIndexedDigits, CultureInfo.InvariantCulture));

			for (int pos = 0; pos < tempC.Length; pos++)
			{
				doubledEvenDigits += Convert.ToInt32(tempC.Substring(pos, 1), CultureInfo.InvariantCulture);
			}

			checkDigit = (oddIndexedDigits + doubledEvenDigits) % 10;
			checkDigit = checkDigit == 0 ? 0 : 10 - checkDigit;

			return checkDigit;
		}
		#endregion
	}
}
