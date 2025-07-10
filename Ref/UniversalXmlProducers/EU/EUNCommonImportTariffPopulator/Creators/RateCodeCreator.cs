using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public class RateCodeCreator : IRateCodeCreator
	{
		public RateCodeCreator(IEnumerable<measureType1> measureTypes)
		{
			this.measureTypes = measureTypes;
		}
		readonly IEnumerable<measureType1> measureTypes;

		public string Get(measure measure)
		{
			var measureType = measureTypes.FirstOrDefault(x => x.measureType == measure.measureType);
			if (measureType != null)
			{
				switch (measureType.measureTypeSeriesId)
				{
					case "C": // Applicable duty
						return "A00";
					case "J": // Countervailing charge
						return "A20";
					case "S": // Supplementary amount
						switch (measure.measureType)
						{
							case "652":
							case "654":
							case "656":
							case "658":
								return "A20";
						}
						return string.Empty;
					case "D": // Anti-dumping or countervailing duties
						switch (measure.measureType)
						{
							case "551": // Provisional anti-dumping duty
								return "A35";
							case "552": // Definitive anti-dumping duty
								return "A30";
							case "553": // Provisional countervailing duty
								return "A45";
							case "554": // Definitive countervailing duty
								return "A40";
						}
						return string.Empty;
				}
			}
			return string.Empty;
		}
	}
}
