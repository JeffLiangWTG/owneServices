using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ReportTesting
{
	/// <summary>
	/// Helps test the function of your SQL report function.  Doesn't care about menus, excel sheets, Documents.xml.
	/// It's all about putting data into the database, firing your new report function, and showing us the results.
	/// Quite a bit like the transformation tests.
	/// </summary>
	public abstract class ReportFunctionalTestCase : TestCaseWithFactory
	{
		/// <summary>
		/// Prepare rows in database (no need to save) that you will query.  Prepare about 10 rows of all different scenarios.
		/// </summary>
		protected abstract void PrepareTestData();

		/// <summary>
		/// Tells us the name of your fucntion, e.g. Report_ShowUsTheMoney, and we will fire: select * from Report_ShowUsTheMoney() ORDER BY <ExpectedColumnsInAnyOrder[0]>, <ExpectedColumnsInAnyOrder[1]>, <ExpectedColumnsInAnyOrder[2]>...
		/// </summary>
		protected abstract ZString ObjectName { get; }

		/// <summary>
		/// Tell us which rows you expect to see.  The data is sorted by the name of the first expected column,
		/// </summary>
		protected abstract void AssertTestResults(DataTable resultsOrderedByExpectedColumnNames);

		/// <summary>
		/// Tell us which columns will be returned.  This will help ensure that if you rename columns in the function, you also rename them in the Excel sheet
		/// </summary>
		protected abstract List<ReportSchemaColumn> ExpectedColumnsInAnyOrder { get; }

		protected abstract SqlObjectType SqlObjectType { get; }

		protected override DbConnection TestConnection
		{
			get { return Db.Connection; }
		}

		/// <summary>
		/// If your function or proc needs parameters, return a list of values here.
		/// Give them as SQL-syntax parameter value strings.  e.g.
		/// '6FD2BB6D-6D28-427B-B326-33C14C21B910', '2008-11-04 10:06:49', 0.47, 'Foo'
		/// </summary>
		protected virtual List<string> ParametersValuesList
		{
			get { return new List<string>() { }; }
		}

		public static string QuoteParameter(object value)
		{
			return string.Format(CultureInfo.InvariantCulture, "'{0}'", value);
		}

		protected string FormatRowsValues(DataRow row, DataTable parentTable, bool ignoreGuid = false) => FormatRowsValues(row, parentTable.Columns.Cast<DataColumn>(), ignoreGuid: ignoreGuid);

		protected string FormatRowsValues(DataRow row, IEnumerable<DataColumn> dataColumns, bool ignoreGuid = false)
		{
			var sb = new ZStringBuilder();
			foreach (DataColumn column in dataColumns)
			{
				if (ignoreGuid && column.DataType == typeof(Guid))
				{
					continue;
				}
				sb.Append(string.Format(CultureInfo.InvariantCulture, "[{0}]='{1}'", column.ColumnName, FormatForOutput(row, column)));
			}
			return sb.ToStringWithDelimiterBetweenAppends("; ");
		}

		string FormatForOutput(DataRow row, DataColumn column)
		{
			if (column.DataType.UnderlyingSystemType == typeof(DateTime) && !string.IsNullOrEmpty(row[column.ColumnName].ToString()))
			{
				return ((ZDateTime)Convert.ToDateTime(row[column.ColumnName], CultureInfo.InvariantCulture)).ToISO8601String();
			}
			else
			{
				return row[column.ColumnName].ToString();
			}
		}

		public void TestRows()
		{
			PrepareTestData();
			Factory.Save();
			var results = FireReportAndReturnDataTable();
			Assert("No rows were returned. Your setup scenario must not expect the function to return no results because it's easy to treat this as a false success.  Change PrepareTestData() to ensure that at least one result is always returned (e.g. set up at least one good record and at least one bad record). OR, you have rows available but your DB object is not finding them.", results.Rows.Count > 0);
			var assertionCountBefore = AssertionCount;
			AssertTestResults(results);
			Assert("You did not make any assertions in AssertTestResults()", assertionCountBefore < AssertionCount);
		}

		protected virtual bool ShouldTestColumns
		{
			get { return true; }
		}

		public void TestColumns()
		{
			if (!ShouldTestColumns)
			{
				Assert(true);
				return;
			}

			if (SqlObjectType == ReportTesting.SqlObjectType.FunctionScalar && ExpectedColumnsInAnyOrder.Count != 1)
			{
				Assert("Object type 'FunctionScalar' must have exactly one expected column. Its name is not important and does not need to match any underlying schema column.", false);
			}

			var table = FireReportAndReturnDataTable(true);
			var actualColumns = new List<ReportSchemaColumn>();
			foreach (DataColumn column in table.Columns)
			{
				var col = new ReportSchemaColumn(column.DataType, column.ColumnName);
				actualColumns.Add(col);
			}
			CombineAssertions(delegate
			{
				foreach (var expectedColumn in ExpectedColumnsInAnyOrder)
				{
					var matchingActualColumn = actualColumns.Find(act => act.ColumnName.ToUpperInvariant() == expectedColumn.ColumnName.ToUpperInvariant() && act.DataType == expectedColumn.DataType);
					AssertNotNull(string.Format("No matching column was found.  Expected {0}; actual columns returned by SQL function were \r\n:{1}", expectedColumn, FormatAllColumns(actualColumns))
						, matchingActualColumn);
				}
				if (AllowColumnCountMismatch)
				{
					Assert("Column count mismatch without AllowColumnCountMismatch. You need to define at least one expected column and the live object most return at least the same number of columns.",
										ExpectedColumnsInAnyOrder.Count <= actualColumns.Count && ExpectedColumnsInAnyOrder.Count > 0);
				}
				else
				{
					AssertEquals("Column count mismatch. Actual: " + FormatAllColumns(actualColumns) + "\r\n\r\n Expected:" + FormatAllColumns(ExpectedColumnsInAnyOrder), ExpectedColumnsInAnyOrder.Count, actualColumns.Count);
				}
			});
		}

		/// <summary>
		/// Set this to true to allow your test to specify FEWER columns than the live object will return.
		/// Useful for when you're adding a test for a derived class and don't want to have to define/describe all the base object's coluymns (just your new ones).
		/// </summary>
		protected virtual bool AllowColumnCountMismatch
		{
			get { return false; }
		}

		protected virtual DataTable FireReportAndReturnDataTable(bool isNoResultsQuery = false, string additionalWhereClause = "")
		{
			var table = new DataTable();
			var sqlText = CreateCommandText(isNoResultsQuery, additionalWhereClause);
			using (var command = TestConnection.Command(sqlText))
			{
				using (var reader = command.ExecuteReader())
				{
					table.Load(reader, LoadOption.OverwriteChanges);
				}
			}
			return table;
		}

		protected virtual string CreateCommandText(bool isNoResultsQuery, string additionalWhereClause = "")
		{
			var sqlText = "";
			var sqlTextWithPlaceholderForAllButProcs = isNoResultsQuery ? "select top 0 * from {0}  where 1=2" : "select * from {0} ";
			string parameters = "";
			switch (SqlObjectType)
			{
				case ReportTesting.SqlObjectType.FunctionTable:
					parameters = string.Join(", ", ParametersValuesList.ToArray());
					sqlText = $"{string.Format(sqlTextWithPlaceholderForAllButProcs, ObjectName + "(" + parameters + ")")}{additionalWhereClause}";
					break;

				case ReportTesting.SqlObjectType.FunctionScalar:
					parameters = string.Join(", ", ParametersValuesList.ToArray());
					sqlText = string.Format(CultureInfo.InvariantCulture, "select dbo.{0} AS {1} ORDER BY {1}", ObjectName + "(" + parameters + ")", ExpectedColumnsInAnyOrder[0].ColumnName);
					break;

				case ReportTesting.SqlObjectType.ViewOrTable:
					sqlText = string.Format(sqlTextWithPlaceholderForAllButProcs, ObjectName);
					break;

				case ReportTesting.SqlObjectType.StoredProc:
					var parameters2 = string.Join(", ", ParametersValuesList.ToArray());
					sqlText = "EXEC " + ObjectName + " " + parameters2;
					break;
			}

			sqlText = AddOrderByForAllExceptProcsAndScalarFunctions(isNoResultsQuery, sqlText);
			return sqlText;
		}

		string AddOrderByForAllExceptProcsAndScalarFunctions(bool isNoResultsQuery, string sqlText)
		{
			if (!isNoResultsQuery && SqlObjectType != ReportTesting.SqlObjectType.StoredProc && SqlObjectType != ReportTesting.SqlObjectType.FunctionScalar && ExpectedColumnsInAnyOrder != null)
			{
				sqlText += " ORDER BY ";
				for (int i = 0; i < ExpectedColumnsInAnyOrder.Count; i++)
				{
					sqlText += ExpectedColumnsInAnyOrder[i].ColumnName;
					if (i < ExpectedColumnsInAnyOrder.Count - 1)
					{
						sqlText += ",";
					}
				}
			}
			return sqlText;
		}

		string FormatAllColumns(List<ReportSchemaColumn> cols)
		{
			var sb = new ZStringBuilder();
			foreach (var col in (from ReportSchemaColumn c in cols orderby c.ColumnName select c))
			{
				sb.Append(col.ToString());
			}
			return sb.ToStringWithNewLineBetweenAppends();
		}

		protected void AssertContainsMoreHelpfully(string expected, string actual, string message = null)
		{
			if (actual.Contains(expected))
			{
				Assert(true);
			}
			else
			{
				var actualPairs = Regex.Split(actual, "; ");
				var expectedPairs = Regex.Split(expected, "; ");
				CombineAssertions(message, delegate
				{
					foreach (var expectedPair in expectedPairs)
					{
						if (!actualPairs.Any(p => p == expectedPair))
						{
							var otherValue = TryToGetActualValueBasdedOnKey(actualPairs, expectedPair);
							Assert("Was expecting this, did not get it: " + expectedPair + otherValue, false);
						}
					}
				});
			}
		}

		string TryToGetActualValueBasdedOnKey(string[] actualPairs, string expectedPair)
		{
			var expectedKey = Regex.Split(expectedPair, "]='").FirstOrDefault();
			if (expectedKey != null)
			{
				return ". However I did find this guy, which might be what you meant: " + actualPairs.Where(p => p.StartsWith(expectedKey, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			}
			return "";
		}
	}

	public enum SqlObjectType
	{
		/// <summary>
		/// Select * from Report_Foo(x, y, z) where a=b"
		/// </summary>
		FunctionTable,

		/// <summary>
		/// Select Foo(x, y, z)"
		/// </summary>
		FunctionScalar,

		/// <summary>
		/// Select * from Report_Foo where a=b"
		/// </summary>
		ViewOrTable,

		/// <summary>
		/// Exec Report_Foo a, b, c
		/// </summary>
		StoredProc
	}

	public class ReportSchemaColumn
	{
		public ReportSchemaColumn(Type type, string name)
		{
			this.DataType = type;
			this.ColumnName = name;
		}

		public Type DataType { get; private set; }
		public ZString ColumnName { get; private set; }

		public override string ToString()
		{
			return string.Format("{0} type {1}", ColumnName, DataType.FullName);
		}
	}
}
