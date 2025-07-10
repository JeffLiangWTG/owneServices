using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public sealed class SeaVesselParser
	{
		readonly static Regex ZZO_RadioCallSignRegex = new Regex("^[A-Z0-9 \\p{P}\\p{S}]+$");

		public static bool TryParse(IEnumerable<string> rows, out List<RefVesselZZ> refVesselZZCodeList)
		{
			refVesselZZCodeList = rows.Select(r => ParserHelper.SplitComma(r)).Where(x => x.Length > 2 && ZZO_RadioCallSignRegex.IsMatch(x[2]))
			.Select(i => new RefVesselZZ()
			{
				ZZO_Code = i[1],
				ZZO_RadioCallSign = i[2],
			}).ToList();

			if (refVesselZZCodeList.Count > 0)
			{
				return true;
			}
			ErrorWriter.WriteError("No RefVesselZZCodeList is parsed");
			return false;
		}
	}
}
