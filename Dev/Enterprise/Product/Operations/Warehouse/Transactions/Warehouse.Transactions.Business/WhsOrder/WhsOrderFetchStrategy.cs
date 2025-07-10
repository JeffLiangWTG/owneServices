using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderFetchStrategy : WhsPickableDocketFetchStrategy
	{
		public WhsOrderFetchStrategy(WhsOrder order)
			: base(order)
		{
		}

		protected WhsOrder Order => (WhsOrder)BusinessObject;

		#region FetchForViewCore

		protected override void AddDocketSpecificFetchHintsForView(string columnName)
		{
			base.AddDocketSpecificFetchHintsForView(columnName);

			if (columnName == nameof(WhsOrder.OrderGoodsHandlingInstructions))
			{
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, Docket.PK);
				Factory.AddFetchHint(typeof(StmNote), GoodsHandlingInstructionDocketQuery());
				Factory.AddFetchHint(typeof(StmNote), GoodsHandlingInstructionOrgHeaderQuery());
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, Order.WD_OH_Client);
			}
			else if (columnName == nameof(WhsOrder.VehicleNo))
			{
				Factory.AddFetchHint(WhsDocketReferenceSchema.WX_WD, Docket.PK);
			}
			else if (columnName == nameof(WhsOrder.AuditStatus))
			{
				AddFetchHintForPackages();
				Factory.AddFetchHint(WhsPickSchema.PK, Docket.WD_WP);
			}
			else if (columnName == nameof(WhsOrder.WarehouseOrderStatus) && Order.WD_DocketStatus == DocketStatus.Codes.Picking)
			{
				Factory.AddFetchHint(WhsOrderStatusViewSchema.PK, Docket.PK);
			}
			else if (columnName == nameof(WhsOrder.LoadID))
			{
				AddFetchHintForLoadId();
			}
			else if (columnName == nameof(WhsOrder.TrolleyNumber))
			{
				AddFetchHintForTrolleyNumber();
			}
			else if (columnName.StartsWith(nameof(WhsOrder.CarrierBookingAgentDocAddress), StringComparison.Ordinal)
				|| columnName.StartsWith(nameof(WhsOrder.Consignee), StringComparison.Ordinal)
				|| columnName.StartsWith(nameof(WhsOrder.TransportCo), StringComparison.Ordinal)
				|| columnName.StartsWith(nameof(WhsOrder.DistributionCentreNameOrPK), StringComparison.Ordinal))
			{
				AddFetchHintForOrgAddress();
			}
			else if (columnName == Invariant($"{nameof(WhsOrder.SalesChannel)}+{nameof(WhsOrder.SalesChannel.WSH_Description)}"))
			{
				Factory.AddFetchHint(WhsSalesChannelSchema.PK, Order.WD_WSH_SalesChannel);
			}
			else if (columnName == nameof(WhsOrder.JobHeader) + "+" + nameof(WhsOrder.JobHeader.JH_ProfitLossReasonCode)
				|| columnName == nameof(WhsOrder.JobHeader) + "+" + nameof(WhsOrder.JobHeader.JH_TotalProfitRevenueMargin))
			{
				AddFetchHintJobHeader();
			}
			else if (columnName == nameof(WhsOrder.WarehouseOrderStatusDescription))
			{
				Factory.AddFetchHint(WhsOrderStatusViewSchema.PK, Order.PK);
			}
			else if (columnName == nameof(WhsOrder.OutboundLocation))
			{
				Factory.AddFetchHint(WhsOutboundLocationViewSchema.Instance, new ZQuery(WhsOutboundLocationViewSchema.WOU_WD_Order, Order.PK));
			}
		}

		void AddFetchHintForPackages()
		{
			var packageJobQuery = new ZDBOnlyQuery(typeof(PkgPackageJob));
			packageJobQuery.AddToFilter(PkgPackageJobSchema.KJ_ParentTableCode, WhsDocketSchema.Constants.Prefix);
			packageJobQuery.AddToFilter(PkgPackageJobSchema.KJ_ParentID, Docket.PK);
			Factory.AddFetchHint(typeof(PkgPackageJob), packageJobQuery);
		}

		void AddFetchHintForOrgAddress()
		{
			var addressQuery = new ZDBOnlyQuery(typeof(OrgAddress));
			var warehouseSubQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsWarehouseSchema.WW_OA_WarehouseAddress);
			warehouseSubQuery.AddToFilter(WhsWarehouseSchema.PK, Docket.WD_WW_Whs);
			addressQuery.AddSubQuery(warehouseSubQuery, JoinCondition.And);
			Factory.AddFetchHint(typeof(OrgAddress), addressQuery);
		}

		void AddFetchHintForLoadId()
		{
			Factory.AddFetchHint(WhsLoadOrderSchema.WOV_WD_Docket, Docket.PK);
			var loadSubOrderQuery = new ZDBOnlySubQuery(typeof(WhsLoadOrder), WhsLoadOrderSchema.WOV_WLO_Load);
			loadSubOrderQuery.AddToFilter(WhsLoadOrderSchema.WOV_WD_Docket, Docket.PK);
			var loadQuery = new ZDBOnlyQuery(typeof(WhsLoad));
			loadQuery.AddSubQuery(loadSubOrderQuery, JoinCondition.And);
			Factory.AddFetchHint(WhsLoadSchema.Instance, loadQuery);
		}

		void AddFetchHintForTrolleyNumber()
		{
			Factory.AddFetchHint(WhsOrderTrolleyViewSchema.Instance, new ZQuery(WhsOrderTrolleyViewSchema.WTR_WD_PK, Docket.PK));
		}

		void AddFetchHintJobHeader()
		{
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, Docket.PK)
				.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK)
				.AddToFilter(JobHeaderSchema.JH_IsActive, true);

			Factory.AddFetchHint(JobHeaderSchema.Instance, query);
		}

		protected override FetchStrategyFlag GetDocketSpecificFetchForViewStrategyFlag(string columnName)
		{
			var fetchStrategyFlags = FetchStrategyFlag.None;
			if (columnName == nameof(WhsOrder.OrderGoodsHandlingInstructions))
			{
				fetchStrategyFlags |= FetchStrategyFlag.RequireClient;
			}
			else if (columnName == nameof(WhsOrder.ProductCount))
			{
				fetchStrategyFlags |= FetchStrategyFlag.RequireLines;
			}
			else if (columnName == nameof(WhsOrder.AuditStatus))
			{
				fetchStrategyFlags |= FetchStrategyFlag.RequireWarehouse;
			}
			else if (columnName == WhsDocketSchema.Constants.WD_WL_CrossDock)
			{
				fetchStrategyFlags |= FetchStrategyFlag.RequireWarehouse;
			}
			else if (columnName == nameof(WhsPickableDocket.HasDangerousGoods))
			{
				fetchStrategyFlags |= FetchStrategyFlag.RequireOrgSupplierPart;
				fetchStrategyFlags |= FetchStrategyFlag.RequireLines;
			}
			else if (columnName.StartsWith(nameof(WhsOrder.CarrierBookingAgentDocAddress), StringComparison.Ordinal)
				|| columnName.StartsWith(nameof(WhsOrder.Consignee), StringComparison.Ordinal)
				|| columnName.StartsWith(nameof(WhsOrder.TransportCo), StringComparison.Ordinal)
				|| columnName.StartsWith(nameof(WhsOrder.DistributionCentreNameOrPK), StringComparison.Ordinal))
			{
				fetchStrategyFlags |= FetchStrategyFlag.RequireJobDocAddress;
				fetchStrategyFlags |= FetchStrategyFlag.RequireWarehouse;
			}

			return fetchStrategyFlags;
		}

		#endregion

		#region FetchForFactorySaveBeforeTransaction

		protected override void FetchForFactorySaveBeforeTransactionCore()
		{
			base.FetchForFactorySaveBeforeTransactionCore();

			// tested in TestPickingAllPickLines_DBHits
			if (Order.HasChanges && Order.IsCustomsTransaction)
			{
				Factory.AddFetchHint(StmALogSchema.SL_Parent, Order.PK);
			}
			Factory.AddFetchHint(JobDocAddressSchema.Instance, FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, Order.PK));
		}

		#endregion
	}
}
