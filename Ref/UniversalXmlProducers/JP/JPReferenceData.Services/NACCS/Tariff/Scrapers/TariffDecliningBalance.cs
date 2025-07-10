using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	sealed class TariffDecliningBalance : IOtherScraper
	{
		public TariffDecliningBalance(IWebSourceProvider sourceProvider, string url)
		{
			this.url = url;
			this.sourceProvider = sourceProvider;

			Tariffs = new List<SimpleTariff>();
		}

		readonly string url;
		readonly IWebSourceProvider sourceProvider;

		const string CompositeKey = "98";
		const string Separator = "*";

		DynamicCsvRecord NomenclatureRecord { get; set; }

		List<SimpleTariff> Tariffs { get; }

		void IOtherScraper.Load()
		{
			var filePath = Path.GetTempFileName();

			if (FileDownloader.TryDownload(sourceProvider, url, filePath).Result && CsvReaderHelper.TryRead(filePath, out var lines))
			{
				string[] previousValues = null;

				foreach (var line in lines)
				{
					var values = JapaneseLocalHelper.ConvertFullWidthToHalfWidth(line).Split(',');

					var firstValue = values.FirstOrDefault().Trim();
					var lastValue = values.LastOrDefault().Replace(" ", string.Empty);

					if (previousValues == null)
					{
						AddNomenclature(firstValue);
						previousValues = new string[3];
					}
					else
					{
						if (decimal.TryParse(lastValue, out _))
						{
							LoadCore(lastValue, previousValues, values);
						}
					}

					for (var i = 0; i < 3; i++)
					{
						var value = values[i];
						var previousValue = previousValues[i];

						previousValues[i] = string.IsNullOrWhiteSpace(value) ? previousValue : value;
					}
				}
			}
		}

		void AddNomenclature(string description)
		{
			var properties = new string[Tariff.Header.PropertiesCount];
			properties[0] = "Nomenclature";
			properties[2] = CompositeKey;
			properties[3] = "Item code related to the declaration of Article 14, Item 18 of the Declining Balance Law (items of 10,000 yen or less)";
			properties[4] = description;

			NomenclatureRecord = new DynamicCsvRecord(properties);
		}

		void LoadCore(string tariffCode, string[] previousValues, string[] currentValues)
		{
			var existingTariff = Tariffs.FirstOrDefault(c => c.Code.Equals(tariffCode, StringComparison.OrdinalIgnoreCase));
			if (existingTariff == null)
			{
				existingTariff = new SimpleTariff(tariffCode);
				Tariffs.Add(existingTariff);
			}

			var customsOffice = BuildCustomsOffice(previousValues, currentValues);
			existingTariff.CustomsOffices.Add(customsOffice);
		}

		static CustomsOffice BuildCustomsOffice(string[] previousValues, string[] currentValues)
		{
			var descriptions = new List<string>();

			for (var i = 0; i < 3; i++)
			{
				var value = currentValues[i];
				var description = string.IsNullOrWhiteSpace(value) ? previousValues[i] : value;
				descriptions.Add(description);
			}

			return new CustomsOffice
			{
				CustomsName = descriptions[0],
				OfficialSignature = descriptions[1],
				Department = descriptions[2]
			};
		}

		void IOtherScraper.Pack(List<DynamicCsvRecord> records)
		{
			records.Add(NomenclatureRecord);

			var propertiesCount = Tariff.Header.PropertiesCount;

			foreach (var tariff in Tariffs.OrderBy(c => c.Code))
			{
				var properties = new string[propertiesCount];
				properties[0] = "Tariff";
				properties[1] = tariff.Code;
				properties[2] = CompositeKey;
				properties[4] = tariff.GetFullDescription();

				records.Add(new DynamicCsvRecord(properties));
			}
		}

		void IOtherScraper.Release()
		{
			Tariffs.Clear();
		}

		sealed class SimpleTariff
		{
			public SimpleTariff(string code)
			{
				Code = code;
				CustomsOffices = new List<CustomsOffice>();
			}

			public string Code { get; }

			public List<CustomsOffice> CustomsOffices { get; }

			public string GetFullDescription()
			{
				var descriptionBuilder = new StringBuilder();

				foreach (var group in CustomsOffices.GroupBy(c => c.CustomsName))
				{
					if (descriptionBuilder.Length > 0)
					{
						descriptionBuilder.Append('|');
					}

					descriptionBuilder.Append(group.Key);
					descriptionBuilder.Append('-');
					descriptionBuilder.Append(string.Join(Separator, group.Select(c => c.ToString()).Distinct()));
				}

				return descriptionBuilder.ToString();
			}
		}
	}
}
