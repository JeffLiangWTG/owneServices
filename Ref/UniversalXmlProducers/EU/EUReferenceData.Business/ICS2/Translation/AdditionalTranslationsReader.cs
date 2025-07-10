using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public sealed class AdditionalTranslationsReader : IAdditionalTranslationsReader
	{
		public AdditionalTranslationsReader(string path)
		{
			this.path = Argument.NotNullOrEmpty(path, nameof(path));
		}

		public IReadOnlyCollection<Language> GetAllTranslations()
		{
			var result = new List<Language>();
			var jsonDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

			if (!Directory.Exists(jsonDirectoryPath))
			{
				return result.AsReadOnly();
			}


			foreach (var file in Directory.GetFiles(jsonDirectoryPath, "*.json", SearchOption.TopDirectoryOnly))
			{
				var language = Path.GetFileNameWithoutExtension(file).ToUpper(CultureInfo.InvariantCulture);

				try
				{
					var json = File.ReadAllText(file);
					var dataParsers = JsonSerializer.Deserialize<DataParser[]>(json, options);
					result.Add(new Language(language, dataParsers));
				}
				catch
				{
					throw new InvalidOperationException($"Failed to read {language} JSON");
				}
			}

			return result.AsReadOnly();
		}

		static readonly JsonSerializerOptions options = new()
		{
			ReadCommentHandling = JsonCommentHandling.Skip
		};

		readonly string path;
	}
}
