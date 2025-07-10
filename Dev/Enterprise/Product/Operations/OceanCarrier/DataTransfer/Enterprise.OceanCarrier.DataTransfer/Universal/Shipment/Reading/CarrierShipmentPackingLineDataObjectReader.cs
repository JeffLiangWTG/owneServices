using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.OceanCarrier.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.OceanCarrier.DataTransfer.Universal.Shipment
{
	class CarrierShipmentPackingLineDataObjectReader : DataObjectReader<PackingLine, CarrierShipmentCargo>
	{
		public CarrierShipmentPackingLineDataObjectReader(
			PackingLine dataObject,
			CarrierShipmentHeader carrierShipmentHeader,
			CarrierShipmentContainerLinkManager containerLinkManager,
			bool isInsertCarrierShipmentHeader,
			IXmlImportLogger logger,
			UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			this.carrierShipmentHeader = carrierShipmentHeader;
			this.containerLinkManager = containerLinkManager;
			this.isInsertCarrierShipmentHeader = isInsertCarrierShipmentHeader;
		}

		protected override CarrierShipmentCargo GetExistingBusinessObject()
		{
			if (isInsertCarrierShipmentHeader)
			{
				return null;
			}

			var matchedBreakBulkList = carrierShipmentHeader.GetMatchedBreakBulkList(dataObject.ReferenceNumber.GetValueOrDefault());

			return matchedBreakBulkList;
		}

		protected override void PopulateBusinessObject(CarrierShipmentCargo carrierShipmentCargo)
		{
			PopulateBreakBulk(carrierShipmentCargo);
			PopulateContainerBreakBulkCargoLink(carrierShipmentCargo);
		}

		void PopulateBreakBulk(CarrierShipmentCargo carrierShipmentCargo)
		{
			if (!IsBreakBulk(dataObject))
			{
				return;
			}

			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_CSH_CarrierShipment, carrierShipmentHeader.PK);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_CargoMovementTypeDestination, "BB");
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_CargoMovementTypeOrigin, "BB");
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_CargoType, Constants.ContainerModes.BreakBulk);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_PieceCount, dataObject.PackQty);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_F3_NKPackType, dataObject.PackType?.Code);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_RH_NKCommodityCode, dataObject.Commodity?.Code);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_DescriptionOfGoods, dataObject.DetailedDescription);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_IdentificationReference, dataObject.ReferenceNumber);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_IsStackable, !dataObject.NonStackable);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_CargoWeight, dataObject.Weight);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_UnitOfWeight, dataObject.WeightUnit?.Code);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_ChargeableLength, dataObject.Length);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_ChargeableUnitOfDimension, dataObject.LengthUnit?.Code);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_ChargeableWidth, dataObject.Width);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_ChargeableHeight, dataObject.Height);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_DunnageWeight, dataObject.DunnageWeight);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_DeliveryDrayage, string.Empty);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_ReceiptDrayage, string.Empty);
		}

		void PopulateContainerBreakBulkCargoLink(CarrierShipmentCargo carrierShipmentCargo)
		{
			var parentContainerId = GetParentContainer();

			if (parentContainerId == ZGuid.Empty)
			{
				SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_IsTopLevel, ZBool.True);
			}
			else
			{
				SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_IsTopLevel, ZBool.False);
				PopulateCargoLink(parentContainerId, carrierShipmentCargo.PK);
			}
		}

		void PopulateCargoLink(ZGuid parentContainerId, ZGuid childContainerId)
		{
			var parentChildCargoLink = factory.LoadTop1<CarrierShipmentCargoLink>(
				new ZQuery(CarrierShipmentCargoLinkSchema.CCK_CSC_Child, childContainerId));

			if (parentChildCargoLink == null)
			{
				var carrierShipmentCargoLink = factory.New<CarrierShipmentCargoLink>();
				SetValue(carrierShipmentCargoLink, CarrierShipmentCargoLinkSchema.CCK_CSC_Parent, parentContainerId);
				SetValue(carrierShipmentCargoLink, CarrierShipmentCargoLinkSchema.CCK_CSC_Child, childContainerId);
			}
			else if (parentChildCargoLink.CCK_CSC_Parent != parentContainerId)
			{
				SetValue(parentChildCargoLink, CarrierShipmentCargoLinkSchema.CCK_CSC_Parent, parentContainerId);
			}
		}

		ZGuid GetParentContainer()
		{
			var parentContainer = containerLinkManager.GetContainer(dataObject.ContainerLink ?? ZInt.Zero);

			return parentContainer?.PK ?? ZGuid.Empty;
		}

		ZBool IsBreakBulk(PackingLine packingLine)
		{
			if (packingLine.Vehicle == null)
			{
				return true;
			}

			return !(packingLine.Vehicle.Color.HasValue
				|| packingLine.Vehicle.Make.HasValue
				|| packingLine.Vehicle.Model.HasValue
				|| packingLine.Vehicle.NumberOfDoors.HasValue
				|| packingLine.Vehicle.Transmission.Code.HasValue
				|| packingLine.Vehicle.Year.HasValue
				|| packingLine.Vehicle.VehicleType.Code.HasValue
				|| packingLine.Vehicle.Registration.Number.HasValue);
		}

		readonly CarrierShipmentHeader carrierShipmentHeader;

		readonly CarrierShipmentContainerLinkManager containerLinkManager;

		protected override LogType LogTypeForReasonNotAbleToUpdate => LogType.Warning;

		readonly bool isInsertCarrierShipmentHeader;
	}
}
