using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	internal abstract class AgencyContainerWorkflowDescriptorTestBase<TContainer, TWorkflowDescriptor> : WorkflowDescriptorTestCase<TWorkflowDescriptor> where TContainer : AgencyShipmentContainer where TWorkflowDescriptor : AgencyContainerWorkflowDescriptor, new()
	{
		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		#region SubTypes
		public override void TestSubTypes()
		{
			AssertEquals("1 sub type", 1, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Cargo type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			CodeDescriptionPairList containerModeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[0].List;
			AssertContainsExactElementsInAnyOrder(new AgencyCargoTypeCodeDescriptionPairList(), containerModeList);
		}

		#endregion
		#region Conditions
		public virtual void TestConditionList1()
		{
			AssertEquals(typeof(AgencyContainerWorkflowCondition1CodeList), WorkflowDescriptor.GetConditionList1(null).GetType());
		}

		public virtual void TestConditionList2()
		{
			AssertEquals(typeof(AgencyContainerWorkflowCondition2CodeList), WorkflowDescriptor.GetConditionList2(null).GetType());
		}

		#endregion
		#region MessageRecipientPartyList
		public void TestGetMessageRecipientPartyList()
		{
			var container1 = NewContainerWithOrgParties(Constants.TransportModes.Rail);
			AssertHasRecipientParty(container1, MessageRecipientPartyTypeList.Codes.DeliveryCartage, true);
			AssertHasRecipientParty(container1, MessageRecipientPartyTypeList.Codes.PickupCartage, true);
			var container2 = NewContainerWithOrgParties(Constants.TransportModes.Road);
			AssertHasRecipientParty(container2, MessageRecipientPartyTypeList.Codes.DeliveryCartage, true);
			AssertHasRecipientParty(container2, MessageRecipientPartyTypeList.Codes.PickupCartage, true);
			var container3 = NewContainerWithOrgParties(Constants.TransportModes.Air);
			AssertHasRecipientParty(container3, MessageRecipientPartyTypeList.Codes.DeliveryCartage, false);
			AssertHasRecipientParty(container3, MessageRecipientPartyTypeList.Codes.PickupCartage, false);
		}

		void AssertHasRecipientParty(AgencyShipmentContainer container, ZString partyType, bool hasRecipientParty)
		{
			var recipientParty = new AgencyContainerWorkflowDescriptor().GetMessageRecipientParty(container, partyType).Select(recipient => recipient.Party).SingleOrDefault();
			AssertEquals(hasRecipientParty, recipientParty != null);
		}

		#endregion
		#region Workflow Trigger
		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Consignor | MessageRecipientPartyType.Consignee | MessageRecipientPartyType.BillToParty | MessageRecipientPartyType.Principal | MessageRecipientPartyType.PickupCartage | MessageRecipientPartyType.DeliveryCartage | MessageRecipientPartyType.DepartureContainerYard | MessageRecipientPartyType.ArrivalContainerYard | MessageRecipientPartyType.DepartureCTO | MessageRecipientPartyType.ArrivalCTO | MessageRecipientPartyType.SendingAgent | MessageRecipientPartyType.ReceivingAgent | MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;
			}
		}

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[] { JobContainerSchema.JC_RC, JobContainerSchema.JC_ContainerCount, JobContainerSchema.JC_ContainerNum, JobContainerSchema.JC_SetPointTemp, JobContainerSchema.JC_SetPointTempUnit, JobContainerSchema.JC_RH_NKContainerCommodityCode, JobContainerSchema.JC_AirVentFlow };
			}
		}

		public void TestAgencyContainerEventDataModel()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			var eventDataModel = WorkflowDescriptor.GetEventDataModel(container);
			Assert("Event data model should be AgencyShipmentContainer", eventDataModel is AgencyContainerEventDataModel);
		}

		#endregion
		#region Container With Configured Organisation Parties
		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { RoadContainerWithOrgParties, RailContainerWithOrgParties };
		}

		protected IWorkflowProvider GetTestParentWithConfiguredOrganisationPartiesForXmlFallbackTest()
		{
			return RoadContainerWithOrgParties;
		}

		protected TContainer RoadContainerWithOrgParties
		{
			get
			{
				return roadContainerWithOrgParties ?? (roadContainerWithOrgParties = NewContainerWithOrgParties(Constants.TransportModes.Rail));
			}
		}

		TContainer roadContainerWithOrgParties;
		protected TContainer RailContainerWithOrgParties
		{
			get
			{
				return railContainerWithOrgParties ?? (railContainerWithOrgParties = NewContainerWithOrgParties(Constants.TransportModes.Rail));
			}
		}

		TContainer railContainerWithOrgParties;
		protected virtual TContainer NewContainerWithOrgParties(string tranportMode)
		{
			TContainer container = Factory.NewWithValidTestData<TContainer>();
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			container.JC_JS_FCLBookingOnlyLink = shipment.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.ConsigneePK = ConsigneeOrg.PK;
			shipment.ConsignorPK = ConsignorOrg.PK;
			shipment.JS_OH_DeliveryAgent = PrincipalOrg.PK;
			shipment.DocsAndCartage.PickupCartageCoPK = PickupCartageOrg.PK;
			shipment.DocsAndCartage.DeliveryCartageCoPK = DeliveryCartageOrg.PK;
			container.JC_OA_DepartureContainerYardAddress = DepartureContainerYardOrg.MainAddress.PK;
			container.JC_OA_ArrivalContainerYardAddress = ArrivalContainerYardOrg.MainAddress.PK;
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			shipment.JS_JX = sailing.PK;
			origin.JA_Calc_DepartureCTOAddressOrg = DepartureCTOOrg.PK;
			destination.JB_Calc_ArrivalCTOAddressOrg = ArrivalCTOOrg.PK;
			var port1 = PrincipalOrg.CarrierAppointedAgentPorts_Agency.AddNew();
			port1.O5_PortOrCountry = "AUSYD";
			port1.O5_OH = SendingAgentOrg.PK;
			port1.O5_OA_AgentOfficeAddress = SendingAgentOrg.MainAddress.PK;
			port1.O5_SeaAirCarrierOrForwarderType = "AGY";
			port1.O5_AgentDirection = "BTH";
			var port2 = PrincipalOrg.CarrierAppointedAgentPorts_Agency.AddNew();
			port2.O5_PortOrCountry = "AUBNE";
			port2.O5_OH = ReceivingAgentOrg.PK;
			port2.O5_OA_AgentOfficeAddress = ReceivingAgentOrg.MainAddress.PK;
			port2.O5_SeaAirCarrierOrForwarderType = "AGY";
			port2.O5_AgentDirection = "BTH";
			JobHeader.Loader jobLoader = new JobHeader.Loader(shipment);
			JobHeader job = jobLoader.TryCreate();
			job.JH_OA_LocalChargesAddr = BillToPartyOrg.MainAddress.PK;
			var firstTransport = shipment.Transports.AddNew();
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKLoadPort = shipment.JS_RL_NKOrigin; //"AUSYD";
			firstTransport.JW_RL_NKDiscPort = "AUMEL";
			firstTransport.JW_TransportType = Constants.TransportPlanningType.Other;
			firstTransport.JW_OA_DepartureLocation = PickupCartageOrg.MainAddress.PK;
			firstTransport.JW_OA_CarrierAddress = PickupCartageOrg.MainAddress.PK;
			firstTransport.JW_TransportMode = tranportMode;
			var secondTransport = shipment.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = shipment.JS_RL_NKDestination; //"AUBNE";
			secondTransport.JW_TransportType = Constants.TransportPlanningType.Other;
			secondTransport.JW_OA_ArrivalLocation = DeliveryCartageOrg.MainAddress.PK;
			secondTransport.JW_OA_CarrierAddress = DeliveryCartageOrg.MainAddress.PK;
			secondTransport.JW_TransportMode = tranportMode;
			return container;
		}
		#endregion
	}
}
