using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	sealed class SpecialCargoCodeWithLanguageParser : SpecialCargoCodeParser
	{
		protected override RefCusCodeListLanguageParser GetRefCusCodeListLanguageParserCore() => new SpecialCargoCodeLanguageParser();

		public override bool TryParse(IEnumerable<string> rows, out List<RefCusCodeList> refCusCodeLists)
		{
			var result = base.TryParse(rows, out refCusCodeLists);

			var codes = refCusCodeLists.Select(x => x.ZZD_Code).Distinct();
			var codesWithEngDesc = ((SpecialCargoCodeLanguageParser)RefCusCodeListLanguageParser).CodeWithEngDescriptionDictionary.Keys.Distinct();

			if (codes.Count() != codesWithEngDesc.Count() || codes.Any(c => codesWithEngDesc.All(d => !d.Equals(c, StringComparison.Ordinal))))
			{
				throw new UnhandledApplicationException("Hard coded file for refCusCodeListLanguage is not corresponded with automatically updated refCusCode, please notify the product manager to update it.");
			}

			return result;
		}
	}
}
