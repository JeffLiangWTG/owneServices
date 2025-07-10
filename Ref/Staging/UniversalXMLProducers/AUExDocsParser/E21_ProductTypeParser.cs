using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;
using ExcelParser;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.AUExDocsParser
{
	public class E21_ProductTypeParser : IExDocsParser
	{
		public IEnumerable<RowResult> Parse(string filePath)
		{
			Argument.NotNullOrEmpty(filePath, nameof(filePath));
			var rowConf = new RowConfiguration();
			rowConf.StartingRow = 2;
			rowConf.ColumnConfigurations.Add(new BasicStringColumnConfiguration(1, "RefCusCodeList", "ZZD_ZZK_NKCodeType", false));
			rowConf.ColumnConfigurations.Add(new BasicStringColumnConfiguration(2, "RefCusCodeList", "ZZD_Code", false));
			rowConf.ColumnConfigurations.Add(new BasicStringColumnConfiguration(3, "RefCusCodeList", "ZZD_Description", false));
			rowConf.ColumnConfigurations.Add(new BasicStringColumnConfiguration(4, "RefCusCodeList", "ZZE_Value", false));

			var allRowResults = ExcelParser.Utilities.ReadCSVFileIntoResults(filePath, rowConf, '\t', '\'');
			return allRowResults;
		}

		public void ExportToXml(IEnumerable<RowResult> rowResults, string filePath, bool initialLoad = false)
		{
			Argument.NotNull(rowResults, nameof(rowResults));
			Argument.NotNullOrEmpty(filePath, nameof(filePath));
			Utils.ExportToXMLFile(
				"AU E21 ExDoc Product Type",
				Utils.GenerateDataSetElementsForE21,
				rowResults,
				filePath,
				Utils.GetWriterConfiguration(),
				initialLoad);
		}
	}
}
