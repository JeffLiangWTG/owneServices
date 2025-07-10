using System;
using CargoWise.Types;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	static class ZDateTimeExtensions
	{
		/// <summary>
		/// Converts the ZDateTime to a DateTime with Unpsecified DateTimeKind. WARNING - an exception will be thrown
		/// if ToUnspecifiedDateTime() is invoked on an empty or invalid ZDateTime!".
		/// </summary>
		public static DateTime ToUnspecifiedDateTime(this ZDateTime dateTime)
		{
			return new ZDateTime(dateTime, DateTimeKind.Unspecified).ToDateTime();
		}
	}
}
