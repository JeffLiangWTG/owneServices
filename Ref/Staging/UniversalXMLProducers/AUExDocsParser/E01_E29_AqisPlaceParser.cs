using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;
using ExcelParser;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.AUExDocsParser
{
	public class E01_AqisPlaceParser : IExDocsParser
	{
		public IEnumerable<RowResult> Parse(string filePath)
		{
			Argument.NotNullOrEmpty(filePath, nameof(filePath));
			var rowConf = new RowConfiguration();
			rowConf.StartingRow = 2;
			rowConf.ColumnConfigurations.Add(new BasicStringColumnConfiguration(1, "RefCusCodeList", "ZZD_Code", false));
			rowConf.ColumnConfigurations.Add(new BasicStringColumnConfiguration(2, "RefCusCodeList", "ZZD_Description", false));
			rowConf.ColumnConfigurations.Add(new BasicStringColumnConfiguration(3, "RefCusCodeList", "Commodities", false));

			var allRowResults = ExcelParser.Utilities.ReadCSVFileIntoResults(filePath, rowConf, '\t', '\'');
			return allRowResults;
		}

		public void ExportToXml(IEnumerable<RowResult> rowResults, string filePath, bool initialLoad = false)
		{
			Argument.NotNull(rowResults, nameof(rowResults));
			Argument.NotNullOrEmpty(filePath, nameof(filePath));
			Utils.ExportToXMLFile(
				"AU E01 ExDoc Aquis Place",
				Utils.GenerateDataSetElementsForE01,
				rowResults,
				filePath,
				Utils.GetWriterConfiguration("AQISP"),
				initialLoad);
		}
	}

	public class E29_AqisPlaceParser : IExDocsParser
	{
		public IEnumerable<RowResult> Parse(string filePath)
		{
			var rowConf = new RowConfiguration();
			rowConf.StartingRow = 2;
			rowConf.ColumnConfigurations.Add(new BasicStringColumnConfiguration(1, "RefCusCodeList", "ZZD_Code", false));
			rowConf.ColumnConfigurations.Add(new BasicStringColumnConfiguration(2, "RefCusCodeList", "ZZD_Description", false));
			rowConf.ColumnConfigurations.Add(new BasicStringColumnConfiguration(3, "RefCusCodeList", "State", false));

			var allRowResults = ExcelParser.Utilities.ReadCSVFileIntoResults(filePath, rowConf, '\t', '\'');
			return allRowResults;
		}

		public void ExportToXml(IEnumerable<RowResult> rowResults, string filePath, bool initialLoad = false)
		{
			Utils.ExportToXMLFile(
				"AU E29 ExDoc Aquis Place",
				Utils.GenerateDataSetElementsForE29,
				rowResults,
				filePath,
				Utils.GetWriterConfiguration("AQISP"),
				initialLoad);
		}
	}
}
