using CargoWise.Types;
using Enterprise.eManifest.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eManifest.DataTransfer.Universal
{
	class SupplierBookingHeaderWriter : TopLevelDataObjectWriter<SupplierBookingHeader, UniversalShipment>
	{
		public SupplierBookingHeaderWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.eManifest;
		}

		protected override void PopulateDataObject(SupplierBookingHeader sourceBO, UniversalShipment targetDataObject)
		{
			PopulateHeaderDetails(sourceBO, targetDataObject);

			var data = ProcessCollection(sourceBO.BookingLines, new SupplierBookingLineWriter(writeManager) { IncludeParent = false });
			targetDataObject.SetSubShipmentCollection(() => data != null ? new DataObjectList<UniversalShipment>(data) : null);
		}

		void PopulateHeaderDetails(SupplierBookingHeader sourceBO, UniversalShipment targetDataObject)
		{
			targetDataObject.OwnerRef = sourceBO.DH_SupplierReference;
			targetDataObject.TotalWeight = sourceBO.DH_GrossWeightInKg;
			targetDataObject.TotalVolume = sourceBO.DH_CubicInM3;
			targetDataObject.TotalNoOfPieces = sourceBO.DH_PiecesManifested;
			targetDataObject.AddOrgAddress(writeManager, sourceBO.Consignor, DocAddressType.ConsignorDocumentaryAddress, sourceBO.ConsignorContact);
			targetDataObject.AddOrgAddress(writeManager, sourceBO.DispatchAddress, DocAddressType.PickUpAddress);
		}
	}
}
