using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public sealed class AQISCommodityStatisticalClassificationParser : BaseCMRReferenceDataParser
	{
		protected override string FileNamePrefix => ApplicationConfig.AQISCommodityStatisticalClassificationFilePrefix;

		protected override string OutputXMLName => "RefCusTariff_AU_AQISCommodityStatisticalClassification.xml";

		protected override string DataSource => "AU AQIS Commodity Statistical Classification";

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate) => CMRXMLWriterConfigurationBuilder.BuildRefCusTariffAttributeConfiguration(CMRConstants.TariffAttributeNames.AQSCMSTC, Constants.TariffTypes.IMP);

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			var tariffDic = GetTariffs(content);
			foreach (var tariff in tariffDic.Values)
			{
				xmlWriter.PopulateData(tariff);
			}
		}

		internal static Dictionary<string, List<string>> Parse(string fileNamePrefix, string indexUri)
		{
			var clientHelper = new HttpClientHelper();
			var (content, _) = CMRReferenceFileDownloader.Download(clientHelper, fileNamePrefix, indexUri);

			return ConvertToAttributeListDict(GetTariffs(content));
		}

		static Dictionary<string, List<string>> ConvertToAttributeListDict(Dictionary<string, RefCusTariff> tariffDict)
		{
			var result = new Dictionary<string, List<string>>();
			foreach (var tariff in tariffDict.Values)
			{
				var attributes = tariff.RefCusTariffAttributes.Select(attr => attr.ZZ3_Value).ToList();
				result.Add(tariff.ZZ1_TariffCode, attributes);
			}
			return result;
		}

		static Dictionary<string, RefCusTariff> GetTariffs(string content)
		{
			var tariffConverter = new LineToEntityConverter<RefCusTariff>(TariffMappings, 1);
			var attributeConverter = new LineToEntityConverter<RefCusTariffAttribute>(TariffAttributeMappings, 1);

			using (var reader = new StringReader(content))
			{
				var line = string.Empty;
				var tariffDic = new Dictionary<string, RefCusTariff>();
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						var code = tariffConverter.Convert(line);
						var attribute = attributeConverter.Convert(line);
						if (!string.IsNullOrWhiteSpace(code.ZZ1_TariffCode) && attribute.ZZ3_Value != null)
						{
							code.ZZ1_TariffCode = Regex.Replace(code.ZZ1_TariffCode, @"\s", "");
							if (tariffDic.TryGetValue(code.ZZ1_TariffCode, out var refCusTariff))
							{
								refCusTariff.RefCusTariffAttributes = refCusTariff.RefCusTariffAttributes.Concat(new RefCusTariffAttribute[] { attribute }).ToArray();
							}
							else
							{
								code.RefCusTariffAttributes = new RefCusTariffAttribute[] { attribute };
								tariffDic.Add(code.ZZ1_TariffCode, code);
							}
						}
						else
						{
							Console.Error.WriteLine(InsufficientInfoErrorMessage, line);
						}
					}
				}

				return tariffDic;
			}
		}

		static PropertyMapping<RefCusTariff>[] TariffMappings => new PropertyMapping<RefCusTariff>[]
		{
			new PropertyMapping<RefCusTariff>(tariff => tariff.ZZ1_TariffCode, 6, 11)
		};

		static PropertyMapping<RefCusTariffAttribute>[] TariffAttributeMappings => new PropertyMapping<RefCusTariffAttribute>[]
		{
			new PropertyMapping<RefCusTariffAttribute>(entity => entity.ZZ3_Value, 1, 4)
		};
	}
}
