using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class PackageTypeAttributeParserConfig : RefCusCodeListAttributeParserConfig
	{
		public override string ZZE_ZXE_NKName => "Remarks";

        public override string GetZZE_Value(string[] columns) => string.Join(" - ", new[] { columns[3], columns[4] }.Where(c => !string.IsNullOrWhiteSpace(c)));
	}
}
