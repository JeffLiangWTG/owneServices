using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradeDetailValueSuperceder
	{
		public OrgTradeDetailValueSuperceder(OrgTradeDetail tradeDetail, ZDate supercedingFromDate)
		{
			this.tradeDetail = tradeDetail;
			this.supercedingFromDate = supercedingFromDate;
		}
		readonly OrgTradeDetail tradeDetail;
		readonly ZDate supercedingFromDate;

		BusinessObjectFactory Factory => tradeDetail.Factory;

		public void SupercedeOverlappedPeriods()
		{
			if (!CanSupercede)
			{
				return;
			}

			var periodQuery = new ZDBOnlyQuery(typeof(OrgTradePeriod));
			periodQuery.AddToFilter(GetTradePeriodQuery());

			var detailSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeDetail), OrgTradePeriodSchema.PAS_PA);
			detailSubQuery.AddToFilter(GetTradeDetailQuery());

			periodQuery.AddSubQuery(detailSubQuery, JoinCondition.And);

			var periodsToSupercede = Factory.Load<OrgTradePeriod>(periodQuery);
			foreach (var periodsGroup in periodsToSupercede.GroupBy(x => x.PAS_PA))
			{
				var tradeDetailToSupercede = Factory.Load<OrgTradeDetail>(periodsGroup.Key);
				tradeDetailToSupercede.ProspectDetail.PAP_ExpiryDate = supercedingFromDate;
				tradeDetailToSupercede.ProspectDetail.PAP_ExpiryReason = OrgTradeProspectExpiryReasonList.Codes.Superseded;
				foreach (var period in periodsGroup)
				{
					period.PAS_IsSuperseded = true;
				}
			}
		}

		public ZString SupercedingWarningMessage
		{
			get
			{
				var builder = new ZStringBuilder();
				var tradeDetailsToSupercede = GetSupercedingTradeDetails();
				var tradeLane = tradeDetail.Parent;

				foreach (var tradeDetailToSupercede in tradeDetailsToSupercede)
				{
					foreach (var entityPivot in tradeDetailToSupercede.SalesAssociationPivotCollectionGlobal)
					{
						if (entityPivot.AssociatedEntity is OrgOpportunity)
						{
							var message = Res.GetString("b8f21838-03fb-4cdd-9e21-f5d9d27485de", "{0} - {1} {2} {3} {4} {5} (Superseded from {6})",
									entityPivot.AssociatedEntity.ID,
									tradeLane.Product.MP_Name,
									tradeLane.OriginCode,
									tradeLane.DestinationCode,
									tradeDetail.PA_TradeMode,
									tradeDetail.PA_TradeType,
									supercedingFromDate.ToShortDateString()
								);
							builder.AppendLine("    " + message);
						}
					}
				}

				return builder.ToString();
			}
		}

		public IEnumerable<OrgTradeDetail> GetSupercedingTradeDetails()
		{
			if (!CanSupercede)
			{
				return System.Array.Empty<OrgTradeDetail>();
			}

			var detailQuery = new ZDBOnlyQuery(typeof(OrgTradeDetail));
			detailQuery.AddToFilter(GetTradeDetailQuery());

			var periodSubQuery = new ZDBOnlySubQuery(typeof(OrgTradePeriod), OrgTradePeriodSchema.PAS_PA);
			periodSubQuery.AddToFilter(GetTradePeriodQuery());

			detailQuery.AddSubQuery(periodSubQuery, JoinCondition.And);

			var tradeDetailsToSupercede = Factory.Load<OrgTradeDetail>(detailQuery);
			return tradeDetailsToSupercede;
		}

		ZQuery GetTradePeriodQuery()
		{
			var periodQuery = new ZQuery();
			periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_IsTraded, ZBool.False);
			periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_IsSuperseded, ZBool.False);
			periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_OH_Client, tradeDetail.Parent.OW_OH_Primary);
			periodQuery.AddToFilter(OrgTradePeriodSchema.PAS_Period, SQLComparisonOperator.GreaterThanOrEqualTo, supercedingFromDate);
			return periodQuery;
		}

		ZQuery GetTradeDetailQuery()
		{
			var detailQuery = new ZDBOnlyQuery(typeof(OrgTradeDetail));
			detailQuery.AddToFilter(OrgTradeDetailSchema.PA_Status, OpportunityTradeStatus.Codes.Successful);
			detailQuery.AddToFilter(OrgTradeDetailSchema.PA_TradeMode, tradeDetail.PA_TradeMode);
			detailQuery.AddToFilter(OrgTradeDetailSchema.PA_TradeType, tradeDetail.PA_TradeType);
			if (!tradeDetail.PA_OP.IsEmpty)
			{
				detailQuery.AddToFilter(OrgTradeDetailSchema.PA_OP, tradeDetail.PA_OP);
			}
			else
			{
				detailQuery.AddToFilter(OrgTradeDetailSchema.PA_OP, null);
			}

			var prospectSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeProspect), OrgTradeProspectSchema.PAP_PA);
			prospectSubQuery.AddToFilter(OrgTradeProspectSchema.PAP_RecurrenceType, SQLComparisonOperator.NotEqual, OrgTradeProspectRecurrenceTypeList.Codes.OneOff);

			var salesSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgTradeDetailSchema.PA_OW);
			salesSubQuery.AddToFilter(OrgSalesSchema.OW_IsTraded, ZBool.False);
			salesSubQuery.AddToFilter(OrgSalesSchema.OW_OH_Primary, tradeDetail.Parent.OW_OH_Primary);
			salesSubQuery.AddToFilter(OrgSalesSchema.OW_MP_Product, tradeDetail.Parent.OW_MP_Product);
			salesSubQuery.AddToFilter(OrgSalesSchema.OW_Service, tradeDetail.Parent.OW_Service);

			if (!tradeDetail.Parent.OW_OriginID.IsEmpty)
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_OriginID, tradeDetail.Parent.OW_OriginID);
			}
			else
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_OriginID, null);
			}

			if (!tradeDetail.Parent.OW_DestinationID.IsEmpty)
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_DestinationID, tradeDetail.Parent.OW_DestinationID);
			}
			else
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_DestinationID, null);
			}

			if (!tradeDetail.Parent.OW_WW.IsEmpty)
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_WW, tradeDetail.Parent.OW_DestinationID);
			}
			else
			{
				salesSubQuery.AddToFilter(OrgSalesSchema.OW_WW, null);
			}

			var pivotSubQuery = new ZDBOnlySubQuery(typeof(OrgSalesValueAssociationPivot), OrgSalesValueAssociationPivotSchema.SVP_TradeId, notIn: true);
			pivotSubQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_ActivityId, tradeDetail.SalesAssociationPivotCollectionGlobal.Select(x => x.SVP_ActivityId).ToArray());

			detailQuery.AddSubQuery(prospectSubQuery, JoinCondition.And);
			detailQuery.AddSubQuery(salesSubQuery, JoinCondition.And);
			detailQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

			return detailQuery;
		}

		bool CanSupercede =>
			!tradeDetail.IsActual
			&& !tradeDetail.IsOneoff
			&& !tradeDetail.IsSuperceded
			&& supercedingFromDate.IsValid;
	}
}
