using System;
using CargoWise.Types;

namespace Enterprise.Rating.Rateable
{
	// Helper for converting nullable .NET types and custom ZTypes.
	public static class NullableHelper
	{
		public static Guid? ToNullable(ZGuid pk) => pk.IsValid ? pk.ToGuid() : null;
		public static ZGuid ToZGuid(Guid? pk) => pk ?? ZGuid.Empty;

		public static DateTime? ToNullable(ZDateTime d) => d.IsValid ? d.ToDateTime() : null;
		public static DateTime? ToNullable(ZDateTime? d) => d.HasValue ? ToNullable(d.Value) : null;
	}
}
