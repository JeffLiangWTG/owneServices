using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class TariffAdditionalCodeLoadTariffCriteriaSet
	{
		public TariffAdditionalCodeLoadTariffCriteriaSet(TariffView tariff, ITariffAdditionalCodeSelectionCriteria selectionCriteria)
		{
			this.tariff = Argument.NotNull(tariff, nameof(tariff));
			this.selectionCriteria = Argument.NotNull(selectionCriteria, nameof(selectionCriteria));
			CacheKey = GetCriteriaSetCacheKey(tariff.PK, selectionCriteria);
		}

		internal readonly Guid CriteriaId = Guid.NewGuid();
		readonly TariffView tariff;
		readonly ITariffAdditionalCodeSelectionCriteria selectionCriteria;

		public string CacheKey { get; }

		public static string GetCriteriaSetCacheKey(ZGuid tariffPk, ITariffAdditionalCodeSelectionCriteria criteria)
			=> FormattableString.Invariant($"TariffAdditionalCodes_{tariffPk}_{criteria.Category}_{criteria.ValidEffectiveDate().ToISO8601String()}_{criteria.TradeGroupCountry}_{criteria.DataGrouping}");

		public bool IsCached() => tariff.Factory.TryGetValueFromCacheOnly<IEnumerable<TariffAdditionalCodeView>>(CacheKey, out _);

		public void AddApplicableTableValueParameterRows(DataTable criteriaParameterTable)
		{
			ApplicableTariffAdditionalCodeLoader.AddNewCriteriaSetRow(criteriaParameterTable, CriteriaId, tariff.PK, selectionCriteria);

			if (tariff.IsTariffNationalCode)
			{
				ApplicableTariffAdditionalCodeLoader.AddNewCriteriaSetRow(criteriaParameterTable, CriteriaId, tariff.ZZ1_ZZ1_Tariff, selectionCriteria);
			}
		}
	}
}
