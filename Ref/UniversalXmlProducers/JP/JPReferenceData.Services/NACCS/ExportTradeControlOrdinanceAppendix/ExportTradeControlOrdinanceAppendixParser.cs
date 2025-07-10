using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class ExportTradeControlOrdinanceAppendixParser : RefCusCodeListParser
	{
		public override string CurrentCodeType => Constants.CodeType.ExportTradeControlOrdinanceAppendix;

		public override bool TryParse(IEnumerable<string> rows, out List<RefCusCodeList> refCusCodeLists)
		{
			refCusCodeLists = new List<RefCusCodeList>();

			var codeRegex = new Regex("^[A-Z0-9]{1,5}$");
			var cashedDescriptionHeader = string.Empty;
			var rowCount = rows.Count();

			for (var i = 0; i < rowCount; i++)
			{
				var row = rows.ElementAt(i);

				if (string.IsNullOrWhiteSpace(row))
				{
					continue;
				}

				var columns = row.Split(',');

				if (columns.Length < 2)
				{
					throw new UnhandledApplicationException($"Can't split to 2 columns from [{i}]{row}");
				}

				var code = columns[1].Trim();
				var description = columns[0].Trim();

				if (code == "コード")
				{
					var previousColumns = rows.ElementAt(i - 1).Split(',');
					cashedDescriptionHeader = previousColumns[0].Trim();
				}

				var fullDescription = cashedDescriptionHeader + "*" + description;
				if (code.StartsWith("(注)", StringComparison.Ordinal))
				{
					if (code.Length < 6)
					{
						throw new UnhandledApplicationException($"Can't parse the code from [{i}]{row}");
					}

					code = code.Substring(6);
					description = rows.ElementAt(i - 1).Split(',')[0].Trim();

					for (var j = i + 1; j < rowCount; j++)
					{
						var curRow = rows.ElementAt(j);
						if (curRow.StartsWith("（注）", StringComparison.Ordinal))
						{
							fullDescription = cashedDescriptionHeader + "*" + description + curRow.Split(',')[0].Trim();
							break;
						}
					}
				}

				if (!string.IsNullOrEmpty(description) && codeRegex.IsMatch(code))
				{
					var refCusCodeList = new RefCusCodeList()
					{
						ZZD_Code = code.PadLeft(5, '0'),
						ZZD_Description = fullDescription,
						ZZD_StartDate = GetZZD_StartDate(columns),
						ZZD_EndDate = GetZZD_EndDate(columns),
					};

					if (ValidateRefCusCodeList(refCusCodeLists, refCusCodeList))
					{
						refCusCodeLists.Add(refCusCodeList);
					}
				}
			}

			if (refCusCodeLists.Any())
			{
				return true;
			}

			ErrorWriter.WriteError("No RefCusCodeList is parsed");
			return false;
		}
	}
}
