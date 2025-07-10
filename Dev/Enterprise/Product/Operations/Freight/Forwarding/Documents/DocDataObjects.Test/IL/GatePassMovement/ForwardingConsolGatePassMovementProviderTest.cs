using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class ForwardingConsolGatePassMovementProviderTest : DataProviderTestCase<IGatePassMovementProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ForwardingConsolGatePassMovementProvider(null));
			AssertNotNull(Provider);
		}

		public void TestFactory()
		{
			AssertNotNull(Provider.Factory);
		}

		public void TestSourceType()
		{
			AssertEquals("ForwardingConsol", Provider.SourceType);
		}

		public void TestSourceID()
		{
			AssertEquals("UNIQ123", Provider.SourceID);
		}

		public void TestMessageReferenceNumber()
		{
			AssertEquals("100000007", Provider.MessageReferenceNumber);
			Provider.MessageReferenceNumber = ZString.Empty;
			AssertNullOrEmpty(consol.JK_GMN);
		}

		public void TestProcessType()
		{
			AssertEquals("When Destination is in Israel", "1", Provider.ProcessType);

			consol.JK_RL_NKDischargePort = "USNYC";
			AssertEquals("When Destination is Out of Israel", "3", Provider.ProcessType);
		}

		public void TestOriginSiteCode()
		{
			AssertEquals("ILASH", Provider.OriginSiteCode);
		}

		public void TestDestinationSiteCode()
		{
			AssertNullOrEmpty(Provider.DestinationSiteCode);
		}

		public void TestCargoTypeCode()
		{
			AssertEquals("FCL", Provider.CargoTypeCode);
		}

		public void TestCargoIdentifierTypeCode()
		{
			AssertEquals("When TransportMode = Sea", "11", Provider.CargoIdentifierTypeCode);

			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("When TransportMode = Road", "1", Provider.ProcessType);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("When TransportMode = Air", "1", Provider.ProcessType);
		}

		public void TestCargoIdentifierKey1()
		{
			AssertNullOrEmpty("When arrivalTransport == null and TransportMode in (Road,Sea)", Provider.CargoIdentifierKey1);

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "ILASH";
			transport.JW_ArrivalPortRouteId = "ARR123";

			AssertEquals("When arrivalTransport!= null and TransportMode in (Road,Sea)", "ARR123", Provider.CargoIdentifierKey1);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertNullOrEmpty("When arrivalTransport!= null and TransportMode = Air not ETD/ATD", Provider.CargoIdentifierKey1);

			transport.JW_ETD = new ZDateTime(2023, 1, 1);
			AssertEquals("When arrivalTransport!= null and TransportMode = Air and ETD exist", "2023", Provider.CargoIdentifierKey1);

			transport.JW_ATD = new ZDateTime(2024, 1, 1);
			AssertEquals("When arrivalTransport!= null and TransportMode = Air and ATD exist", "2024", Provider.CargoIdentifierKey1);
		}

		public void TestCargoIdentifierKey2()
		{
			AssertNullOrEmpty("When Sea and PDN Not Exist", Provider.CargoIdentifierKey2);

			var pdnconsol = consol.Numbers.AddNew();
			pdnconsol.CE_EntryType = IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber;
			pdnconsol.CE_EntryNum = "PDN456";
			AssertEquals("When Sea, Take form PDN", "PDN456", Provider.CargoIdentifierKey2);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.MasterBillAirlinePrefix = "114";
			consol.MasterBillMAWB = "22882053";
			AssertEquals("When Air, take from MasterBillAirlinePrefix-MasterBillMAWB", "114-22882053", Provider.CargoIdentifierKey2);
		}

		public void TestCargoIdentifierKey3()
		{
			AssertNullOrEmpty(Provider.CargoIdentifierKey3);
		}

		public void TestCargoIdentifierKey3IsVisible()
		{
			AssertEquals(false, Provider.CargoIdentifierKey3IsVisible);
		}

		public void TestTransportMode()
		{
			AssertEquals("When is Sea", "SEA", Provider.TransportMode);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("When is Air", "AIR", Provider.TransportMode);

			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("When is Road", "ROA", Provider.TransportMode);
		}

		public void TestCargoTypeCodeCollection()
		{
			AssertEquals("When Sea", FreightCodePairLists.ConsolModeList(consol.JK_AgentType, consol.JK_TransportMode).CodesAsString, Provider.CargoTypeCodeCollection.CodesAsString);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("When Air", FreightCodePairLists.ConsolModeList(consol.JK_AgentType, consol.JK_TransportMode).CodesAsString, Provider.CargoTypeCodeCollection.CodesAsString);

			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("When Air", FreightCodePairLists.ConsolModeList(consol.JK_AgentType, consol.JK_TransportMode).CodesAsString, Provider.CargoTypeCodeCollection.CodesAsString);
		}

		public void TestBusinessObject()
		{
			AssertType<ForwardingConsol>("BusinessObject must be of the expected type", Provider.BusinessObject);
			AssertSame("BusinessObject must be the same as original ForwardingConsol", consol, Provider.BusinessObject);
		}

		public void TestMessages()
		{
			AssertType<EDIMessageCollection>("Messages must be of the expected type", Provider.Messages);
			AssertSame("Messages must be the same as of the original Consol", consol.Messages, Provider.Messages);
		}

		public void TestGatePassMovementProvider()
		{
			AssertType<ForwardingConsolGatePassMovementProvider>("GatePassMovementProvider type is ForwardingConsolGatePassMovementProvider", consol.GatePassMovementProvider);
		}

		protected override IGatePassMovementProvider GetProvider()
		{
			return new ForwardingConsolGatePassMovementProvider(consol);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "UNIQ123";
			consol.JK_RL_NKDischargePort = "ILASH";
			consol.JK_ConsolMode = "FCL";
			consol.JK_GMN = "100000007";
			consol.JK_RL_NKDischargePort = "ILASH";
		}

		ForwardingConsol consol;
	}
}
