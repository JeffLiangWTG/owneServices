using System;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public static class ZDateExtensions
{
	public static ZDateTimeOffset ToZDateTimeOffsetWithCurrentTimeIfDateIsToday(this ZDate date)
	{
		var kind = date.ToZDateTime().Kind;
		var now = kind == DateTimeKind.Utc
			? ZDateTime.UtcNow
			: ZDateTime.Now;
		return date == now.Date
			? now.ToOffset()
			: date.ToZDateTime().ToOffset();
	}
}
