using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	sealed class TariffSmallAmountImportedGoods : IOtherScraper
	{
		public TariffSmallAmountImportedGoods(IWebSourceProvider sourceProvider, string url)
		{
			this.url = url;
			this.sourceProvider = sourceProvider;

			Tariffs = new List<SimpleTariff>();
			TariffWithoutDescriptionMappings = new Dictionary<string, string>();
		}

		readonly string url;
		readonly IWebSourceProvider sourceProvider;

		const string DefaultCompositeKey = "99";
		const string OtherCompositeKey = "99..07";

		const string Separator = @"*";
		const string MappingSeparator = @"、*";

		DynamicCsvRecord Nomenclature { get; set; }

		DynamicCsvRecord OtherNomenclature { get; set; }

		List<SimpleTariff> Tariffs { get; }

		Dictionary<string, string> TariffWithoutDescriptionMappings { get; }

		void IOtherScraper.Load()
		{
			var filePath = Path.GetTempFileName();

			if (FileDownloader.TryDownload(sourceProvider, url, filePath).Result && CsvReaderHelper.TryRead(filePath, out var lines))
			{
				var transportMode = string.Empty;
				string[] previousValues = null;

				foreach (var line in lines)
				{
					var values = JapaneseLocalHelper.ConvertFullWidthToHalfWidth(line).Split(',');

					if (values.Length < 5)
					{
						continue;
					}

					var firstValue = values.FirstOrDefault().Trim();
					var lastValue = values.LastOrDefault().Replace(" ", string.Empty);

					if (previousValues == null)
					{
						BuildNomenclature(firstValue);
						previousValues = new string[5];
					}
					else
					{
						switch (firstValue)
						{
							case "(航空)":
								transportMode = "航空";
								break;
							case "(海上)":
								transportMode = "海上";
								break;
						}

						var codes = values[3].Replace(" ", string.Empty);

						if (codes.StartsWith(DefaultCompositeKey, StringComparison.OrdinalIgnoreCase))
						{
							var lastRate = previousValues[4].Trim();
							var currentRate = values[4].Trim();
							var finalRate = string.IsNullOrWhiteSpace(currentRate) ? lastRate : currentRate;

							if (string.IsNullOrWhiteSpace(transportMode))
							{
								AddNormalTariffs(codes, previousValues, values, finalRate);
							}
							else
							{
								AddSpecialTariffs(codes, previousValues, values, finalRate);
							}
						}
						else if (firstValue == "7" && OtherNomenclature == null)
						{
							BuildOtherNomenclature(values[1], values[4]);
						}
					}

					for (var i = 0; i < 5; i++)
					{
						var value = values[i];
						var previousValue = previousValues[i];

						previousValues[i] = string.IsNullOrWhiteSpace(value) ? previousValue : value;
					}
				}
			}
		}

		void BuildNomenclature(string description)
		{
			var properties = new string[Tariff.Header.PropertiesCount];
			properties[0] = "Nomenclature";
			properties[2] = DefaultCompositeKey;
			properties[3] = "Simplified tariff applicable to low-value imported goods";
			properties[4] = description;

			Nomenclature = new DynamicCsvRecord(properties);
		}

		void BuildOtherNomenclature(string description, string rate)
		{
			var properties = new string[Tariff.Header.PropertiesCount];
			properties[0] = "Nomenclature";
			properties[2] = OtherCompositeKey;
			properties[3] = "OTHER(N.E.S.)";
			properties[4] = description;
			properties[7] = rate;

			OtherNomenclature = new DynamicCsvRecord(properties);
		}

		void AddNormalTariffs(string tariffCodes, string[] previousValues, string[] currentValues, string rate)
		{
			var fullDescription = OtherNomenclature != null ? string.Empty : BuildDescription(previousValues, currentValues);
			var mappings = tariffCodes.Split(MappingSeparator, StringSplitOptions.RemoveEmptyEntries);

			if (mappings.Length == 2)
			{
				TariffWithoutDescriptionMappings.Add(mappings[0], mappings[1]);
			}

			var index = 0;

			foreach (var code in mappings)
			{
				AddOrUpdateTariff(code, fullDescription, rate, index > 0);
				index++;
			}
		}

		static string BuildDescription(string[] previousValues, string[] currentValues)
		{
			var descriptions = new List<string>();

			for (var i = 0; i < 3; i++)
			{
				var value = currentValues[i];
				var description = string.IsNullOrWhiteSpace(value) ? previousValues[i] : value;

				if (!string.IsNullOrWhiteSpace(description))
				{
					descriptions.Add(description);
				}
			}

			return string.Join(Separator, descriptions);
		}

		void AddSpecialTariffs(string tariffCode, string[] previousValues, string[] currentValues, string rate)
		{
			var customsOffice = BuildCustomsOffice(previousValues, currentValues);

			var simpleTariff = AddOrUpdateTariff(tariffCode, string.Empty, rate, false);
			simpleTariff.CustomsOffices.Add(customsOffice);

			if (TariffWithoutDescriptionMappings.TryGetValue(tariffCode, out var mappingCode))
			{
				var mappingTariff = AddOrUpdateTariff(mappingCode, string.Empty, rate, true);
				mappingTariff.CustomsOffices.Add(customsOffice);
			}
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

		SimpleTariff AddOrUpdateTariff(string code, string fullDescription, string rate, bool isLimitedNonExciseGoods)
		{
			var existingTariff = Tariffs.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
			if (existingTariff != null)
			{
				existingTariff.IsLimitedNonExciseGoods = existingTariff.IsLimitedNonExciseGoods || isLimitedNonExciseGoods;
			}
			else
			{
				existingTariff = new SimpleTariff(code)
				{
					Rate = rate,
					Description = fullDescription,
					IsOtherTariff = OtherNomenclature != null,
					IsLimitedNonExciseGoods = isLimitedNonExciseGoods
				};

				Tariffs.Add(existingTariff);
			}

			return existingTariff;
		}

		void IOtherScraper.Pack(List<DynamicCsvRecord> records)
		{
			var propertiesCount = Tariff.Header.PropertiesCount;

			void AddTariffs(SimpleTariff tariff)
			{
				var properties = new string[propertiesCount];
				properties[0] = "Tariff";
				properties[1] = tariff.Code;
				properties[2] = tariff.CompositeKey;
				properties[4] = tariff.GetFullDescription();
				properties[7] = tariff.Rate;

				records.Add(new DynamicCsvRecord(properties));
			}

			if (Nomenclature != null)
			{
				records.Add(Nomenclature);

				foreach (var tariff in Tariffs.Where(c => !c.IsOtherTariff).OrderBy(c => c.Code))
				{
					AddTariffs(tariff);
				}
			}

			if (OtherNomenclature != null)
			{
				records.Add(OtherNomenclature);

				foreach (var tariff in Tariffs.Where(c => c.IsOtherTariff && c.CustomsOffices.Count > 0).OrderBy(c => c.Code))
				{
					AddTariffs(tariff);
				}
			}
		}

		void IOtherScraper.Release()
		{
			Tariffs.Clear();
		}

		sealed class SimpleTariff
		{
			const string DuplicateTariffEnd = @"申告貨物が消費税非課税品目である場合に限り";

			public SimpleTariff(string code)
			{
				Code = code;
				CustomsOffices = new List<CustomsOffice>();
			}

			public string Code { get; }

			public string CompositeKey => IsOtherTariff ? OtherCompositeKey : DefaultCompositeKey;

			public string Description { get; set; }

			public List<CustomsOffice> CustomsOffices { get; }

			public bool IsLimitedNonExciseGoods { get; set; }

			public bool IsOtherTariff { get; set; }

			public string Rate { get; set; }

			public string GetFullDescription()
			{
				var fullDescription = Description;

				if (IsOtherTariff)
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

					fullDescription = descriptionBuilder.ToString();
				}

				return IsLimitedNonExciseGoods ? string.Concat(fullDescription, Separator, DuplicateTariffEnd) : fullDescription;
			}
		}
	}
}
