using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.OceanCarrier.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.OceanCarrier.DataTransfer.Universal.Shipment
{
	public class CarrierShipmentDataObjectReader : ShipmentDataObjectReader<CarrierShipmentHeader>
	{
		public CarrierShipmentDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			containerLinkManager = new CarrierShipmentContainerLinkManager();
		}

		public bool IsInsert => IsNewBO;

		public override DataContextType DataContextType
		{
			get { return DataContextType.CarrierShipment; }
		}

		protected sealed override void PopulateBusinessObject(CarrierShipmentHeader carrierShipmentHeader)
		{
			BeforePopulateBusinessObject(carrierShipmentHeader);
			PopulateHeader(carrierShipmentHeader);
			PopulateParties(carrierShipmentHeader);
			PopulateContainers(carrierShipmentHeader);
			PopulateBreakBulk(carrierShipmentHeader);
		}

		protected virtual void BeforePopulateBusinessObject(CarrierShipmentHeader carrierShipmentHeader)
		{
		}

		void PopulateHeader(CarrierShipmentHeader carrierShipmentHeader)
		{
			var carrierShipmentReference = dataObject.DataContext.DataTargetCollection.FirstOrDefault()?.Key ?? ZString.Empty;
			SetValue(carrierShipmentHeader, CarrierShipmentHeaderSchema.CSH_CarrierShipmentReference, carrierShipmentReference);
			SetValue(carrierShipmentHeader, CarrierShipmentHeaderSchema.CSH_DeclaredValue, dataObject.GoodsValue);
			SetValue(carrierShipmentHeader, CarrierShipmentHeaderSchema.CSH_RX_NKDeclaredValueCurrency, dataObject.GoodsValueCurrency?.Code);
			SetValue(carrierShipmentHeader, CarrierShipmentHeaderSchema.CSH_RL_NKPlaceOfDeliveryCode, dataObject.PlaceOfDelivery?.Code);
			SetValue(carrierShipmentHeader, CarrierShipmentHeaderSchema.CSH_RL_NKPlaceOfReceiptCode, dataObject.PlaceOfReceipt?.Code);
			SetValue(carrierShipmentHeader, CarrierShipmentHeaderSchema.CSH_RL_NKPortOfDischargeCode, dataObject.PortOfDischarge?.Code);
			SetValue(carrierShipmentHeader, CarrierShipmentHeaderSchema.CSH_RL_NKPortOfLoadingCode, dataObject.PortOfLoading?.Code);
			SetValue(carrierShipmentHeader, CarrierShipmentHeaderSchema.CSH_RL_NKPortOfOriginCode, dataObject.PortOfOrigin?.Code);
			SetValue(carrierShipmentHeader, CarrierShipmentHeaderSchema.CSH_RL_NKPortOfDestinationCode, dataObject.PortOfDestination?.Code);
			SetValue(carrierShipmentHeader, CarrierShipmentHeaderSchema.CSH_RequestedTransportDocumentReference, dataObject.WayBillNumber);
			if (dataObject.WayBillType != null)
			{
				SetValue(carrierShipmentHeader, CarrierShipmentHeaderSchema.CSH_RequestedTransportDocumentType,
					string.IsNullOrEmpty(dataObject.WayBillType?.Code)
						? "NON"
						: dataObject.WayBillType.Code);
			}
		}

		public CarrierShipmentHeader PopulateParties(CarrierShipmentHeader carrierShipmentHeader)
		{
			var organizationAddressCollection = dataObject.OrganizationAddressCollection ?? new List<OrganizationAddress>();
			foreach (var organizationAddress in organizationAddressCollection)
			{
				new OrganisationDataObjectReader(organizationAddress, logger, factory).GetMatchedOrNew(carrierShipmentHeader);
			}

			return carrierShipmentHeader;
		}

		void PopulateContainers(CarrierShipmentHeader carrierShipmentHeader)
		{
			var containerCollection = dataObject.ContainerCollection ?? new DataObjectList<Container>();
			foreach (var cargoContainer in containerCollection)
			{
				new CarrierShipmentContainerDataObjectReader(cargoContainer, carrierShipmentHeader, containerLinkManager, IsInsert, logger, factory).ReadIntoBusinessObject();
			}
		}

		void PopulateBreakBulk(CarrierShipmentHeader carrierShipmentHeader)
		{
			var packingLineCollection = dataObject.PackingLineCollection ?? new DataObjectList<PackingLine>();
			foreach (var cargoBreakBulk in packingLineCollection)
			{
				new CarrierShipmentPackingLineDataObjectReader(cargoBreakBulk, carrierShipmentHeader, containerLinkManager, IsInsert, logger, factory).ReadIntoBusinessObject();
			}
		}

		protected override CarrierShipmentHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var dataTarget = dataObject.DataContext.DataTargetCollection.FirstOrDefault();

			if (dataTarget == null || !dataTarget.Key.HasValue)
			{
				return null;
			}

			return factory.LoadTop1<CarrierShipmentHeader>(new ZQuery(CarrierShipmentHeaderSchema.CSH_CarrierShipmentReference, dataTarget.Key));
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CarrierShipmentHeader carrierShipmentHeader)
		{
			if (dataObject.GoodsValue > 0 && string.IsNullOrEmpty(dataObject.GoodsValueCurrency?.Code))
			{
				return Res.GetString("2ed6bc52-818f-452b-ad2c-dcb6e1fa3306", "The XML document includes the Goods Value:{0}. However, the Goods Value Currency has not been specified.", dataObject.GoodsValue);
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(carrierShipmentHeader);
		}

		protected override IMatchingBusinessEntityFinder<CarrierShipmentHeader> GetCombinedReferenceMatcher()
		{
			return null;
		}

		readonly CarrierShipmentContainerLinkManager containerLinkManager;
	}
}
