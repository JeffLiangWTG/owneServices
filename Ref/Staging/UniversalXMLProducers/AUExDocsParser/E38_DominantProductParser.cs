using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;
using ExcelParser;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.AUExDocsParser
{
	public class E38_DominantProductParser : IExDocsParser
	{
		public IEnumerable<RowResult> Parse(string filePath)
		{
			Argument.NotNullOrEmpty(filePath, nameof(filePath));
			var rowConf = new RowConfiguration();
			rowConf.StartingRow = 2;
			rowConf.ColumnConfigurations.Add(new BasicStringColumnConfiguration(1, "RefCusCodeList", "ZZD_Code", false));

			var allRowResults = ExcelParser.Utilities.ReadCSVFileIntoResults(filePath, rowConf, '\t', '\'');
			return allRowResults;
		}

		public void ExportToXml(IEnumerable<RowResult> rowResults, string filePath, bool initialLoad = false)
		{
			Argument.NotNull(rowResults, nameof(rowResults));
			Argument.NotNullOrEmpty(filePath, nameof(filePath));
			Utils.ExportToXMLFile(
				"AU E38 ExDoc Dominant Product",
				Utils.GenerateDataSetElementsForE38,
				rowResults,
				filePath,
				Utils.GetWriterConfiguration("DOMP"),
				initialLoad);
		}
	}
}
