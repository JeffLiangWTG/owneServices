namespace Enterprise.Rating.Web.Model
{
	internal static class LocationExtension
	{
		public static bool IsValidOptionalLocation(this Location location) =>
			location != null
			&& location.Type != null
			&& location.Value != null
			&& location.IsEmptyType() == location.IsEmptyValue();

		public static bool IsEmptyLocation(this Location location) =>
			location.IsEmptyType()
			&& location.IsEmptyValue();

		public static bool IsEmptyType(this Location location) =>
			string.IsNullOrEmpty(location.Type);

		public static bool IsEmptyValue(this Location location) =>
			string.IsNullOrEmpty(location.Value);
	}
}
