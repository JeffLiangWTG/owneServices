using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class ApplicableConditionLoader : BaseLoader<RefCusCondition>
	{
		public ApplicableConditionLoader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override DataTable GetEmptyCriteriaTable()
		{
			const string tableName = "ConditionCriteriaSets";

			var criteriaTable = new DataTable(tableName);
			criteriaTable.Locale = CultureInfo.InvariantCulture;

			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.CriteriaId, typeof(Guid));
			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.TariffPK, typeof(Guid));
			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.TradeGroupCountry, typeof(string));
			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.DataGrouping, typeof(string));
			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.EffectiveDate, typeof(DateTime));
			criteriaTable.Columns.Add(TvpConditionSelectionCriteria.Columns.IsImport, typeof(bool));
			criteriaTable.Columns.Add(TvpConditionSelectionCriteria.Columns.IsExport, typeof(bool));
			criteriaTable.Columns.Add(TvpConditionSelectionCriteria.Columns.ConditionClass, typeof(string));
			criteriaTable.Columns.Add(TvpConditionSelectionCriteria.Columns.ConditionType, typeof(string));
			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.Preference, typeof(string));
			criteriaTable.Columns.Add(TvpSelectionCriteria.Columns.OrderNumber, typeof(string));

			return criteriaTable;
		}

		protected override IEnumerable<(Guid CriteriaId, Guid dataPk)> LoadDataPksFromDatabase(DataTable criteriaDataTable, DataTable additionalCodesDataTable, DataTable secondTradeGroupsDataTable)
		{
			var criteriaIdAndPks = new List<(Guid CriteriaId, Guid ConditionPk)>();
			var sql = FormattableString.Invariant($"EXEC {GetMultiCriteriaSetConditionsProcedure.QualifiedName} {GetMultiCriteriaSetConditionsProcedure.Parameters.CriteriaTvp}, {GetMultiCriteriaSetConditionsProcedure.Parameters.AdditionalCodesTvp}, {GetMultiCriteriaSetConditionsProcedure.Parameters.SecondTradeGroupTvp}");

			using (var command = ((IDbConnected)factory).Connection.Command(sql))
			{
				command.AddTableValuedParameter(GetMultiCriteriaSetConditionsProcedure.Parameters.CriteriaTvp, TvpConditionSelectionCriteria.QualifiedName, criteriaDataTable);
				command.AddTableValuedParameter(GetMultiCriteriaSetConditionsProcedure.Parameters.AdditionalCodesTvp, TvpAdditionalCodes.QualifiedName, additionalCodesDataTable);
				command.AddTableValuedParameter(GetMultiCriteriaSetConditionsProcedure.Parameters.SecondTradeGroupTvp, TvpSecondTradeGroup.QualifiedName, secondTradeGroupsDataTable);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var criteriaId = (Guid)reader[GetMultiCriteriaSetConditionsProcedure.Columns.CriteriaId];
						var conditionPk = (Guid)reader[GetMultiCriteriaSetConditionsProcedure.Columns.DataPk];
						criteriaIdAndPks.Add((criteriaId, conditionPk));
					}
				}
			}

			return criteriaIdAndPks;
		}

		protected override SchemaColumn DataPkSchemaColumn => RefCusConditionSchema.PK;
	}
}
