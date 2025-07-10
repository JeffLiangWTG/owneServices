using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Types;

namespace Enterprise.ReportTesting
{
	public class ReportTestScenario
	{
		public string ScenarioName { get; set; }
		public List<string> ParametersValuesList { get; set; }
		public Dictionary<string, string> FiltersUsed { get; set; }
		public int ExpectedRowCount { get; set; }
		readonly Dictionary<int, List<string>> ExpectedTextLookup = new Dictionary<int, List<string>>();
		public string AdditionalWhereClause { get; set; }

		public void AddExpectedText(int rowNumber, string expectedText)
		{
			List<string> listText;

			if (ExpectedTextLookup.ContainsKey(rowNumber))
			{
				listText = ExpectedTextLookup[rowNumber];
			}
			else
			{
				listText = new List<string>();
				ExpectedTextLookup[rowNumber] = listText;
			}
			listText.Add(expectedText);
		}

		public bool MustPerformAdditionalChecksForRowNumber(int rowNumber)
		{
			return ExpectedTextLookup.ContainsKey(rowNumber);
		}

		public List<string> GetExpectedTextList(int rowNumber)
		{
			return ExpectedTextLookup.ContainsKey(rowNumber) ? ExpectedTextLookup[rowNumber] : null;
		}
	}

	public class ExpectedReportOutput
	{
		readonly List<string> RowData = new List<string>();

		public void AddRow(string row)
		{
			RowData.Add(row);
		}

		public List<string> GetRowData()
		{
			return RowData;
		}
	}

	public class ReportDataObjectDetails
	{
		public ZString ObjectName;
		public SqlObjectType SqlObjectType;
		public List<ReportSchemaColumn> ExpectedColumnsInOrder;
		public List<string> ParameterNames = new List<string>();
		protected Dictionary<string, Type> ParameterTypeLookup = new Dictionary<string, Type>();

		protected void AddParameter(string parameterName, Type parameterType)
		{
			ParameterNames.Add(parameterName);
			ParameterTypeLookup[parameterName] = parameterType;
		}

		public Type GetParameterType(string parameterName)
		{
			return ParameterTypeLookup[parameterName];
		}
	}

	public class ReportColumnHelper
	{
		readonly List<string> ColumnNames = new List<string>();
		readonly Dictionary<string, ReportSchemaColumn> ColumnSchemaLookup = new Dictionary<string, ReportSchemaColumn>();
		readonly Dictionary<string, string> ColumnValueLookup = new Dictionary<string, string>();

		public ReportColumnHelper(List<ReportSchemaColumn> columnList)
		{
			foreach (var columnSchema in columnList)
			{
				var name = columnSchema.ColumnName;
				ColumnNames.Add(name);
				ColumnSchemaLookup.Add(name, columnSchema);
				ColumnValueLookup.Add(name, "");
			}
		}

		public void SetColumnValue(string columnName, object columnValue, int decimalPrecision = 5)
		{
			if (!ColumnNames.Contains(columnName))
			{
				throw new FieldAccessException($"Invalid column name. Column {columnName} was not found in the list.");
			}
			ColumnValueLookup[columnName] = FormatValue(columnName, columnValue, decimalPrecision);
		}

		public string GetRowText()
		{
			var sb = new StringBuilder();
			int counter = 0;
			int totalColumns = ColumnNames.Count;

			foreach (var key in ColumnNames)
			{
				sb.Append("[");
				sb.Append(key);
				sb.Append("]='");
				sb.Append(ColumnValueLookup[key]);
				sb.Append("'");

				counter++;
				if (counter < totalColumns)
				{
					sb.Append("; ");
				}
			}
			return sb.ToString();
		}

		string FormatValue(string columnName, object columnValue, int decimalPrecision)
		{
			if (columnValue == null)
			{
				return "";
			}

			var columnSchema = ColumnSchemaLookup[columnName];

			if (columnSchema.DataType.UnderlyingSystemType == typeof(DateTime))
			{
				return (new ZDateTime(columnValue)).ToISO8601String();
			}
			else if (columnSchema.DataType.UnderlyingSystemType == typeof(decimal))
			{
				if (columnValue.GetType() == typeof(ZDecimal))
				{
					return (new ZDecimal(columnValue)).ToString("#0.0000", CultureInfo.InvariantCulture);
				}
				else
				{
					var formatString = string.Format(CultureInfo.InvariantCulture, "F{0}", decimalPrecision);
					return (new ZDecimal(columnValue)).ToString(formatString, CultureInfo.InvariantCulture);
				}
			}
			else
			{
				return columnValue.ToString();
			}
		}
	}

