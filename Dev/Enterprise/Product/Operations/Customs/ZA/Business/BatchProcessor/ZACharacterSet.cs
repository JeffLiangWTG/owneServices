namespace Enterprise.Customs.ZA.Business.BatchProcessor
{
	public class ZACharacterSet : Edifact.UNOBCharacterSet
	{
		public ZACharacterSet() : base()
		{
			SubElementDelimiterChar = ':';
			ElementDelimiterChar = '+';
			EscapeCharacterChar = '?';
			SegmentDelimiterChar = '\'';

			validCharacters = ValidCharacters;
			ReplaceEscapedCharactersWithSpace = false;
		}
	}
}
