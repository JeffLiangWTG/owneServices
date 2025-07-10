using System;
using CargoWise.Types;
public static class ZStringExtensions
{
	public static ZString GetFirstNLines(this ZString inputText, int maxLines)
	{
		if (string.IsNullOrEmpty(inputText) || maxLines <= 0)
		{
			return string.Empty;
		}
		var newlineSeperator = inputText.Contains("\r\n") ? "\r\n" : "\n";
		var lines = inputText.Split(new[] { newlineSeperator });
		return ZString.Join(newlineSeperator, lines, 0, Math.Min(maxLines, lines.Length));
	}
}
