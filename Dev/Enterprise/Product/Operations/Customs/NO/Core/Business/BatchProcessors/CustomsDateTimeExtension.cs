using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

public static class CustomsDateTimeExtension
{
	public const string DateFormat = "yyyyMMdd";

	public static ZString ToShortCustomsFormatDateString(this ZDateTime dateTime, int yearDigits = 2)
	{
		var year = new string('y', yearDigits);
		return dateTime.ToCustomsFormatString($"{year}MMdd");
	}

	public static ZString ToShortCustomsFormatDateString(this ZDate dateTime, int yearDigits = 2)
	{
		var year = new string('y', yearDigits);
		return dateTime.ToString($"{year}MMdd", CultureInfo.InvariantCulture);
	}

	public static ZString ToCustomsFormatTimeString(this ZDateTime dateTime)
	{
		return dateTime.ToCustomsFormatString("HHmm");
	}

	public static ZString ToCustomsFormatString(this ZDateTime dateTime, ZString format)
	{
		return dateTime.ToString(format, CultureInfo.InvariantCulture);
	}
}
