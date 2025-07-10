using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveFetchStrategy : WhsDocketFetchStrategy
	{
		public WhsReceiveFetchStrategy(WhsReceive receive)
			: base(receive)
		{
		}

		protected WhsReceive Receive
		{
			get { return (WhsReceive)BusinessObject; }
		}

		#region FetchForFactorySaveCore

		protected override void FetchForFactorySaveCore()
		{
			base.FetchForFactorySaveCore();
			Factory.AddFetchHint(StmALogSchema.SL_Parent, Docket.PK);
		}

		#endregion

		#region FetchForViewCore

		protected override void AddDocketSpecificFetchHintsForView(string columnName)
		{
			base.AddDocketSpecificFetchHintsForView(columnName);

			if (columnName.StartsWith("TransportCo", StringComparison.Ordinal) // table column name constants are not multilingual
				|| columnName == nameof(WhsReceive.JobHeader) + "+" + nameof(WhsReceive.JobHeader.JH_ProfitLossReasonCode)
				|| columnName == nameof(WhsReceive.JobHeader) + "+" + nameof(WhsReceive.JobHeader.JH_TotalProfitRevenueMargin)) 
			{
				var query = new ZQuery(JobHeaderSchema.JH_ParentID, Docket.PK);
				query.AddToFilter(JobHeaderSchema.JH_GC, Env.CurrentCompany.PK);
				Factory.AddFetchHint(typeof(JobHeader), query);
			}
			else if (columnName == nameof(WhsReceive.ReceiveGoodsHandlingInstructions))
			{
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, Docket.PK);
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, Receive.WD_OH_Client);
				Factory.AddFetchHint(typeof(StmNote), GoodsHandlingInstructionDocketQuery());
				Factory.AddFetchHint(typeof(StmNote), GoodsHandlingInstructionOrgHeaderQuery());
			}
			else if (columnName == nameof(WhsOrder.VehicleNo))
			{
				Factory.AddFetchHint(WhsDocketReferenceSchema.WX_WD, Docket.PK);
			}
			else if (columnName == nameof(WhsReceive.WD_TotalExpectedQuantity)
				|| columnName == nameof(WhsReceive.TotalPalletsReceived))
			{
				var query = new ZQuery(WhsDocketLineSchema.WE_WD, Docket.PK);
				query.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, true);
				Factory.AddFetchHint(WhsDocketLineSchema.Instance, query);
			}
		}

		protected override FetchStrategyFlag GetDocketSpecificFetchForViewStrategyFlag(string columnName)
		{
			var fetchStrategyFlags = FetchStrategyFlag.None;
			if (columnName == nameof(WhsReceive.ReceiveGoodsHandlingInstructions))
			{
				fetchStrategyFlags |= FetchStrategyFlag.RequireClient;
			}
			else if (columnName == nameof(WhsReceive.ProductCount))
			{
				fetchStrategyFlags |= FetchStrategyFlag.RequireLines;
			}
			else if (columnName.StartsWith(nameof(WhsReceive.Supplier), StringComparison.Ordinal)
				|| columnName.StartsWith(nameof(WhsDocket.TransportCo), StringComparison.Ordinal))
			{
				fetchStrategyFlags |= FetchStrategyFlag.RequireJobDocAddress;
			}

			return fetchStrategyFlags;
		}

		#endregion
	}
}
