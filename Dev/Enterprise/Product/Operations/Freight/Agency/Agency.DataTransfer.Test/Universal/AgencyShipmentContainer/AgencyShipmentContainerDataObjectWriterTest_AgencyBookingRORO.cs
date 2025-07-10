using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	sealed class AgencyShipmentContainerDataObjectWriterTest_AgencyBookingRORO : UniversalShipmentDataObjectWriterTest
	{
		protected override string GetExpectedDataObjectXml()
		{
			using (var retriever = new EmbeddedResourceRetriever())
			{
				return retriever.GetString("Enterprise.Freight.Agency.DataTransfer.Test.Universal.AgencyShipmentContainer.TestFiles.AgencyBookingRoroContainer_UniversalShipment.xml");
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
			var vehicle1 = booking.BookedContainers.AddNew();
			vehicle1.JC_ContainerNum = "VIN000001";
			vehicle1.JC_ContainerJobID = "D00001000";
			vehicle1.JC_SealNum = "SEAL00";
			vehicle1.JC_SealParty = "CAR";
			vehicle1.JC_AdditionalSealNum = "SEAL01";
			vehicle1.JC_AdditionalSealParty = "CRD";
			vehicle1.JC_Additional2SealNum = "SEAL02";
			vehicle1.JC_Additional2SealParty = "CTO";
			var vehicle2 = booking.BookedContainers.AddNew();
			vehicle2.JC_ContainerNum = "VIN000002";
			vehicle2.JC_ContainerJobID = "D00001001";
			vehicle2.JC_SealNum = "SEAL03";
			vehicle2.JC_SealParty = "CAR";
			vehicle2.JC_AdditionalSealNum = "SEAL04";
			vehicle2.JC_AdditionalSealParty = "CRD";
			vehicle2.JC_Additional2SealNum = "SEAL05";
			vehicle2.JC_Additional2SealParty = "CTO";
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
			return vehicle1;
		}

		protected override ITopLevelDataObjectWriter GetWriter(IDataWritingManager manager)
		{
			return new AgencyShipmentContainerDataObjectWriter(manager);
		}
	}
}
