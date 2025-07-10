using System.Collections.Generic;
using Enterprise.OceanCarrier.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using Container = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;

namespace Enterprise.OceanCarrier.DataTransfer.Universal.Shipment
{
	sealed class CarrierShipmentContainerDataObjectWriter : DataObjectWriter<CarrierShipmentCargo, Container>
	{
		readonly CarrierShipmentContainerLinkManager containerLinkManager;

		public CarrierShipmentContainerDataObjectWriter(IDataWritingManager manager, CarrierShipmentContainerLinkManager carrierShipmentContainerLinkManager)
			: base(manager)
		{
			this.containerLinkManager = carrierShipmentContainerLinkManager;
		}

		protected override Container PopulateDataObject(CarrierShipmentCargo carrierShipmentCargo)
		{
			var containerData = new Container(writeManager.WriterStrategy);

			containerData.ContainerCount = carrierShipmentCargo.CSC_PieceCount;
			containerData.ContainerType = ContainerType.New(carrierShipmentCargo.ChargeableEquipmentType);
			containerData.ContainerNumber = carrierShipmentCargo.CSC_EquipmentNo;
			containerData.Commodity = new Commodity() { Code = carrierShipmentCargo.CSC_RH_NKCommodityCode, Description = carrierShipmentCargo.CommodityCode?.RH_DescriptionMultilingual };
			containerData.GoodsDescription = carrierShipmentCargo.CSC_DescriptionOfGoods;
			containerData.IsEmptyContainer = carrierShipmentCargo.CSC_IsEmpty;
			containerData.IsShipperOwned = carrierShipmentCargo.CSC_IsShipperOwned;
			containerData.IsNonOperating = carrierShipmentCargo.CSC_ReeferNonOperated;
			containerData.GoodsWeight = carrierShipmentCargo.CSC_CargoWeight;
			containerData.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(carrierShipmentCargo.CSC_UnitOfWeight, carrierShipmentCargo.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight));
			containerData.GrossWeight = carrierShipmentCargo.GetTotalCargoGrossWeightMeasure();
			containerData.DunnageWeight = carrierShipmentCargo.CSC_DunnageWeight;
			containerData.TareWeight = carrierShipmentCargo.CSC_EquipmentTareWeight;

			containerData.OverhangBack = carrierShipmentCargo.CSC_OOGDoor;
			containerData.OverhangFront = carrierShipmentCargo.CSC_OOGFront;
			containerData.OverhangHeight = carrierShipmentCargo.CSC_OOGTop;
			containerData.OverhangLeft = carrierShipmentCargo.CSC_OOGLeft;
			containerData.OverhangRight = carrierShipmentCargo.CSC_OOGRight;
			containerData.LengthUnit = ListHelper.GetWithDescription<UnitOfLength>(carrierShipmentCargo.CSC_OOGUnit, carrierShipmentCargo.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length));

			containerData.Seal = carrierShipmentCargo.CSC_SealNumber1;
			containerData.SecondSeal = carrierShipmentCargo.CSC_SealNumber2;
			containerData.ThirdSeal = carrierShipmentCargo.CSC_SealNumber3;
			containerData.SealPartyType = ListHelper.GetWithDescription<CodeDescriptionPair>(carrierShipmentCargo.CSC_SealParty1, carrierShipmentCargo.Lookups.SealParty_List);
			containerData.SecondSealPartyType = ListHelper.GetWithDescription<CodeDescriptionPair>(carrierShipmentCargo.CSC_SealParty2, carrierShipmentCargo.Lookups.SealParty_List);
			containerData.ThirdSealPartyType = ListHelper.GetWithDescription<CodeDescriptionPair>(carrierShipmentCargo.CSC_SealParty3, carrierShipmentCargo.Lookups.SealParty_List);

			containerData.SetAdditionalSealNumberCollection(() => new List<SealNumber>()
			{
				new SealNumber() { Number = carrierShipmentCargo.CSC_SealNumber4 },
				new SealNumber() { Number = carrierShipmentCargo.CSC_SealNumber5 },
				new SealNumber() { Number = carrierShipmentCargo.CSC_SealNumber6 }
			});

			containerData.GrossWeightVerificationDateTime = carrierShipmentCargo.CSC_VGMWeighingDateTime.ToZDateTime();
			containerData.GrossWeightVerificationType = ListHelper.GetWithDescription<CodeDescriptionPair>(carrierShipmentCargo.CSC_VGMWeighingMethod, carrierShipmentCargo.Lookups.GrossWeightVerificationTypeList);
			containerData.Link = containerLinkManager?.GetContainerLink(carrierShipmentCargo) ?? 0;

			return containerData;
		}
	}
}