using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.FR.Testing
{
	public class ForwardingConsolTRCDetailsProviderTest : TestCaseWithFactory
	{
		public void TestITRCDetailsMembers()
		{
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var receivingForwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			receivingForwarderAddress.OA_Code = "ReceivingForwarderAdr";
			receivingForwarderAddress.OA_OH = receivingForwarder.PK;

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var sendingForwarderAddress = Factory.NewWithValidTestData<OrgAddress>();
			sendingForwarderAddress.OA_Code = "SendingForwarderAdr";
			sendingForwarderAddress.OA_OH = sendingForwarder.PK;

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			var shippingLineAddress = Factory.NewWithValidTestData<OrgAddress>();
			shippingLineAddress.OA_Code = "ShippingLineAdr";
			shippingLineAddress.OA_OH = shippingLine.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "123";
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_ETD = new ZDateTime(2023, 5, 10);
			transport.JW_ETA = new ZDateTime(2023, 5, 15);

			consol.JK_BookingReference = "111,222;333:444";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_MasterBillNum = "020";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarderAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.PK;
			consol.JK_OA_ShippingLineAddress = shippingLineAddress.PK;

			var iTRCDetails = new ForwardingConsolTRCDetailsProvider(consol) as ITRCDetails;

			AssertEquals(consol, iTRCDetails.BusinessObject);
			AssertEquals("ForwardingConsol", iTRCDetails.SourceType);
			AssertEquals("123", iTRCDetails.SourceID);
			AssertEquals("SGSIN", iTRCDetails.PortOfOrigin);
			AssertEquals("AUSYD", iTRCDetails.PortOfDestination);
			var arrivalPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var loadPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "SGSIN"));
			AssertEquals(arrivalPort, iTRCDetails.OperationalPortImport);
			AssertEquals(loadPort, iTRCDetails.OperationalPortExport);
			AssertEquals("111,222;333:444", iTRCDetails.BookingConfirmationReference);
			AssertEquals(consol.JK_ConsolMode_List, iTRCDetails.ContainerModeList);
			AssertEquals("FCL", iTRCDetails.ContainerMode);
			AssertEquals(consol.JK_AgentType_List, iTRCDetails.ShipmentTypeList);
			AssertEquals("CLD", iTRCDetails.ShipmentType);
			AssertEquals("020", iTRCDetails.WaybillNumber);
			AssertEquals(receivingForwarderAddress, iTRCDetails.ReceivingForwarder);
			AssertEquals(sendingForwarderAddress, iTRCDetails.SendingForwarder);
			AssertEquals(shippingLineAddress, iTRCDetails.Carrier);
			AssertEquals(new ZDateTime(2023, 05, 10), iTRCDetails.ETD);
			AssertEquals(new ZDateTime(2023, 05, 15), iTRCDetails.ETA);
		}
	}
}
