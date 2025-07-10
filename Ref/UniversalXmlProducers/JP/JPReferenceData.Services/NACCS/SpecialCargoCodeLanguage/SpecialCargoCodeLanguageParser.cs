using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	sealed class SpecialCargoCodeLanguageParser : RefCusCodeListLanguageParser
	{
		readonly Regex codeRegex = new Regex("^[A-Z]{3}$");

		public override string ZXA_ZX6_NKLanguage => "EN";

		protected override RefCusCodeListLanguageParserConfig[] GetConfigsCore() => new[] { new SpecialCargoCodeLanguageParserConfig(this) };

		#region CodeWithEngDescriptionDictionary

		public IReadOnlyDictionary<string, string> CodeWithEngDescriptionDictionary
		{
			get
			{
				if (languageCodeDescriptionDictionary == null)
				{
					GetAndParserLanguageFile();
				}

				return languageCodeDescriptionDictionary;
			}
		}
		Dictionary<string, string> languageCodeDescriptionDictionary;

		void GetAndParserLanguageFile()
		{
			var dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var filePath = Path.Combine(dirPath, AppConfig.NACCS.CodeLists.SpecialCargoCodeEngDescFileName);

			if (CsvReaderHelper.TryRead(filePath, out var records))
			{
				ParseLanguageRecords(records);
			}
		}

		void ParseLanguageRecords(IList<string> records)
		{
			languageCodeDescriptionDictionary = new Dictionary<string, string>();

			for (var i = 0; i < records.Count; i++)
			{
				var record = records[i].Trim();
				if (string.IsNullOrEmpty(record))
				{
					continue;
				}

				var columns = record.Split(',');
				if (columns.Length < 2)
				{
					throw new UnhandledApplicationException($"The line {i + 1} of {AppConfig.NACCS.CodeLists.SpecialCargoCodeEngDescFileName} doesn't have more than 2 columns which splits by ','.");
				}

				var code = columns.Last().Trim();
				if (!codeRegex.IsMatch(code))
				{
					throw new UnhandledApplicationException($"The line {i + 1} of {AppConfig.NACCS.CodeLists.SpecialCargoCodeEngDescFileName} doesn't have a valid code which matches the regex rule - {codeRegex}.");
				}

				var description = record.Substring(0, record.Length - 4).Trim('"');
				languageCodeDescriptionDictionary.Add(code, description);
			}
		}

		#endregion
	}
}
