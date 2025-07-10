using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using System.IO;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	class StatisticalClassificationPeriodCharacteristicParser
	{
		public static Dictionary<string, List<string>> Parse(string fileNamePrefix, string indexUri)
		{
			var clientHelper = new HttpClientHelper();
			(var content, _) = CMRReferenceFileDownloader.Download(clientHelper, fileNamePrefix, indexUri);
			return Parse(content);
		}

		internal static Dictionary<string, List<string>> Parse(string content)
		{
			var result = new Dictionary<string, List<string>>();
			var tariffAttributeConverter = new LineToEntityConverter<RefCusTariffAttribute>(TariffAttributeMappings, 1);

			using (var reader = new StringReader(content))
			{
				var line = string.Empty;
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						var tariffAttribute = tariffAttributeConverter.Convert(line);
						if (tariffAttribute.ZZ3_Name != null && tariffAttribute.ZZ3_Value != null)
						{
							// Remove the space between the second and third token, e.g. '99993006 15'
							tariffAttribute.ZZ3_Name = tariffAttribute.ZZ3_Name.Replace(" ", "");
							if (!result.TryGetValue(tariffAttribute.ZZ3_Name, out var list))
							{
								list = new List<string>();
								result.Add(tariffAttribute.ZZ3_Name, list);
							}
							list.Add(tariffAttribute.ZZ3_Value);
						}
					}
				}
			}

			return result;
		}

		static PropertyMapping<RefCusTariffAttribute>[] TariffAttributeMappings => new[]
		{
			new PropertyMapping<RefCusTariffAttribute>(entity => entity.ZZ3_Value, 1, 4),
			new PropertyMapping<RefCusTariffAttribute>(entity => entity.ZZ3_Name, 6, 11)
		};
	}
}
