using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HLPProcessingSubLoadListDataObjectWriter))]
	class HLPProcessingSubLoadListDataObjectWriterTest : HLPProcessingLoadListDataObjectWriterTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Sub load list cannot be null", () =>
			{
				new HLPProcessingSubLoadListDataObjectWriter(subLoadList: null, new DataWritingManager(new ActionInfo(null, null)));
			});
		}

		public void TestWriteData()
		{
			var loadList = GetLoadListForTesting();

			using (loadList.Split())
			{
				AssertEquals("2 sub load lists", 2, loadList.SubLoadLists.Count);

				var dataObjectCrticalInfo = loadList.SubLoadLists.Select(subLoadList => WriteToDataObjectAndGetCriticalData(subLoadList));

				AssertContainsExactElementsInAnyOrder("Should populate sub xus",
					new[]
					{
						@"IsMasterHouse:N,
ShipmentType:HVL,
ServiceLevel:STD,
OriginPort:AUSYD,
DestinationPort:USLAX,
PackingLinesCount:1,
GoodsDescription:Various Cargo,
ContainerMode:LSE,
GoodsValue:15.0,
GoodsValueCurrency:USD",
						@"IsMasterHouse:N,
ShipmentType:HVL,
ServiceLevel:EXP,
OriginPort:AUSYD,
DestinationPort:USLAX,
PackingLinesCount:1,
GoodsDescription:Various Cargo,
ContainerMode:LSE,
GoodsValue:0,
GoodsValueCurrency:",
					}, dataObjectCrticalInfo);
			}
		}

		public void TestWriteData_MasterHouse()
		{
			var loadList = GetLoadListForTesting();
			loadList.HVL_IsMasterHouse = true;

			using (loadList.Split())
			{
				AssertEquals("Pre-req: 3 sub load lists", 3, loadList.SubLoadLists.Count);

				var dataObjectCrticalInfo = loadList.SubLoadLists.Select(subLoadList => WriteToDataObjectAndGetCriticalData(subLoadList));

				AssertContainsExactElementsInAnyOrder("Should populate sub xus",
					new[]
					{
						@"IsMasterHouse:N,
ShipmentType:HVL,
ServiceLevel:STD,
OriginPort:AUSYD,
DestinationPort:AUSYD,
PackingLinesCount:1,
GoodsDescription:Various Cargo,
ContainerMode:LSE,
GoodsValue:10,
GoodsValueCurrency:AUD",
						@"IsMasterHouse:N,
ShipmentType:HVL,
ServiceLevel:STD,
OriginPort:AUSYD,
DestinationPort:,
PackingLinesCount:1,
GoodsDescription:Various Cargo,
ContainerMode:LSE,
GoodsValue:10,
GoodsValueCurrency:USD",
						@"IsMasterHouse:N,
ShipmentType:HVL,
ServiceLevel:EXP,
OriginPort:AUSYD,
DestinationPort:,
PackingLinesCount:1,
GoodsDescription:Various Cargo,
ContainerMode:LSE,
GoodsValue:0,
GoodsValueCurrency:",
					}, dataObjectCrticalInfo);
			}
		}

		public void TestWriteData_WaybillNumberIsNotPopulated()
		{
			var loadList = GetLoadListForTesting();

			using (loadList.Split())
			{
				var subLoadList = loadList.SubLoadLists[0];
				var subLoadListXUS = new HLPProcessingSubLoadListDataObjectWriter(subLoadList, new DataWritingManager(new ActionInfo(null, subLoadList.ActualLoadList))).GetDataObject(subLoadList.ActualLoadList);

				CombineAssertions("Waybill is not populated for sub load list XUS", () =>
				{
					AssertNullOrEmpty(subLoadListXUS.WayBillNumber);
					AssertNullOrEmpty(subLoadListXUS.WayBillType?.Code);
				});
			}
		}

		public void TestWriteData_IncoTermIsNotPopulated()
		{
			var loadList = GetLoadListForTesting();

			using (loadList.Split())
			{
				var subLoadList = loadList.SubLoadLists[0];
				var subLoadListXUS = new HLPProcessingSubLoadListDataObjectWriter(subLoadList, new DataWritingManager(new ActionInfo(null, subLoadList.ActualLoadList))).GetDataObject(subLoadList.ActualLoadList);

				AssertNull(subLoadListXUS.ShipmentIncoTerm);
			}
		}

		public void TestWriteData_PopulatedConsignorConsigneeAddress()
		{
			var loadList = GetLoadListForTesting();

			var org = Factory.New<OrgHeader>();
			var destination = org.Addresses.AddNew();
			destination.Address1 = "Address";

			loadList.HVL_OA_DestinationDepot = destination.PK;

			using (loadList.Split())
			{
				var subLoadList = loadList.SubLoadLists[0];
				var subLoadListXUS = new HLPProcessingSubLoadListDataObjectWriter(subLoadList, new DataWritingManager(new ActionInfo(null, subLoadList.ActualLoadList))).GetDataObject(subLoadList.ActualLoadList);

				var consignorAddressDO = subLoadListXUS.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsignorDocumentaryAddress));
				var consigneeAddressDO = subLoadListXUS.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsigneeDocumentaryAddress));

				CombineAssertions("Consignor Consignee Addresses are populated", () =>
				{
					AssertNotNull("consignor address", consignorAddressDO);
					AssertNotNull("consignee address", consigneeAddressDO);

					AssertEquals("DMY", consignorAddressDO.Address1);
					AssertEquals("Address", consigneeAddressDO.Address1);
				});
			}
		}

		public void TestWriteData_PopulatePackingLines()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();

			var consignment1 = Factory.New<HVLVConsignment>();
			consignment1.HVC_WeightUQ = "KG";
			consignment1.HVC_VolumeUQ = "M3";

			var item1 = consignment1.Items.AddNew();
			item1.HVI_ActualWeight = 1;
			item1.HVI_ActualVolume = 1;

			var item2 = consignment1.Items.AddNew();
			item2.HVI_ManifestedWeight = 1;
			item2.HVI_ManifestedVolume = 1;

			var subLoadList = new SubLoadList(loadList, new HVLVItem[] { item1, item2 }, Guid.Empty, "STD");
			var subLoadListXUS = new HLPProcessingSubLoadListDataObjectWriter(subLoadList, new DataWritingManager(new ActionInfo(null, subLoadList.ActualLoadList))).GetDataObject(subLoadList.ActualLoadList);

			var packlines = subLoadListXUS.PackingLineCollection;
			AssertEquals("1 packline created", 1, packlines.Count);
			var packLine = packlines[0];
			CombineAssertions("Packline details", () => {
				AssertEquals("Weight", 2m, packLine.Weight);
				AssertEquals("Weight Unit", "KG", packLine.WeightUnit.Code);
				AssertEquals("Volume", 2m, packLine.Volume);
				AssertEquals("Volume Unit", "M3", packLine.VolumeUnit.Code);
				AssertEquals("Pack Type", "PKG", packLine.PackType.Code);
				AssertEquals("PackQty", (ZLong)2, packLine.PackQty);
				AssertEquals("ContainerLink", 1, packLine.ContainerLink);
			});
		}

		protected override Shipment GetDataObject(HVLVOriginLoadList loadList)
		{
			var subLoadList = new SubLoadList(loadList, Array.Empty<HVLVItem>(), Guid.Empty, "STD");
			var writer = new HLPProcessingSubLoadListDataObjectWriter(subLoadList, new DataWritingManager(new ActionInfo(null, subLoadList.ActualLoadList)));
			return writer.GetDataObject(subLoadList.ActualLoadList);
		}

		HVLVOriginLoadList GetLoadListForTesting()
		{
			HVLVTestHelper.SetExchangeRate(Factory, "USD", 0.5m);
			var loadListDestination = Factory.NewWithValidTestData<OrgHeader>();
			var loadListDestinationAddress = loadListDestination.Addresses.AddNew();
			loadListDestinationAddress.Address1 = "Address1";
			loadListDestinationAddress.OA_RL_NKRelatedPortCode = "USLAX";

			HVLVTestHelper.SetRefZoneHeader(loadListDestination.PK, Factory);
			Factory.Save();

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_DestinationDepot = loadListDestinationAddress.PK;
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_INCO = "FOB";

			var billToParty = Factory.NewWithValidTestData<OrgAddress>();
			billToParty.OA_RL_NKRelatedPortCode = "AUSYD";
			billToParty.OA_Address1 = "DMY";

			var destinationDepot1 = Factory.New<OrgAddress>();
			destinationDepot1.OA_RL_NKRelatedPortCode = "AUSYD";
			destinationDepot1.OA_Address1 = "2 Destination Depot Street";

			var destinationDepot2 = Factory.New<OrgAddress>();
			destinationDepot2.OA_RL_NKRelatedPortCode = "NZAKL";
			destinationDepot2.OA_Address1 = "3 Destination Depot Street";

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "STD";

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HVH_BookingHeader = bookingHeader1.PK;
			consignment1.HVC_OA_DestinationDepot = destinationDepot1.PK;
			consignment1.HVC_RX_NKGoodsValueCurrency = "AUD";
			consignment1.HVC_GoodsValue = 10;
			consignment1.Items.AddNew().HVI_HVL_LoadList = loadList.PK;
			consignment1.Items.AddNew().HVI_HVL_LoadList = loadList.PK;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader2.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader2.HVH_RS_NKBookingServiceLevel = "STD";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_HVH_BookingHeader = bookingHeader2.PK;
			consignment2.HVC_OA_DestinationDepot = destinationDepot2.PK;
			consignment2.HVC_RX_NKGoodsValueCurrency = "USD";
			consignment2.HVC_GoodsValue = 10;
			consignment2.Items.AddNew().HVI_HVL_LoadList = loadList.PK;

			var bookingHeader3 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader3.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader3.HVH_RS_NKBookingServiceLevel = "EXP";

			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment3.HVC_HVH_BookingHeader = bookingHeader3.PK;
			consignment3.HVC_OA_DestinationDepot = destinationDepot2.PK;
			consignment3.Items.AddNew().HVI_HVL_LoadList = loadList.PK;

			return loadList;
		}

		string WriteToDataObjectAndGetCriticalData(SubLoadList subLoadList)
		{
			var dataObjectWriter = new HLPProcessingSubLoadListDataObjectWriter(subLoadList, new DataWritingManager(new ActionInfo(null, subLoadList.ActualLoadList)));
			var dataObject = dataObjectWriter.GetDataObject(subLoadList.ActualLoadList);

			return @$"IsMasterHouse:{dataObject.IsMasterHouse},
ShipmentType:{dataObject.ShipmentType.Code},
ServiceLevel:{dataObject.ServiceLevel.Code},
OriginPort:{dataObject.PortOfOrigin.Code},
DestinationPort:{dataObject.PortOfDestination.Code},
PackingLinesCount:{dataObject.PackingLineCollection.Count},
GoodsDescription:{dataObject.GoodsDescription},
ContainerMode:{dataObject.ContainerMode.Code},
GoodsValue:{dataObject.GoodsValue},
GoodsValueCurrency:{dataObject.GoodsValueCurrency.Code}";
		}
	}
}
