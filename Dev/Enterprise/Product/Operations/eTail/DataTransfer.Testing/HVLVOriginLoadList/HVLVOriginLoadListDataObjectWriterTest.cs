using System;
using System.Linq;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class HVLVOriginLoadListDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWriteGeneralDataToDataObject()
		{
			var loadList = HVLVOriginLoadListDataContextManagerTest.SampleLoadList();
			var dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).GetDataObject(loadList);

			CombineAssertions(() =>
			{
				AssertEquals("Expected HVL_TransportMode written to dataObject.TransportMode", "SEA", dataObject.TransportMode.Code);
				AssertEquals("Expected HVL_HVL_VesselName written to dataObject.VesselName", "AIDA", dataObject.VesselName);
				AssertEquals("Expected HVL_VoyageFlight written to dataObject.VoyageFlightNo", "VOYAGE001", dataObject.VoyageFlightNo);
				AssertEquals("Expected HVL_Status written to dataObject.OperationalStatus", HVLVOriginLoadListStatus.Codes.Open, dataObject.OperationalStatus.Code);
				AssertEquals("Expected HVL_ServiceLevel written to dataObject.ServiceLevel", "STD", dataObject.ServiceLevel.Code);
				AssertEquals("Expected HVL_INCO written to dataObject.ShipmentIncoTerm", "FC1", dataObject.ShipmentIncoTerm.Code);
			});
		}

		public void TestWriteContainerToDataObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = "LSE";

			var loadList = Factory.New<HVLVOriginLoadList>();
			loadList.HVL_ContainerNumber = "12456543";
			loadList.HVL_TransportMode = "SEA";

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = ContainerTypes.Tank;
			loadList.HVL_RC_ContainerType = refContainer.PK;

			var dataObjectWriter = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList)));
			var dataObject = dataObjectWriter.GetDataObject(loadList);

			var containers = dataObject.ContainerCollection;
			AssertEquals(1, containers.Count);

			var container = containers[0];
			CombineAssertions("container details", () =>
			{
				AssertEquals("Link", 1, container.Link);
				AssertEquals("Container Number", "12456543", container.ContainerNumber);
				AssertEquals("Container Type", "TNK", container.ContainerType.Code);
				AssertEquals("Container Mode", "GRP", container.FCL_LCL_AIR.Code);
				AssertEquals("Delivery Mode", "CFS/CFS", container.DeliveryMode);
			});

			loadList.HVL_ContainerNumber = "";
			loadList.HVL_RC_ContainerType = Guid.Empty;

			dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).GetDataObject(loadList);
			AssertNull("Don't create container if loadlist container number and container type are empty", dataObject.ContainerCollection);
		}

		public void TestWriteDateCollectionToDataObject()
		{
			var loadList = HVLVOriginLoadListDataContextManagerTest.SampleLoadList();
			var dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).GetDataObject(loadList);

			var arrivalDate = dataObject.DateCollection.FirstOrDefault(d => d.Type == DateType.Arrival);
			var departureDate = dataObject.DateCollection.FirstOrDefault(d => d.Type == DateType.Departure);

			CombineAssertions(() =>
			{
				AssertEquals("Expected HVL_E_Arv written to dataObject.DateCollection", loadList.HVL_E_Arv.ToString(), arrivalDate.Value.ToString());
				AssertEquals("Expected HVL_E_Dep written to dataObject.DateCollection", loadList.HVL_E_Dep.ToString(), departureDate.Value.ToString());
			});
		}

		public void TestWriteOrgAddressCollectionToDataObject()
		{
			var loadList = HVLVOriginLoadListDataContextManagerTest.SampleLoadList();
			var dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).GetDataObject(loadList);

			CombineAssertions(() =>
			{
				var carrierAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.Carrier));
				AssertEquals("Carrier Address: ", "Carrier Avenue", carrierAddress.Address1);
				AssertEquals("Carrier City: ", "Los Angeles", carrierAddress.City);

				var departureCFSAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.DepartureCFSAddress));
				AssertEquals("departureCFSAddress Address: ", "1 Origin Depot to Departure Street", departureCFSAddress.Address1);
				AssertEquals("departureCFSAddress City", "Brisbane", departureCFSAddress.City);
				AssertEquals("departureCFSAddress Postcode: ", "4000", departureCFSAddress.Postcode);

				var arrivalCFSAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.ArrivalCFSAddress));
				AssertEquals("arrivalCFSAddress Address: ", "2 Destination Depot Street", arrivalCFSAddress.Address1);
				AssertEquals("arrivalCFSAddress City: ", "Sydney", arrivalCFSAddress.City);
				AssertEquals("arrivalCFSAddress Postcode: ", "2000", arrivalCFSAddress.Postcode);
			});
		}

		public void TestWriteWayBillNumbersToDataObject()
		{
			var loadList = HVLVOriginLoadListDataContextManagerTest.SampleLoadList();
			var dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).GetDataObject(loadList);

			var masterbill = dataObject.AdditionalBillCollection.FirstOrDefault(x => x.BillType.Code.ToString() == BillTypeList.Codes.MasterBill);
			var housebill = dataObject.AdditionalBillCollection.FirstOrDefault(x => x.BillType.Code.ToString() == BillTypeList.Codes.HouseBill);

			AssertEquals("Master Bill", "111222333", masterbill.BillNumber);
			AssertEquals("House Bill", "456456456", housebill.BillNumber);
		}

		public void TestWritePackingLinesToDataObject()
		{
			var loadList = HVLVOriginLoadListDataContextManagerTest.SampleLoadList();
			AssertEquals("precondition - loadList has 2 HVLVItems", 2, loadList.Items.Count);
			var dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).GetDataObject(loadList);

			CombineAssertions("each item in loadlist is mapped to a packing line in data object", () =>
			{
				AssertEquals(2, dataObject.PackingLineCollection.Count);

				var packingLine_1 = dataObject.PackingLineCollection.First(packingLine => packingLine.ReferenceNumber.Value == "HVI001");
				AssertEquals("HVI123456789", packingLine_1.OrderReference.Value);

				var packingLine_2 = dataObject.PackingLineCollection.First(packingLine => packingLine.ReferenceNumber.Value == "HVI002");
				AssertEquals("Lamelo Ball for MVP", packingLine_2.OrderReference.Value);
			});
		}

		public void TestWriteIsMasterHouse()
		{
			var loadList = HVLVOriginLoadListDataContextManagerTest.SampleLoadList();
			loadList.HVL_IsMasterHouse = true;
			var dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).GetDataObject(loadList);

			Assert(dataObject.IsMasterHouse.Value);
		}

		public void TestWritePackingLinesToDataObject_WhenLoadListHasOuterPackage()
		{
			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepot.MainAddress.Address1 = "123 NVIDIA Street";
			destinationDepot.OH_Code = "NVIDIA";

			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.MainAddress.Address1 = "789 AMD Street";
			lastMileCarrier.OH_Code = "AMD";

			var loadList = HVLVOriginLoadListDataContextManagerTest.SampleLoadList();
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_PackageReference = "CantTouchThis";
			outerPackage.HVO_PackageBarcode = "CantTouchThisEither";
			outerPackage.HVO_ContainerNumber = "ContainerCantTouch";
			outerPackage.HVO_Status = "LDG";
			outerPackage.HVO_F3_NKPackageType = "PKG";
			outerPackage.HVO_RH_NKCommodityCode = "KFC";
			outerPackage.HVO_Volume = 1m;
			outerPackage.HVO_VolumeUQ = "M3";
			outerPackage.HVO_Weight = 2m;
			outerPackage.HVO_WeightUQ = "KG";
			outerPackage.HVO_Length = 3m;
			outerPackage.HVO_Height = 4m;
			outerPackage.HVO_Width = 5m;
			outerPackage.HVO_UnitOfDimension = "M";

			outerPackage.HVO_OA_DestinationDepot = destinationDepot.MainAddress.PK;
			outerPackage.HVO_OH_LastMileCarrier = lastMileCarrier.PK;

			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_HVO_OuterPackage = outerPackage.PK;
			item1.HVI_ItemId = "Schlegend1";

			var item2 = Factory.NewWithValidTestData<HVLVItem>();
			item2.HVI_HVO_OuterPackage = outerPackage.PK;
			item2.HVI_ItemId = "Schlegend2";

			loadList.OuterPackages.Add(outerPackage);

			var dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).GetDataObject(loadList);
			var outerPackagePackingLine = dataObject.PackingLineCollection.First(packingLine => packingLine.ReferenceNumber.Value == "CantTouchThis");

			CombineAssertions("PackingLine for outerpackage are populated with outerpackage details", () =>
			{
				AssertEquals("Is HVLV Clearance", true, outerPackagePackingLine.IsHVLVClearance);
				AssertEquals("Reference Number", "CantTouchThis", outerPackagePackingLine.ReferenceNumber);
				AssertEquals("Status", "LDG", outerPackagePackingLine.Status);
				AssertEquals("Barcode", "CantTouchThisEither", outerPackagePackingLine.Barcode);
				AssertEquals("Container Number", "ContainerCantTouch", outerPackagePackingLine.ContainerNumber);
				AssertEquals("Pack Type", "PKG", outerPackagePackingLine.PackType.Code);
				AssertEquals("Commodity", "KFC", outerPackagePackingLine.Commodity.Code);
				AssertEquals("Volume", 1m, outerPackagePackingLine.Volume);
				AssertEquals("Volume UQ", "M3", outerPackagePackingLine.VolumeUnit.Code);
				AssertEquals("Weight", 2m, outerPackagePackingLine.Weight);
				AssertEquals("Weight UQ", "KG", outerPackagePackingLine.WeightUnit.Code);
				AssertEquals("Length", 3m, outerPackagePackingLine.Length);
				AssertEquals("Height", 4m, outerPackagePackingLine.Height);
				AssertEquals("Width", 5m, outerPackagePackingLine.Width);
				AssertEquals("Unit Of Dimension", "M", outerPackagePackingLine.LengthUnit.Code);

				AssertEquals("Should contain 4 organization addresses", 4, outerPackagePackingLine.OrganizationAddressCollection.Count);

				var customsDepotAddress = outerPackagePackingLine.OrganizationAddressCollection.Single(x => x.AddressType.Equals("CustomsDepotAddress"));
				AssertEquals("Customs Depot Address", "123 NVIDIA Street", customsDepotAddress.Address1);

				var arrivalCFSAddress = outerPackagePackingLine.OrganizationAddressCollection.Single(x => x.AddressType.Equals("ArrivalCFSAddress"));
				AssertEquals("Arrival CFS Address", "123 NVIDIA Street", arrivalCFSAddress.Address1);

				var deliveryLocalCartage = outerPackagePackingLine.OrganizationAddressCollection.Single(x => x.AddressType.Equals("DeliveryLocalCartage"));
				AssertEquals("Delivery Local Cartage", "789 AMD Street", deliveryLocalCartage.Address1);

				var pickupLocalCartage = outerPackagePackingLine.OrganizationAddressCollection.Single(x => x.AddressType.Equals("PickupLocalCartage"));
				AssertEquals("Pickup Local Cartage", "789 AMD Street", pickupLocalCartage.Address1);
			});

			CombineAssertions("PackingLine for outerpackage created with children packing lines for each item attached to ", () =>
			{
				AssertEquals(outerPackagePackingLine.PackingLineCollection.Count, 2);
				AssertContainsExactElementsInAnyOrder("Children PackingLines should match items", new[] { "Schlegend1", "Schlegend2" }, outerPackagePackingLine.PackingLineCollection.Select(x => x.ReferenceNumber.ToString()).ToArray());
			});
		}

		public void TestOnlyActiveItemsAreExportedInXUS()
		{
			var loadList = HVLVOriginLoadListDataContextManagerTest.SampleLoadList();

			var activeItem = loadList.Items.OfType<HVLVItem>().Single(x => x.HVI_ItemId == "HVI001");
			activeItem.HVI_IsActive = true;

			var inactiveItem = loadList.Items.OfType<HVLVItem>().Single(x => x.HVI_ItemId == "HVI002");
			inactiveItem.HVI_IsActive = false;

			var dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).GetDataObject(loadList);
			var packingLine = dataObject.PackingLineCollection.Single();
			AssertEquals("The packing line created is from the active item", "HVI001", packingLine.ReferenceNumber.Value);
		}

		public void TestWritePortsToDataObject()
		{
			var loadList = HVLVOriginLoadListDataContextManagerTest.SampleLoadList();
			var dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).GetDataObject(loadList);

			var originPort = dataObject.PortOfOrigin;
			var destinationPort = dataObject.PortOfDestination;

			CombineAssertions(() =>
			{
				AssertNotNull(originPort);
				AssertEquals("AUSYD", originPort.Code);

				AssertNotNull(destinationPort);
				AssertEquals("USLAX", destinationPort.Code);
			});
		}

		public void TestWriteTransportLegsToDataObject()
		{
			var loadList = HVLVOriginLoadListDataContextManagerTest.SampleLoadList();
			var dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).GetDataObject(loadList);

			CombineAssertions("Transport is populated for consol matching", () =>
			{
				var transportLegs = dataObject.TransportLegCollection;
				AssertEquals(1, transportLegs.Count);

				var transportLeg = transportLegs.Single();

				AssertEquals("VOYAGE001", transportLeg.VoyageFlightNo);
				AssertEquals("AUSYD", transportLeg.PortOfLoading.Code);
				AssertEquals("USLAX", transportLeg.PortOfDischarge.Code);
				AssertEquals(new ZDateTime(2020, 3, 5), transportLeg.EstimatedArrival);
				AssertEquals(new ZDateTime(2020, 3, 4), transportLeg.EstimatedDeparture);
				AssertEquals("AIDA", transportLeg.VesselName);
				AssertEquals(TransportMode.Sea, transportLeg.TransportMode);

				var carrier = transportLeg.Carrier;
				AssertEquals("CARRIERORG", carrier.OrganizationCode);
				AssertEquals("Carrier Avenue", carrier.Address1);
			});
		}

		public void TestExportLoadListAsConsolUniversalShipmentForMatchOrCreateOnly()
		{
			var loadList = HVLVOriginLoadListDataContextManagerTest.SampleLoadList();
			var dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).
								ExportLoadListAsConsolUniversalShipmentForMatchOrCreateOnly(loadList);

			CombineAssertions("Master way bill number is populated for Consol matching", () =>
			{
				AssertEquals("111222333", dataObject.WayBillNumber);
				AssertEquals("MWB", dataObject.WayBillType.Code);
			});

			CombineAssertions("Consol details is populated for consol matching", () =>
			{
				AssertEquals("AUSYD", dataObject.PortOfLoading.Code);
				AssertEquals("USLAX", dataObject.PortOfDestination.Code);
				AssertEquals("Container mode", "GRP", dataObject.ContainerMode.Code);
			});

			CombineAssertions("ShippingLineAddress is populated for Consol matching", () =>
			{
				var shippingLineAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.ShippingLineAddress));

				AssertEquals("shippingLineAddress Address: ", "Carrier Avenue", shippingLineAddress.Address1);
				AssertEquals("shippingLineAddress City: ", "Los Angeles", shippingLineAddress.City);
			});

			AssertNull("IsNeutralMaster should be null", dataObject.IsNeutralMaster);
			AssertEquals("Consol Shipment Type (Agent Type) should be hard coded as 'AGT'", "AGT", dataObject.ShipmentType.Code);
		}

		public void TestExportLoadListAsConsolUniversalShipmentForMatchOrCreateOnly_ShouldPopulateIsNeutralMasterAndWayBillNumber()
		{
			var refAirline = RefAirline.LoadFromAirline2LetterCode(Factory, "AA");

			var loadList = HVLVOriginLoadListDataContextManagerTest.SampleLoadList();
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Lodged;
			loadList.HVL_IsNeutralMaster = true;
			loadList.HVL_TransportMode = TransportModes.Air;

			loadList.HVL_MasterBillNumber = "";
			loadList.HVL_VoyageFlight = "AA123";

			var dataObject = new HVLVOriginLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(null, loadList))).
								ExportLoadListAsConsolUniversalShipmentForMatchOrCreateOnly(loadList);

			CombineAssertions("IsNeutralMaster should be mapped for Consol matching", () =>
			{
				AssertNotNull(dataObject.IsNeutralMaster);
				Assert("Value", dataObject.IsNeutralMaster.Value.GetValueOrDefault());
				Assert("CreateAndAllocateNeutralStock", dataObject.IsNeutralMaster.CreateAndAllocateNeutralStock.GetValueOrDefault());
			});

			AssertEquals("WayBillNumber should be populated for Consol matching", refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode, dataObject.WayBillNumber);
		}
	}
}
