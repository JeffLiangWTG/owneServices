using System.Linq;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using DataContext = Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class HVLVOriginLoadListDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		#region Test Populate Data

		public void TestReadGeneralDataFromDataObject()
		{
			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML);

			Factory.SaveForTesting();

			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			originLoadList = Factory.LoadTop1<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.PK, originLoadList.PK));

			CombineAssertions("Origin Load List", () =>
			{
				AssertEquals("AIDA", originLoadList.HVL_VesselName);
				AssertEquals("VOYAGE001", originLoadList.HVL_VoyageFlight);
				AssertEquals(TransportModes.Sea, originLoadList.HVL_TransportMode);
				AssertEquals(HVLVOriginLoadListStatus.Codes.Open, originLoadList.HVL_Status);
				AssertEquals("STD", originLoadList.HVL_RS_NKServiceLevel);
				AssertEquals("FC1", originLoadList.HVL_INCO);
			});
		}

		public void TestUpdateExistingLoadList()
		{
			var existingOriginLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			existingOriginLoadList.HVL_UniqueReference = "LOADLIST1";

			var destinationDepot = Factory.Load<OrgAddress>(new ZQuery(OrgAddressSchema.OA_Code, "2DDS")).FirstOrDefault();

			AssertEquals("pre-condition", "OPN", existingOriginLoadList.HVL_Status);

			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML_WithValidDataTarget);
			var importedOriginLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();

			AssertEquals("Should update existing loadlist", existingOriginLoadList.PK, importedOriginLoadList.PK);
			AssertEquals("Loadlist status should be updated", "LDG", importedOriginLoadList.HVL_Status);
			AssertEquals("Loadlist destination depot should be updated", destinationDepot.PK, importedOriginLoadList.HVL_OA_DestinationDepot);
		}

		public void TestPopulateContainerValuesFromDataObject()
		{
			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML);
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "TNK";
			refContainer.RC_Description = "Ref Container Tank Description";

			Factory.SaveForTesting();

			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			originLoadList = Factory.LoadTop1<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.PK, originLoadList.PK));
			refContainer = Factory.LoadFromUniqueKey<RefContainer>(RefContainerSchema.PK, originLoadList.HVL_RC_ContainerType);

			CombineAssertions("Origin Load List container number and type", () =>
			{
				AssertEquals("Expect originLoadList ContainerNumber to be C3", "C3", originLoadList.HVL_ContainerNumber);
				AssertEquals("Expect originLoadList ContainerType", "TNK", refContainer.RC_Code);
			});
		}

		public void TestPopulateDateValuesFromDataObject()
		{
			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML);

			Factory.SaveForTesting();

			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			originLoadList = Factory.LoadTop1<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.PK, originLoadList.PK));

			CombineAssertions("Origin Load List departure and arrival dates", () =>
			{
				AssertEquals(new ZDateTime(2020, 3, 4), originLoadList.HVL_E_Dep);
				AssertEquals(new ZDateTime(2020, 3, 5), originLoadList.HVL_E_Arv);
			});
		}

		public void TestPopulateAdditionalBillValuesFromDataObject()
		{
			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML);

			Factory.SaveForTesting();

			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			originLoadList = Factory.LoadTop1<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.PK, originLoadList.PK));

			CombineAssertions("Origin Load List bill numbers", () =>
			{
				AssertEquals("111222333", originLoadList.HVL_MasterBillNumber);
				AssertEquals("456456456", originLoadList.HVL_HouseBillNumber);
			});
		}

		public void TestPopulateOrganisationValuesFromDataObject()
		{
			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML);

			#region Carrier Org and Address

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIERORG";
			carrier.MainAddress.OA_OH = carrier.PK;
			carrier.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			carrier.MainAddress.OA_Address1 = "Carrier Avenue";
			carrier.MainAddress.OA_City = "Los Angeles";

			#endregion

			#region Origin Depot Address

			var originDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			originDepotAddress.OA_Address1 = "1 Origin Depot to Departure Street";
			originDepotAddress.OA_City = "Brisbane";
			originDepotAddress.OA_PostCode = "4000";
			originDepotAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var originDepotHeader = Factory.New<OrgHeader>();
			originDepotHeader.OH_Code = "ORIGINORG";
			originDepotAddress.OA_OH = originDepotHeader.PK;

			#endregion

			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			originLoadList = Factory.LoadTop1<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.PK, originLoadList.PK));

			var originDepotOrgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ORIGINORG"));
			var destinationDepotOrgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "DESORG"));

			CombineAssertions("Origin Load List", () =>
			{
				AssertEquals("Expect LoadList Carrier OrgHeader to be CARRIERORG", "CARRIERORG", originLoadList.Carrier.OH_Code);
				AssertEquals("Expect LoadList Origin Depot OrgAddress' OA_Code to be ORIGINORG", originDepotOrgHeader.PK, originLoadList.OriginDepot.OA_OH);
				AssertEquals("Expect LoadList Destination Depot OrgAddress' OA_Code to be DESORG", destinationDepotOrgHeader.PK, originLoadList.DestinationDepot.OA_OH);
			});
		}

		public void TestPopulateOrganisationValuesFromDataObject_WhenFailToReadDestinationDepot_ThrowException()
		{
			var depotMissingErrorMessage = string.Join(System.Environment.NewLine, new[]
			{
				"Can not find destination depot."
			});
			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXMLWithoutDestinationDepot);

			AssertExceptionThrown(
				typeof(DataObjectReadFailureException),
				depotMissingErrorMessage,
				() => new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject());

			var depotInvalidErrorMessage = string.Join(System.Environment.NewLine, new[]
			{
				"Destination depot is invalid."
			});
			var loadListDataObjectWithWrongDestinationDepot = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXMLWithWrongDestinationDepot);

			AssertExceptionThrown(
				typeof(DataObjectReadFailureException),
				depotInvalidErrorMessage,
				() => new HVLVOriginLoadListDataObjectReader(loadListDataObjectWithWrongDestinationDepot, new DummyLogger(), Factory).ReadIntoBusinessObject());
		}

		public void TestPopulateItemsMatchExistingItem()
		{
			var existingHVLVItem = Factory.NewWithValidTestData<HVLVItem>();
			existingHVLVItem.HVI_ItemId = "IanIsHot";

			var existingHVLVItem2 = Factory.NewWithValidTestData<HVLVItem>();
			existingHVLVItem2.HVI_ItemId = "WindItUp";

			var existingHVLVItem3 = Factory.NewWithValidTestData<HVLVItem>();
			existingHVLVItem3.HVI_ItemId = "";
			existingHVLVItem3.HVI_ShipperReference = "OhYayArea";

			Factory.SaveForTesting();
			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML);

			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			originLoadList = Factory.LoadTop1<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.PK, originLoadList.PK));

			CombineAssertions("Existing HVLVItems that match packing line collection are attached to loadlist", () =>
			{
				AssertEquals(originLoadList.PK, existingHVLVItem.HVI_HVL_LoadList);
				AssertEquals(originLoadList.PK, existingHVLVItem2.HVI_HVL_LoadList);
				AssertEquals(ZGuid.Empty, existingHVLVItem3.HVI_HVL_LoadList);
			});
		}

		public void TestPopulateItems_WhenPackingLineContentIsComplete_ThenDetachExistingItems()
		{
			var existingOriginLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			existingOriginLoadList.HVL_UniqueReference = "LOADLIST1";

			var existingHVLVItem = Factory.NewWithValidTestData<HVLVItem>();
			existingHVLVItem.HVI_HVL_LoadList = existingOriginLoadList.PK;

			var existingHVLVItem2 = Factory.NewWithValidTestData<HVLVItem>();
			existingHVLVItem2.HVI_HVL_LoadList = existingOriginLoadList.PK;

			Factory.SaveForTesting();

			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleExistingLoadListXML_WithPartialPackingLine);
			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions("Existing HVLVItems that are attached to a loadlist with a Partial packingLineCollection are not detached", () =>
			{
				AssertEquals(existingOriginLoadList.PK, existingHVLVItem.HVI_HVL_LoadList);
				AssertEquals(existingOriginLoadList.PK, existingHVLVItem2.HVI_HVL_LoadList);
			});

			loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleExistingLoadListXML_WithCompletePackingLine);
			new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions("Existing HVLVItems that are attached to a loadlist with a Complete packingLineCollection are detached", () =>
			{
				AssertEquals(ZGuid.Empty, existingHVLVItem.HVI_HVL_LoadList);
				AssertEquals(ZGuid.Empty, existingHVLVItem2.HVI_HVL_LoadList);
			});
		}

		public void TestPopulateOuterPackageFromPackingLine()
		{
			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepot.MainAddress.Address1 = "123 NVIDIA Street";
			destinationDepot.OH_Code = "NVIDIA";

			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.MainAddress.Address1 = "789 AMD Street";
			lastMileCarrier.OH_Code = "AMD";

			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML_WithOuterPackage);
			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var outerPackage = originLoadList.OuterPackages.Single() as HVLVOuterPackage;

			CombineAssertions("Outerpackage details are populated from packing line", () =>
			{
				AssertEquals("Package Reference", "OuterPackageRef", outerPackage.HVO_PackageReference);
				AssertEquals("Status", "OPN", outerPackage.HVO_Status);
				AssertEquals("Package Barcode", "Ian", outerPackage.HVO_PackageBarcode);
				AssertEquals("Container Number", "1223456", outerPackage.HVO_ContainerNumber);
				AssertEquals("Pack Type", "BAG", outerPackage.HVO_F3_NKPackageType);
				AssertEquals("Commodity Code", "KFC", outerPackage.HVO_RH_NKCommodityCode);
				AssertEquals("Volume", 1.1m, outerPackage.HVO_Volume);
				AssertEquals("Volume UQ", "M3", outerPackage.HVO_VolumeUQ);
				AssertEquals("Weight", 2.2m, outerPackage.HVO_Weight);
				AssertEquals("Weight UQ", "KG", outerPackage.HVO_WeightUQ);
				AssertEquals("Length", 3.3m, outerPackage.HVO_Length);
				AssertEquals("Height", 4.4m, outerPackage.HVO_Height);
				AssertEquals("Width", 5.5m, outerPackage.HVO_Width);
				AssertEquals("Unit of Dimension", "M", outerPackage.HVO_UnitOfDimension);
				AssertEquals("Destination Depot", "123 NVIDIA Street", outerPackage.DestinationDepot.Address1);
				AssertEquals("Last Mile Carrier", "789 AMD Street", outerPackage.LastMileCarrier.MainAddress.Address1);
			});
		}

		public void TestPopulateItems_WhenPackingLineHasOuterPackage()
		{
			var existingHVLVItem = Factory.NewWithValidTestData<HVLVItem>();
			existingHVLVItem.HVI_ItemId = "OuterPackageItem1";
			var existingHVLVItem2 = Factory.NewWithValidTestData<HVLVItem>();
			existingHVLVItem2.HVI_ItemId = "OuterPackageItem2";

			var existingOuterPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			existingOuterPackage.HVO_PackageReference = "OuterPackageRef";
			existingOuterPackage.HVO_F3_NKPackageType = PkgUnit.Bag;

			var lastMileDelivery = Factory.NewWithValidTestData<OrgHeader>();
			lastMileDelivery.OH_Code = "LCLDLVRY";
			lastMileDelivery.OH_FullName = "Local Delivery";

			Factory.SaveForTesting();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("item1 not attached to loadlist", ZGuid.Empty, existingHVLVItem.HVI_HVL_LoadList);
				AssertEquals("item1 not attached to outerpackage", ZGuid.Empty, existingHVLVItem.HVI_HVO_OuterPackage);
				AssertEquals("item2 not attached to loadlist", ZGuid.Empty, existingHVLVItem2.HVI_HVL_LoadList);
				AssertEquals("item2 not attached to outerpackage", ZGuid.Empty, existingHVLVItem2.HVI_HVO_OuterPackage);
				AssertEquals("outerpackage not attached to loadlist", ZGuid.Empty, existingOuterPackage.HVO_HVL_LoadList);
			});

			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML_WithOuterPackage);
			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions("Post Conditions", () =>
			{
				AssertEquals("item1 attached to loadlist", originLoadList.PK, existingHVLVItem.HVI_HVL_LoadList);
				AssertEquals("item1 attached to outerpackage", existingOuterPackage.PK, existingHVLVItem.HVI_HVO_OuterPackage);
				AssertEquals("item2 attached to loadlist", originLoadList.PK, existingHVLVItem2.HVI_HVL_LoadList);
				AssertEquals("item2 attached to outerpackage", existingOuterPackage.PK, existingHVLVItem2.HVI_HVO_OuterPackage);
				AssertEquals("outerpackage attached to loadlist", originLoadList.PK, existingOuterPackage.HVO_HVL_LoadList);
				AssertEquals("loadlist has outerpackage", 1, originLoadList.OuterPackages.Count);
			});
		}

		public void TestPopulateItems_WhenPackingLineHasNewOuterPackage()
		{
			var existingHVLVItem = Factory.NewWithValidTestData<HVLVItem>();
			existingHVLVItem.HVI_ItemId = "OuterPackageItem1";
			var existingHVLVItem2 = Factory.NewWithValidTestData<HVLVItem>();
			existingHVLVItem2.HVI_ItemId = "OuterPackageItem2";

			var lastMileDelivery = Factory.NewWithValidTestData<OrgHeader>();
			lastMileDelivery.OH_Code = "LCLDLVRY";
			lastMileDelivery.OH_FullName = "Local Delivery";

			Factory.SaveForTesting();

			var existingOuterPackage = Factory.LoadTop1<HVLVOuterPackage>(new ZQuery(HVLVOuterPackageSchema.HVO_PackageReference, "OuterPackageRef"));
			AssertNull("OuterPackage should not exist yet", existingOuterPackage);

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("item1 not attached to loadlist", ZGuid.Empty, existingHVLVItem.HVI_HVL_LoadList);
				AssertEquals("item1 not attached to outerpackage", ZGuid.Empty, existingHVLVItem.HVI_HVO_OuterPackage);
				AssertEquals("item2 not attached to loadlist", ZGuid.Empty, existingHVLVItem2.HVI_HVL_LoadList);
				AssertEquals("item2 not attached to outerpackage", ZGuid.Empty, existingHVLVItem2.HVI_HVO_OuterPackage);
			});

			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML_WithOuterPackage);
			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var importedOuterPackage = Factory.LoadTop1<HVLVOuterPackage>(new ZQuery(HVLVOuterPackageSchema.HVO_PackageReference, "OuterPackageRef"));
			AssertNotNull("OuterPackage should be created", importedOuterPackage);

			CombineAssertions("Post Conditions", () =>
			{
				AssertEquals("item1 attached to loadlist", originLoadList.PK, existingHVLVItem.HVI_HVL_LoadList);
				AssertEquals("item1 attached to outerpackage", importedOuterPackage.PK, existingHVLVItem.HVI_HVO_OuterPackage);
				AssertEquals("item2 attached to loadlist", originLoadList.PK, existingHVLVItem2.HVI_HVL_LoadList);
				AssertEquals("item2 attached to outerpackage", importedOuterPackage.PK, existingHVLVItem2.HVI_HVO_OuterPackage);
				AssertEquals("outerpackage attached to loadlist", originLoadList.PK, importedOuterPackage.HVO_HVL_LoadList);
				AssertEquals("loadlist has outerpackage", 1, originLoadList.OuterPackages.Count);
			});
		}

		public void TestPopulateIsMasterHouse()
		{
			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML);
			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();

			Assert("When no <IsMasterHouse> is specified in XML, originLoadListBO.HVL_IsMasterHouse should be false", !originLoadList.HVL_IsMasterHouse);

			var masterHouseLoadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML_IsMasterHouse);
			originLoadList = new HVLVOriginLoadListDataObjectReader(masterHouseLoadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();

			Assert("HVL_IsMasterHouse should be populated when <IsMasterHouse> is specified in XML", originLoadList.HVL_IsMasterHouse);
		}

		public void TestPopulatePortsFromDataObject()
		{
			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML);
			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("AUSYD", originLoadList.HVL_RL_NKOrigin);
				AssertEquals("USLAX", originLoadList.HVL_RL_NKDestination);
			});
		}

		public void TestReaderUsesBranchContextForLogsWhenPopulatingDataObject()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "LTT";
			Factory.SaveForTesting();

			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML);
			var loadListDataContext = loadListDataObject.DataContext as DataContext;
			loadListDataContext.EventBranch = new UniversalDataBuss.DataObjects.Branch { Code = "LTT", Name = "Linus Tech Tips" };
			loadListDataObject.OperationalStatus.Code = "LDG";

			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			originLoadList = Factory.LoadTop1<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.PK, originLoadList.PK));

			var latestLodgeLog = originLoadList.Logs.MostRecentLogByEventTime(
									AutoEvents.StatusUpdated,
									x => x.Parameters.TryGetValue(EventReferenceParameters.Codes.New, out var logTypeValue)
									&& logTypeValue == HVLVOriginLoadListStatus.Codes.Lodged);

			CombineAssertions(() =>
			{
				AssertNotNull("STU log with reference |NEW=LDG exists", latestLodgeLog);
				AssertEquals("Log uses branch context", "LTT", latestLodgeLog.SL_GB_NKBranch);
				AssertEquals("HVLV Origin Load List has LDG status after import when Operational Status is LDG", "LDG", originLoadList.HVL_Status);
			});
		}

		#endregion

		public void Test_WhenImportLoadList_RejectsUpdatingLoadlistWithStatusCON()
		{
			var existingLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			existingLoadList.HVL_UniqueReference = "LOADLIST1";
			existingLoadList.HVL_VoyageFlight = "CFZ001";
			existingLoadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Consolidated;

			Factory.SaveForTesting();

			var loadListDataObject = HVLVOriginLoadListDataContextManagerTest.GetDataObjectFromSampleLoadListXUS(HVLVOriginLoadListDataContextManagerTest.SampleLoadListXML_WithOuterPackage);
			loadListDataObject.DataContext.DataTargetCollection.First().Key = "LOADLIST1";
			loadListDataObject.VoyageFlightNo = "CFZ002";
			var logger = new TestErrorLogger();
			var originLoadList = new HVLVOriginLoadListDataObjectReader(loadListDataObject, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var expectErrorMessage = @"Cannot populate HVLVOriginLoadList because:
Load List LOADLIST1 cannot be updated via XUS as it has already been consolidated.";

			AssertMultilineASCIIEquals("Service Task Logs", expectErrorMessage, logger.GetErrors());
			AssertEquals("Loadlist should not be updated", "CFZ001", existingLoadList.HVL_VoyageFlight);
		}

		#region Implementations

		protected override void SetUp()
		{
			base.SetUp();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "DESORG";

			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_OH = orgHeader.PK;
			destinationDepot.OA_RL_NKRelatedPortCode = "AUSYD";
			destinationDepot.OA_Address1 = "2 Destination Depot Street";
			destinationDepot.OA_City = "Sydney";
			destinationDepot.OA_PostCode = "2000";
			destinationDepot.OA_Code = "2DDS";
			orgHeader.Addresses.Add(destinationDepot);

			Factory.SaveForTesting();
		}

		#endregion
	}
}
