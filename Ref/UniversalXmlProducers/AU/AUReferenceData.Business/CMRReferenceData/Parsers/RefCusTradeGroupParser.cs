using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class RefCusTradeGroupParser : BaseCMRReferenceDataParser
	{
		protected override string FileNamePrefix => ApplicationConfig.RefCusTradeGroupFilePrefix;

		protected override string OutputXMLName => "AU CMR Trade Groups.xml";

		protected override string DataSource => "AU CMR Trade Groups";

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate) =>
			CMRXMLWriterConfigurationBuilder.BuildRefCusTradeGroupsConfiguration();

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			var tradeGroupConverter = new LineToEntityConverter<RefCusTradeGroup>(TradeGroupMappings, 1);
			var tradeGroupCountryDictionary = TradeGroupCountriesByTradeGroup;

			using (var reader = new StringReader(content))
			{
				var line = string.Empty;
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						var tradeGroup = tradeGroupConverter.Convert(line);
						if (tradeGroup.ZZA_TradeGroup != null)
						{
							if (tradeGroup.ZZA_EndDate == Constants.RefData_Common.MaximumDateTime || DateTime.Compare(DateTime.Now.Date, tradeGroup.ZZA_EndDate.Date) <= 0)
							{
								tradeGroup.ZZA_TradeGroup = tradeGroup.ZZA_TradeGroup.Replace(" ", "");

								if (tradeGroupCountryDictionary.TryGetValue(tradeGroup.ZZA_TradeGroup, out var tradeGroupCountries))
								{
									tradeGroup.RefCusTradeGroupCountries = tradeGroupCountries.ToArray();
								}

								xmlWriter.PopulateData(tradeGroup);
							}
						}
						else
						{
							Console.Error.WriteLine(InsufficientInfoErrorMessage, line);
						}
					}
				}
			}
		}

		public virtual Dictionary<string, List<RefCusTradeGroupCountry>> TradeGroupCountriesByTradeGroup => RefCusTradeGroupCountryParser.Parse(
				ApplicationConfig.RefCusTradeGroupCountryFilePrefix,
				ApplicationConfig.AUReferenceFilesDirectory);

		static PropertyMapping<RefCusTradeGroup>[] TradeGroupMappings => new[]
		{
			new PropertyMapping<RefCusTradeGroup>(entity => entity.ZZA_Description, 55, 250),
			new PropertyMapping<RefCusTradeGroup>(entity => entity.ZZA_TradeGroup, 1, 4),
			new PropertyMapping<RefCusTradeGroup>(entity => entity.ZZA_StartDate, 32, 8),
			new PropertyMapping<RefCusTradeGroup>(entity => entity.ZZA_EndDate, 41, 8, Constants.RefData_Common.MaximumDateTime),
		};
	}
}
