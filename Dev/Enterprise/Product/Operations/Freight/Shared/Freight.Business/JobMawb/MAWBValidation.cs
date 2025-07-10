using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public static class MAWBValidation
	{
		public static void CheckMawbCheckDigit(ZPropertyInfo info)
		{
			ZString value = (ZString)info.Value;

			if (value.Length != 8)
			{
				info.AddError(Res.GetString("bb2adc31-8acc-4f00-9664-29f1f29535f4", "Value should be 8 digits in length"));
			}
			else if (!ZInt.CanParse(value))
			{
				info.AddError(Res.GetString("fa86592c-6ff6-48a2-9d23-5731affa5634", "Please enter only numerals"));
			}
			else
			{
				int checkDigit;
				int calculatedCheckDigit = Math.DivRem(int.Parse(value), 10, out checkDigit) % 7;

				if (checkDigit != calculatedCheckDigit)
				{
					info.AddError(Res.GetString("52a835ae-d01b-439b-b906-3a92b381e800", "Incorrect Check Digit. Last digit should be '{0}'", calculatedCheckDigit));
				}
			}
		}
	}
}
