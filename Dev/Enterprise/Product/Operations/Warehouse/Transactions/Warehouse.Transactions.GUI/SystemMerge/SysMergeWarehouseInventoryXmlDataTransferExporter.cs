using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class SysMergeWarehouseInventoryXmlDataTransferExporter : XmlDataTransferExporter
	{
		public SysMergeWarehouseInventoryXmlDataTransferExporter()
			: base(new SysMergeWarehouseInventoryValueObjectDataAdapter(), true)
		{
		}

		#region PromptUserAndExportCore

		protected override void PromptUserAndExportCore(IList selectedElements)
		{
			var clientPKs = selectedElements.Cast<OrgHeader>().Select(org => org.PK);
			if (CheckDataIsReadyToBeExported())
			{
				var receivesToExport = LoadReceivesAdjustmentsAndTransfersToExport(clientPKs);
				base.PromptUserAndExportCore(receivesToExport);
			}
		}

		#region CheckDataIsReadyToBeExported

		bool CheckDataIsReadyToBeExported()
		{
			var result = true;

			// for all non-bonded warehouses
			var warehouseSubQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsPickSchema.WP_WW_Whs);
			warehouseSubQuery.AddToFilter(WhsWarehouseSchema.WW_IsVirtualWarehouse, ZBool.False);

			// find any non-finalised / non-cancelled picks
			var pickSubQuery = new ZDBOnlySubQuery(typeof(WhsPick), WhsDocketSchema.WD_WP);
			pickSubQuery.AddToFilter(WhsPickSchema.WP_PickStatus, SQLComparisonOperator.NotEqual, PickStatus.Codes.Finalised);
			pickSubQuery.AddToFilter(WhsPickSchema.WP_PickStatus, SQLComparisonOperator.NotEqual, PickStatus.Codes.Cancelled);
			pickSubQuery.AddSubQuery(warehouseSubQuery, JoinCondition.And);

			// find any non-finalised / non-cancelled whs jobs
			var docketStatusQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			docketStatusQuery.AddToFilter(WhsDocketSchema.WD_FinalisedDate, SQLComparisonOperator.Equal, ZDateTime.Empty);
			docketStatusQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Cancelled);

			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			query.AddToFilter(docketStatusQuery);
			query.AddSubQuery(pickSubQuery, JoinCondition.Or);

			if (FactoryProvider.Current.ExistsInDatabase(WhsDocketSchema.Constants.TableName, query))
			{
				Globals.Message.ShowError(Res.GetString("b8ebe27e-0116-4b14-a4e4-979d144b01f9", "Please finalize all Warehouse Jobs before running System Merge."));
				result = false;
			}

			return result;
		}

		#endregion

		#region LoadReceivesAdjustmentsAndTransfersToExport

		IList LoadReceivesAdjustmentsAndTransfersToExport(IEnumerable<ZGuid> clientPKs)
		{
			// LEAVE THESE COMMENTS IN PLACE FOR DESCRIBING BUSINESS CASES. PASTED INTO MS SQL TO READ MORE EASILY.

			//select
			//    WD_PK
			//from 
			//    WhsDocket
			//    join dbo.WhsDocketLine on WE_WD = WD_PK
			//    join WhsInventory on WI_WE_InDocketLine = WE_PK
			//    join dbo.WhsWarehouse on WW_PK = WD_WW_Whs
			//where
			//    WI_TotalUnits > 0
			//    and (WW_IsVirtualWarehouse = 0

			//    -- Bonded Warehouse

			//    -- if Inventory was not Split (into Damaged or Held),
			//    -- and there were no partial Transfers/Adjustments to the Inventory,
			//    -- and Manual DB update WI_TotalUnits = WI_TotalUnits - WI_CommittedUnits was not run for the line.
			//    or (WI_TotalUnits = WE_TransactionQuantity and WI_TotalUnits - WI_CommittedUnits > 0)

			//    -- Bonded Warehouse with partial Transfers/Adjustments *or* bad data (we cannot be sure which)

			//    -- if DB was updated manually by running WI_TotalUnits = WI_TotalUnits - WI_CommittedUnits.
			//    -- and if there were no splits to inventory
			//    -- and if there were no partial Transfers/Adjustments to the inventory
			//    or (WE_TransactionQuantity - WI_CommittedUnits > 0)

			//    -- Correct only if the DB was not updated Manually by WI_TotalUnits = WI_TotalUnits - WI_CommittedUnits.
			//    --or (WI_TotalUnits - WI_CommittedUnits > 0)) // This Part Is not used in Query Below, but one day may be required!!!
			//group by
			//    WD_PK
			var receiveQuery = new ZDBOnlyQuery(typeof(WhsReceive)) { AllowTableValuedParameters = true };
			receiveQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, clientPKs);
			receiveQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, DocketStatus.Codes.Finalised);

			var parameters = new ZSqlParameterCollection();
			receiveQuery.AddFilterAndZSQLParameterCollection(@"
WD_PK in 
(
select
    WD_PK
from 
    dbo.WhsDocket
    join dbo.WhsDocketLine on WE_WD = WD_PK
    join dbo.WhsInventoryView on WI_WE_InDocketLine = WE_PK
    join dbo.WhsWarehouse on WW_PK = WD_WW_Whs
	LEFT JOIN 
		(
			select 
				WZ_WE_InventoryLine, 
				sum(WZ_Units) as CommittedUnits
			from 
				dbo.WhsCommittedStock
			group by 
				WZ_WE_InventoryLine
		) PickLines on WZ_WE_InventoryLine = WI_WE_InDocketLine
where
    WI_TotalUnits > 0
	and (WW_IsVirtualWarehouse = 0
    or (WI_TotalUnits = WE_TransactionQuantity and WI_TotalUnits - ISNULL(CommittedUnits, 0) > 0)
    or (WE_TransactionQuantity - ISNULL(CommittedUnits, 0) > 0))
group by
	WD_PK
)", parameters, JoinCondition.And);

			return FactoryProvider.Current.Load<WhsDocket>(receiveQuery);
		}

		#endregion

		#endregion

		#region FactoryProvider

		DataTransferBusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (factoryProvider == null)
				{
					factoryProvider = new DataTransferBusinessObjectFactoryProvider();
					factoryProvider.Current.RefreshEnabled = false;
				}
				return factoryProvider;
			}
		}

		DataTransferBusinessObjectFactoryProvider factoryProvider;

		#endregion
	}
}
