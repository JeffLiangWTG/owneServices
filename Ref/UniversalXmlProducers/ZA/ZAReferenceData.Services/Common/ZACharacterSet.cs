using System;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Common
{
	public sealed class ZACharacterSet : Enterprise.Edifact.UNOBCharacterSet
	{
		public static ZACharacterSet Instance => instance.Value;
		static readonly Lazy<ZACharacterSet> instance = new Lazy<ZACharacterSet>(() => new ZACharacterSet());

		ZACharacterSet() : base()
		{
			SubElementDelimiterChar = ':';
			ElementDelimiterChar = '+';
			EscapeCharacterChar = '?';
			SegmentDelimiterChar = '\'';
			ReplaceEscapedCharactersWithSpace = false;
		}
	}
}
