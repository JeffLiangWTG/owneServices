using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.ReportTableProviders
{
	[CodeAlive("This is used by reflection in TableProviderFactory.cs.")]
	public class GroupStaffSecurityTableProvider : TableProvider
	{
		public override DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool needsToAddWhereClause, int maximumNumberOfRows)
		{
			DataTable table = null;
			SecurityFilterField filterField = report.FilterCollection["Security Right"] as SecurityFilterField;
			if (filterField != null)
			{
				MultipleSelectionLookup groupsMultipleSelectionLookup = report.FilterCollection["Limit Staff To Those That Belong To All These Groups"] as MultipleSelectionLookup;
				GlbGroupCollection groupsToFilterBy = groupsMultipleSelectionLookup != null ? groupsMultipleSelectionLookup.GetCollection<GlbGroupCollection>() : null;

				filterField.HideDeniedRights = ZBool.True;

				table = new DataTable("GroupStaffSecurity");
				table.Locale = CultureInfo.InvariantCulture;
				foreach (string column in ColumnNames)
				{
					table.Columns.Add(column);
				}

				SecurityIterator<string> iterator = new SecurityIterator<string>(new BusinessObjectFactory(), new GroupStaffSecuritySummaryGenerator(), treatLocalAdministratorCheckPointDifferently: false);
				iterator.Setup(groupsToFilterBy.Cast<GlbGroup>().ToArray(), filterField.FilterContainer.LookupKey);
				var orderByStaff = false;
				if (needsToAddWhereClause)
				{
					orderByStaff = report.SortOrderCollection.SelectedOrder.FieldList == ColumnNames[1];
				}

				PopulateTable(table, iterator.GetSummaries(), filterField, orderByStaff);
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

		void PopulateTable(DataTable table, IEnumerable<ISecuritySummary<string>> summaries, SecurityFilterField filterField, bool orderByStaff)
		{
			if (orderByStaff)
			{
				summaries = summaries.OrderBy(summary => summary.Staff.GS_Code);
			}

			foreach (var summary in summaries)
			{
				if (filterField.HideDeniedRights && string.IsNullOrEmpty(summary.Summary))
				{
					continue;
				}

				DataRow row = table.NewRow();
				row[0] = summary.Checkpoint.DisplayTextPathToSecurityRight;
				row[1] = summary.Staff.GS_Code;
				row[2] = summary.Staff.GS_FullName;
				row[3] = summary.Staff.GS_SystemCreateTimeUtc;
				row[4] = summary.Staff.GS_LastActivityDate;
				row[5] = summary.Summary;
				row[6] = summary.Staff.GS_IsActive;
				row[7] = summary.Staff.GS_IsController;

				table.Rows.Add(row);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		string[] ColumnNames
		{
			get
			{
				return new string[] { "SecurityRight", "Code", "FullName", "SystemCreateTimeUTC", "LastActivityDate", "Groups", "IsActive", "IsController" };
			}
		}
	}
}
