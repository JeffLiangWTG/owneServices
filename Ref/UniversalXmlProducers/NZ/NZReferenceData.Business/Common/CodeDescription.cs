using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public class CodeDescription
	{
		public string Code { get; set; }
		public string Description { get; set; }

		public static Dictionary<string, CodeDescription> GetCodeDescriptionsFromJsonPath(string path, ILogger logger)
		{
			var dictionary = new Dictionary<string, CodeDescription>();

			if (string.IsNullOrWhiteSpace(path))
			{
				return dictionary;
			}

			if (!File.Exists(path))
			{
				logger.LogError($"Could not open lookup item in provided path: '{path}'");
				return dictionary;
			}

			try
			{
				string json = File.ReadAllText(path);
				var items = JsonSerializer.Deserialize<List<CodeDescription>>(json);

				if (items != null)
				{
					foreach (var item in items)
					{
						if (!string.IsNullOrEmpty(item.Code))
						{
							dictionary[item.Code] = item;
						}
					}
				}
			}
			catch (JsonException ex)
			{
				logger.LogError($"Invalid JSON in file '{path}': {ex.Message}");
			}
#pragma warning disable CA1031
			catch (Exception)
#pragma warning restore CA1031
			{
				// Ignore general exceptions and return an empty dictionary
			}

			return dictionary;
		}
	}

}
