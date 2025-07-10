using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class AdditionalTranslationsProvider
	{
		public AdditionalTranslationsProvider(IAdditionalTranslationsReader reader, IAdditionalTranslationSupporter additionalTranslationSupporter)
		{
			this.reader = Argument.NotNull(reader, nameof(reader));
			dataParserKey = Argument.NotNull(additionalTranslationSupporter, nameof(additionalTranslationSupporter)).DataParserKey;
			lookup = new(InitializeLookup);
		}

		public IReadOnlyCollection<Translation> GetTranslations(string code)
		{
			if (lookup.Value.TryGetValue(code, out var languages))
			{
				return languages.AsReadOnly();
			}

			return [];
		}

		Dictionary<string, List<Translation>> InitializeLookup()
		{
			var processedLanguages = new HashSet<string>();
			var processedCodes = new HashSet<string>();
			var lookup = new Dictionary<string, List<Translation>>();
			var translations = reader.GetAllTranslations();

			foreach (var translation in translations)
			{
				var language = translation.Key;
				CheckUniqueLanguage(language, processedLanguages);
				var dataParser = GetDataParser(translation, language);

				foreach (var additionalTranslation in dataParser?.AdditionalTranslations ?? [])
				{
					var code = additionalTranslation.Code;
					var description = additionalTranslation.Description;
					CheckUniqueCode(code, processedCodes);
					AddTranslation(code, language, description, lookup);
				}

				processedCodes.Clear();
			}

			if (lookup.Count == 0)
			{
				throw new InvalidOperationException($"No translations for data parser {dataParserKey}");
			}

			return lookup;
		}

		DataParser GetDataParser(Language translation, string language)
		{
			var dataParsers = translation.DataParsers.Where(x => x.Key == dataParserKey).ToArray();

			if (dataParsers.Length > 1)
			{
				throw new InvalidOperationException($"Duplicated data parser {dataParserKey} for language {language}");
			}

			return dataParsers.Length == 1 ? dataParsers[0] : null;
		}

		void CheckUniqueLanguage(string language, HashSet<string> processedLanguages)
		{
			if (!processedLanguages.Add(language))
			{
				throw new InvalidOperationException($"Duplicated language {language} for data parser {dataParserKey}");
			}
		}

		void CheckUniqueCode(string code, HashSet<string> processedCodes)
		{
			if (!processedCodes.Add(code))
			{
				throw new InvalidOperationException($"Duplicated code {code} for data parser {dataParserKey}");
			}
		}

		static void AddTranslation(string code, string language, string description, Dictionary<string, List<Translation>> lookup)
		{
			if (!lookup.TryGetValue(code, out var languages))
			{
				languages = [];
				lookup[code] = languages;
			}

			languages.Add(new Translation(language, description));
		}

		readonly IAdditionalTranslationsReader reader;
		readonly Lazy<Dictionary<string, List<Translation>>> lookup;
		readonly string dataParserKey;
	}

	public record Translation(string Language, string Description);
}
