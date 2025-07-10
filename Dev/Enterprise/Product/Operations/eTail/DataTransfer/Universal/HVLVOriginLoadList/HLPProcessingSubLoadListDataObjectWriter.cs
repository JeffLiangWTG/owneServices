using System.Linq;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using static Enterprise.Core.Constants;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using IncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using Shipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using ShipmentTypes = Enterprise.Core.Constants.ShipmentTypes;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public sealed class HLPProcessingSubLoadListDataObjectWriter : HLPProcessingLoadListDataObjectWriter
	{
		public HLPProcessingSubLoadListDataObjectWriter(SubLoadList subLoadList, IDataWritingManager manager)
			: base(manager)
		{
			this.subLoadList = Argument.NotNull(subLoadList, nameof(subLoadList));
		}

		readonly SubLoadList subLoadList;

		protected override void PopulateShipmentType(Shipment dataObject)
		{
			dataObject.ShipmentType = new CodeDescriptionPair() { Code = ShipmentTypes.HighVolumeLowValue };
		}

		protected override void PopulateDataObjectCore(HVLVOriginLoadList loadListBO, Shipment dataObject)
		{
			dataObject.IsMasterHouse = false;
			dataObject.ServiceLevel = ListHelper.GetWithDescription<ServiceLevel>(subLoadList.ServiceLevel, loadListBO.Lookups.ServiceLevels);
			if (loadListBO.HVL_IsMasterHouse)
			{
				dataObject.PortOfDestination = ListHelper.GetWithName(subLoadList.DestinationPort, loadListBO.Lookups.Destinations);
			}
			else
			{
				dataObject.PortOfDestination = new UNLOCO() { Code = loadListBO.HVL_RL_NKDestination };
			}

			var shipperAddress = GetShipperAddress(loadListBO);
			if (shipperAddress != null)
			{
				dataObject.PortOfOrigin = new UNLOCO() { Code = shipperAddress.RelatedPortCodeWithFallback };
			}

			dataObject.GoodsDescription = VariousCargo;

			dataObject.GoodsValue = subLoadList.ActiveItemsCalculatedGoodsValue;
			dataObject.GoodsValueCurrency = new Currency() { Code = subLoadList.ActiveItemsCalculatedGoodsValueCurrencyCode };
		}

		protected override OrgAddress GetShipperAddress(HVLVOriginLoadList loadListBO) => loadListBO.Factory.Load<OrgAddress>(subLoadList.BillToPartyPK);

		protected override void PopulatePackingLines(HVLVOriginLoadList originLoadListBO, Shipment dataObject)
		{
			var items = subLoadList.ActiveItems;
			var weightUnit = Env.Registry.FreightWeightUnit;
			var volumeUnit = Env.Registry.FreightVolumeUnit;

			dataObject.SetPackingLineCollection(() =>
			{
				var result = new DataObjectList<PackingLine>();

				var packingLine = new PackingLine(writeManager.WriterStrategy);
				packingLine.PackQty = items.Count();
				packingLine.WeightUnit = new UnitOfWeight() { Code = weightUnit };
				packingLine.VolumeUnit = new UnitOfVolume() { Code = volumeUnit };
				packingLine.Weight = TotalCalculation.GetTotalWeight(items, x => x.HVI_ActualWeight == 0 ? x.HVI_ManifestedWeight : x.HVI_ActualWeight, x => x.Consignment.HVC_WeightUQ, weightUnit);
				packingLine.Volume = TotalCalculation.GetTotalVolume(items, x => x.HVI_ActualVolume == 0 ? x.HVI_ManifestedVolume : x.HVI_ActualVolume, x => x.Consignment.HVC_VolumeUQ, volumeUnit);
				packingLine.PackType = new PackageType() { Code = items.OfType<HVLVItem>().SameOrDefault(item => item.HVI_F3_NKPackType).FallbackIfEmpty(PkgUnit.Package) };
				packingLine.ContainerLink = 1;
				packingLine.GoodsDescription = VariousCargo;

				result.Add(packingLine);
				return result;
			});
		}

		protected override IncoTerm GetIncoTerm(HVLVOriginLoadList loadListBO) => null;
	}
}
