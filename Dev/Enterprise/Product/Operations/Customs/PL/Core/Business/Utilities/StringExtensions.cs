using System;

namespace Enterprise.Customs.PL.Business;

public static class StringExtensions
{
	public static string IfNullOrEmpty(this string firstString, Func<string> otherStringFallback)
		=> !string.IsNullOrEmpty(firstString)
			? firstString
			: otherStringFallback.Invoke();
}
