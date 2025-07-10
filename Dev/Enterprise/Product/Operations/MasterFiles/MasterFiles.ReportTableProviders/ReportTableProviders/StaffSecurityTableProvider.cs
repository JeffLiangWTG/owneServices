using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.ReportTableProviders
{
	[CodeAlive("This is used by reflection in TableProviderFactory.cs.")]
	public class StaffSecurityTableProvider : TableProvider
	{
		public override DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool needsToAddWhereClause, int maximumNumberOfRows)
		{
			DataTable table = null;
			SecurityFilterField filterField = report.FilterCollection["Security Right"] as SecurityFilterField;
			if (filterField != null)
			{
				MultipleSelectionLookup groupsMultipleSelectionLookup = report.FilterCollection["Limit Staff To Those That Belong To All These Groups"] as MultipleSelectionLookup;
				GlbGroupCollection groupsToFilterBy = groupsMultipleSelectionLookup != null ? groupsMultipleSelectionLookup.GetCollection<GlbGroupCollection>() : null;

				FilterField hideDeniedControl = report.FilterCollection["Hide Denied"];
				if (hideDeniedControl != null)
				{
					string hide = hideDeniedControl.ValueAsObject.ToString();
					if (String.Equals("Y", hide, StringComparison.OrdinalIgnoreCase))
					{
						filterField.HideDeniedRights = ZBool.True;
					}
				}

				GroupBy groupBy = report.GroupByCollection.SelectedGroupBy;
				if (groupBy != null)
				{
					if (String.Equals(groupBy.DisplayName, (NoResString)"Staff", StringComparison.OrdinalIgnoreCase))
					{
						filterField.GroupByStaff = ZBool.True;
						filterField.GroupBySummary = ZBool.False;
					}
				}

				table = new DataTable("StaffSecurity");
				foreach (string column in ColumnNames)
				{
					table.Columns.Add(column);
				}

				SecurityIterator<string> iterator = new SecurityIterator<string>(new BusinessObjectFactory(), new StringSecuritySummaryGenerator(), treatLocalAdministratorCheckPointDifferently: true);
				iterator.Setup(groupsToFilterBy.Cast<GlbGroup>().ToArray(), filterField.FilterContainer.LookupKey);
				PopulateTable(table, iterator.GetSummaries(), filterField);
			}
			else
			{
				throw new DataProviderException("The report filter is missing the Security Right filter field.");
			}

			return table;
		}

		public override DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool needsToAddWhereClause)
		{
			return GetDataTable(tableName, dataSourceString, report, needsToAddWhereClause, -1);
		}

		void PopulateTable(DataTable table, IEnumerable<ISecuritySummary<string>> summaries, SecurityFilterField filterField)
		{
			ISecurityCheckpoint currentCheckpoint = null;
			List<DataRow> buf = new List<DataRow>();
			foreach (var summary in summaries)
			{
				if (filterField.HideDeniedRights && summary.Summary.Equals(StringSecuritySummaryGenerator.Denied, StringComparison.Ordinal))
				{
					continue;
				}

				if (summary.Checkpoint != currentCheckpoint)
				{
					FlushBuf(table, buf);
					currentCheckpoint = summary.Checkpoint;
				}

				DataRow row = table.NewRow();
				row[0] = summary.Checkpoint.DisplayTextPathToSecurityRight;
				row[1] = summary.Staff.GS_FullName;
				row[2] = summary.Summary;

				if (filterField.GroupByStaff)
				{
					table.Rows.Add(row);
				}
				else
				{
					buf.Add(row);
				}
			}
			FlushBuf(table, buf);
		}

		void FlushBuf(DataTable table, List<DataRow> buf)
		{
			if (buf.Count > 0)
			{
				var query =
					from row in buf
					group row[1].ToString() by new { SecurityRight = row[0].ToString(), Summary = row[2].ToString() } into rowGroup
					orderby rowGroup.Key.Summary
					select new { SecurityRight = rowGroup.Key.SecurityRight, Summary = rowGroup.Key.Summary, Staffs = rowGroup.ToArray() };
				foreach (var rowData in query)
				{
					bool firstRow = true;
					StringBuilder sb = new StringBuilder();
					for (int i = 0; i < rowData.Staffs.Length; i++)
					{
						if (sb.Length > 0)
						{
							sb.Append(" ");
						}

						sb.Append(rowData.Staffs[i]);
						if (i < rowData.Staffs.Length - 1)
						{
							sb.Append(",");
						}

						if (sb.Length > 750 || i == rowData.Staffs.Length - 1)
						{
							DataRow row = table.NewRow();
							row[0] = firstRow ? rowData.SecurityRight : String.Empty;
							row[1] = sb.ToString();
							row[2] = firstRow ? rowData.Summary : String.Empty;
							table.Rows.Add(row);

							sb = new StringBuilder();
							if (firstRow)
							{
								firstRow = false;
							}
						}
					}
				}
				buf.Clear();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		string[] ColumnNames
		{
			get
			{
				return new string[] { "SecurityRight", "StaffName", "Summary" };
			}
		}
	}
}
