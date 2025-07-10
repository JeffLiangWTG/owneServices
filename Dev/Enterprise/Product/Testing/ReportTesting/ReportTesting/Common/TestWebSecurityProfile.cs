using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting;
using Enterprise.MasterFiles.Business;
using FlexCel.XlsAdapter;
using NUnit.Framework;

namespace Enterprise.ReportTesting.Common
{
	[TemplateName("Web Security Profile")]
	public class TestWebSecurityProfile : TemplateTestCase
	{
		[SnailTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHasAllColumns()
		{
			var excelColumns = GetColumns();
			var excelColumnsSql = GetSQLForColumns();

			var rightsFromSql = new List<string>();

			var sql = "select * from Report_WebSecurityProfile('', NULL, NULL, '')";
			var command = Db.Connection.Command(sql); // This test requires direct query to DB
			using (var reader = command.ExecuteReader())
			{
				for (int i = 0; i < reader.FieldCount; ++i)
				{
					string fieldName = reader.GetName(i);
					if (fieldName.ToCharArray()[2] != '_')
					{
						rightsFromSql.Add(fieldName);
					}
				}
			}

			var rightsFromList = WebSecurityRightsList.New().Select(wsr => wsr.Description).Where(x => x != null).Select(x => x.Replace("Web ", "")).ToList();

			const string message =
				@"The Web Security Profile Report doesnt seem to have all of the Web Security Rights Listed.
Only in Report:
	Length: {0}
	Values: {1}

Only in List:
	Length: {2}
	Values: {3}

Please add the missing {4} web security rights.
";

			var onlyInReport = new List<String>();
			foreach (string securityRight in excelColumns)
			{
				if (!rightsFromList.Contains(securityRight))
				{
					onlyInReport.Add(securityRight);
				}
			}

			var onlyInCW1 = new List<String>();
			foreach (string securityRight in rightsFromList)
			{
				if (!excelColumns.Contains(securityRight))
				{
					onlyInCW1.Add(securityRight);
				}
			}

			var delta = onlyInCW1.Count;
			AssertEquals(String.Format(message, onlyInReport.Count, FormatListForDisplay(onlyInReport), onlyInCW1.Count, FormatListForDisplay(onlyInCW1), delta), 0, delta);

			var onlyInReportSql = new List<String>();
			foreach (string securityRight in excelColumnsSql)
			{
				if (!rightsFromSql.Contains(securityRight))
				{
					onlyInReportSql.Add(securityRight);
				}
			}

			var onlyInSql = new List<String>();
			foreach (string securityRight in rightsFromSql)
			{
				if (!excelColumnsSql.Contains(securityRight))
				{
					onlyInSql.Add(securityRight);
				}
			}

			delta = onlyInReportSql.Count;
			AssertEquals(String.Format(message, onlyInReportSql.Count, FormatListForDisplay(onlyInReportSql), onlyInSql.Count, FormatListForDisplay(onlyInSql), delta), 0, delta);
		}

		[SnailTest]
		public void TestRunReport_WhenUserSelectExcessiveNumberOfOrganisations()
		{
			// Arrange
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var orgFilter = Report
				.FilterCollection
				.Select(filter => filter)
				.OfType<MultipleSelectionLookup>()
				.Single(filter => filter.DisplayName == "Organizations");

			var orgHeaderProvider = new DummyOrgHeaderCollectionProvider();
			orgHeaderProvider.FillWithDummyOrgHeaders("DMY_OH_", 250); // DMY_OH_ is not Schema, it is prefix for OrgHeader Code
			((BusinessObjectCollection)orgHeaderProvider.Collection).Load();
			orgFilter.SetCollectionProvider(orgHeaderProvider);

			using (var stream = new MemoryStream())
			using (var excelInterface = new DocumentEngine.FlexCelInterface.ExcelInterface())
			using (Report.SuspendFilterValidationCheckingForTesting())
			{
				// Act
				Report.Save(stream);

				// Assert
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);
				var worksheet = excelInterface.WorkSheets[0];
				var orgMatch = Regex.Match(worksheet.ToString(), @"Organization\: ((?<value>DMY_OH_\d+)(?:, )?)+", RegexOptions.Singleline);

				AssertEquals("Expecting correct number of organisations in Excel header", 250, orgMatch.Groups["value"].Captures.Count);
			}
		}

		ICollection<string> GetColumns()
		{
			var file = new XlsFile(BaseSourcePath + TemplateLocation);

			return Enumerable.Range(7, file.GetColCount(1))
				.Select(col => file.GetCellValue(16, col) as string)
				.Where(s => !String.IsNullOrWhiteSpace(s))
				.Select(x => x.Replace("Web ", "").Replace("<CustomisedColumn(", "").Replace(")>", ""))
				.ToList();
		}

		ICollection<string> GetSQLForColumns()
		{
			var file = new XlsFile(BaseSourcePath + TemplateLocation);

			return Enumerable.Range(7, file.GetColCount(1))
				.Select(col => file.GetCellValue(18, col) as string)
				.Where(s => !String.IsNullOrWhiteSpace(s))
				.Select(col => col.Split(new char[] { '.', '>' })[1]) //We want the code, which is between . and > like <ReportData.WebBookingAddEdit>
				.Select(x => x.Replace("Web ", ""))
				.ToList();
		}

		string FormatListForDisplay(IEnumerable<string> strings)
		{
			var strippedStrings = strings.Select(s => Regex.Replace(s, "\r|\n", "")).OrderBy(s => s);
			return '"' + String.Join("\", \"", strippedStrings);
		}

		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "WebSecurityProfile" };
		}
	}
}
