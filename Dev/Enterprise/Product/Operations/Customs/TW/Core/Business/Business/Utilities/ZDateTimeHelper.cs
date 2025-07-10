using System.Globalization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public static class ZDateTimeHelper
	{
		public static ZString ToTaiWanDateString(this ZDateTime zDateTime)
		{
			var result = ZString.Empty;
			if (zDateTime.IsValid)
			{
				result = ZString.Format((NoResString)"{0}年{1}月{2}日", new TaiwanCalendar().GetYear(zDateTime.ToDateTime()), zDateTime.Month.ToString("D2", CultureInfo.InvariantCulture), zDateTime.Day.ToString("D2", CultureInfo.InvariantCulture));
			}
			return result;
		}

		public static ZString ToTaiWanShortYear(this ZDateTime zDateTime)
		{
			var result = ZString.Empty;
			if (zDateTime.IsValid)
			{
				result = ToTaiWanYear(zDateTime).Right(2);
			}
			return result;
		}

		public static ZString ToTaiWanYear(this ZDateTime zDateTime)
		{
			var result = ZString.Empty;
			if (zDateTime.IsValid)
			{
				result = new TaiwanCalendar().GetYear(zDateTime.ToDateTime()).ToString(CultureInfo.InvariantCulture);
			}
			return result;
		}

		public static ZString ToTaiWanShortDate(this ZDateTime zDateTime)
		{
			var result = ZString.Empty;
			if (zDateTime.IsValid)
			{
				result = ZString.Format("{0}{1}", ToTaiWanShortYear(zDateTime), zDateTime.ToString("MMdd", CultureInfo.InvariantCulture));
			}
			return result;
		}
	}
}
