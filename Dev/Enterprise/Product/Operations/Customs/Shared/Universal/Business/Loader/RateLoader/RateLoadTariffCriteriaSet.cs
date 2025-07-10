using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class RateLoadTariffCriteriaSet
	{
		public RateLoadTariffCriteriaSet(TariffView tariff, IZZRateSelectionCriteria selectionCriteria)
		{
			this.tariff = Argument.NotNull(tariff, nameof(tariff));
			this.selectionCriteria = Argument.NotNull(selectionCriteria, nameof(selectionCriteria));
			CacheKey = GetCriteriaSetCacheKey(tariff.PK, selectionCriteria);
		}

		internal readonly Guid CriteriaId = Guid.NewGuid();
		readonly TariffView tariff;
		readonly IZZRateSelectionCriteria selectionCriteria;

		public string CacheKey { get; }

		public static string GetCriteriaSetCacheKey(ZGuid tariffPk, IZZRateSelectionCriteria criteria)
			=> FormattableString.Invariant($"ApplicableRates_{tariffPk}_{criteria.TradeGroupCountry}_{criteria.FlatSecondTradeGroups()}_{criteria.DataGrouping}_{criteria.ValidEffectiveDate().ToISO8601String()}_{criteria.PrimaryPreference}_{criteria.FlatAdditionalCodes()}_{criteria.ConcessionOrder}_{criteria.RateType}_{criteria.RateCode}");

		public bool IsCached() => tariff.Factory.TryGetValueFromCacheOnly<IEnumerable<RateView>>(CacheKey, out _);

		public void AddApplicableTableValueParameterRows(DataTable criteriaParameterTable, DataTable secondTradeGroupsDataTable)
		{
			ApplicableRateLoader.AddNewCriteriaSetRow(criteriaParameterTable, secondTradeGroupsDataTable, CriteriaId, tariff.PK, selectionCriteria);

			if (tariff.IsTariffNationalCode)
			{
				ApplicableRateLoader.AddNewCriteriaSetRow(criteriaParameterTable, secondTradeGroupsDataTable, CriteriaId, tariff.ZZ1_ZZ1_Tariff, selectionCriteria);
			}
		}
	}
}
