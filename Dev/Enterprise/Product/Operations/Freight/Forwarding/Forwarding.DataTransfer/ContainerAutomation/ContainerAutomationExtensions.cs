using System.Globalization;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public static class ContainerAutomationExtensions
	{
		public static string Capitalize(this string s)
		{
			return s.Length == 0 ? string.Empty : char.ToUpper(s[0], CultureInfo.InvariantCulture) + s.Substring(1);
		}

		public static string Serialize(this bool b) => b.ToString().Capitalize();
	}
}
