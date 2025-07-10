using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public static class ConditionApplicabilitiesLoader
	{
		public static DataTable GetEmptyConditionSelectionCriteriaTable()
		{
			const string tableName = "ConditionSelectionCriteria";

			var dataTable = new DataTable(tableName);
			dataTable.Locale = CultureInfo.InvariantCulture;

			dataTable.Columns.Add(TvpConditionSelectionCriteria.Columns.CriteriaId, typeof(Guid));
			dataTable.Columns.Add(TvpConditionSelectionCriteria.Columns.TariffPK, typeof(Guid));
			dataTable.Columns.Add(TvpConditionSelectionCriteria.Columns.TradeGroupCountry, typeof(string));
			dataTable.Columns.Add(TvpConditionSelectionCriteria.Columns.DataGrouping, typeof(string));
			dataTable.Columns.Add(TvpConditionSelectionCriteria.Columns.EffectiveDate, typeof(DateTime));
			dataTable.Columns.Add(TvpConditionSelectionCriteria.Columns.IsImport, typeof(bool));
			dataTable.Columns.Add(TvpConditionSelectionCriteria.Columns.IsExport, typeof(bool));
			dataTable.Columns.Add(TvpConditionSelectionCriteria.Columns.ConditionClass, typeof(string));
			dataTable.Columns.Add(TvpConditionSelectionCriteria.Columns.ConditionType, typeof(string));
			dataTable.Columns.Add(TvpConditionSelectionCriteria.Columns.Preference, typeof(string));
			dataTable.Columns.Add(TvpConditionSelectionCriteria.Columns.OrderNumber, typeof(string));
			return dataTable;
		}

		public static void AddNewCriteriaSetRow(DataTable criteriaParameterTable, DataTable secondTradeGroupsDataTable, Guid criteriaId, ZGuid tariffPk, IZZConditionSelectionCriteria selectionCriteria, bool needSecondTradeGroup = true)
		{
			var criteriaRow = criteriaParameterTable.NewRow();
			criteriaRow[TvpConditionSelectionCriteria.Columns.CriteriaId] = criteriaId;
			criteriaRow[TvpConditionSelectionCriteria.Columns.TariffPK] = tariffPk.ToGuid();
			criteriaRow[TvpConditionSelectionCriteria.Columns.TradeGroupCountry] = selectionCriteria.TradeGroupCountry;
			criteriaRow[TvpConditionSelectionCriteria.Columns.DataGrouping] = selectionCriteria.DataGrouping;
			criteriaRow[TvpConditionSelectionCriteria.Columns.Preference] = selectionCriteria.PrimaryPreference;
			criteriaRow[TvpConditionSelectionCriteria.Columns.OrderNumber] = selectionCriteria.ConcessionOrder;
			criteriaRow[TvpConditionSelectionCriteria.Columns.EffectiveDate] = selectionCriteria.ValidEffectiveDate().ToDateTime();
			criteriaRow[TvpConditionSelectionCriteria.Columns.ConditionClass] = selectionCriteria.ConditionClass;
			criteriaRow[TvpConditionSelectionCriteria.Columns.ConditionType] = selectionCriteria.ConditionType;
			criteriaRow[TvpConditionSelectionCriteria.Columns.IsImport] = selectionCriteria.Direction == ConditionChecker.ConditionDirection.Import;
			criteriaRow[TvpConditionSelectionCriteria.Columns.IsExport] = selectionCriteria.Direction == ConditionChecker.ConditionDirection.Export;
			criteriaParameterTable.Rows.Add(criteriaRow);

			if (needSecondTradeGroup)
			{
				AddNewSecondTradeGroupCriteriaSetRow(secondTradeGroupsDataTable, criteriaId, selectionCriteria);
			}
		}

		static void AddNewSecondTradeGroupCriteriaSetRow(DataTable secondTradeGroupsDataTable, Guid criteriaId, IZZConditionSelectionCriteria selectionCriteria)
		{
			if (selectionCriteria.SecondTradeGroups is ISet<ZString> secondTradeGroups)
			{
				foreach (var secondTradeGroup in secondTradeGroups.Where(x => !x.IsEmpty))
				{
					var secondTradeGroupsRow = secondTradeGroupsDataTable.NewRow();
					secondTradeGroupsRow[TvpSecondTradeGroup.Columns.Id] = Guid.NewGuid();
					secondTradeGroupsRow[TvpSecondTradeGroup.Columns.CriteriaId] = criteriaId;
					secondTradeGroupsRow[TvpSecondTradeGroup.Columns.SecondTradeGroup] = secondTradeGroup;
					secondTradeGroupsDataTable.Rows.Add(secondTradeGroupsRow);
				}
			}
		}
	}
}
