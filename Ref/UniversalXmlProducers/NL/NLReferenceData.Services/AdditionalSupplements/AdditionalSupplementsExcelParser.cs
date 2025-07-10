using System.Collections.Generic;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class AdditionalSupplementsExcelParser : CommonExcelParserAbstract<AdditionalSupplementsData>
	{
		string StartingCodeChar { get; set; }
		protected override int StartingRow => 6;

		protected override AdditionalSupplementsData ExtractDataFromRow(XlsFile xlsFile, string key, int rowId)
		{
			return new AdditionalSupplementsData
			{
				Code = key,
				Description = xlsFile.GetStr(rowId, 2)
			};
		}

		protected override bool ShouldProcessFile(XlsFile xlsFile)
		{
			StartingCodeChar = xlsFile.GetStr(2, 2);

			return !string.IsNullOrWhiteSpace(StartingCodeChar);
		}

		protected override bool IsKeyValid(string key) => key.StartsWith(StartingCodeChar, System.StringComparison.Ordinal);

		public IList<AdditionalSupplementsData> ReadXlsFileIntoResults(IList<string> files)
		{
			return ReadXlsFile(files);
		}
	}
}
