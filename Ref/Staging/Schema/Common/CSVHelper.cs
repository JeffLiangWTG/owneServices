using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.Common
{
	public static class CSVHelper
	{
		public static List<string[]> GetCSVLinesInArrays(bool ignoreFirstRow, string csvText)
		{
			List<string[]> result = new List<string[]>();
			string[] allLines = csvText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
			int i = 0;
			foreach (var line in allLines)
			{
				i++;
				if (ignoreFirstRow == true && i == 1)
				{
					continue;
				}
				if (string.IsNullOrEmpty(line))
				{
					continue;
				}
				var elements = line.Split(',');
				result.Add(elements);
			}
			return result;
		}

		public static List<string[]> GetCSVLinesInArrays(bool ignoreFirstRow, Stream stream, bool hasFieldsEnclosedInQuotes = true)
		{
			Argument.NotNull(stream, nameof(stream));
			var result = new List<string[]>();
			using (var reader = new Microsoft.VisualBasic.FileIO.TextFieldParser(stream))
			{
				reader.HasFieldsEnclosedInQuotes = hasFieldsEnclosedInQuotes;
				reader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
				reader.SetDelimiters(@",");
				while (!reader.EndOfData)
				{
					result.Add(reader.ReadFields());
				}
			}
			if (ignoreFirstRow && result.Count != 0)
			{
				result.RemoveAt(0);
			}
			return result;
		}
	}
}
