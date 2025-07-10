using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentVsConsolHelperTest : TestCaseWithFactory
	{
		public void TestAllowAttachingDirectShipmentsToAgentConsols()
		{
			Assert(ShipmentVsConsolHelper.AllowAttachingDirectShipmentsToAgentConsols(new[] { directShipment1, directShipment2 },
				new[] { preCarriageConsol, onForwardingConsol }));
		}

		public void TestIsPreCarriageForDirectShipment()
		{
			Assert(ShipmentVsConsolHelper.IsPreCarriageForDirectShipment(preCarriageConsol, directShipment1));
			Assert(!ShipmentVsConsolHelper.IsPreCarriageForDirectShipment(onForwardingConsol, directShipment1));
			Assert(!ShipmentVsConsolHelper.IsPreCarriageForDirectShipment(directDepartureConsol1, directShipment1));
			Assert(!ShipmentVsConsolHelper.IsPreCarriageForDirectShipment(directArrivalConsol1, directShipment1));

			preCarriageConsol.JK_RL_NKLoadPort = "CATOR";
			Assert("PreCarriage agent consol is either Road/Rail or Domestic Air/Sea", !ShipmentVsConsolHelper.IsPreCarriageForDirectShipment(preCarriageConsol, directShipment1));
			preCarriageConsol.JK_TransportMode = "ROA";
			Assert("PreCarriage agent consol is either Road/Rail or Domestic Air/Sea", ShipmentVsConsolHelper.IsPreCarriageForDirectShipment(preCarriageConsol, directShipment1));
			preCarriageConsol.JK_RL_NKDischargePort = "USNYC";
			Assert("PreCarriage consol's discharge port should match direct departure consol's load port", !ShipmentVsConsolHelper.IsPreCarriageForDirectShipment(preCarriageConsol, directShipment1));
			preCarriageConsol.JK_RL_NKDischargePort = directShipment1.JS_RL_NKOrigin;
			preCarriageConsol.Transports[0].JW_ETA = directDepartureConsol1.Transports[0].JW_ETD.AddDays(1);
			Assert("PreCarriage consol's ETA should be earlier than direct departure consol's ETD", !ShipmentVsConsolHelper.IsPreCarriageForDirectShipment(preCarriageConsol, directShipment1));
		}

		public void TestIsOnForwardingForDirectShipment()
		{
			Assert(ShipmentVsConsolHelper.IsOnForwardingForDirectShipment(onForwardingConsol, directShipment1));
			Assert(!ShipmentVsConsolHelper.IsOnForwardingForDirectShipment(preCarriageConsol, directShipment1));
			Assert(!ShipmentVsConsolHelper.IsOnForwardingForDirectShipment(directDepartureConsol1, directShipment1));
			Assert(!ShipmentVsConsolHelper.IsOnForwardingForDirectShipment(directArrivalConsol1, directShipment1));

			onForwardingConsol.JK_RL_NKDischargePort = "NZAKL";
			Assert("OnForwarding agent consol is either Road/Rail or Domestic Air/Sea", !ShipmentVsConsolHelper.IsOnForwardingForDirectShipment(onForwardingConsol, directShipment1));
			onForwardingConsol.JK_TransportMode = "RAI";
			Assert("OnForwarding agent consol is either Road/Rail or Domestic Air/Sea", ShipmentVsConsolHelper.IsOnForwardingForDirectShipment(onForwardingConsol, directShipment1));
			onForwardingConsol.JK_RL_NKLoadPort = "AUBNE";
			Assert("OnForwarding consol's load port should match direct arrival consol's discharge port", !ShipmentVsConsolHelper.IsOnForwardingForDirectShipment(onForwardingConsol, directShipment1));
			onForwardingConsol.JK_RL_NKLoadPort = directShipment1.JS_RL_NKDestination;
			onForwardingConsol.Transports[0].JW_ETD = directArrivalConsol1.Transports[0].JW_ETA.AddDays(-1);
			Assert("OnForwarding consol's ETD should be after direct arrival consol's ETA", !ShipmentVsConsolHelper.IsPreCarriageForDirectShipment(onForwardingConsol, directShipment1));
		}

		#region ExclusiveGatewayServiceChecker

		public void TestExclusiveGatewayServiceChecker_Collect()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DEF").RS_IsGateway = true;

			var permissionHelper = new PermissionHelper();

			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", "STD", Factory);
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.RequestPermissionByImpersonation = permissionHelper.RequestPermission;

			var consol = FreightTestHelper.GetConsol<CommonConsol>("CONSOL1", "SEA", "AGT", "USLAX", "AUSYD", ZDateTime.Today, ZDateTime.Today.AddDays(5), Factory, shipment);
			consol.JK_PrepaidCollect = "CCX";
			consol.RequestPermissionByImpersonation = permissionHelper.RequestPermission;

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var agentPorts = receivingAgent.AppointedGatewayAgentPorts.AddNew();
			agentPorts.O5_OA_AgentOfficeAddress = receivingAgent.MainAddress.PK;
			agentPorts.O5_PortOrCountry = consol.JK_RL_NKDischargePort;
			agentPorts.O5_AgentDirection = "BTH";
			agentPorts.O5_SeaAgentStatus = "GTA";
			agentPorts.O5_AirAgentStatus = "GTA";
			agentPorts.O5_RailAgentStatus = "GTA";
			agentPorts.O5_RoadAgentStatus = "GTA";

			Env.Security.ConsolAttachShipmentWithDifferentGatewayServiceLevel.IsAllowed = false;
			var checker = new ShipmentVsConsolHelper.ExclusiveGatewayServiceChecker();

			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = "GTA";

			consol.JK_RS_NKGatewayServiceLevel = "";
			shipment.JS_RS_NKGatewayServiceLevel = "";
			AssertEquals(
				"No error when consol's Gateway Service Level is empty",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "";
			shipment.JS_RS_NKGatewayServiceLevel = "DIR";
			AssertEquals(
				"No error when consol's Gateway Service Level is empty",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "";
			AssertNotEquals(
				"Show error when shipment's Gateway Service Level is empty",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "DIR";
			AssertEquals(
				"No error when shipment's Gateway Service Level equals consol's Gateway Service Level but there is no supporting G/W SL been predefined",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "STD";
			AssertEquals(
				"No error when shipment's Gateway Service Level is different but there is no supporting G/W SL been predefined",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = "";
			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "";
			AssertEquals(
				"No error when payment is collect and receiving agent type is empty",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "STD";
			AssertEquals(
				"No error when supporting shipment's Gateway Service Level is different, But agent type is empty",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_ReceivingForwarderHandlingType = "GTA";
			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "STD";
			AssertEquals(
				"No error when supporting shipment's Gateway Service Level is different but there is no predefined",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			var gatewayService = agentPorts.ExclusiveGatewayServices.AddNew();
			gatewayService.O7_RS_NKGatewayService = "DIR";
			gatewayService.O7_RS_NKShipmentServiceLevel = "STD";

			consol.JK_ReceivingForwarderHandlingType = "GTA";
			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "STD";
			AssertEquals(
				"No error when supporting shipment's Gateway Service Level is pre-defined",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "DIR";
			AssertNotEquals(
				"Show error when shipment's Gateway Service Level equals consol's Gateway Service Level but NOT match with existing supporting G/W SL been predefined",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "DEF";
			AssertNotEquals(
				"Show error when shipment's Gateway Service Level different with consol's Gateway Service Level and NOT match with existing supporting G/W SL been predefined",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			Env.Security.ConsolAttachShipmentWithDifferentGatewayServiceLevel.IsAllowed = true;
			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "DEF";
			permissionHelper.HasCalled = false;
			AssertEquals(
				"No error when supporting shipment's Gateway Service Level is NOT pre-defined but user has permision",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));
			Assert(!permissionHelper.HasCalled);

			Env.Security.ConsolAttachShipmentWithDifferentGatewayServiceLevel.IsAllowed = false;
			checker = new ShipmentVsConsolHelper.ExclusiveGatewayServiceChecker();
			permissionHelper.HasCalled = false;
			AssertNotEquals(
				"Show error when supporting shipment's Gateway Service Level is NOT pre-defined and user doesn't have permision",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));
			Assert(permissionHelper.HasCalled);

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "DIR";
			checker = new ShipmentVsConsolHelper.ExclusiveGatewayServiceChecker();
			permissionHelper.HasCalled = false;
			AssertNotEquals(
				"Show error when supporting shipment's Gateway Service Level is the same but is NOT pre-defined and user doesn't have permision",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));
			Assert(permissionHelper.HasCalled);

			permissionHelper.HasCalled = false;
			permissionHelper.ReturnValue = true;
			checker = new ShipmentVsConsolHelper.ExclusiveGatewayServiceChecker();
			AssertEquals(
				"No error when user input a credentials with a higher rights",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));
			Assert(permissionHelper.HasCalled);
		}

		public void TestExclusiveGatewayServiceChecker_Prepaid()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DEF").RS_IsGateway = true;

			var permissionHelper = new PermissionHelper();

			var shipment = FreightTestHelper.GetShipment<CommonShipment>("SHP", "STD", Factory);
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.RequestPermissionByImpersonation = permissionHelper.RequestPermission;

			var consol = FreightTestHelper.GetConsol<CommonConsol>("CONSOL1", "SEA", "AGT", "USLAX", "AUSYD", ZDateTime.Today, ZDateTime.Today.AddDays(5), Factory, shipment);
			consol.JK_PrepaidCollect = "PPD";
			consol.RequestPermissionByImpersonation = permissionHelper.RequestPermission;

			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var agentPorts = sendingAgent.AppointedGatewayAgentPorts.AddNew();
			agentPorts.O5_OA_AgentOfficeAddress = sendingAgent.MainAddress.PK;
			agentPorts.O5_PortOrCountry = consol.JK_RL_NKLoadPort;
			agentPorts.O5_AgentDirection = "BTH";
			agentPorts.O5_SeaAgentStatus = "GTA";
			agentPorts.O5_AirAgentStatus = "GTA";
			agentPorts.O5_RailAgentStatus = "GTA";
			agentPorts.O5_RoadAgentStatus = "GTA";

			Env.Security.ConsolAttachShipmentWithDifferentGatewayServiceLevel.IsAllowed = false;
			var checker = new ShipmentVsConsolHelper.ExclusiveGatewayServiceChecker();

			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = "GTA";

			consol.JK_RS_NKGatewayServiceLevel = "";
			shipment.JS_RS_NKGatewayServiceLevel = "";
			AssertEquals(
				"No error when consol's Gateway Service Level is empty",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "";
			shipment.JS_RS_NKGatewayServiceLevel = "DIR";
			AssertEquals(
				"No error when consol's Gateway Service Level is empty",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "";
			AssertNotEquals(
				"Show error when shipment's Service Level is empty",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "DIR";
			AssertEquals(
				"No error when shipment's Service Level equals consol's Gateway Service Level but there is no supporting G/W SL been predefined",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "STD";
			AssertEquals(
				"No error when shipment's Gateway Service Level is different but there is no supporting G/W SL been predefined",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = "";
			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "";
			AssertEquals(
				"No error when payment is collect and sending agent type is empty",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "STD";
			AssertEquals(
				"No error when supporting shipment Service Level is different, But agent type is empty",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_SendingForwarderHandlingType = "GTA";
			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "STD";
			AssertEquals(
				"No error when supporting shipment Service Level is different but there is no predefined",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			var gatewayService = agentPorts.ExclusiveGatewayServices.AddNew();
			gatewayService.O7_RS_NKGatewayService = "DIR";
			gatewayService.O7_RS_NKShipmentServiceLevel = "STD";

			consol.JK_SendingForwarderHandlingType = "GTA";
			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "STD";
			AssertEquals(
				"No error when supporting shipment Service Level is pre-defined",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "DIR";
			AssertNotEquals(
				"Show error when shipment's Gateway Service Level equals consol's Gateway Service Level but NOT match with existing supporting G/W SL been predefined",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "DEF";
			AssertNotEquals(
				"Show error when shipment's Gateway Service Level different with consol's Gateway Service Level and NOT match with existing supporting G/W SL been predefined",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));

			Env.Security.ConsolAttachShipmentWithDifferentGatewayServiceLevel.IsAllowed = true;
			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "DEF";
			permissionHelper.HasCalled = false;
			AssertEquals(
				"No error when supporting shipment Service Level is NOT pre-defined but user has permision",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));
			Assert(!permissionHelper.HasCalled);

			Env.Security.ConsolAttachShipmentWithDifferentGatewayServiceLevel.IsAllowed = false;
			checker = new ShipmentVsConsolHelper.ExclusiveGatewayServiceChecker();
			permissionHelper.HasCalled = false;
			AssertNotEquals(
				"Show error when supporting shipment Service Level is NOT pre-defined and user doesn't have permision",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));
			Assert(permissionHelper.HasCalled);

			consol.JK_RS_NKGatewayServiceLevel = "DIR";
			shipment.JS_RS_NKGatewayServiceLevel = "DIR";
			checker = new ShipmentVsConsolHelper.ExclusiveGatewayServiceChecker();
			permissionHelper.HasCalled = false;
			AssertNotEquals(
				"Show error when supporting shipment's Gateway Service Level is the same but is NOT pre-defined and user doesn't have permision",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));
			Assert(permissionHelper.HasCalled);

			permissionHelper.HasCalled = false;
			permissionHelper.ReturnValue = true;
			checker = new ShipmentVsConsolHelper.ExclusiveGatewayServiceChecker();
			AssertEquals(
				"No error when user input a credentials with a higher rights",
				string.Empty,
				checker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(AttachDetachAction.Attach, shipment, consol));
			Assert(permissionHelper.HasCalled);
		}

		class PermissionHelper
		{
			public bool HasCalled { get; set; }
			public bool ReturnValue { get; set; }

			public bool RequestPermission(string message, SecurityCheckpoint permission)
			{
				HasCalled = true;
				return ReturnValue;
			}
		}

		#endregion

		#region Implementation

		CommonConsol preCarriageConsol;
		CommonConsol onForwardingConsol;

		CommonShipment directShipment1;
		CommonConsol directDepartureConsol1;
		CommonConsol directArrivalConsol1;

		CommonShipment directShipment2;

		ZDateTime today;

		protected override void SetUp()
		{
			base.SetUp();

			today = ZDateTime.Today;

			directShipment1 = Factory.New<CommonShipment>();
			directShipment1.JS_UniqueConsignRef = "DIRECTSHIPMENT1";
			directShipment1.JS_TransportMode = "SEA";
			directShipment1.JS_RL_NKOrigin = "USLAX";
			directShipment1.JS_RL_NKDestination = "AUSYD";
			directShipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			directDepartureConsol1 = FreightTestHelper.GetConsol<CommonConsol>("DRT Departure 1", "SEA", "DRT", "USLAX", "SGSIN", today.AddDays(2), today.AddDays(3), Factory, directShipment1);
			directArrivalConsol1 = FreightTestHelper.GetConsol<CommonConsol>("DRT Arrival 1", "SEA", "DRT", "SGSIN", "AUSYD", today.AddDays(4), today.AddDays(5), Factory, directShipment1);

			directShipment2 = Factory.New<CommonShipment>();
			directShipment2.JS_UniqueConsignRef = "DIRECTSHIPMENT2";
			directShipment2.JS_TransportMode = "AIR";
			directShipment2.JS_RL_NKOrigin = "USLAX";
			directShipment2.JS_RL_NKDestination = "AUSYD";
			directShipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			_ = FreightTestHelper.GetConsol<CommonConsol>("DRT Departure 2", "AIR", "DRT", "USLAX", "NZAKL", today.AddDays(2), today.AddDays(3), Factory, directShipment2);
			_ = FreightTestHelper.GetConsol<CommonConsol>("DRT Arrival 2", "AIR", "DRT", "NZAKL", "AUSYD", today.AddDays(4), today.AddDays(5), Factory, directShipment2);

			preCarriageConsol = FreightTestHelper.GetConsol<CommonConsol>("AGT Pre-carriage", "SEA", "AGT", "USCHI", "USLAX", today, today.AddDays(1), Factory);
			onForwardingConsol = FreightTestHelper.GetConsol<CommonConsol>("AGT On-forwarding", "SEA", "AGT", "AUSYD", "AUMEL", today.AddDays(6), today.AddDays(7), Factory);

			Factory.Save();
		}

		#endregion
	}
}
