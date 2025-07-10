using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HLPProcessingHVMShipmentDataObjectReader))]
	class HLPProcessingHVMShipmentDataObjectReaderTest : TestCaseWithFactory
	{
		public void TestReadIntoExistingShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "TestConsol";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "00001";
			shipment.JS_RS_NKServiceLevel = "STD";
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;

			var anotherShipment1 = consol.Shipments.AddNew();
			anotherShipment1.JS_HouseBill = "00001";
			anotherShipment1.JS_RS_NKServiceLevel = "EXP";
			anotherShipment1.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;

			var anotherShipment2 = consol.Shipments.AddNew();
			anotherShipment2.JS_HouseBill = "00002";
			anotherShipment2.JS_RS_NKServiceLevel = "STD";
			anotherShipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;

			var anotherShipment3 = consol.Shipments.AddNew();
			anotherShipment3.JS_HouseBill = "00001";
			anotherShipment3.JS_RS_NKServiceLevel = "STD";
			anotherShipment3.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			Factory.Save();

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_HouseBillNumber = "00001";
			loadList.HVL_RS_NKServiceLevel = "STD";
			loadList.HVL_IsMasterHouse = true;

			var loadListDataObject = GetDataObject(loadList);
			xmlLogger.TopLevelDataObject = loadListDataObject;
			var hvmShipment = new HLPProcessingHVMShipmentDataObjectReader(loadListDataObject, xmlLogger, universalFactory, consol, linkManager).ReadIntoBusinessObject();

			CombineAssertions("Should match and read into existing HVM shipment", () =>
			{
				Assert(hvmShipment.IsInDatabase);
				AssertEquals(shipment.PK, hvmShipment.PK);
			});
		}

		public void TestReadIntoExistingShipment_EmptyWaybillMatching()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "TestConsol";

			var stdShipment1 = consol.Shipments.AddNew();
			stdShipment1.JS_HouseBill = "00001";
			stdShipment1.JS_RS_NKServiceLevel = "STD";
			stdShipment1.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;

			var stdShipment2 = consol.Shipments.AddNew();
			stdShipment2.JS_HouseBill = "00002";
			stdShipment2.JS_RS_NKServiceLevel = "STD";
			stdShipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;

			var noServiceLevelShipment = consol.Shipments.AddNew();
			noServiceLevelShipment.JS_HouseBill = "00003";
			noServiceLevelShipment.JS_RS_NKServiceLevel = "";
			noServiceLevelShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;

			Factory.Save();

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_RS_NKServiceLevel = "STD";
			loadList.HVL_IsMasterHouse = true;

			var loadListDataObject = GetDataObject(loadList);
			xmlLogger.TopLevelDataObject = loadListDataObject;
			var hvmShipment = new HLPProcessingHVMShipmentDataObjectReader(loadListDataObject, xmlLogger, universalFactory, consol, linkManager).ReadIntoBusinessObject();

			AssertEquals("Should output error when waybill number is empty and multiple matching HVM shipment were found.", "There are multiple HVM shipments on consol 'TestConsol'. Load list failed to be merged.", xmlLogger.GetErrorDetail());

			xmlLogger = new ConvertTracker(1, default, null);
			xmlLogger.TopLevelDataObject = loadListDataObject;
			loadListDataObject.ServiceLevel = new ServiceLevel() { Code = ZString.Empty };
			hvmShipment = new HLPProcessingHVMShipmentDataObjectReader(loadListDataObject, xmlLogger, universalFactory, consol, linkManager).ReadIntoBusinessObject();

			CombineAssertions("Should successfully read into HVM Shipment when single match.", () =>
			{
				Assert(!xmlLogger.HasError);
				Assert(hvmShipment.IsInDatabase);
				AssertEquals(noServiceLevelShipment.PK, hvmShipment.PK);
			});
		}

		Shipment GetDataObject(HVLVOriginLoadList loadList)
		{
			return new HLPProcessingMasterHouseLoadListDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, loadList))).GetDataObject(loadList);
		}

		UniversalObjectFactory universalFactory;
		ConvertTracker xmlLogger;
		IContainerLinkManager<ForwardingConsol> linkManager;

		protected override void SetUp()
		{
			universalFactory = new UniversalObjectFactory(Factory);
			xmlLogger = new ConvertTracker(1, default, null);
			linkManager = new Mock<IContainerLinkManager<ForwardingConsol>>().Object;
		}
	}
}
