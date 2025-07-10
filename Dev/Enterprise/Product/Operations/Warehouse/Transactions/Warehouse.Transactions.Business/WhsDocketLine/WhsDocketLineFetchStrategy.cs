using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketLineFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsDocketLineFetchStrategy(WhsDocketLine docketLine)
			: base(docketLine)
		{
		}

		protected sealed override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			var line = (WhsDocketLine)BusinessObject;

			// these are performance tested in the GUI
			Factory.AddFetchHint(typeof(OrgSupplierPart), line.WE_OP);

			FetchForLoadCore(line);
		}

		protected virtual void FetchForLoadCore(WhsDocketLine line)
		{
			Factory.AddFetchHint(typeof(WhsLocation), WhsLocationViewSchema.PK, line.WE_WL);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var needsBondedWhsAttributeFetchHint = false;
			var needsAreaFetchHint = false;

			var line = (WhsDocketLine)BusinessObject;
			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case WhsDocketLine.Schema.CustomsData:
					case WhsDocketLine.Schema.StorageMain:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_DeclarationReference:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_CustomsDeadline:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_InwardStyle:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_InwardProcedure:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_EntryKey:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_EntryLineNo:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_EntryDate:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_CustomsQty:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_CustomsUnitOfQty:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_BondedWhsQty:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_BondedWhsUnitOfQty:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_TILV:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_RN_NKCountryOfOrigin:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_ValueForDuty:
					case WhsBondedWarehouseAttributeSchema.Constants.WB_AddInfo:
						needsBondedWhsAttributeFetchHint = true;
						break;

					case WhsDocketLine.Schema.WE_CustomTextBlob1:
						var query = new ZQuery(WhsDocketLineSchema.PK, line.PK);
						query.IncludeBlob(WhsDocketLineSchema.WE_CustomTextBlob1);
						Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, line.PK);
						break;

					case WhsDocketLine.Schema.HasEDocsOrNotesAttached:
						Factory.AddFetchHint(StmNoteSchema.ST_ParentID, line.PK);
						Factory.AddFetchHint(WhsBondedWarehouseAttributeSchema.WB_ParentID, line.PK);
						break;

					// These columns will trigger loading of all areas in the warehouse.
					// Add a fetch hint for this, so that this occurs with the first load of WhsArea by the grid (may be for another column).
					case WhsDocketLine.Schema.CustomsTariffLookup:
					case WhsDocketLine.Schema.CustomsTariffItem:
					case WhsDocketLine.Schema.CustomsTariffDesc:
						needsAreaFetchHint = true;
						break;
				}
			}

			if (needsBondedWhsAttributeFetchHint)
			{
				Factory.AddFetchHint(WhsBondedWarehouseAttributeSchema.WB_ParentID, line.PK);
			}

			if (needsAreaFetchHint)
			{
				Factory.AddFetchHint(WhsAreaSchema.WA_WW_Whs, line.WarehousePK);
			}
		}
	}
}
