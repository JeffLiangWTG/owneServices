using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using Microsoft.Playwright;
using NUnit.Framework;

namespace ReferenceDataUpdateService.Web.Test.WebUI.SearchForm
{
	[TestFixture]
	public class PortalSearchFormTestEngine : PlaywrightTest
	{
		private string _stagingDbConnectionString;
		private string _safeDbConnectionString;
		private IDbConnection _stagingDbConnection;
		private IDbConnection _safeDbConnection;

		[TestCaseSource(nameof(GetTestCases))]
		public async Task RunTest(Type testCaseType)
		{
			var testCase = (IPortalSearchFormTest)Activator.CreateInstance(testCaseType);
			using (var stagingCommand = _stagingDbConnection.CreateCommand())
			using (var safeCommand = _safeDbConnection.CreateCommand())
			{
				testCase.PrepareData(stagingCommand, safeCommand);
			}

			await Page.GotoAsync($"http://localhost:19488{testCase.GetPageUrl().ToString()}");

			var filters = testCase.GetFilters();
			foreach (var filter in filters)
			{
				var filterSelect = Page.GetByRole(AriaRole.Combobox,
					new PageGetByRoleOptions { Name = $"{filter.Name} Filter Operation" });
				await filterSelect.SelectOptionAsync(new SelectOptionValue { Label = filter.Operation.ToString() });

				var filterInput = Page.GetByRole(AriaRole.Textbox,
					new PageGetByRoleOptions() { Name = $"{filter.Name} Filter Value" });
				await filterInput.FillAsync(filter.Data);
			}

			var findButton = Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "Find" });
			await findButton.ClickAsync();

			try
			{
				// This is IMPORTANT, DON'T delete it.
				// This is used to verify the search operation is completed and has rendered the results.
				// It waits for the result count text to have non-empty content
				// which occurs if the table has 0 or more rows in the results table.
				await Page.WaitForFunctionAsync("element => element && element.offsetParent !== null && element.textContent.trim().length > 0",
					await Page.Locator("div.col.m-auto span.text-info").ElementHandleAsync(),
					new PageWaitForFunctionOptions { Timeout = 10000 }
				);
			}
			catch (TimeoutException)
			{
				Assert.Fail("Timeout occurred while waiting for the result count text to have non-empty content.");
			}

			var actual = await BuildResultTableData();
			var expected = testCase.GetExpectedResult();
			Assert.That(actual, Is.EquivalentTo(expected));
		}

		/// <summary>
		/// Build the data from the result table on the UI
		/// Each item represents a row in the table on the UI
		/// For each row, the order of the columns is the same with the order of the columns in the table on the UI
		/// For text and number columns, the value will be a string representing the text of the cell.
		/// For boolean columns (with a checkbox), the value will be the ToString() value of the bool representing the checked status of the checkbox.
		/// For links, the value will be a string representing the href value.
		/// </summary>
		/// <returns></returns>
		private async Task<IEnumerable<IEnumerable<string>>> BuildResultTableData()
		{
			var htmlTable = Page.Locator("table#result-table");

			var htmlHeaders = htmlTable.Locator("thead tr th");
			var columnCount = await htmlHeaders.CountAsync();

			var htmlRows = await Page.Locator("table#result-table tbody tr").AllAsync();
			var rowCount = htmlRows.Count;

			var res = new string[rowCount][];
			await Task.WhenAll(htmlRows.Select(async (htmlRow, rowIndex) =>
			{
				var rowData = new string[columnCount];
				var htmlCells = await htmlRow.Locator("td").AllAsync();
				await Task.WhenAll(htmlCells.Select(async (htmlCell, index) =>
				{
					var checkbox = htmlCell.GetByRole(AriaRole.Checkbox);
					var containsCheckbox = await checkbox.CountAsync() > 0;
					if (containsCheckbox)
					{
						var checkboxValue = await checkbox.IsCheckedAsync();
						rowData[index] = checkboxValue.ToString();
						return;
					}

					var link = htmlCell.GetByRole(AriaRole.Link);
					var containsLink = await link.CountAsync() > 0;
					if (containsLink)
					{
						var linkValue = await link.GetAttributeAsync("href");
						rowData[index] = linkValue ?? string.Empty;
						return;
					}

					rowData[index] = await htmlCell.InnerTextAsync();
				}));

				res[rowIndex] = rowData;
			}));

			return res;
		}

		public override Task SetUp()
		{
			var stagingDbName = CreateDatabaseAttribute.GetDbName("9879243546C341CABECCEFCB10F381FA");
			var safeDbName = CreateDatabaseAttribute.GetDbName("42E84E5D04B342AF860AB398AD2D657E");
			_stagingDbConnectionString = TestConnectionString.GetAdmin(stagingDbName);
			_safeDbConnectionString = TestConnectionString.GetAdmin(safeDbName);

			_stagingDbConnection = new SqlConnection(_stagingDbConnectionString);
			_stagingDbConnection.Open();

			_safeDbConnection = new SqlConnection(_safeDbConnectionString);
			_safeDbConnection.Open();

			return base.SetUp();
		}

		public override Task TearDown()
		{
			try
			{
				CleanUpData();
			}
			finally
			{
				_stagingDbConnection?.Dispose();
				_safeDbConnection?.Dispose();
			}

			return base.TearDown();
		}

		private void CleanUpData()
		{
			var sql = @"
EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all'

DECLARE @TableName sysname
DECLARE @SqlStatement NVARCHAR(MAX)
DECLARE TableCursor CURSOR FOR
SELECT name
FROM sys.tables where name not like '%History'
OPEN TableCursor
FETCH NEXT FROM TableCursor INTO @TableName
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SqlStatement = 'delete from ' + QUOTENAME(@TableName)
    EXEC sp_executesql @SqlStatement
    FETCH NEXT FROM TableCursor INTO @TableName
END
CLOSE TableCursor
DEALLOCATE TableCursor

EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all'
";
			using (var stagingCommand = _stagingDbConnection.CreateCommand())
			{
				stagingCommand.CommandText = sql;
				stagingCommand.ExecuteNonQuery();
			}

			using (var safeCommand = _safeDbConnection.CreateCommand())
			{
				safeCommand.CommandText = sql;
				safeCommand.ExecuteNonQuery();
			}
		}

		public static IEnumerable<Type> GetTestCases()
		{
			var interfaceType = typeof(IPortalSearchFormTest);
			return Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => interfaceType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
		}
	}
}
