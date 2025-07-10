using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.USReferenceData.Services;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class A99TariffsParser
	{
		public A99TariffsParser(string outputPath)
		{
			var publishTime = ApplicationConfig.Instance.A99TariffPublicationTime;
			this.publicationTime = publishTime;
			this.outputFileName = Path.Combine(outputPath, publishTime.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture) + "_USA99Tariff.xml");
		}

		readonly string outputFileName;
		readonly DateTime publicationTime;

		public string ParseToXMLFile(string filePath)
		{
			var sBuilder = new StringBuilder();
			var jArray = GetJsonArray(filePath, sBuilder);
			if (jArray != null)
			{
				var refCusTarriffs = jArray.ToList();
				FileHelper.ExportToXMLFile("US A99 Tariff", outputFileName, new A99XmlWriterConfiguration(), publicationTime, refCusTarriffs, UpdateType.Partial);
				sBuilder.AppendLine(CultureInfo.InvariantCulture, $"{filePath} processed, xml file generated with {refCusTarriffs.Count} Tariff records in {outputFileName}");
			}
			else
			{
				sBuilder.AppendLine("No tariff data defined in json file.");
			}
			return sBuilder.ToString();
		}
		static IEnumerable<RefCusTariff> GetJsonArray(string filename, StringBuilder stringBuilder)
		{
			var refCusTariffSchemas = LoadRefCusTariffJsonData(filename, out var validationErrors);

			if (validationErrors.Count > 0)
			{
				foreach (var error in validationErrors)
				{
					stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Validation Error: {error}");
				}
				Console.Error.WriteLine($"{filename} has invalid json format.");
				yield break;
			}

			if (refCusTariffSchemas == null || refCusTariffSchemas.Count == 0)
			{
				stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"No tariff data defined in json file.");
				yield break;
			}

			foreach (var tariffSchema in refCusTariffSchemas)
			{
				if (string.IsNullOrWhiteSpace(tariffSchema.TariffCode))
				{
					stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Schema Error in {filename}: Required properties are missing from object: TariffCode");
					continue;
				}

				if (string.IsNullOrWhiteSpace(tariffSchema.Country))
				{
					stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Schema Error in {filename}: Required properties are missing from object: Country");
					continue;
				}

				if (string.IsNullOrWhiteSpace(tariffSchema.TariffType))
				{
					stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Schema Error in {filename}: Required properties are missing from object: TariffType");
					continue;
				}


				if (tariffSchema.StartDate == DateTime.MinValue)
				{
					tariffSchema.EndDate = tariffSchema.EndDate.FixedEndDate();
					stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Invalid or missing start date for tariff: {tariffSchema.TariffCode}");
					continue;
				}

				if (tariffSchema.EndDate == DateTime.MinValue)
				{
					stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Invalid or missing end date for tariff: {tariffSchema.TariffCode}");
					continue;
				}

				tariffSchema.StartDate = tariffSchema.StartDate.FixedStartDate();
				tariffSchema.EndDate = tariffSchema.EndDate.FixedEndDate();
				yield return new RefCusTariff
				{
					ZZ1_TariffCode = tariffSchema.TariffCode.Replace(".", ""),
					ZZ1_ZZI_NKTariffType = tariffSchema.TariffType,
					ZZ1_StartDate = tariffSchema.StartDate,
					ZZ1_Description = tariffSchema.Description,
					ZZ1_ZZI_ZZZ_NKDataGrouping = tariffSchema.Country,
					ZZ1_EndDate = tariffSchema.EndDate,
					ZZ1_ZZZ_NKDataGrouping = tariffSchema.Country,
					RefCusTariffAttributes = tariffSchema.GetTariffAttributes(),
					RefCusRates = tariffSchema.GetCusRates(),
					RefCusTariffRelationships = tariffSchema.GetCusTariffRelationships(),
					RefCusConditions = tariffSchema.GetTariffConditions()
				};
			}
		}
		static List<RefCusTariffSchema> LoadRefCusTariffJsonData(string filePath, out List<string> validationErrors)
		{
			var validationErrorsDeserialize = new List<string>();
			var jsonData = File.ReadAllText(filePath);

			var settings = new JsonSerializerSettings
			{
				Error = (sender, args) =>
				{
					validationErrorsDeserialize.Add(args.ErrorContext.Error.Message);
					args.ErrorContext.Handled = true;
				}
			};

			var result = new List<RefCusTariffSchema>();
			try
			{
				result = JsonConvert.DeserializeObject<List<RefCusTariffSchema>>(jsonData, settings);
			}
			catch (JsonException ex)
			{
				validationErrorsDeserialize.Add($"Error deserializing JSON: {ex.Message}");
			}
			validationErrors = validationErrorsDeserialize;
			return result;
		}
	}
}
