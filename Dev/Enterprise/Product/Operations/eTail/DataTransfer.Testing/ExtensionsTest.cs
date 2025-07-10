using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class ExtensionsTest : TestCaseWithFactory
	{
		public void TestAttachMatchingItems_ForOuterPackage()
		{
			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_ItemId = "HVI001";

			var item2 = Factory.NewWithValidTestData<HVLVItem>();
			item2.HVI_ItemId = "HVI002";

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_PackageReference = "testOuterPackage1";

			var outerPackagePackingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = "testOuterPackage1",
			};

			outerPackagePackingLine.SetAdditionalServiceCollection(() => new List<AdditionalService>
			{
				new AdditionalService()
					{
						Location = new OrganizationAddress
						{
							AddressType = AddressTypes.DeliveryLocalCartage,
							OrganizationCode = "LCLDLVRY",
							CompanyName = "Local Delivery"
						}
					}
			});

			outerPackagePackingLine.SetPackingLineCollection(() => new List<PackingLine> {
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "HVI001" },
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "HVI002" }
			});

			CombineAssertions("Precondition - items are not attached to outerpackage", () =>
			{
				AssertEquals(ZGuid.Empty, item1.HVI_HVO_OuterPackage);
				AssertEquals(ZGuid.Empty, item2.HVI_HVO_OuterPackage);
			});

			outerPackage.AttachMatchingItems(outerPackagePackingLine, new UniversalObjectFactory(Factory));

			CombineAssertions("Precondition - items are attached to outerpackage", () =>
			{
				AssertEquals(outerPackage.PK, item1.HVI_HVO_OuterPackage);
				AssertEquals(outerPackage.PK, item2.HVI_HVO_OuterPackage);
			});
		}

		public void TestAttachMatchingItems_ForLoadList_WithPackingLine()
		{
			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_ItemId = "HVI001";

			var itemPackingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = "HVI001",
			};

			AssertEquals("Precondition - item not attached to loadlist", ZGuid.Empty, item1.HVI_HVL_LoadList);

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.AttachMatchingItems(itemPackingLine, new DummyLogger(), new UniversalObjectFactory(Factory));

			AssertEquals("item attached to loadlist", originLoadList.PK, item1.HVI_HVL_LoadList);
		}

		public void TestAttachMatchingItems_ForLoadList_WithPackingLine_ShouldIgnoreItemsLoadedOnShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var itemAlreadyLoadedOnShipment = Factory.NewWithValidTestData<HVLVItem>();
			itemAlreadyLoadedOnShipment.HVI_ItemId = "HVI001";
			itemAlreadyLoadedOnShipment.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemPackingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = "HVI001",
			};

			AssertEquals("Precondition - item not attached to loadlist", ZGuid.Empty, itemAlreadyLoadedOnShipment.HVI_HVL_LoadList);

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var logger = new TestErrorLogger();
			originLoadList.AttachMatchingItems(itemPackingLine, logger, new UniversalObjectFactory(Factory));

			CombineAssertions("items NOT loaded on shipment are attached to loadlist", () =>
			{
				AssertEquals("item not updated because it is loadedOnShipment", ZGuid.Empty, itemAlreadyLoadedOnShipment.HVI_HVL_LoadList);
				AssertEquals($@"The HVLV Item with ID {itemAlreadyLoadedOnShipment.HVI_ItemId} has been skipped as it is already attached to Shipment with ID {shipment.JS_UniqueConsignRef}.", logger.GetWarnings());
			});
		}

		public void TestAttachMatchingItems_ForLoadList_WithPackingLineCollection()
		{
			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_ItemId = "HVI001";

			var item1PackingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = "HVI001",
			};

			var item2 = Factory.NewWithValidTestData<HVLVItem>();
			item2.HVI_ItemId = "HVI002";

			var item2PackingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = "HVI002",
			};

			var packingLineCollection = new List<PackingLine>() { item1PackingLine, item2PackingLine };

			CombineAssertions("Precondition - items are not attached to loadlist", () =>
			{
				AssertEquals(ZGuid.Empty, item1.HVI_HVL_LoadList);
				AssertEquals(ZGuid.Empty, item2.HVI_HVL_LoadList);
			});

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.AttachMatchingItems(packingLineCollection, new DummyLogger(), new UniversalObjectFactory(Factory));

			CombineAssertions("items are attached to loadlist", () =>
			{
				AssertEquals(originLoadList.PK, item1.HVI_HVL_LoadList);
				AssertEquals(originLoadList.PK, item2.HVI_HVL_LoadList);
			});
		}

		public void TestAttachMatchingItemsNotLoadedOnShipment_ForLoadList_WithPackingLineCollection()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var itemAlreadyLoadedOnShipment = Factory.NewWithValidTestData<HVLVItem>();
			itemAlreadyLoadedOnShipment.HVI_ItemId = "HVI001";
			itemAlreadyLoadedOnShipment.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemToSkipPackingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = "HVI001",
			};

			var itemToLoad = Factory.NewWithValidTestData<HVLVItem>();
			itemToLoad.HVI_ItemId = "HVI002";

			var itemToLoadPackingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = "HVI002",
			};

			var packingLineCollection = new List<PackingLine>() { itemToSkipPackingLine, itemToLoadPackingLine };

			CombineAssertions("Precondition - items are not attached to loadlist", () =>
			{
				AssertEquals(ZGuid.Empty, itemAlreadyLoadedOnShipment.HVI_HVL_LoadList);
				AssertEquals(ZGuid.Empty, itemToLoad.HVI_HVL_LoadList);
			});

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var logger = new TestErrorLogger();
			originLoadList.AttachMatchingItems(packingLineCollection, logger, new UniversalObjectFactory(Factory));

			CombineAssertions("items NOT loaded on shipment are attached to loadlist", () =>
			{
				AssertEquals("item not updated because it is loadedOnShipment", ZGuid.Empty, itemAlreadyLoadedOnShipment.HVI_HVL_LoadList);
				AssertEquals("item updated because it is NOT loadedOnShipment", originLoadList.PK, itemToLoad.HVI_HVL_LoadList);
				AssertEquals($@"The HVLV Item with ID {itemAlreadyLoadedOnShipment.HVI_ItemId} has been skipped as it is already attached to Shipment with ID {shipment.JS_UniqueConsignRef}.", logger.GetWarnings());
			});
		}
	}
}
