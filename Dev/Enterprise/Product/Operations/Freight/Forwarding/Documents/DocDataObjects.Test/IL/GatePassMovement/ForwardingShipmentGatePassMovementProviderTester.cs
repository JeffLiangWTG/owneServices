using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class ForwardingShipmentGatePassMovementProviderTester : DataProviderTestCase<IGatePassMovementProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ForwardingShipmentGatePassMovementProvider(null));
			AssertNotNull(Provider);
		}

		public void TestFactory()
		{
			AssertNotNull(Provider.Factory);
		}

		public void TestSourceType()
		{
			AssertEquals("ForwardingShipment", Provider.SourceType);
		}

		public void TestSourceID()
		{
			AssertEquals("UNIQ123", Provider.SourceID);
		}

		public void TestMessageReferenceNumber()
		{
			AssertEquals("100000007", Provider.MessageReferenceNumber);
			Provider.MessageReferenceNumber = ZString.Empty;
			AssertNullOrEmpty(shipment.JS_GMN);
		}

		public void TestProcessType()
		{
			AssertEquals("When Destination is in Israel", "1", Provider.ProcessType);

			shipment.JS_RL_NKDestination = "USNYC";
			AssertEquals("When Destination is Out of Israel", "3", Provider.ProcessType);
		}

		public void TestOriginSiteCode()
		{
			AssertEquals("ILASH", Provider.OriginSiteCode);
		}

		public void TestDestinationSiteCode()
		{
			AssertEquals("ILTLV", Provider.DestinationSiteCode);
		}

		public void TestCargoTypeCode()
		{
			AssertEquals("FCL", Provider.CargoTypeCode);
		}

		public void TestCargoIdentifierTypeCode()
		{
			AssertEquals("When TransportMode = Sea", "11", Provider.CargoIdentifierTypeCode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("When TransportMode = Road", "1", Provider.ProcessType);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("When TransportMode = Air", "1", Provider.ProcessType);
		}

		public void TestCargoIdentifierKey1()
		{
			AssertNullOrEmpty("When arrivalTransport == null and TransportMode in (Road,Sea)", Provider.CargoIdentifierKey1);

			var transport = shipment.ArrivalConsol.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "ILASH";
			transport.JW_ArrivalPortRouteId = "ARR123";

			AssertEquals("When arrivalTransport!= null and TransportMode in (Road,Sea)", "ARR123", Provider.CargoIdentifierKey1);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertNullOrEmpty("When arrivalTransport!= null and TransportMode = Air not ETD/ATD", Provider.CargoIdentifierKey1);

			transport.JW_ETD = new ZDateTime(2023, 1, 1);
			AssertEquals("When arrivalTransport!= null and TransportMode = Air and ETD exist", "2023", Provider.CargoIdentifierKey1);

			transport.JW_ATD = new ZDateTime(2024, 1, 1);
			AssertEquals("When arrivalTransport!= null and TransportMode = Air and ATD exist", "2024", Provider.CargoIdentifierKey1);
		}

		public void TestCargoIdentifierKey2()
		{
			AssertNullOrEmpty("When Sea and FDN Not Exist", Provider.CargoIdentifierKey2);

			var fdnShipment = shipment.Numbers.AddNew();
			fdnShipment.CE_EntryType = IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber;
			fdnShipment.CE_EntryNum = "FDN456";
			AssertEquals("When Sea, Take form FDN", "FDN456", Provider.CargoIdentifierKey2);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("When Air, take from arrivalConsol.MasterBillAirlinePrefix-MasterBillMAWB", "114-22882053", Provider.CargoIdentifierKey2);
		}

		public void TestCargoIdentifierKey3()
		{
			AssertNullOrEmpty("When is not Air", Provider.CargoIdentifierKey3);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("When is Air", "HB123", Provider.CargoIdentifierKey3);
		}

		public void TestCargoIdentifierKey3IsVisible()
		{
			AssertEquals("When is not Air", false, Provider.CargoIdentifierKey3IsVisible);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("When is Air", true, Provider.CargoIdentifierKey3IsVisible);
		}

		public void TestTransportMode()
		{
			AssertEquals("When is Sea", "SEA", Provider.TransportMode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("When is Air", "AIR", Provider.TransportMode);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("When is Road", "ROA", Provider.TransportMode);
		}

		public void TestCargoTypeCodeCollection()
		{
			AssertEquals("When Sea", FreightCodePairLists.JS_PackingModeList(shipment.JS_TransportMode).CodesAsString, Provider.CargoTypeCodeCollection.CodesAsString);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("When Air", FreightCodePairLists.JS_PackingModeList(shipment.JS_TransportMode).CodesAsString, Provider.CargoTypeCodeCollection.CodesAsString);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("When Air", FreightCodePairLists.JS_PackingModeList(shipment.JS_TransportMode).CodesAsString, Provider.CargoTypeCodeCollection.CodesAsString);
		}

		public void TestBusinessObject()
		{
			AssertType<ForwardingShipment>("BusinessObject must be of the expected type", Provider.BusinessObject);
			AssertSame("BusinessObject must be the same as original ForwardingShipment", shipment, Provider.BusinessObject);
		}

		public void TestMessages()
		{
			AssertType<EDIMessageCollection>("Messages must be of the expected type", Provider.Messages);
			AssertSame("Messages must be the same as of the original Shipment", shipment.Messages, Provider.Messages);
		}

		public void TestGatePassMovementProvider()
		{
			AssertType<ForwardingShipmentGatePassMovementProvider>("GatePassMovementProvider type is ForwardingShipmentGatePassMovementProvider", shipment.GatePassMovementProvider);
		}

		protected override IGatePassMovementProvider GetProvider()
		{
			return new ForwardingShipmentGatePassMovementProvider(shipment);
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_UniqueConsignRef = "UNIQ123";
			shipment.JS_RL_NKDestination = "ILASH";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_GMN = "100000007";
			shipment.JS_HouseBill = "HB123";

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKDischargePort = "ILASH";
			consol.MasterBillAirlinePrefix = "114";
			consol.MasterBillMAWB = "22882053";

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "ILASH";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Ashdod";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "IL";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var importBroker = Factory.New<OrgHeader>();
			importBroker.OH_FullName = "I'm Import Broker";
			importBroker.OH_RL_NKClosestPort = "ILTLV";
			importBroker.MainAddress.Address1 = "Unit 400";
			importBroker.MainAddress.Address2 = "51 What Lane";
			importBroker.MainAddress.City = "Tel Aviv";
			importBroker.MainAddress.Postcode = "5024";
			importBroker.MainAddress.OA_RN_NKCountryCode = "IL";

			shipment.JS_OH_ImportBroker = importBroker.PK;

			var importReleaseDepot = Factory.New<OrgHeader>();
			importReleaseDepot.OH_FullName = "I'm Import Release Depo";
			importReleaseDepot.OH_RL_NKClosestPort = "ILTLV";
			importReleaseDepot.MainAddress.Address1 = "Unit 500";
			importReleaseDepot.MainAddress.Address2 = "52 What Lane";
			importReleaseDepot.MainAddress.City = "Jerusalem";
			importReleaseDepot.MainAddress.Postcode = "5025";
			importReleaseDepot.MainAddress.OA_RN_NKCountryCode = "IL";
			importReleaseDepot.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "ILTLV", "IL");

			shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.MainAddress.PK;
		}

		ForwardingShipment shipment;
	}
}
