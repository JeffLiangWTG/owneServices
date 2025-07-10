using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class TradeDetailTimeline : AutoTradeDetailTimeline
	{
		public TradeDetailTimeline(BusinessObjectFactory factory, OrgTradePeriod prospectPeriod)
			: base(factory)
		{
			this.prospectPeriod = prospectPeriod;
		}

		readonly OrgTradePeriod prospectPeriod;

		public override ZString ActivityID
		{
			get
			{
				var detail = prospectPeriod.TradeDetail;
				return string.Join(",", detail.SalesAssociationPivotCollectionGlobal.Select(x => x.AssociatedEntity.ID));
			}
		}

		public override ZString Status
		{
			get
			{
				var detail = prospectPeriod.TradeDetail;
				return detail.Lookups.TradeLaneStatuses.GetDescriptionFromCode(detail.PA_Status);
			}
		}

		public override ZDateTime Period => prospectPeriod.PAS_Period;

		public override ZBool IsSuperceded => prospectPeriod.PAS_IsSuperseded;

		public override ZBool IsForecast => prospectPeriod.PAS_IsForecast;

		public override ZString Currency => prospectPeriod.PAS_RX_NKCurrency;

		public override ZDecimal Value => prospectPeriod.PAS_EstimatedProfit;

		public override ZBool IsExpired => prospectPeriod.PAS_IsExpired;

		public override ZString ExpiryReason
		{
			get
			{
				if (IsExpired || IsSuperceded)
				{
					var prospectDetail = prospectPeriod.TradeDetail.ProspectDetail;
					return prospectDetail.Lookups.ExpiryReasons.GetDescriptionFromCode(prospectDetail.PAP_ExpiryReason);
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		[ResourceStringData("Enterprise.MarketingManager.Business.TradeDetailTimeline|EstimateType", Caption = "Estimate Type")]
		public ZString EstimateType => IsForecast ? Res.GetString("7dbbaa8d-7645-4e90-8ce5-8b0d8ec5fb58", "Forecast (Static)") : Res.GetString("df74d974-7750-4cca-898d-8afc2ca018b7", "Committed");
	}
}