	public class ReportParameterHelper
	{
		readonly ReportDataObjectDetails dataObjectDetails;
		readonly Dictionary<string, string> ParameterValueLookup = new Dictionary<string, string>();

		public ReportParameterHelper(ReportDataObjectDetails reportDataObjectDetails)
		{
			dataObjectDetails = reportDataObjectDetails;

			foreach (var name in dataObjectDetails.ParameterNames)
			{
				ParameterValueLookup.Add(name, "null");
			}
		}

		public void SetParameterValue(string parameterName, string parameterValue)
		{
			if (!dataObjectDetails.ParameterNames.Contains(parameterName))
			{
				throw new FieldAccessException($"Invalid parameter name. Parameter {parameterName} was not found in the list.");
			}
			ParameterValueLookup[parameterName] = parameterValue;
		}

		public List<string> GetParametersValuesList()
		{
			var parameterValues = new List<string>();

			foreach (var name in dataObjectDetails.ParameterNames)
			{
				string theValue = ParameterValueLookup[name];
				if (theValue != "null")
				{
					Type theType = dataObjectDetails.GetParameterType(name);
					if (theType == typeof(string) || theType == typeof(DateTime) || theType == typeof(Guid))
					{
						theValue = "'" + theValue + "'";
					}
				}
				parameterValues.Add(theValue);
			}
			return parameterValues;
		}

		public Dictionary<string, string> GetFiltersUsed()
		{
			var filtersUsed = new Dictionary<string, string>();

			foreach (var name in dataObjectDetails.ParameterNames)
			{
				var value = ParameterValueLookup[name];
				if (value != "null")
				{
					filtersUsed[name] = value;
				}
			}
			return filtersUsed;
		}
	}

	public abstract class ReportDataChecker : ReportFunctionalTestCase
	{
		protected ReportDataObjectDetails reportDataObjectDetails;
		protected ReportTestScenario scenario;

		protected void CheckReportData(ReportTestScenario testScenario, ExpectedReportOutput expectedOutput)
		{
			scenario = testScenario;

			var expectedData = expectedOutput.GetRowData();
			AssertEquals($"The prepared expected data collection does not have the expected number of rows for scenario: {scenario.ScenarioName}", scenario.ExpectedRowCount, expectedData.Count);

			var reportData = FireReportAndReturnDataTable(additionalWhereClause: testScenario.AdditionalWhereClause);
			AssertEquals($"The result set retrieved from the database does not have the expected number of rows for scenario: {scenario.ScenarioName}", scenario.ExpectedRowCount, reportData.Rows.Count);

			int rowIndex = -1;

			foreach (DataRow reportDataRow in reportData.Rows)
			{
				rowIndex++;
				var formattedRowFromDb = FormatRowsValues(reportDataRow, reportData);
				var expectedRow = expectedData[rowIndex];
				AssertContainsMoreHelpfully(expectedRow, formattedRowFromDb);

				int lookupKey = rowIndex + 1;
				if (scenario.MustPerformAdditionalChecksForRowNumber(lookupKey))
				{
					var expectedTextList = scenario.GetExpectedTextList(lookupKey);
					foreach (var expectedText in expectedTextList)
					{
						AssertContains($"The expected text snippet was not retrieved from the database for scenario: {scenario.ScenarioName}", expectedText, formattedRowFromDb);
					}
				}
			}
		}

		protected override SqlObjectType SqlObjectType => reportDataObjectDetails.SqlObjectType;

		protected override ZString ObjectName => reportDataObjectDetails.ObjectName;

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder => reportDataObjectDetails.ExpectedColumnsInOrder;

		protected override List<string> ParametersValuesList => scenario.ParametersValuesList;
	}
}
