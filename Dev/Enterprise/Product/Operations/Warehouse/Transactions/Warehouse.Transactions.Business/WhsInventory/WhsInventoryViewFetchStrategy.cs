using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsInventoryViewFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsInventoryViewFetchStrategy(WhsInventoryView inventory)
			: base(inventory)
		{
		}

		#region FetchForValidateCore

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();

			Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, Inventory.WI_WE_InDocketLine);
			AddFetchHintForLocationStringValidation();

			//// these are tested in TestPerformanceOfRunPreSaveValidation_DBHits in WhsReceive.cs
			AddFetchHintForPalletIDStockValidation();
			AddFetchHintForPalletIDCannotBeUsedByMultipleClientsValidation();
		}

		#region AddFetchHintForLocationStringValidation

		void AddFetchHintForLocationStringValidation()
		{
			WhsLocation location = Inventory.Location;
			if (location != null)
			{
				ZQuery query = new ZQuery(WhsLocationViewSchema.WLV_WR, location.WLV_WR);
				query.AddToFilter(WhsLocationViewSchema.WLV_Column, location.WLV_Column);
				query.AddToFilter(WhsLocationViewSchema.WLV_Level, location.WLV_Level);
				query.AddToFilter(WhsLocationViewSchema.WLV_Tray, location.WLV_Tray);

				Factory.AddFetchHint(WhsLocationViewSchema.Instance, query);
			}
		}

		#endregion

		#region AddFetchHintForPalletIDStockValidation

		void AddFetchHintForPalletIDStockValidation()
		{
			if (!Inventory.WI_WL.IsEmpty && !Inventory.WI_PalletID.IsEmpty)
			{
				var query = new ZQuery(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);
				query.AddToFilter(WhsDocketLineSchema.WE_PalletID, SQLComparisonOperator.NotEqual, ZString.Empty);
				query.AddToFilter(WhsDocketLineSchema.WE_PalletID, Inventory.WI_PalletID);
				query.AddToFilter(WhsDocketLineSchema.WE_WL, SQLComparisonOperator.NotEqual, null);

				Factory.AddFetchHint(WhsDocketLineSchema.Instance, query);
			}
		}

		#endregion

		#region AddFetchHintForPalletIDCannotBeUsedByMultipleClientsValidation

		void AddFetchHintForPalletIDCannotBeUsedByMultipleClientsValidation()
		{
			if (!Inventory.WI_PalletID.IsEmpty)
			{
				var docket = Inventory.Docket;
				var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
				query.AddToFilter(WhsInventoryViewSchema.WI_PalletID, SQLComparisonOperator.NotEqual, ZString.Empty);
				query.AddToFilter(WhsInventoryViewSchema.WI_PalletID, Inventory.WI_PalletID);
				query.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, SQLComparisonOperator.NotEqual, docket.WD_OH_Client);
				query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);

				var warehouseSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsInventoryViewSchema.WI_WD);
				warehouseSubQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, docket.WD_WW_Whs);

				query.AddSubQuery(warehouseSubQuery, JoinCondition.And);

				Factory.AddFetchHint(WhsInventoryViewSchema.Instance, query);
			}
		}

		#endregion

		#endregion

		#region FetchForViewCore

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (TableColumn column in columns)
			{
				if (column.ColumnName.Contains(WhsInventoryView.Schema.CustomsData))
				{
					Factory.AddFetchHint(WhsBondedWarehouseAttributeSchema.WB_ParentID, Inventory.WI_WE_InDocketLine);
				}

				switch (column.ColumnName)
				{
					case WhsInventoryView.Schema.WI_CustomTextBlob1:
						var query = new ZQuery(WhsDocketLineSchema.PK, Inventory.WI_WE_InDocketLine);
						query.IncludeBlob(WhsDocketLineSchema.WE_CustomTextBlob1);
						Factory.AddFetchHint(WhsDocketLineSchema.Instance, query);
						Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, Inventory.WI_WE_InDocketLine);
						break;

					case WhsInventoryView.Schema.ConsigneeNameOrPK:
						Factory.AddFetchHint(WhsDocketLineSchema.PK, Inventory.WI_WE_InDocketLine);
						Factory.AddFetchHint(typeof(WhsLocation), WhsLocationViewSchema.PK, Inventory.WI_WL);
						Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, Inventory.WI_WE_InDocketLine);
						Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, Inventory.WI_WE_InDocketLine);
						break;

					default:
						Factory.AddFetchHint(StmNoteSchema.ST_ParentID, Inventory.WI_WE_InDocketLine);
						break;
				}
			}
		}

		#endregion

		#region Inventory

		WhsInventoryView Inventory
		{
			get { return (WhsInventoryView)BusinessObject; }
		}

		#endregion
	}

	internal class WhsInventoryViewTypeFetchStrategy : IRowFetchStrategy
	{
		#region IRowFetchStrategy members

		void IRowFetchStrategy.FetchForLoad(BusinessObjectFactory factory, DataRow[] rows)
		{
			foreach (var row in rows)
			{
				factory.AddFetchHint(WhsInventoryViewSchema.PK, new ZGuid(row[WhsInventoryViewSchema.PK.Name]));
				factory.AddFetchHint(WhsDocketLineSchema.PK, new ZGuid(row[WhsInventoryViewSchema.WI_WE_InDocketLine.Name]));
			}
		}

		#endregion
	}
}
