using System.Linq;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	class HVLVOuterPackageWriter : DataObjectWriter<HVLVOuterPackage, PackingLine>
	{
		public HVLVOuterPackageWriter(IDataWritingManager manager, UniversalShipment headerShipment)
			: base(manager)
		{
			this.headerShipment = headerShipment;
		}

		readonly UniversalShipment headerShipment;

		protected override PackingLine PopulateDataObject(HVLVOuterPackage packageBO)
		{
			var packageDataObject = new PackingLine(writeManager.WriterStrategy);

			PopulateOuterPackageDetails(packageBO, packageDataObject);
			PopulateOrgAddresses(packageBO, packageDataObject);
			PopulatePackingLines(packageBO, packageDataObject);

			return packageDataObject;
		}

		void PopulateOuterPackageDetails(HVLVOuterPackage packageBO, PackingLine packageDataObject)
		{
			packageDataObject.IsHVLVClearance = true;
			packageDataObject.ReferenceNumber = packageBO.HVO_PackageReference;
			packageDataObject.Status = packageBO.HVO_Status;
			packageDataObject.Barcode = packageBO.HVO_PackageBarcode;
			packageDataObject.ContainerNumber = packageBO.HVO_ContainerNumber;
			packageDataObject.PackType = ListHelper.GetWithDescription<PackageType>(packageBO.HVO_F3_NKPackageType, packageBO.Lookups.PackageTypes);
			packageDataObject.Commodity = ListHelper.GetWithDescription<Commodity>(packageBO.HVO_RH_NKCommodityCode, packageBO.Lookups.CommodityCodes);

			packageDataObject.Volume = packageBO.HVO_Volume;
			packageDataObject.VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(packageBO.HVO_VolumeUQ, packageBO.Lookups.HVO_VolumeUQ_List);

			packageDataObject.Weight = CalculatePackageGrossWeight(packageBO);
			packageDataObject.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(packageBO.HVO_WeightUQ, packageBO.Lookups.HVO_WeightUQ_List);

			packageDataObject.TareWeight = packageBO.HVO_Weight;

			packageDataObject.Length = packageBO.HVO_Length;
			packageDataObject.Height = packageBO.HVO_Height;
			packageDataObject.Width = packageBO.HVO_Width;
			packageDataObject.LengthUnit = ListHelper.GetWithDescription<UnitOfLength>(packageBO.HVO_UnitOfDimension, packageBO.Lookups.HVO_UnitOfDimensionList);
			packageDataObject.PackQty = 1;
		}

		void PopulateOrgAddresses(HVLVOuterPackage packageBO, PackingLine packageDataObject)
		{
			packageDataObject.AddOrgAddress(writeManager, packageBO.DestinationDepot, DocAddressType.CustomsDepotAddress);
			packageDataObject.AddOrgAddress(writeManager, packageBO.DestinationDepot, DocAddressType.ArrivalCFSAddress);
			packageDataObject.AddOrgAddress(writeManager, packageBO.LastMileCarrier?.MainAddress, AddressTypes.DeliveryLocalCartage);
			packageDataObject.AddOrgAddress(writeManager, packageBO.LastMileCarrier?.MainAddress, AddressTypes.PickupLocalCartage);
		}

		void PopulatePackingLines(HVLVOuterPackage packageBO, PackingLine packageDataObject)
		{
			var outerPackageItems = packageBO.ActiveItems;
			if (outerPackageItems.Any())
			{
				var packingLines = ProcessCollection(outerPackageItems, new HVLVItemDataObjectWriter(writeManager, headerShipment), CollectionContent.Complete);
				if (packageDataObject.PackingLineCollection != null)
				{
					packageDataObject.PackingLineCollection.AddRange(packingLines);
				}
				else
				{
					packageDataObject.SetPackingLineCollection(() => packingLines.ToList());
				}
			}
		}

		ZDecimal CalculatePackageGrossWeight(HVLVOuterPackage packageBO)
		{
			var grossWeight = packageBO.HVO_Weight;

			foreach (var item in packageBO.ActiveItems)
			{
				var itemWeight = item.HVI_ActualWeight > 0 ? item.HVI_ActualWeight : item.HVI_ManifestedWeight;
				if (item.Consignment.HVC_WeightUQ != packageBO.HVO_WeightUQ)
				{
					itemWeight = Weight.Convert(itemWeight, item.Consignment.HVC_WeightUQ, packageBO.HVO_WeightUQ);
				}

				grossWeight += itemWeight;
			}

			return grossWeight;
		}
	}
}
