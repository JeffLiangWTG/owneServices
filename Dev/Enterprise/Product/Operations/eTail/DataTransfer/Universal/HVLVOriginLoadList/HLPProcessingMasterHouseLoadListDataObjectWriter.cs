using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public sealed class HLPProcessingMasterHouseLoadListDataObjectWriter : HLPProcessingLoadListDataObjectWriter
	{
		public HLPProcessingMasterHouseLoadListDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateShipmentType(UniversalShipment dataObject)
		{
			dataObject.ShipmentType = new CodeDescriptionPair() { Code = ShipmentTypes.HighVolumeLowValueMaster };
		}

		protected override void PopulateDataObjectCore(HVLVOriginLoadList originLoadListBO, UniversalShipment dataObject)
		{
			dataObject.WayBillNumber = originLoadListBO.HVL_HouseBillNumber;
			dataObject.WayBillType = ListHelper.GetWithDescription<WayBillType>(WayBillTypeList.Codes.House, new WayBillTypeList());
			if (originLoadListBO.OuterPackages.Count > 0)
			{
				dataObject.GoodsDescription = VariousCargo;
			}
			dataObject.TotalNoOfPacks = originLoadListBO.Items.Count;
		}

		protected override OrgAddress GetShipperAddress(HVLVOriginLoadList loadListBO) => loadListBO.OriginDepot;

		protected override void PopulatePackingLines(HVLVOriginLoadList originLoadListBO, UniversalShipment dataObject)
		{
			var outerPackages = originLoadListBO.OuterPackages;
			var weightUnit = Env.Registry.FreightWeightUnit;
			var volumeUnit = Env.Registry.FreightVolumeUnit;

			dataObject.SetPackingLineCollection(() =>
			{
				var result = new DataObjectList<PackingLine>();
				foreach (HVLVOuterPackage outerPackage in outerPackages)
				{
					var packingLine = new PackingLine(writeManager.WriterStrategy);
					packingLine.ContainerLink = 1;
					packingLine.Weight = outerPackage.HVO_Weight;
					packingLine.WeightUnit = new UnitOfWeight() { Code = outerPackage.HVO_WeightUQ };
					packingLine.Volume = outerPackage.HVO_Volume;
					packingLine.VolumeUnit = new UnitOfVolume() { Code = outerPackage.HVO_VolumeUQ };
					packingLine.PackType = new PackageType() { Code = outerPackage.HVO_F3_NKPackageType.IsEmpty ? PkgUnit.Package : outerPackage.HVO_F3_NKPackageType };
					packingLine.Height = outerPackage.HVO_Height;
					packingLine.Length = outerPackage.HVO_Length;
					packingLine.Width = outerPackage.HVO_Width;
					packingLine.LengthUnit = new UnitOfLength() { Code = outerPackage.HVO_UnitOfDimension };
					packingLine.Commodity = new Commodity() { Code = outerPackage.HVO_RH_NKCommodityCode };
					packingLine.ReferenceNumber = outerPackage.HVO_PackageBarcode.SubstringSafe(0, 46);
					packingLine.AddOrgAddress(writeManager, outerPackage.DestinationDepot, AddressTypes.LastKnownCFSFacility);
					packingLine.PackQty = 1;
					packingLine.GoodsDescription = VariousCargo;

					packingLine.SetUNDGCollection(() =>
					{
						var undgList = new List<UNDG>();
						var undgs = outerPackage.Items.SelectMany(item => ((HVLVItem)item).UNDGs);
						var undgWriter = new UNDGDataObjectWriter(writeManager);
						undgs.ForEach(undg => undgList.Add(undgWriter.GetDataObject(undg)));
						return undgList;
					});

					result.Add(packingLine);
				}

				return result;
			});
		}
	}
}
