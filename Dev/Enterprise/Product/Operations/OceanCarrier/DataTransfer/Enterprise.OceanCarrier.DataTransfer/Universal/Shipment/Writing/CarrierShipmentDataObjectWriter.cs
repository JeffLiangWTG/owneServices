using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.OceanCarrier.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.OceanCarrier.DataTransfer.Universal.Shipment
{
	public sealed class CarrierShipmentDataObjectWriter : TopLevelDataObjectWriter<CarrierShipmentHeader, UniversalShipment>
	{
		readonly CarrierShipmentContainerLinkManager containerLinkManager;

		public CarrierShipmentDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
			this.containerLinkManager = new CarrierShipmentContainerLinkManager();
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.CarrierShipment;
		}

		protected override void PopulateDataObject(CarrierShipmentHeader carrierShipmentHeader, UniversalShipment dataObject)
		{
			Argument.NotNull(carrierShipmentHeader, "Carrier Shipment Header");
			PopulateDataContext(carrierShipmentHeader, dataObject);
			PopulateHeader(carrierShipmentHeader, dataObject);
			PopulateParties(carrierShipmentHeader, dataObject);
			PopulateContainers(carrierShipmentHeader, dataObject);
			PopulatePackingLines(carrierShipmentHeader, dataObject);
		}

		void PopulateDataContext(CarrierShipmentHeader carrierShipmentHeader, UniversalShipment dataObject)
		{
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataSource(DataContextType.CarrierShipment, carrierShipmentHeader.CSH_CarrierShipmentReference);
		}

		void PopulateHeader(CarrierShipmentHeader carrierShipmentHeader, UniversalShipment dataObject)
		{
			dataObject.BookingConfirmationReference = carrierShipmentHeader.CSH_CarrierShipmentReference;
			dataObject.PlaceOfDelivery = UNLOCO.New(carrierShipmentHeader.PlaceOfDeliveryCode);
			dataObject.PlaceOfReceipt = UNLOCO.New(carrierShipmentHeader.PlaceOfReceiptCode);
			dataObject.PortOfDischarge = UNLOCO.New(carrierShipmentHeader.PortOfDischargeCode);
			dataObject.PortOfLoading = UNLOCO.New(carrierShipmentHeader.PortOfLoadingCode);
			dataObject.PortOfOrigin = UNLOCO.New(carrierShipmentHeader.PortOfOriginCode);
			dataObject.PortOfDestination = UNLOCO.New(carrierShipmentHeader.PortOfDestinationCode);
			dataObject.TransportMode = new CodeDescriptionPair() { Code = TransportModes.Sea, Description = TransportModeDescriptions.Sea };
			dataObject.WayBillNumber = carrierShipmentHeader.CSH_RequestedTransportDocumentReference;
			dataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(carrierShipmentHeader.CSH_RequestedTransportDocumentType, new WayBillTypeList());

			if (carrierShipmentHeader.CSH_DeclaredValue > 0 || !carrierShipmentHeader.CSH_RX_NKDeclaredValueCurrency.IsEmpty)
			{
				dataObject.GoodsValue = carrierShipmentHeader.CSH_DeclaredValue;
				dataObject.GoodsValueCurrency = Currency.New(carrierShipmentHeader.DeclaredValueCurrency);
			}
		}

		void PopulateParties(CarrierShipmentHeader carrierShipmentHeader, UniversalShipment dataObject)
		{
			dataObject.SetOrganizationAddressCollection(() => ProcessCollection(carrierShipmentHeader.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)
			{
				PopulateGeoLocation = true,
				PopulateValidationStatus = true
			}));

			if (dataObject.OrganizationAddressCollection != null)
			{
				RemoveAddressRegistrationNumberCollection(carrierShipmentHeader, dataObject);
			}
		}

		void RemoveAddressRegistrationNumberCollection(CarrierShipmentHeader carrierShipmentHeader, UniversalShipment dataObject)
		{
			foreach (var party in dataObject.OrganizationAddressCollection)
			{
				party.GovRegNum = null;
				party.GovRegNumType = null;
				party.SetRegistrationNumberCollection(() => null);
			}
		}

		void PopulateContainers(CarrierShipmentHeader carrierShipmentHeader, UniversalShipment dataObject)
		{
			var containers = carrierShipmentHeader.Cargoes.Find(container => container.CSC_CargoType == ContainerModes.Containerised && container.CSC_IsTopLevel);
			if (containers.Any())
			{
				dataObject.SetContainerCollection(() =>
					new DataObjectList<Container>(ProcessCollection(containers,
						new CarrierShipmentContainerDataObjectWriter(writeManager, containerLinkManager))));
			}
		}

		void PopulatePackingLines(CarrierShipmentHeader carrierShipmentHeader, UniversalShipment dataObject)
		{
			var packingLines = carrierShipmentHeader.Cargoes.Find(container => container.CSC_CargoType == ContainerModes.BreakBulk);
			if (packingLines.Any())
			{
				dataObject.SetPackingLineCollection(() =>
					new DataObjectList<PackingLine>(ProcessCollection(packingLines,
						new CarrierShipmentPackingLineDataObjectWriter(writeManager, containerLinkManager, carrierShipmentHeader))));
			}
		}
	}
}
