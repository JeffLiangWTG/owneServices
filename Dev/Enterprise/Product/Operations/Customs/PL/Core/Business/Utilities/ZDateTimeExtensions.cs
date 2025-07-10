using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

static class ZDateTimeExtensions
{
	public static ZDateTime TrimSeconds(this ZDateTime dateTime) => new (dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, 0);
}
