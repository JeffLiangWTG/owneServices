using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	internal class OrgTradePeriodAndValuePartitionBuilder
	{
		public OrgTradePeriodAndValuePartitionBuilder(BusinessObjectFactory factory, OrgHeader org, ZDate period, ZDate periodFrom, ZDate periodTo, IEnumerable<OrgTradeDetail> tradeDetails)
		{
			this.factory = factory;
			PartitionPeriod = period;
			Build(org, periodFrom, periodTo, tradeDetails);
		}
		readonly BusinessObjectFactory factory;

		public ZDate PartitionPeriod { get; private set; }
		public ICollection<OrgTradePeriod> TradePeriods { get; private set; }
		public Dictionary<ZGuid, Dictionary<TradePeriodKey, OrgTradePeriod>> TradePeriodsMap { get; private set; }
		public Dictionary<ZGuid, Dictionary<TradeValueBreakdownKey, OrgTradeValue>> TradeValuesMap { get; private set; }
		ICollection<OrgTradePeriod> DuplicateTradePeriodsToDelete { get; set; }
		ICollection<OrgTradeValue> DuplicateTradeValuesToDelete { get; set; }

		void Build(OrgHeader org, ZDate periodFrom, ZDate periodTo, IEnumerable<OrgTradeDetail> tradeDetails)
		{
			DuplicateTradePeriodsToDelete = new List<OrgTradePeriod>(1);
			DuplicateTradeValuesToDelete = new List<OrgTradeValue>(1);

			var periodFilterQuery = new ZQuery(OrgTradePeriodSchema.PAS_IsTraded, ZBool.True);
			periodFilterQuery.AddToFilter(OrgTradePeriodSchema.PAS_OH_Client, org.PK);
			if (!PartitionPeriod.IsEmpty)
			{
				periodFilterQuery.AddToFilter(OrgTradePeriodSchema.PAS_Period, PartitionPeriod);
			}
			else
			{
				periodFilterQuery.AddToFilter(OrgTradePeriodSchema.PAS_Period, SQLComparisonOperator.GreaterThanOrEqualTo, periodFrom);
				periodFilterQuery.AddToFilter(OrgTradePeriodSchema.PAS_Period, SQLComparisonOperator.LessThan, periodTo);
			}

			var periodQuery = new ZDBOnlyQuery(typeof(OrgTradePeriod));
			periodQuery.AddToFilter(periodFilterQuery);

			var detailSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeDetail), OrgTradePeriodSchema.PAS_PA) { AllowTableValuedParameters = true };
			detailSubQuery.AddToFilter(OrgTradeDetailSchema.PA_OW, tradeDetails.Select(x => x.PA_OW).Distinct());
			periodQuery.AddSubQuery(detailSubQuery, JoinCondition.And);

			var periods = factory.Load<OrgTradePeriod>(periodQuery);
			var detailsDictionary = tradeDetails.ToDictionary(x => x.PK);
			TradePeriods = periods.Where(x => detailsDictionary.ContainsKey(x.PAS_PA)).ToList();

			foreach (var tradePeriod in TradePeriods)
			{
				factory.AddFetchHint(OrgTradeValueSchema.PAV_PAS, tradePeriod.PK);
			}

			TradePeriodsMap = new Dictionary<ZGuid, Dictionary<TradePeriodKey, OrgTradePeriod>>(TradePeriods.Count);
			TradeValuesMap = new Dictionary<ZGuid, Dictionary<TradeValueBreakdownKey, OrgTradeValue>>(TradePeriods.Count);

			foreach (var tradePeriodGroup in TradePeriods.GroupBy(x => x.PAS_PA))
			{
				foreach (var tradePeriod in tradePeriodGroup)
				{
					if (!TradePeriodsMap.TryGetValue(tradePeriod.PAS_PA, out Dictionary<TradePeriodKey, OrgTradePeriod> periodMap))
					{
						periodMap = new Dictionary<TradePeriodKey, OrgTradePeriod>(1);
						TradePeriodsMap.Add(tradePeriod.PAS_PA, periodMap);
					}
					var periodKey = TradeLinesValueHelper.CreateTradePeriodKey(tradePeriod);
					if (!periodMap.ContainsKey(periodKey))
					{
						periodMap.Add(periodKey, tradePeriod);

						var tradedValues = factory.Load<OrgTradeValue>(new ZQuery(OrgTradeValueSchema.PAV_PAS, tradePeriod.PK));
						foreach (var tradeValue in tradedValues)
						{
							if (!TradeValuesMap.TryGetValue(tradePeriod.PK, out Dictionary<TradeValueBreakdownKey, OrgTradeValue> valueMap))
							{
								valueMap = new Dictionary<TradeValueBreakdownKey, OrgTradeValue>(1);
								TradeValuesMap.Add(tradePeriod.PK, valueMap);
							}
							var valueKey = TradeLinesValueHelper.CreateTradeValueKey(tradeValue);
							if (!valueMap.ContainsKey(valueKey))
							{
								valueMap.Add(valueKey, tradeValue);
							}
							else
							{
								DuplicateTradeValuesToDelete.Add(tradeValue);
							}
						}
					}
					else
					{
						DuplicateTradePeriodsToDelete.Add(tradePeriod);
					}
				}
			}
		}

		public void DeleteDuplicates()
		{
			foreach (var tradeValue in DuplicateTradeValuesToDelete)
			{
				tradeValue.Delete();
			}

			foreach (var tradePeriod in DuplicateTradePeriodsToDelete)
			{
				foreach (var tradeValue in tradePeriod.TradeValues)
				{
					if (!tradeValue.IsDeleted)
					{
						tradeValue.FetchStrategy.FetchForDelete();
					}
				}
			}

			foreach (var tradePeriod in DuplicateTradePeriodsToDelete)
			{
				tradePeriod.Delete();
			}
		}
	}
}
