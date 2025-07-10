#region Modified on 24/06/2009 Samuel Wang [samuel.wang@cargowise.com]
/*
 * Modified the highlighter format for FlexCel report
 */
#endregion
#region Copyright © 2008 Rickard Nilsson [rickard@rickardnilsson.net]
/*
 * This software is an altered version of the original and is provied 'as-is'.
 */
#endregion

using System.Text.RegularExpressions;

using CodeFormatter;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Business
{
	public class CodeFormatterForWeb
	{
		public CodeFormatterForWeb()
		{
		}

		public string HighlightCodeBlock(string text)
		{
			if (text.Contains((NoResString)"[/code]")) // Constant string used in code.
			{
				text = codeRegex.Replace(text, new MatchEvaluator(CodeEvaluator));
				text = Regex.Replace(text, @"\[code:.*?\]", "");
				text = Regex.Replace(text, @"\[/code\]", "");
			}

			return text;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		string CodeEvaluator(Match match)
		{
			if (!match.Success)
			{
				return match.Value;
			}

			HighlightOptions options = new HighlightOptions();

			options.Language = match.Groups["lang"].Value;
			options.DisplayLineNumbers = match.Groups["linenumbers"].Value == "on";
			options.AlternateLineNumbers = match.Groups["altlinenumbers"].Value == "on";

			return Highlight(options, match.Value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string used in code.")]
		string Highlight(HighlightOptions options, string text)
		{
			switch (options.Language)
			{
				case "csharp":
					CSharpFormat csf = new CSharpFormat();
					csf.LineNumbers = options.DisplayLineNumbers;
					csf.Alternate = options.AlternateLineNumbers;
					return csf.FormatCode(text);

				case "tsql":
					TsqlFormat tsqlf = new TsqlFormat();
					tsqlf.LineNumbers = options.DisplayLineNumbers;
					tsqlf.Alternate = options.AlternateLineNumbers;
					return tsqlf.FormatCode(text);
			}

			return string.Empty;
		}

		#region RegEx

		readonly Regex codeRegex = new Regex(@"\[code:(?<lang>.*?)(?:;ln=(?<linenumbers>(?:on|off)))?(?:;alt=(?<altlinenumbers>(?:on|off)))?(?:;(?<title>.*?))?\](?<code>.*?)\[/code\]",
			RegexOptions.Compiled
			| RegexOptions.CultureInvariant
			| RegexOptions.IgnoreCase
			| RegexOptions.Singleline);

		#endregion

		class HighlightOptions
		{
			string language;
			bool displayLineNumbers;
			bool alternateLineNumbers;

			public HighlightOptions()
			{
			}

			public bool DisplayLineNumbers
			{
				get { return displayLineNumbers; }
				set { displayLineNumbers = value; }
			}
			public string Language
			{
				get { return language; }
				set { language = value; }
			}
			public bool AlternateLineNumbers
			{
				get { return alternateLineNumbers; }
				set { alternateLineNumbers = value; }
			}
		}
	}
}
