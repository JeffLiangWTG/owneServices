using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HLPProcessingHVLShipmentDataObjectReader))]
	class HLPProcessingHVLShipmentDataObjectReaderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var dummyShipmentDataObject = new Mock<Shipment>().Object;
			var dummyConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var dummyShipment = Factory.NewWithValidTestData<ForwardingShipment>();

			CombineAssertions("Sub load list cannor be null", () =>
			{
				AssertExceptionThrown<ArgumentException>(() => new HLPProcessingHVLShipmentDataObjectReader(dummyShipmentDataObject, xmlLogger, universalFactory, dummyConsol, subLoadList: null));
				AssertExceptionThrown<ArgumentException>(() => new HLPProcessingHVLShipmentDataObjectReader(dummyShipmentDataObject, xmlLogger, universalFactory, dummyShipment, subLoadList: null));
			});
		}

		public void TestReadIntoHVLShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var hvlShipment = consol.Shipments.AddNew();
			hvlShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			hvlShipment.ConsignorDocumentaryAddress.E2_OA_Address = billToParty.PK;
			hvlShipment.ConsignorDocumentaryAddress.DocAddressType = DocAddressType.ConsignorDocumentaryAddress;
			hvlShipment.JS_RS_NKServiceLevel = "STD";
			hvlShipment.JS_RL_NKDestination = "NZAKL";

			var loadList = GetLoadListForTesting();

			using (loadList.Split())
			{
				var subLoadList = loadList.SubLoadLists.Single();
				var subLoadListXUS = new HLPProcessingSubLoadListDataObjectWriter(subLoadList, new DataWritingManager(new ActionInfo(null, subLoadList.ActualLoadList))).GetDataObject(subLoadList.ActualLoadList);
				xmlLogger.TopLevelDataObject = subLoadListXUS;

				var shipment = new HLPProcessingHVLShipmentDataObjectReader(subLoadListXUS, xmlLogger, universalFactory, consol, subLoadList).ReadIntoBusinessObject();

				AssertEquals("Should read into existing shipment", hvlShipment.PK, shipment.PK);
			}
		}

		public void TestReadIntoHVLShipment_MasterHouse()
		{
			var hvmShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			hvmShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;

			var hvlShipment = hvmShipment.CoLoadShipments.AddNew();
			hvlShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			hvlShipment.ConsignorDocumentaryAddress.E2_OA_Address = billToParty.PK;
			hvlShipment.ConsignorDocumentaryAddress.DocAddressType = DocAddressType.ConsignorDocumentaryAddress;
			hvlShipment.JS_RS_NKServiceLevel = "STD";
			hvlShipment.JS_RL_NKDestination = "AUSYD";

			var loadList = GetLoadListForTesting();
			loadList.HVL_IsMasterHouse = true;

			using (loadList.Split())
			{
				var subLoadList = loadList.SubLoadLists.Single();
				var subLoadListXUS = new HLPProcessingSubLoadListDataObjectWriter(subLoadList, new DataWritingManager(new ActionInfo(null, subLoadList.ActualLoadList))).GetDataObject(subLoadList.ActualLoadList);
				xmlLogger.TopLevelDataObject = subLoadListXUS;

				var shipment = new HLPProcessingHVLShipmentDataObjectReader(subLoadListXUS, xmlLogger, universalFactory, hvmShipment, subLoadList).ReadIntoBusinessObject();

				AssertEquals("Should read into existing shipment", hvlShipment.PK, shipment.PK);
			}
		}

		public void TestReadIntoHVLShipment_MasterHouse_DifferentDestinationCountry()
		{
			var hvmShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			hvmShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;

			var hvlShipment = hvmShipment.CoLoadShipments.AddNew();
			hvlShipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			hvlShipment.ConsignorDocumentaryAddress.E2_OA_Address = billToParty.PK;
			hvlShipment.ConsignorDocumentaryAddress.DocAddressType = DocAddressType.ConsignorDocumentaryAddress;
			hvlShipment.JS_RS_NKServiceLevel = "STD";
			hvlShipment.JS_RL_NKDestination = "NZAKL";

			var loadList = GetLoadListForTesting();
			loadList.HVL_IsMasterHouse = true;

			using (loadList.Split())
			{
				var subLoadList = loadList.SubLoadLists.Single();
				var subLoadListXUS = new HLPProcessingSubLoadListDataObjectWriter(subLoadList, new DataWritingManager(new ActionInfo(null, subLoadList.ActualLoadList))).GetDataObject(subLoadList.ActualLoadList);
				xmlLogger.TopLevelDataObject = subLoadListXUS;

				var shipment = new HLPProcessingHVLShipmentDataObjectReader(subLoadListXUS, xmlLogger, universalFactory, hvmShipment, subLoadList).ReadIntoBusinessObject();

				AssertNotEquals("Should not read into existing shipment since the destination country doesn't match", hvlShipment.PK, shipment.PK);
			}
		}

		HVLVOriginLoadList GetLoadListForTesting()
		{
			var destinationDepot = Factory.New<OrgAddress>();
			destinationDepot.OA_RL_NKRelatedPortCode = "AUSYD";
			destinationDepot.OA_Address1 = "2 Destination Depot Street";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_OA_BillToParty = billToParty.PK;
			bookingHeader.HVH_RS_NKBookingServiceLevel = "STD";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HVH_BookingHeader = bookingHeader.PK;
			consignment.HVC_OA_DestinationDepot = destinationDepot.PK;
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.Items.AddNew().HVI_HVL_LoadList = loadList.PK;
			consignment.Items.AddNew().HVI_HVL_LoadList = loadList.PK;

			return loadList;
		}

		OrgAddress billToParty;
		UniversalObjectFactory universalFactory;
		ConvertTracker xmlLogger;

		protected override void SetUp()
		{
			universalFactory = new UniversalObjectFactory(Factory);
			xmlLogger = new ConvertTracker(1, default, null);

			billToParty = Factory.NewWithValidTestData<OrgAddress>();
			billToParty.OA_Address1 = "DMY";
		}
	}
}
