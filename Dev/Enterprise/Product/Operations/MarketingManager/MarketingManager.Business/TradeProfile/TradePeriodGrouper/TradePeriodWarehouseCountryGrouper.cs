using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodWarehouseCountryGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
			foreach (var period in tradePeriods)
			{
				factory.AddFetchHint(WhsWarehouseSchema.Constants.TableName, period.TradeDetail.Parent.OW_WW);
			}
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var factory = tradePeriods.First().Factory;

			var warehouseBranchPks = tradePeriods.Where(x => x.TradeDetail.Parent.Warehouse != null).Select(x => x.TradeDetail.Parent.Warehouse.WW_GB_RelatedCompanyBranch);
			var branches = factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.PK, warehouseBranchPks));
			var branchUnlocos = factory.Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, branches.Select(x => x.GB_RL_NKHomePort)));
			var warehouseCountries = factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, branchUnlocos.Select(x => x.RL_RN_NKCountryCode)));

			var warehouseCountryGrouped = tradePeriods.GroupBy(x => GetWarehouseHomePortCountry(x));
			foreach (var grouping in warehouseCountryGrouped)
			{
				var country = grouping.Key;
				var countryCode = country?.RN_Code ?? ZString.Empty;
				var countryDescription = country?.RN_DescMultilingual ?? ZString.Empty;
				var description = GetCountryGroupDescription(countryCode, countryDescription);
				yield return new Grouping(description, grouping);
			}
		}

		static RefCountry GetWarehouseHomePortCountry(OrgTradePeriod period)
		{
			var warehouse = period.TradeDetail.Parent.Warehouse;
			var warehouseHomePort = OrgSales.GetWarehouseHomePort(warehouse);
			return warehouseHomePort?.Country;
		}
	}
}
