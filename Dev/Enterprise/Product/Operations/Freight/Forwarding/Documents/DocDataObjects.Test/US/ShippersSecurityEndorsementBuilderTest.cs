using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.US.Testing
{
	sealed class ShippersSecurityEndorsementBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateConsignor()
		{
			var shipment = CreateShipment();

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "MAERSK";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.Address1 = "Unit 13";
			consignor.MainAddress.Address2 = "4 Lost Lane";
			consignor.MainAddress.City = "Sydney";
			consignor.MainAddress.Postcode = "2000";
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var endorsement = new ShippersSecurityEndorsementBuilder(shipment).Build();
			AssertionHelper.AssertAddressData(shipment.Consignor.MainAddress, endorsement.Consignor);
		}

		public void TestPopulateAWBNumber()
		{
			var shipment = CreateShipment();

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Loose;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			var endorsement = new ShippersSecurityEndorsementBuilder(shipment).Build();
			AssertEquals(consol.AWBHeader.EH_BillNumber, endorsement.ForwardingAgentReference);
		}

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_UniqueConsignRef = "S00001000";

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_RL_NKLoadPort = "USLAX";
			shipment.JS_RL_NKDischargePort = "SGSIN";
			return shipment;
		}
	}
}
