using System;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public static class FactHelpers
	{
		public static Guid? ConvertToNullableGuid(this ZGuid pk) => pk.IsValid ? pk.ToGuid() : null;

		public static DateTime? ConvertToNullableDateTime(this ZDate date) => date.IsValid ? date.ToDateTime() : null;

		public static DateTime? ConvertToNullableDateTime(this ZDateTime dateTime) => dateTime.IsValid ? dateTime.ToDateTime() : null;

		public static DateTime? ConvertToNullableDateTime(this ZDateTimeOffset dateTime) => dateTime.IsValid ? dateTime.ToDateTime() : null;
	}
}
