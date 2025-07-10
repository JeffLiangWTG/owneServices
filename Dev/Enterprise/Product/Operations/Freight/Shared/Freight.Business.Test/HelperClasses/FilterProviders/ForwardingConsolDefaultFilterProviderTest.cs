using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ForwardingConsolDefaultFilterProvider))]
	sealed class ForwardingConsolDefaultFilterProviderTest : DefaultFilterProviderTest<ForwardingConsolDefaultFilterProvider>
	{
		const string TransportMode = "Transport Mode";
		const string ContainerMode = "Container Mode";
		const string ConsolType = "Consol Type";
		const string LoadDischarge = "End Ports (First Load / Last Disch.)";
		const string OriginDestination = "Origin / Destination";
		const string MasterBill = "Master Bill";
		const string ETD = "ETD";
		const string ETA = "ETA";
		const string Carrier = "Carrier";
		const string SendingReceiveAgent = "Send / Receive Agents";

		#region TestTransportMode

		public void TestTransportMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, TransportMode);

			Provider.TransportMode = Core.Constants.TransportModes.Sea;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, TransportMode, "Property", (ZString)Core.Constants.TransportModes.Sea);
		}

		#endregion

		#region TestContainerMode

		public void TestContainerMode()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ContainerMode);

			Provider.ContainerMode = Core.Constants.ContainerModes.FCL;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ContainerMode, "Property", (ZString)Core.Constants.ContainerModes.FCL);
		}

		#endregion

		#region ConsolType

		public void TestConsolType()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ConsolType);

			Provider.ConsolType = Core.Constants.AgentType.Agent;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ConsolType, "Property", (ZString)Core.Constants.AgentType.Agent);
		}

		#endregion

		#region TestLoadPort

		public void TestLoadPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, LoadDischarge);

			Provider.LoadPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, LoadDischarge, "Property1", HomePort);
		}

		#endregion

		#region TestDischargePort

		public void TestDischargePort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, LoadDischarge);

			Provider.DischargePort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, LoadDischarge, "Property2", HomePort);
		}

		#endregion

		#region TestOriginPort

		public void TestOriginPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OriginDestination);

			Provider.OriginPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OriginDestination, "Property1", HomePort);
		}

		#endregion

		#region TestDestinationPort

		public void TestDestinationPort()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, OriginDestination);

			Provider.DestinationPort = HomePort;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, OriginDestination, "Property2", HomePort);
		}

		#endregion

		#region TestMasterBill

		public void TestMasterBill()
		{
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, MasterBill);

			Provider.MasterBill = "MasterBill";
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, MasterBill, "Property", (ZString)"MasterBill");
		}

		#endregion

		#region TestETDFrom

		public void TestETDFrom()
		{
			ZDateTime today = ZDateTime.Today;

			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETD);

			Provider.ETDFrom = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETD, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETD, "Property1", today);
		}

		#endregion

		#region TestETDTo

		public void TestETDTo()
		{
			ZDateTime today = ZDateTime.Today;

			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETD);

			Provider.ETDTo = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETD, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETD, "Property2", today);
		}

		#endregion

		#region TestETAFrom

		public void TestETAFrom()
		{
			ZDateTime today = ZDateTime.Today;

			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETA);

			Provider.ETAFrom = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETA, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETA, "Property1", today);
		}

		#endregion

		#region TestETATo

		public void TestETATo()
		{
			ZDateTime today = ZDateTime.Today;

			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, ETA);

			Provider.ETATo = today.AddHours(8);
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, ETA, "PropertySearch", (ZString)"Date range");
			AssertHasDefault("Should only keep the date part", Collection, ETA, "Property2", today);
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			var carrier = ZGuid.NewZGuid();
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, Carrier);

			Provider.Carrier = carrier;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, Carrier, "Property", carrier);
		}

		#endregion

		#region TestSendingAgent

		public void TestSendingAgent()
		{
			var sendingAgent = ZGuid.NewZGuid();
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, SendingReceiveAgent);

			Provider.SendingAgent = sendingAgent;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, SendingReceiveAgent, "Property1", sendingAgent);
		}

		#endregion

		#region TestReceivingAgent

		public void TestReceivingAgent()
		{
			var receivingAgent = ZGuid.NewZGuid();
			Provider.SetDefaultFilters(Collection);
			AssertNoDefaults(Collection, SendingReceiveAgent);

			Provider.ReceivingAgent = receivingAgent;
			Provider.SetDefaultFilters(Collection);
			AssertHasDefault(Collection, SendingReceiveAgent, "Property2", receivingAgent);
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobConsol; }
		}

		#endregion
	}
}
