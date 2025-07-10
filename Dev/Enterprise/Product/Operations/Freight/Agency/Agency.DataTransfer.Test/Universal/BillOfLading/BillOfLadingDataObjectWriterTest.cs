using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	class BillOfLadingDataObjectWriterTest : UniversalShipmentDataObjectWriterTest
	{
		public void TestPopulateAgencyShipment()
		{
			var shipmentBizObj = (BillOfLading)GetShipmentBusinessObject();
			IAgencyShipmentDataObjectWriter writer = new BillOfLadingDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBizObj)));
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			writer.PopulateAgencyShipment(shipmentBizObj, dataObject);
			AssertEquals("data context hasn't been populated", null, dataObject.DataContext);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataSource(DataContextType.BillOfLading, shipmentBizObj.JS_UniqueConsignRef);
			var actualXml = UniversalTestHelper.GetXml(dataObject).Trim();
			var expectedXml = GetExpectedDataObjectXml().Trim();
			AssertMultilineASCIIEquals("Data object for " + shipmentBizObj.GetType().FullName, expectedXml, actualXml);
		}

		public void TestPopulateWayBillType()
		{
			var shipmentBizObj = (BillOfLading)GetShipmentBusinessObject();
			IAgencyShipmentDataObjectWriter writer = new BillOfLadingDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBizObj)));
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			writer.PopulateAgencyShipment(shipmentBizObj, dataObject);
			AssertNull(dataObject.WayBillType);
			shipmentBizObj.JS_HouseBill = "1000006";
			dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			writer = new BillOfLadingDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBizObj)));
			writer.PopulateAgencyShipment(shipmentBizObj, dataObject);
			AssertEquals(WayBillTypeList.Codes.Master, dataObject.WayBillType.Code);
			AssertEquals(WayBillTypeList.Descriptions.Master, dataObject.WayBillType.Description);
		}

		protected override string GetExpectedDataObjectXml()
		{
			using (var retriever = new EmbeddedResourceRetriever())
			{
				return retriever.GetString("Enterprise.Freight.Agency.DataTransfer.Test.Universal.BillOfLading.TestFiles.BillOfLading_UniversalShipment.xml");
			}
		}

		protected override BusinessObject GetShipmentBusinessObject()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_UniqueConsignRef = "S000XXX";
			billOfLading.JS_A_BKD = new ZDateTime(2012, 5, 7);
			billOfLading.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;
			billOfLading.JS_RL_NKHouseBillIssuePlace = "AUSYD";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("NORDCLOUD", Factory.BOFactory).First().RV_FK;
			voyage.JV_VoyageFlight = "001";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			billOfLading.JS_JX = voyage.Sailings[0].PK;
			billOfLading.JS_RL_NKOrigin = "NZAKL";
			billOfLading.JS_RL_NKDestination = "AUSYD";
			billOfLading.JS_BookingReference = "YM001";
			billOfLading.JS_CFSReference = "YM002";
			billOfLading.JS_GoodsValue = 0m;
			billOfLading.JS_RX_NKGoodsValueCurr = "AUD";
			billOfLading.JS_RL_NKPlaceOfReceipt = "SGSIN";
			billOfLading.JS_RL_NKPlaceOfDischarge = "HKHKC";
			var container1 = billOfLading.RealContainers.AddNew();
			container1.JC_ContainerNum = "AAAA00000001";
			container1.JC_SealNum = "SEAL00";
			container1.JC_SealParty = "CAR";
			container1.JC_AdditionalSealNum = "SEAL01";
			container1.JC_AdditionalSealParty = "CRD";
			container1.JC_Additional2SealNum = "SEAL02";
			container1.JC_Additional2SealParty = "CTO";
			container1.JC_SetPointTempUnit = "C";
			var container2 = billOfLading.RealContainers.AddNew();
			container2.JC_ContainerNum = "BBBB00000001";
			container2.JC_SealNum = "SEAL03";
			container2.JC_SealParty = "CAR";
			container2.JC_AdditionalSealNum = "SEAL04";
			container2.JC_AdditionalSealParty = "CRD";
			container2.JC_Additional2SealNum = "SEAL05";
			container2.JC_Additional2SealParty = "CTO";
			container2.JC_SetPointTempUnit = "C";
			var bookedContainer = billOfLading.BookedContainers.AddNew();
			bookedContainer.JC_ContainerNum = "FAKE0001";
			bookedContainer.JC_SetPointTempUnit = "C";
			var packLine1 = billOfLading.OuterPackLines.AddNew();
			packLine1.JL_Description = "shampoo";
			packLine1.JL_PackageCount = 4;
			container1.PackLines.Add(packLine1);
			var packLine2 = billOfLading.OuterPackLines.AddNew();
			packLine2.JL_Description = "chicken";
			packLine2.JL_PackageCount = 6;
			container2.PackLines.Add(packLine2);
			var transportLeg1 = billOfLading.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "SGSIN";
			var transportLeg2 = billOfLading.Transports.AddNew();
			transportLeg2.JW_RL_NKLoadPort = "SGSIN";
			transportLeg2.JW_RL_NKDiscPort = "USMIA";
			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.MainAddress.OA_Address1 = "1 Notify Ave";
			billOfLading.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.MainAddress.OA_Address1 = "2 Notify Pde";
			billOfLading.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;
			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.MainAddress.OA_Address1 = "3 Notify Street";
			billOfLading.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "4 Booking Ln";
			billOfLading.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			return billOfLading;
		}

		protected override ITopLevelDataObjectWriter GetWriter(IDataWritingManager manager)
		{
			return new BillOfLadingDataObjectWriter(manager);
		}
	}
}
