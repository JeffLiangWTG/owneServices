using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	public class PhaseConstantsTest : TestCase
	{
		public void TestPhase()
		{
			AssertEquals("ALL", PhaseConstants.Phase.ALL);
		}

		public void TestGetCommonPhaseList()
		{
			CodeDescriptionPairList list = PhaseConstants.GetCommonPhaseList();
			AssertEquals(1, list.Count);
			AssertEquals("Open Security", list.GetDescriptionFromCode(PhaseConstants.Phase.ALL));
		}

		public void TestCommonLocations()
		{
			AssertEquals("Any Location", PhaseConstants.Locations.AnyLocation);
		}

		public void TestGetCommonLocations()
		{
			CodeDescriptionPairList list = PhaseConstants.GetCommonLocationsList();
			AssertEquals(1, list.Count);
			AssertEquals("Any Location", list.GetDescriptionFromCode(PhaseConstants.Locations.AnyLocation));
		}

		public void TestGetConsolLocationsList()
		{
			CodeDescriptionPairList list = PhaseConstants.GetConsolLocationsList();
			AssertEquals(14, list.Count);

			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.AnyLocation));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.FirstLoadPort));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentPort));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.FirstLoadCountry));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentCountry));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.LoadPort));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.LoadCountry));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.TransitCountry));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.DischargePort));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.DischargeCountry));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.LastDischargePort));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentPort));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.LastDischargeCountry));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentCountry));

			AssertEquals("Port matching Consol’s first load or Own Sending Agent port", list.GetDescriptionFromCode(PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentPort));
			AssertEquals("Port matching Consol’s first load or Own Sending Agent country/region", list.GetDescriptionFromCode(PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentCountry));
			AssertEquals("Port matching Consol’s last discharge or Own Receiving Agent port", list.GetDescriptionFromCode(PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentPort));
			AssertEquals("Ports matching Consol’s last discharge or Own Receiving Agent country/region", list.GetDescriptionFromCode(PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentCountry));
		}

		public void TestGetShipmentLocationsList()
		{
			CodeDescriptionPairList list = PhaseConstants.GetShipmentLocationsList();
			AssertEquals(14, list.Count);

			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.AnyLocation));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.LoadPort));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentPort));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.LoadCountry));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentCountry));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.OriginPort));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.OriginCountry));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.TransitCountry));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.DestinationPort));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.DestinationCountry));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.DischargePort));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentPort));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.DischargeCountry));
			AssertEquals(true, list.ContainsCode(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentCountry));

			AssertEquals("Port matching Consol’s First Load or Own Sending Agent Port", list.GetDescriptionFromCode(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentPort));
			AssertEquals("Port matching Consol’s First Load or Own Sending Agent Country/Region", list.GetDescriptionFromCode(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentCountry));
			AssertEquals("Port matching Consol’s Last Discharge or Own Receiving Agent Port", list.GetDescriptionFromCode(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentPort));
			AssertEquals("Port matching Consol’s Last Discharge or Own Receiving Agent Country/Region", list.GetDescriptionFromCode(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentCountry));
		}

		public void TestDependantType()
		{
			AssertEquals("PRO", PhaseConstants.DependantType.Property);
			AssertEquals("TYP", PhaseConstants.DependantType.TypeName);
		}
	}
}
