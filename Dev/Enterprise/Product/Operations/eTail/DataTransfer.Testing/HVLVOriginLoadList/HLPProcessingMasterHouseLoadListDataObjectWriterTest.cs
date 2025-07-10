using System.Linq;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HLPProcessingMasterHouseLoadListDataObjectWriter))]
	class HLPProcessingMasterHouseLoadListDataObjectWriterTest : HLPProcessingLoadListDataObjectWriterTest
	{
		public void TestWriteData()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_MasterBillNumber = "111222333";
			loadList.HVL_HouseBillNumber = "444555666";

			var dataObject = GetDataObject(loadList);

			CombineAssertions("Data is populated for HVM shipment matching", () =>
			{
				AssertEquals("444555666", dataObject.WayBillNumber);
				AssertEquals("HWB", dataObject.WayBillType.Code);
				AssertNull(dataObject.GoodsDescription);
				AssertEquals(0, dataObject.TotalNoOfPacks);
				AssertEquals("Shipment type should be HVM", ShipmentTypes.HighVolumeLowValueMaster, dataObject.ShipmentType.Code);
			});

			var outerPackage = loadList.OuterPackages.AddNew();
			loadList.Items.AddNew();
			dataObject = GetDataObject(loadList);

			CombineAssertions("After adding the outerpackage and item", () =>
			{
				AssertEquals("Various Cargo", dataObject.GoodsDescription);
				AssertEquals(1, dataObject.TotalNoOfPacks);
			});
		}

		public void TestWriteData_PopulatedConsignorConsigneeAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";

			var consignorAddress = org.Addresses.AddNew();
			consignorAddress.OA_Address1 = "111";

			var consigneeAddress = org.Addresses.AddNew();
			consigneeAddress.OA_Address1 = "222";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_OriginDepot = consignorAddress.PK;
			loadList.HVL_OA_DestinationDepot = consigneeAddress.PK;

			Factory.Save();

			var dataObject = GetDataObject(loadList);

			var consignorAddressDO = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsignorDocumentaryAddress));
			var consigneeAddressDO = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsigneeDocumentaryAddress));

			CombineAssertions("Consignor Consignee Addresses are populated", () =>
			{
				AssertNotNull(consignorAddressDO);
				AssertNotNull(consigneeAddressDO);

				AssertEquals("111", consignorAddressDO.Address1);
				AssertEquals("222", consigneeAddressDO.Address1);
			});
		}

		public void TestWriteData_PopulatePackingLines()
		{
			var destinationDepotAddress = Factory.NewWithValidTestData<OrgAddress>();

			var loadList = Factory.New<HVLVOriginLoadList>();

			var outerPackage1 = loadList.OuterPackages.AddNew();
			outerPackage1.HVO_Weight = 1;
			outerPackage1.HVO_WeightUQ = "KG";
			outerPackage1.HVO_Volume = 1;
			outerPackage1.HVO_VolumeUQ = "M3";
			outerPackage1.HVO_F3_NKPackageType = "PKG";
			outerPackage1.HVO_Height = 1;
			outerPackage1.HVO_Length = 1;
			outerPackage1.HVO_Width = 1;
			outerPackage1.HVO_UnitOfDimension = "M";
			outerPackage1.HVO_RH_NKCommodityCode = "KFC";
			outerPackage1.HVO_PackageBarcode = "123456";
			outerPackage1.HVO_OA_DestinationDepot = destinationDepotAddress.PK;
			outerPackage1.HVO_ContainerNumber = "container1";

			var outerPackage2 = loadList.OuterPackages.AddNew();
			outerPackage2.HVO_Weight = 2;
			outerPackage2.HVO_Volume = 2;
			outerPackage2.HVO_F3_NKPackageType = "PKG";
			outerPackage2.HVO_Height = 2;
			outerPackage2.HVO_Length = 2;
			outerPackage2.HVO_Width = 2;
			outerPackage2.HVO_UnitOfDimension = "M";
			outerPackage2.HVO_RH_NKCommodityCode = "AAA";
			outerPackage2.HVO_PackageBarcode = "234567";
			outerPackage2.HVO_OA_DestinationDepot = destinationDepotAddress.PK;
			outerPackage2.HVO_ContainerNumber = "container2";

			var consignment1 = Factory.New<HVLVConsignment>();
			consignment1.HVC_WeightUQ = "KG";
			consignment1.HVC_VolumeUQ = "M3";

			var item1 = consignment1.Items.AddNew();
			item1.HVI_ActualWeight = 1;
			item1.HVI_ActualVolume = 1;
			item1.HVI_HVO_OuterPackage = outerPackage1.PK;

			var substance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance1.DG_Code = "001";
			var substance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance2.DG_Code = "002";
			item1.UNDGs.AddNew().DI_DG = substance1.PK;
			item1.UNDGs.AddNew().DI_DG = substance2.PK;

			var item2 = consignment1.Items.AddNew();
			item2.HVI_ManifestedWeight = 1;
			item2.HVI_ManifestedVolume = 1;
			item2.HVI_HVO_OuterPackage = outerPackage2.PK;

			var dataObject = GetDataObject(loadList);

			var packlines = dataObject.PackingLineCollection;

			CombineAssertions("Packline details", () => {
				AssertEquals("2 packline created", 2, packlines.Count);

				var firstPackLine = packlines.First(n => n.Commodity.Code.Value == "KFC");
				AssertEquals("Weight", 1m, firstPackLine.Weight);
				AssertEquals("Weight Unit", "KG", firstPackLine.WeightUnit.Code);
				AssertEquals("Volume", 1m, firstPackLine.Volume);
				AssertEquals("Volume Unit", "M3", firstPackLine.VolumeUnit.Code);
				AssertEquals("Pack Type", "PKG", firstPackLine.PackType.Code);
				AssertEquals("Height", 1m, firstPackLine.Height);
				AssertEquals("Length", 1m, firstPackLine.Length);
				AssertEquals("Width", 1m, firstPackLine.Width);
				AssertEquals("Length Unit", "M", firstPackLine.LengthUnit.Code);
				AssertEquals("Reference Number", "123456", firstPackLine.ReferenceNumber);
				AssertEquals("LastKnownTransitWarehouseAddress", destinationDepotAddress.AddressCode,
					firstPackLine.OrganizationAddressCollection.
					First(n => n.AddressType.ToString() == AddressTypes.LastKnownCFSFacility).
					AddressShortCode);
				AssertEquals("Pack Qty", (ZLong)1, firstPackLine.PackQty);
				AssertEquals("ContainerLink", 1, firstPackLine.ContainerLink);
				AssertEquals("UNDGCollection Count", 2, firstPackLine.UNDGCollection.Count);
				AssertContainsExactElementsInAnyOrder("UNDGCode", new[] { "001", "002" }, firstPackLine.UNDGCollection.Select(x => x.UNDGCode.ToString()));
				var secondPackLine = packlines.First(n => n.Commodity.Code.Value == "AAA");
				AssertEquals("Weight", 2m, secondPackLine.Weight);
				AssertEquals("Weight Unit", "KG", secondPackLine.WeightUnit.Code);
				AssertEquals("Volume", 2m, secondPackLine.Volume);
				AssertEquals("Volume Unit", "M3", secondPackLine.VolumeUnit.Code);
				AssertEquals("Pack Type", "PKG", secondPackLine.PackType.Code);
				AssertEquals("Height", 2m, secondPackLine.Height);
				AssertEquals("Length", 2m, secondPackLine.Length);
				AssertEquals("Width", 2m, secondPackLine.Width);
				AssertEquals("Volume Unit", "M", secondPackLine.LengthUnit.Code);
				AssertEquals("Reference Number", "234567", secondPackLine.ReferenceNumber);
				AssertEquals("LastKnownTransitWarehouseAddress", destinationDepotAddress.AddressCode,
					firstPackLine.OrganizationAddressCollection.
					First(n => n.AddressType.ToString() == AddressTypes.LastKnownCFSFacility).
					AddressShortCode);
				AssertEquals("Pack Qty", (ZLong)1m, secondPackLine.PackQty);
				AssertEquals("ContainerLink", 1, secondPackLine.ContainerLink);
			});
		}

		public void TestGetLooseItems()
		{
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();

			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_HVO_OuterPackage = outerPackage.PK;
			item1.HVI_ItemId = "item1";

			var item2 = Factory.NewWithValidTestData<HVLVItem>();
			item2.HVI_HVO_OuterPackage = outerPackage.PK;
			item2.HVI_ItemId = "item2";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.OuterPackages.Add(outerPackage);

			var item3 = Factory.NewWithValidTestData<HVLVItem>();
			item3.HVI_HVL_LoadList = loadList.PK;
			item3.HVI_ItemId = "item3";

			var item4 = Factory.NewWithValidTestData<HVLVItem>();
			item4.HVI_HVL_LoadList = loadList.PK;
			item4.HVI_ItemId = "item4";

			Factory.Save();

			var dataObject = GetDataObject(loadList);
			AssertEquals(dataObject.PackingLineCollection.Count, 1);
		}

		protected override Shipment GetDataObject(HVLVOriginLoadList loadList)
		{
			var writer = new HLPProcessingMasterHouseLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList)));
			return writer.GetDataObject(loadList);
		}
	}
}
