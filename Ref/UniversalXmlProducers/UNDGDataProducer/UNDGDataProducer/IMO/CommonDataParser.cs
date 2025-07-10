using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class CommonDataParser
	{
		readonly IEnumerable<StowSegRecord> _stowSegRecords;
		readonly IEnumerable<SpecProvRecord> _specProvRecords;

		public CommonDataParser(IEnumerable<StowSegRecord> stowSegRecords, IEnumerable<SpecProvRecord> specProvRecords)
		{
			_stowSegRecords = stowSegRecords;
			_specProvRecords = specProvRecords;
		}

		public IEnumerable<UNDGCommonData> Parse()
		{
			foreach (var specProv in _specProvRecords)
			{
				yield return SpecProvParser.GetCommonData(specProv);
			}
			var commonDataIdx = new Dictionary<string, string>();
			foreach (var stowSeg in _stowSegRecords)
			{
				foreach (var commonData in StowSegParser.GetCommonData(stowSeg))
				{
					if (commonDataIdx.Any(x => x.Key == commonData.DC_Index && x.Value == commonData.DC_Type))
					{
						continue;
					}
					commonDataIdx.Add(commonData.DC_Index, commonData.DC_Type);
					yield return commonData;
				}
			}
		}
	}
}
