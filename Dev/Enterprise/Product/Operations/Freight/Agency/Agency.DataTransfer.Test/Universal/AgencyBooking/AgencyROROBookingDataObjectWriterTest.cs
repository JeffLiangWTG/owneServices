using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	sealed class AgencyROROBookingDataObjectWriterTest : UniversalShipmentDataObjectWriterTest
	{
		public void TestPopulateAgencyShipment()
		{
			var shipmentBizObj = (AgencyBooking)GetShipmentBusinessObject();
			IAgencyShipmentDataObjectWriter writer = new AgencyBookingDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBizObj)));
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			writer.PopulateAgencyShipment(shipmentBizObj, dataObject);
			AssertEquals("data context hasn't been populated", null, dataObject.DataContext);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataSource(DataContextType.AgencyBooking, shipmentBizObj.JS_UniqueConsignRef);
			var actualXml = UniversalTestHelper.GetXml(dataObject).Trim();
			var expectedXml = GetExpectedDataObjectXml().Trim();
			AssertMultilineASCIIEquals("Data object for " + shipmentBizObj.GetType().FullName, expectedXml, actualXml);
		}

		protected override string GetExpectedDataObjectXml()
		{
			using (var retriever = new EmbeddedResourceRetriever())
			{
				return retriever.GetString("Enterprise.Freight.Agency.DataTransfer.Test.Universal.AgencyBooking.TestFiles.AgencyBooking_UniversalShipment_Roro.xml");
			}
		}

		protected override BusinessObject GetShipmentBusinessObject()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			booking.JS_UniqueConsignRef = "S000XXX";
			booking.JS_A_BKD = new ZDateTime(2012, 5, 7);
			booking.JS_BookingReference = "YM001";
			booking.JS_CFSReference = "YM002";
			booking.JS_INCO = Core.Constants.DomesticPaymentTerms.Prepaid;
			var container1 = booking.RealContainers.AddNew();
			container1.JC_ContainerNum = "AAAA00000001";
			var container2 = booking.RealContainers.AddNew();
			container2.JC_ContainerNum = "BBBB00000001";
			var bookedContainer1 = booking.BookedContainers.AddNew();
			bookedContainer1.JC_ContainerNum = "FAKE0001";
			bookedContainer1.JC_Description = "shampoo";
			bookedContainer1.JC_SealNum = "SEAL00";
			bookedContainer1.JC_SealParty = "CAR";
			bookedContainer1.JC_AdditionalSealNum = "SEAL01";
			bookedContainer1.JC_AdditionalSealParty = "CRD";
			bookedContainer1.JC_Additional2SealNum = "SEAL02";
			bookedContainer1.JC_Additional2SealParty = "CTO";
			var bookedContainer2 = booking.BookedContainers.AddNew();
			bookedContainer2.JC_Description = "chicken";
			bookedContainer2.JC_ContainerCount = 4;
			bookedContainer2.JC_GoodsValue = 111.22m;
			bookedContainer2.JC_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.Ukraine;
			bookedContainer2.JC_VehicleModel = "HUMP ZAPOROZHETS";
			bookedContainer2.JC_SealNum = "SEAL03";
			bookedContainer2.JC_SealParty = "CAR";
			bookedContainer2.JC_AdditionalSealNum = "SEAL04";
			bookedContainer2.JC_AdditionalSealParty = "CRD";
			bookedContainer2.JC_Additional2SealNum = "SEAL05";
			bookedContainer2.JC_Additional2SealParty = "CTO";
			var transportLeg1 = booking.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "SGSIN";
			var transportLeg2 = booking.Transports.AddNew();
			transportLeg2.JW_RL_NKLoadPort = "SGSIN";
			transportLeg2.JW_RL_NKDiscPort = "USMIA";
			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.MainAddress.OA_Address1 = "1 Notify Ave";
			booking.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.MainAddress.OA_Address1 = "2 Notify Pde";
			booking.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;
			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.MainAddress.OA_Address1 = "3 Notify Street";
			booking.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "4 Booking Ln";
			booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			return booking;
		}

		protected override ITopLevelDataObjectWriter GetWriter(IDataWritingManager manager)
		{
			return new AgencyBookingDataObjectWriter(manager);
		}
	}
}
