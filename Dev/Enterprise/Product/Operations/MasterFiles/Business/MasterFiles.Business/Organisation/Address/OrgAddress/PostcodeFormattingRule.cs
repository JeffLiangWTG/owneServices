using System.Text.RegularExpressions;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[Immutable]
	public class PostcodeFormattingRule
	{
		readonly Regex regex;

		public PostcodeFormattingRule(string iso, string regexString, string format)
		{
			Iso = iso;
			Format = format;
			regex = new Regex(regexString, RegexOptions.Compiled);
		}

		public string Iso { get; }

		public string Format { get; }

		public bool IsFormatted(string value)
		{
			return regex.IsMatch(value);
		}
	}
}
