using System.Text.RegularExpressions;

namespace Enterprise.Packing.Business
{
	public static class GetPackageInfoHelper
	{
		public static string GetCleanSingleLineText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}
			return Regex.Replace(text, " ?[\\t\\r\\n]+ ?", " ").Trim();
		}
	}
}
