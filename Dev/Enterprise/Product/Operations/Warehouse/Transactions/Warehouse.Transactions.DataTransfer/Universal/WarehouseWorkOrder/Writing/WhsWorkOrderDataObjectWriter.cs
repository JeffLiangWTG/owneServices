using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsWorkOrderDataObjectWriter : WhsComponentOrderDataObjectWriter<WhsWorkOrder>
	{
		internal WhsWorkOrderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObject(WhsWorkOrder whsDocketBO, Shipment shipmentDataObject)
		{
			base.PopulateDataObject(whsDocketBO, shipmentDataObject);

			var orderDataObject = shipmentDataObject.Order;
			orderDataObject.PalletsSent = whsDocketBO.WD_TotalPallets;

			PopulateOuterPacksAndPackageType(whsDocketBO, shipmentDataObject);
			ExportRelatedObjects(whsDocketBO, shipmentDataObject, orderDataObject);
		}

		void PopulateOuterPacksAndPackageType(WhsWorkOrder whsDocketBO, Shipment shipmentDataObject)
		{
			var packs = new[]
			{
				(ZInt)whsDocketBO.WD_TotalPallets,
				whsDocketBO.WD_PackagesSent,
				ToZIntSafely(whsDocketBO.WD_TotalUnits, nameof(whsDocketBO.WD_TotalUnits)),
			}.FirstOrDefault(d => d > 0);

			if (packs > 0)
			{
				shipmentDataObject.OuterPacks = packs;
			}

			var packTypeCode = shipmentDataObject.Order.PalletsSent > 0 ? Constants.PkgUnit.Pallet : Constants.PkgUnit.Piece;
			shipmentDataObject.OuterPacksPackageType = new PackageType { Code = packTypeCode, Description = Constants.PkgUnit.GetDescription(packTypeCode) };
		}

		void ExportRelatedObjects(WhsWorkOrder whsDocketBO, Shipment shipmentDataObject, Order orderDataObject)
		{
			var addresses = ProcessCollection(whsDocketBO.DocAddresses, new JobDocAddressDataObjectWriter(writeManager));

			if (addresses != null)
			{
				if (shipmentDataObject.OrganizationAddressCollection == null)
				{
					shipmentDataObject.SetOrganizationAddressCollection(() => addresses);
				}
				else
				{
					shipmentDataObject.OrganizationAddressCollection.AddRange(addresses);
				}
			}

			shipmentDataObject.SetAdditionalReferenceCollection(() => ProcessCollection(whsDocketBO.References, new WhsDocketReferenceDataObjectWriter(writeManager), CollectionContent.Complete));
			orderDataObject.SetOrderLineCollection(() => ProcessCollection(whsDocketBO.Lines, new WhsWorkOrderLineDataObjectWriter(writeManager), CollectionContent.Complete));
		}

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.WarehouseWorkOrder;
	}
}
