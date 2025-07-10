using System;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class TriggerConditionRegexProvider
	{
		internal static Regex GetEventReferenceWithWildcardsRegex(IBaseTrigger trigger) => GetEventReferenceWithWildcardsRegex(trigger.TriggerConditionValue);

		public static Regex GetEventReferenceWithWildcardsRegex(ZString triggerConditionValue)
		{
			var withNext = Regex.Escape(triggerConditionValue);
			withNext = WildcardMatcher.Replace(withNext, MatchEval);
			withNext = "^" + withNext + "$";
			return new Regex(withNext, RegexOptions.IgnoreCase);
		}

		static string MatchEval(Match match)
		{
			switch (match.Value)
			{
				case @"\*":
					return ".*";
				case @"\\\*":
					return @"\*";
				case @"\?":
					return @".";
				case @"\\\?":
					return @"\?";
				default:
					return match.Value;
			}
		}

		internal const string OneCharWildcard = "?";
		internal const string MultiCharWildcard = "*";
		const string Escape = @"\";

		static readonly Regex WildcardMatcher = new Regex(@"[\\]?[\\]?\\(\?|\*)");

		internal const string EscapedOneCharWildcard = Escape + OneCharWildcard;
		internal const string EscapedMultiCharWildcard = Escape + MultiCharWildcard;

		internal static Regex DateRegex
		{
			get { return dateRegex ?? (dateRegex = new Regex(@"\|TYP=[a-zA-Z0-9]{3}(\|OLD=[0-9a-zA-Z\-\./]*)?(\|NEW=[0-9a-zA-Z\-\./]*)?", RegexOptions.Compiled)); }
		}

		[ThreadStatic]
		static Regex dateRegex;

		internal static Regex OldDateRegex
		{
			get { return oldDateRegex ?? (oldDateRegex = new Regex(@"\b((From:\s[0-9a-zA-Z\-\./]+\s){0,1}To:\s[0-9a-zA-Z\-\./]+|From:\s[0-9a-zA-Z\-\./]+\sTo:)", RegexOptions.Compiled)); }
		}

		[ThreadStatic]
		static Regex oldDateRegex;
	}
}
