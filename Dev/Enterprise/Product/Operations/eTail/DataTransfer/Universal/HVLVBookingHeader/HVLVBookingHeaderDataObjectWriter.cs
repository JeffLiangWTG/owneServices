using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVBookingHeaderDataObjectWriter : TopLevelDataObjectWriter<HVLVBookingHeader, UniversalShipment>
	{
		public HVLVBookingHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObject(HVLVBookingHeader bookingHeaderBO, UniversalShipment dataObject)
		{
			dataObject.IsLastMileDeliverySelfBooked = bookingHeaderBO.HVH_UseShipperDeliveryAccount;
			dataObject.ServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(bookingHeaderBO.HVH_RS_NKBookingServiceLevel, bookingHeaderBO.Lookups.BookingServiceLevels);
			dataObject.TotalNoOfPieces = bookingHeaderBO.HVH_ItemCount;
			dataObject.TotalWeight = bookingHeaderBO.HVH_GrossWeight;
			dataObject.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(bookingHeaderBO.HVH_GrossWeightUQ, bookingHeaderBO.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight));
			dataObject.TotalVolume = bookingHeaderBO.HVH_GrossVolume;
			dataObject.TotalVolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(bookingHeaderBO.HVH_GrossVolumeUQ, bookingHeaderBO.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume));
			dataObject.BookingConfirmationReference = bookingHeaderBO.HVH_IsBookingConfirmed ? Freight.Integration.ShipmentStatusList.Codes.Confirmed : Freight.Integration.ShipmentStatusList.Codes.Booked;

			PopulateOrganizations(bookingHeaderBO, dataObject);

			var consignments = bookingHeaderBO.Consignments;
			var data = ProcessCollection(consignments, new HVLVConsignmentDataObjectWriter(writeManager, HVLVConsignmentDataExportStrategyWithoutNotes.Instance));
			dataObject.SetSubShipmentCollection(() => data != null ? new DataObjectList<UniversalShipment>(data) : null);

			if (bookingHeaderBO.DocsAndCartage != null)
			{
				var writer = new LocalProcessingDataObjectWriter(writeManager);
				dataObject.LocalProcessing = writer.GetDataObject(bookingHeaderBO.DocsAndCartage);
			}
		}

		void PopulateOrganizations(HVLVBookingHeader bookingHeaderBO, UniversalShipment dataObject)
		{
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			dataObject.AddOrgAddress(writeManager, bookingHeaderBO.BillToParty, AddressTypes.SendersLocalClient, bookingHeaderBO.BillToPartyContact);
			dataObject.AddOrgAddress(writeManager, bookingHeaderBO.DispatchAddress, DocAddressType.ConsignorPickupDeliveryAddress);
			dataObject.AddOrgAddress(writeManager, bookingHeaderBO.FreightAgent, DocAddressType.ExportBroker);
			dataObject.AddOrgAddress(writeManager, bookingHeaderBO.OriginDepot, DocAddressType.DepartureCFSAddress);
			dataObject.AddOrgAddress(writeManager, bookingHeaderBO.BookedBy, DocAddressType.BookingPartyDocumentaryAddress);
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.HVLVBookingHeader;
	}
}
