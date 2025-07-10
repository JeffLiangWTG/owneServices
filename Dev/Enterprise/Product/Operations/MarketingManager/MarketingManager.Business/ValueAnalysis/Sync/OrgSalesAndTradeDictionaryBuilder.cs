using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	internal class OrgSalesAndTradeDictionaryBuilder
	{
		public OrgSalesAndTradeDictionaryBuilder(BusinessObjectFactory factory, OrgSalesCollection salesCollection)
		{
			this.factory = factory;
			Build(salesCollection);
		}
		readonly BusinessObjectFactory factory;

		public ICollection<OrgTradeDetail> TradeDetails { get; private set; }
		public Dictionary<TradeLaneKey, OrgSales> TradeLanesMap { get; private set; }
		public Dictionary<ZGuid, Dictionary<TradeDetailKey, OrgTradeDetail>> TradeDetailsMap { get; private set; }
		ICollection<OrgSales> NewCreatedOrLoadedTradeLanes { get; set; }
		ICollection<OrgSales> DuplicateTradeLanesToDelete { get; set; }
		ICollection<OrgTradeDetail> DuplicateTradeDetailsToDelete { get; set; }

		void Build(OrgSalesCollection salesCollection)
		{
			DuplicateTradeLanesToDelete = new List<OrgSales>(1);
			DuplicateTradeDetailsToDelete = new List<OrgTradeDetail>(1);

			var existingActualTradeLanes = salesCollection.Cast<OrgSales>().Where(x => x.IsActual).ToList();

			var detailQuery = new ZQuery() { AllowTableValuedParameters = true };
			detailQuery.AddToFilter(OrgTradeDetailSchema.PA_OW, existingActualTradeLanes.Select(x => x.PK)); // Need to build the query AFTER we've set AllowTableValuedParameters to true.
			var details = factory.Load<OrgTradeDetail>(detailQuery);
			TradeDetails = details.ToList();

			TradeLanesMap = new Dictionary<TradeLaneKey, OrgSales>(existingActualTradeLanes.Count);
			TradeDetailsMap = new Dictionary<ZGuid, Dictionary<TradeDetailKey, OrgTradeDetail>>(details.Length);
			NewCreatedOrLoadedTradeLanes = new List<OrgSales>(1);

			Populate(existingActualTradeLanes, details);
		}

		public void Append(OrgSales sales)
		{
			NewCreatedOrLoadedTradeLanes.Add(sales);
			var tradeDetails = sales.Factory.Load<OrgTradeDetail>(new ZQuery(OrgTradeDetailSchema.PA_OW, sales.PK));
			foreach (var detail in tradeDetails)
			{
				TradeDetails.Add(detail);
			}
			Populate(TradeLinesValueHelper.AsEnumerable(sales), tradeDetails);
		}

		public void Update()
		{
			var newCreatedTradeLanePks = NewCreatedOrLoadedTradeLanes.Select(x => x.PK);

			var tradeLanesQuery = new ZQuery(OrgSalesSchema.PK, newCreatedTradeLanePks);
			var newTradeLanes = factory.Load<OrgSales>(tradeLanesQuery);

			var tradeDetailsQuery = new ZQuery(OrgTradeDetailSchema.PA_OW, newCreatedTradeLanePks);
			var newTradeDetails = factory.Load<OrgTradeDetail>(tradeDetailsQuery);

			//Remove new OrgSales and OrgTradeDetail references as they are created in partition factory
			foreach (var sales in NewCreatedOrLoadedTradeLanes)
			{
				var salesKey = TradeLinesValueHelper.CreateActualTradeLaneKey(sales);
				TradeLanesMap.Remove(salesKey);
				TradeDetailsMap.Remove(sales.PK);
			}

			Populate(newTradeLanes, newTradeDetails);

			NewCreatedOrLoadedTradeLanes.Clear();
		}

		void Populate(IEnumerable<OrgSales> tradeLanes, IEnumerable<OrgTradeDetail> tradeDetails)
		{
			foreach (var sales in tradeLanes)
			{
				var salesKey = TradeLinesValueHelper.CreateActualTradeLaneKey(sales);

				if (!TradeLanesMap.ContainsKey(salesKey))
				{
					TradeLanesMap.Add(salesKey, sales);

					foreach (var tradeDetail in tradeDetails.Where(x => x.PA_OW == sales.PK))
					{
						if (!TradeDetailsMap.TryGetValue(sales.PK, out Dictionary<TradeDetailKey, OrgTradeDetail> detailMap))
						{
							detailMap = new Dictionary<TradeDetailKey, OrgTradeDetail>(1);
							TradeDetailsMap.Add(sales.PK, detailMap);
						}
						var detailKey = TradeLinesValueHelper.CreateTradeDetailKey(tradeDetail);

						if (!detailMap.ContainsKey(detailKey))
						{
							detailMap.Add(detailKey, tradeDetail);
						}
						else
						{
							DuplicateTradeDetailsToDelete.Add(tradeDetail);
						}
					}
				}
				else
				{
					DuplicateTradeLanesToDelete.Add(sales);
				}
			}
		}

		public void DeleteDuplicates()
		{
			foreach (var tradeDetail in DuplicateTradeDetailsToDelete)
			{
				tradeDetail.Delete();
			}

			foreach (var sales in DuplicateTradeLanesToDelete)
			{
				foreach (var tradeDetail in sales.TradeDetails)
				{
					if (!tradeDetail.IsDeleted)
					{
						tradeDetail.FetchStrategy.FetchForDelete();
					}
				}
			}

			foreach (var sales in DuplicateTradeLanesToDelete)
			{
				sales.Delete();
			}
		}
	}
}
