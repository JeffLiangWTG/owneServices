using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Universal
{
	public abstract class TariffCriteriaSet<T>
		where T : EnterpriseBusinessObject, ITariffEffectiveDatesRelatedBusinessObject
	{
		protected TariffCriteriaSet(TariffView tariff, IZZApplicabilitySelectionCriteria selectionCriteria)
		{
			this.tariff = Argument.NotNull(tariff, nameof(tariff));
			this.selectionCriteria = Argument.NotNull(selectionCriteria, nameof(selectionCriteria));
			CacheKey = GetCriteriaSetCacheKey(tariff.PK, selectionCriteria);
		}

		internal readonly Guid CriteriaId = Guid.NewGuid();
		readonly TariffView tariff;
		readonly IZZApplicabilitySelectionCriteria selectionCriteria;

		public string CacheKey { get; }

		public string GetCriteriaSetCacheKey(ZGuid tariffPk, IZZApplicabilitySelectionCriteria criteria)
			=> FormattableString.Invariant($"{QualifiedName}_{tariffPk}_{criteria.TradeGroupCountry}_{criteria.FlatSecondTradeGroups()}_{criteria.DataGrouping}_{criteria.ValidEffectiveDate().ToISO8601String()}_{criteria.PrimaryPreference}_{criteria.FlatAdditionalCodes()}_{criteria.ConcessionOrder}{ExtraKeyString(criteria)}");

		public void AddApplicableTableValueParameterRows(DataTable criteriaParameterTable, DataTable additionalCodesParameterTable, DataTable secondTradeGroupsDataTable)
		{
			AddNewCriteriaSetRow(criteriaParameterTable, additionalCodesParameterTable, secondTradeGroupsDataTable, CriteriaId, tariff.PK, selectionCriteria);

			if (tariff.IsTariffNationalCode)
			{
				AddNewCriteriaSetRow(criteriaParameterTable, additionalCodesParameterTable, secondTradeGroupsDataTable, CriteriaId, tariff.ZZ1_ZZ1_Tariff, selectionCriteria);
			}
		}

		public virtual DataRow AddNewCriteriaSetRow(DataTable criteriaParameterTable, DataTable additionalCodesParameterTable, DataTable secondTradeGroupsDataTable, Guid criteriaId, ZGuid tariffPk, IZZApplicabilitySelectionCriteria selectionCriteria)
		{
			var criteriaRow = criteriaParameterTable.NewRow();

			criteriaRow[TvpSelectionCriteria.Columns.CriteriaId] = criteriaId;
			criteriaRow[TvpSelectionCriteria.Columns.TariffPK] = tariffPk.ToGuid();
			criteriaRow[TvpSelectionCriteria.Columns.TradeGroupCountry] = selectionCriteria.TradeGroupCountry;
			criteriaRow[TvpSelectionCriteria.Columns.DataGrouping] = selectionCriteria.DataGrouping;
			criteriaRow[TvpSelectionCriteria.Columns.EffectiveDate] = selectionCriteria.ValidEffectiveDate().ToDateTime();
			criteriaRow[TvpSelectionCriteria.Columns.Preference] = selectionCriteria.PrimaryPreference;
			criteriaRow[TvpSelectionCriteria.Columns.OrderNumber] = selectionCriteria.ConcessionOrder;

			criteriaParameterTable.Rows.Add(criteriaRow);

			var additionalCodes = selectionCriteria.AdditionalCodes;
			if (additionalCodes != null)
			{
				foreach (var additionalCode in additionalCodes.Where(x => !x.IsEmpty))
				{
					var existDataRows = additionalCodesParameterTable.Select(
						$"{TvpAdditionalCodes.Columns.CriteriaId} = '{criteriaId}' AND {TvpAdditionalCodes.Columns.AdditionalCode} = '{additionalCode}'");
					if (existDataRows.Length == 0)
					{
						var additionalCodesRow = additionalCodesParameterTable.NewRow();
						additionalCodesRow[TvpAdditionalCodes.Columns.Id] = Guid.NewGuid();
						additionalCodesRow[TvpAdditionalCodes.Columns.CriteriaId] = criteriaId;
						additionalCodesRow[TvpAdditionalCodes.Columns.AdditionalCode] = additionalCode;
						additionalCodesParameterTable.Rows.Add(additionalCodesRow);
					}
				}
			}

			var secondTradeGroups = selectionCriteria.SecondTradeGroups;
			if (secondTradeGroups != null)
			{
				foreach (var secondTradeGroup in secondTradeGroups.Where(x => !x.IsEmpty))
				{
					var existDataRows = secondTradeGroupsDataTable.Select(
						$"{TvpSecondTradeGroup.Columns.CriteriaId} = '{criteriaId}' AND {TvpSecondTradeGroup.Columns.SecondTradeGroup} = '{secondTradeGroup}'");
					if (existDataRows.Length == 0)
					{
						var secondTradeGroupsRow = secondTradeGroupsDataTable.NewRow();
						secondTradeGroupsRow[TvpSecondTradeGroup.Columns.Id] = Guid.NewGuid();
						secondTradeGroupsRow[TvpSecondTradeGroup.Columns.CriteriaId] = criteriaId;
						secondTradeGroupsRow[TvpSecondTradeGroup.Columns.SecondTradeGroup] = secondTradeGroup;
						secondTradeGroupsDataTable.Rows.Add(secondTradeGroupsRow);
					}
				}
			}

			return criteriaRow;
		}

		protected abstract string QualifiedName { get; }

		protected virtual string ExtraKeyString(IZZApplicabilitySelectionCriteria criteria) => ZString.Empty;
	}
}
