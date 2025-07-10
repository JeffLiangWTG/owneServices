using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.OceanCarrier.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.OceanCarrier.DataTransfer.Universal.Shipment
{
	sealed class CarrierShipmentPackingLineDataObjectWriter : DataObjectWriter<CarrierShipmentCargo, PackingLine>
	{
		readonly CarrierShipmentContainerLinkManager containerLinkManager;
		readonly CarrierShipmentHeader carrierShipmentHeader;

		public CarrierShipmentPackingLineDataObjectWriter(IDataWritingManager manager, CarrierShipmentContainerLinkManager carrierShipmentContainerLinkManager, CarrierShipmentHeader carrierShipmentHeader)
			: base(manager)
		{
			containerLinkManager = carrierShipmentContainerLinkManager;
			this.carrierShipmentHeader = carrierShipmentHeader;
		}

		protected override PackingLine PopulateDataObject(CarrierShipmentCargo carrierShipmentCargo)
		{
			var packingLineData = new PackingLine(writeManager.WriterStrategy);

			packingLineData.PackQty = new ZLong(carrierShipmentCargo.CSC_PieceCount);
			packingLineData.PackType = new PackageType { Code = carrierShipmentCargo.CSC_F3_NKPackType, Description = carrierShipmentCargo.PackType?.F3_DescriptionMultilingual };
			packingLineData.Commodity = new Commodity() { Code = carrierShipmentCargo.CSC_RH_NKCommodityCode, Description = carrierShipmentCargo.CommodityCode?.RH_DescriptionMultilingual };
			packingLineData.DetailedDescription = carrierShipmentCargo.CSC_DescriptionOfGoods;
			packingLineData.ReferenceNumber = carrierShipmentCargo.CSC_IdentificationReference;
			packingLineData.NonStackable = !carrierShipmentCargo.CSC_IsStackable;
			packingLineData.Weight = carrierShipmentCargo.CSC_ChargeableGrossWeight;
			packingLineData.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(carrierShipmentCargo.CSC_ChargeableGrossWeightUnit, carrierShipmentCargo.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight));
			packingLineData.Length = carrierShipmentCargo.CSC_ChargeableLength;
			packingLineData.LengthUnit = ListHelper.GetWithDescription<UnitOfLength>(carrierShipmentCargo.CSC_ChargeableUnitOfDimension, carrierShipmentCargo.Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length));
			packingLineData.Width = carrierShipmentCargo.CSC_ChargeableWidth;
			packingLineData.Height = carrierShipmentCargo.CSC_ChargeableHeight;

			if (!carrierShipmentCargo.CSC_IsTopLevel)
			{
				var parentLink = carrierShipmentCargo.Factory.LoadTop1<CarrierShipmentCargoLink>(new ZQuery(CarrierShipmentCargoLinkSchema.CCK_CSC_Child, carrierShipmentCargo.PK));
				if (parentLink != null)
				{
					var parentCargo = carrierShipmentHeader.Cargoes.FirstOrDefault(cargo => cargo.PK.Equals(parentLink.CCK_CSC_Parent));
					if (parentCargo.CSC_CargoType == Constants.ContainerModes.Containerised)
					{
						packingLineData.ContainerLink = containerLinkManager.GetContainerLink(parentCargo);
						packingLineData.ContainerNumber = parentCargo.CSC_EquipmentNo;
					}
				}
			}

			return packingLineData;
		}
	}
}