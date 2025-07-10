using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class WhsDocketFlattenedDataTransferProcessor<T> : SimpleModuleDataTransferProcessor<T, WhsDocketFlattened>
		where T : WhsDocket
	{
		protected WhsDocketFlattenedDataTransferProcessor(ImportCollectionInfoImplForWhsDocketFlattened importCollectionInfo, WhsDocketCollection docketCollection)
			: base(docketCollection, importCollectionInfo)
		{
		}

		protected override T CreateHeader(IBusinessObjectCollection headerCollection, WhsDocketFlattened flattenedRecord)
		{
			T docket = null;

			var part = new OrgSupplierPart.Loader(Factory).LoadAndReturnMatchingCount(flattenedRecord.LinePart_OP_PartNum, GetOrgHeaderFromCode(flattenedRecord.Client_OH_Code), null, false);
			if (part.TotalMatchCount > 1)
			{
				Globals.Message.ShowError(Res.GetString("WhsDocketFlattenedDataTransferProcessor|CreateHeader|DuplicateProductError", "A duplicate product was matched. Nothing was saved."));
				IsCanceled = true;
			}
			else
			{
				docket = GetHeader(flattenedRecord.WD_TransportReference, x => x.WD_TransportReference);

				CopyIdenticallyNamedProperties(docket, flattenedRecord, WhsDocketSchema.Constants.Prefix);
				SetWarehouse(flattenedRecord.Warehouse_WW_WarehouseCode, docket);
				SetOrgReference(flattenedRecord.Client_OH_Code, (org) => docket.WD_OH_Client = org.PK);
				SetOrgReference(flattenedRecord.Forwarder_OH_Code, (org) => docket.WD_OH_Forwarder = org.PK);
				SetJobDocAddress((NoResString)"Pickup", flattenedRecord, docket.PickUpDocAddress); // Programmatic prefix for properties
				SetJobDocAddress("DropOff", flattenedRecord, docket.DropOffDocAddress);
				SetJobDocAddress((NoResString)"Supplier", flattenedRecord, docket.SupplierDocAddress); // Programmatic prefix for properties
				CreateHeaderCore(docket, flattenedRecord);
				SetLineDetails(flattenedRecord, docket);
			}

			return docket;
		}

		protected abstract void CreateHeaderCore(T docket, WhsDocketFlattened flattenedRecord);

		#region Implementation

		void SetWarehouse(ZString warehouseCode, WhsDocket docket)
		{
			if (!warehouseCode.IsEmpty)
			{
				var whs = Factory.LoadFromNaturalKey<WhsWarehouse>(WhsWarehouseSchema.WW_WarehouseCode, warehouseCode);
				if (whs != null)
				{
					docket.WD_WW_Whs = whs.PK;
				}
			}
		}

		void SetLineDetails(WhsDocketFlattened flattenedRecord, WhsDocket docket)
		{
			if (!flattenedRecord.LinePart_OP_PartNum.IsEmpty)
			{
				var docketLine = docket.Lines.AddNew();
				this.CopyIdenticallyNamedProperties(docketLine, flattenedRecord, (NoResString)"Line", (NoResString)"Line"); // Programmatic prefix for properties

				var part = new OrgSupplierPart.Loader(Factory).Load(flattenedRecord.LinePart_OP_PartNum, docket.Client, null);
				if (part != null)
				{
					docketLine.WE_OP = part.PK;
				}
			}
		}

		#endregion
	}
}
