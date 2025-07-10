using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.OceanCarrier.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;

namespace Enterprise.OceanCarrier.DataTransfer.Universal.Shipment
{
	class CarrierShipmentContainerDataObjectReader : DataObjectReader<UniversalContainer, CarrierShipmentCargo>
	{
		public CarrierShipmentContainerDataObjectReader(
			UniversalContainer dataObject,
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

			var matchedContainerList = carrierShipmentHeader.GetMatchedContainerList(dataObject.ContainerNumber.GetValueOrDefault());

			return matchedContainerList;
		}

		protected override void PopulateBusinessObject(CarrierShipmentCargo carrierShipmentCargo)
		{
			PopulateContainer(carrierShipmentCargo);
			PopulateContainerLink(carrierShipmentCargo);
		}

		void PopulateContainerLink(CarrierShipmentCargo carrierShipmentCargo)
		{
			if (carrierShipmentCargo != null)
			{
				containerLinkManager.CollectContainerLink(carrierShipmentCargo, dataObject);
			}
		}

		void PopulateContainer(CarrierShipmentCargo carrierShipmentCargo)
		{
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_CSH_CarrierShipment, carrierShipmentHeader.PK);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_CargoMovementTypeDestination, Constants.ContainerModes.FCL);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_CargoMovementTypeOrigin, Constants.ContainerModes.FCL);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_IsTopLevel, ZBool.True);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_PieceCount, dataObject.ContainerCount);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_EquipmentNo, dataObject.ContainerNumber);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_RH_NKCommodityCode, dataObject.Commodity?.Code);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_DescriptionOfGoods, dataObject.GoodsDescription);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_IsEmpty, dataObject.IsEmptyContainer);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_IsShipperOwned, dataObject.IsShipperOwned);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_ReeferNonOperated, dataObject.IsNonOperating);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_CargoWeight, dataObject.GoodsWeight);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_UnitOfWeight, dataObject.WeightUnit?.Code);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_DunnageWeight, dataObject.DunnageWeight);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_EquipmentTareWeight, dataObject.TareWeight);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_OOGDoor, dataObject.OverhangBack);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_OOGFront, dataObject.OverhangFront);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_OOGTop, dataObject.OverhangHeight);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_OOGLeft, dataObject.OverhangLeft);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_OOGRight, dataObject.OverhangRight);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_OOGLeft, dataObject.OverhangLeft);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_OOGUnit, dataObject.LengthUnit?.Code);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_SealNumber1, dataObject.Seal);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_SealNumber2, dataObject.SecondSeal);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_SealNumber3, dataObject.ThirdSeal);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_SealParty1, dataObject.SealPartyType?.Code);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_SealParty2, dataObject.SecondSealPartyType?.Code);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_SealParty3, dataObject.ThirdSealPartyType?.Code);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_VGMWeighingDateTime,
				dataObject.GrossWeightVerificationDateTime.HasValue && dataObject.GrossWeightVerificationDateTime != ZDateTime.Empty
					? new DateTimeOffset(dataObject.GrossWeightVerificationDateTime.Value.ToDateTime())
					: ZDateTimeOffset.Empty);
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_VGMWeighingMethod, dataObject.GrossWeightVerificationType);

			var refContainer = new RefContainer.Loader(factory.BOFactory).LoadFromCode((dataObject.ContainerType?.Code).GetValueOrDefault());
			if (refContainer != null)
			{
				SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_RC_ChargeableEquipmentType, refContainer.PK);
			}
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_DeliveryDrayage,"ANY");
			SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_ReceiptDrayage, "ANY");
			SetAdditionalSealNumbers(carrierShipmentCargo);
		}

		void SetAdditionalSealNumbers(CarrierShipmentCargo carrierShipmentCargo)
		{
			var index = 0;
			var additionalSealNumberCollection = dataObject.AdditionalSealNumberCollection ?? new List<SealNumber>();
			foreach (var additionalSealNumber in additionalSealNumberCollection)
			{
				switch (index)
				{
					case 0:
						SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_SealNumber4, additionalSealNumber.Number);
						break;
					case 1:
						SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_SealNumber5, additionalSealNumber.Number);
						break;
					case 2:
						SetValue(carrierShipmentCargo, CarrierShipmentCargoSchema.CSC_SealNumber6, additionalSealNumber.Number);
						break;
				}
				index++;
			}
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CarrierShipmentCargo carrierShipmentCargo)
		{
			if (dataObject?.Link == null || dataObject.Link <= 0)
			{
				return Res.GetString("037c3e52-e640-4179-a33c-396963d80c4b", "Container link must be valid value in {0}.", carrierShipmentHeader.CSH_CarrierShipmentReference);
			}

			if (containerLinkManager.GetContainer(dataObject.Link.Value) != null)
			{
				return Res.GetString("e94cbec9-ee30-4d24-b0f2-e0a6bd22f838", "Duplicated container link {0}.", dataObject.Link.Value);
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(carrierShipmentCargo);
		}

		protected override LogType LogTypeForReasonNotAbleToUpdate => LogType.Warning;

		readonly CarrierShipmentHeader carrierShipmentHeader;

		readonly CarrierShipmentContainerLinkManager containerLinkManager;

		readonly bool isInsertCarrierShipmentHeader;
	}
}
