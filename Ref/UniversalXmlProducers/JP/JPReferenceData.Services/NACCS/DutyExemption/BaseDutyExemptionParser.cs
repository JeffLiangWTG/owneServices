using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public abstract class BaseDutyExemptionParser : RefCusCodeListParser
	{
		protected override RefCusCodeListAttributeParser GetRefCusCodeListAttributeParserCore() => new DutyExemptionAttributeParser();

		public override bool TryParse(IEnumerable<string> rows, out List<RefCusCodeList> refCusCodeLists)
		{
			refCusCodeLists = new List<RefCusCodeList>();
			var columnsToAdd = new string[4];
			var cachedDescriptionHeader = string.Empty;
			var previousFirstColumn = string.Empty;
			var codeRegex = new Regex(@"^[0-9]{1,5}$");
			var descRegex = new Regex(@"^(令|規則)");
			var attributeColumnIndex = DutyExemptionAttributeParserConfig.AttributeValueIndex;
			bool parseResult;

			for (var i = 0; i < rows.Count(); i++)
			{
				var row = rows.ElementAt(i);
				var columns = row.Split(',');
				columns = columns.Select(x => x.Trim()).ToArray();

				if (columns[0] == "コード")
				{
					cachedDescriptionHeader = previousFirstColumn;
				}

				if (codeRegex.IsMatch(columns[0]))
				{
					if (!string.IsNullOrEmpty(columnsToAdd[0]))
					{
						var hasAttribute = !string.IsNullOrWhiteSpace(columnsToAdd[attributeColumnIndex]);
						AddRefCusCodeList(refCusCodeLists, columnsToAdd, hasAttribute);
					}

					columns[0] = columns[0].PadLeft(5, '0');
					columns[1] = $"{columns[2].Replace("（注）", "")} {cachedDescriptionHeader} {columns[1]}";
					columnsToAdd = columns;
				}
				else if (descRegex.IsMatch(columns[1]))
				{
					columnsToAdd[1] = $"{columnsToAdd[1]} {columns[1]}";
				}
				previousFirstColumn = columns[0];
			}
			var needAddAttribute = !string.IsNullOrWhiteSpace(columnsToAdd[attributeColumnIndex]);
			AddRefCusCodeList(refCusCodeLists, columnsToAdd, needAddAttribute);

			parseResult = refCusCodeLists.Any();
			if (!parseResult)
			{
				ErrorWriter.WriteError("No RefCusCodeList is parsed.");
			}

			return parseResult;
		}
	}
}
