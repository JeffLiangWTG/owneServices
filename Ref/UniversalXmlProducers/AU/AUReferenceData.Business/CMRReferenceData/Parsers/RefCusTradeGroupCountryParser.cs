using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	class RefCusTradeGroupCountryParser
	{
		public static Dictionary<string, List<RefCusTradeGroupCountry>> Parse(string fileNamePrefix, string indexUri)
		{
			var clientHelper = new HttpClientHelper();
			(var content, _) = CMRReferenceFileDownloader.Download(clientHelper, fileNamePrefix, indexUri);
			return Parse(content);
		}

		internal static Dictionary<string, List<RefCusTradeGroupCountry>> Parse(string content)
		{
			var result = new Dictionary<string, List<RefCusTradeGroupCountry>>();
			var tradeGroupCountryConverter = new LineToEntityConverter<RefCusTradeGroupCountry>(TradeGroupCountryMappings, 1);

			using (var reader = new StringReader(content))
			{
				var line = string.Empty;
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						var tradeGroupCountry = tradeGroupCountryConverter.Convert(line);
						if (tradeGroupCountry.ZZB_Description != null && tradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode != null)
						{
							tradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode = tradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode.Replace(" ", "");
							if (!result.TryGetValue(tradeGroupCountry.ZZB_Description, out var list))
							{
								list = new List<RefCusTradeGroupCountry>();
								result.Add(tradeGroupCountry.ZZB_Description, list);
							}
							list.Add(
								new RefCusTradeGroupCountry()
								{
									ZZB_RN_NKTradeGroupCountryCode = tradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode,
									ZZB_StartDate = tradeGroupCountry.ZZB_StartDate
								});
						}
					}
				}
			}

			return result;
		}

		static PropertyMapping<RefCusTradeGroupCountry>[] TradeGroupCountryMappings => new[]
		{
			new PropertyMapping<RefCusTradeGroupCountry>(entity => entity.ZZB_RN_NKTradeGroupCountryCode, 32, 2),
			new PropertyMapping<RefCusTradeGroupCountry>(entity => entity.ZZB_StartDate, 11, 8),
			new PropertyMapping<RefCusTradeGroupCountry>(entity => entity.ZZB_Description, 1, 4)
		};
	}
}
