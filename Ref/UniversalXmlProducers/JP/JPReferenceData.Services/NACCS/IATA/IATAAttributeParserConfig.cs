using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class IATAAttributeParserConfig : RefCusCodeListAttributeParserConfig
	{
		public override string ZZE_ZXE_NKName => "IsCustomsAirport";

		public override string GetZZE_Value(string[] columns) => (columns[12]?.Trim() == "○") ? "Y" : "";
	}
}
