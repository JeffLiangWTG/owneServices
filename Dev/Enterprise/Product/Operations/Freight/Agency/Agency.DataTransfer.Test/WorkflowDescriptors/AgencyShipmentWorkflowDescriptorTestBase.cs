using System;
using System.Linq;
using CargoWise.EntityFramework;
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
	internal abstract class AgencyShipmentWorkflowDescriptorTestBase<TShipment, TWorkflowDescriptor> : WorkflowDescriptorTestCase<TWorkflowDescriptor> where TShipment : AgencyShipment where TWorkflowDescriptor : AgencyShipmentWorkflowDescriptor, new()
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
			AssertEquals("FCL", containerModeList[0].Code);
			AssertEquals("Full Container Load", containerModeList[0].Description);
			AssertEquals("LQD", containerModeList[1].Code);
			AssertEquals("Liquid", containerModeList[1].Description);
			AssertEquals("BLK", containerModeList[2].Code);
			AssertEquals("Bulk", containerModeList[2].Description);
			AssertEquals("BBK", containerModeList[3].Code);
			AssertEquals("Break Bulk", containerModeList[3].Description);
			AssertEquals("ROR", containerModeList[4].Code);
			AssertEquals("Roll On/Roll Off", containerModeList[4].Description);
		}

		#endregion
		#region Conditions
		public virtual void TestConditionList1()
		{
			AssertEquals(typeof(AgencyShipmentWorkflowCondition1CodeList), WorkflowDescriptor.GetConditionList1(null).GetType());
		}

		public virtual void TestConditionList2()
		{
			AssertEquals(typeof(AgencyShipmentWorkflowCondition2CodeList), WorkflowDescriptor.GetConditionList2(null).GetType());
		}

		#endregion
		#region Workflow Trigger
		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Consignee | MessageRecipientPartyType.Consignor | MessageRecipientPartyType.BillToParty | MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email | MessageRecipientPartyType.SendingAgent | MessageRecipientPartyType.ReceivingAgent | MessageRecipientPartyType.Principal | MessageRecipientPartyType.DepartureCTO | MessageRecipientPartyType.BookingParty;
			}
		}

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[] { JobShipmentSchema.JS_JX, JobShipmentSchema.JS_RL_NKOrigin, JobShipmentSchema.JS_RL_NKDestination, JobConsolTransportSchema.JW_RL_NKLoadPort, JobConsolTransportSchema.JW_RL_NKDiscPort, JobConsolTransportSchema.JW_Vessel, JobConsolTransportSchema.JW_VoyageFlight };
			}
		}

		#endregion
		#region MessageRecipientPartyList
		public void TestGetMessageRecipientPartyList()
		{
			var hasRecipientParty = !IsAgentShipmentWorkflowDescriptor;
			var shipment1 = NewShipmentWithOrgParties(Constants.TransportModes.Rail);
			AssertHasRecipientParty(shipment1, MessageRecipientPartyTypeList.Codes.DeliveryCartage, hasRecipientParty);
			AssertHasRecipientParty(shipment1, MessageRecipientPartyTypeList.Codes.PickupCartage, hasRecipientParty);
			var shipment2 = NewShipmentWithOrgParties(Constants.TransportModes.Road);
			AssertHasRecipientParty(shipment2, MessageRecipientPartyTypeList.Codes.DeliveryCartage, hasRecipientParty);
			AssertHasRecipientParty(shipment2, MessageRecipientPartyTypeList.Codes.PickupCartage, hasRecipientParty);
			var shipment3 = NewShipmentWithOrgParties(Constants.TransportModes.Air);
			AssertHasRecipientParty(shipment3, MessageRecipientPartyTypeList.Codes.DeliveryCartage, false);
			AssertHasRecipientParty(shipment3, MessageRecipientPartyTypeList.Codes.PickupCartage, false);
		}

		void AssertHasRecipientParty(TShipment tShipment, ZString partyType, bool hasRecipientParty)
		{
			var recipientParty = new TWorkflowDescriptor().GetMessageRecipientParty(tShipment, partyType).Select(recipient => recipient.Party).SingleOrDefault();
			AssertEquals(hasRecipientParty, recipientParty != null);
		}

		bool IsAgentShipmentWorkflowDescriptor
		{
			get
			{
				return WorkflowDescriptor.GetType() == typeof(AgencyShipmentWorkflowDescriptor);
			}
		}

		#endregion
		#region Shipments With Configured Organisation Parties
		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { RoadShipmentWithOrgParties, RailShipmentWithOrgParties };
		}

		protected override IWorkflowProvider GetParentWithConfiguredOrganisationPartiesForXmlFallbackTest()
		{
			return RoadShipmentWithOrgParties;
		}

		protected TShipment RoadShipmentWithOrgParties
		{
			get
			{
				return roadShipmentWithOrgParties ?? (roadShipmentWithOrgParties = NewShipmentWithOrgParties(Constants.TransportModes.Road));
			}
		}

		TShipment roadShipmentWithOrgParties;
		protected TShipment RailShipmentWithOrgParties
		{
			get
			{
				return railShipmentWithOrgParties ?? (railShipmentWithOrgParties = NewShipmentWithOrgParties(Constants.TransportModes.Rail));
			}
		}

		TShipment railShipmentWithOrgParties;
		protected override BusinessObject NewBusinessObjectInTable(ITableSchema table)
		{
			switch (table.TableName)
			{
				case JobShipmentSchema.Constants.TableName:
					return Factory.New<AgencyShipment>();
				default:
					return base.NewBusinessObjectInTable(table);
			}
		}

		protected virtual TShipment NewShipmentWithOrgParties(string transportMode)
		{
			TShipment shipment = Factory.NewWithValidTestData<TShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.ConsigneePK = ConsigneeOrg.PK;
			shipment.ConsignorPK = ConsignorOrg.PK;
			shipment.JS_OH_DeliveryAgent = PrincipalOrg.PK;
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			shipment.JS_JX = sailing.PK;
			shipment.BookingPartyDocumentaryAddress.OrganisationPK = BookingPartyOrg.PK;
			//Adding sailing to add DepartureCTO. It comes from the origin of sailing
			origin.JA_Calc_DepartureCTOAddressOrg = DepartureCTOOrg.PK;
			//Adding ArrivalCTO. It comes from the destination of sailing
			destination.JB_Calc_ArrivalCTOAddressOrg = ArrivalCTOOrg.PK;
			//Setting SendingAgent and receivingAgent. It comes from principal's agents
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
			firstTransport.JW_TransportMode = transportMode;
			var secondTransport = shipment.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = shipment.JS_RL_NKDestination; //"AUBNE";
			secondTransport.JW_TransportType = Constants.TransportPlanningType.Other;
			secondTransport.JW_OA_ArrivalLocation = DeliveryCartageOrg.MainAddress.PK;
			secondTransport.JW_OA_CarrierAddress = DeliveryCartageOrg.MainAddress.PK;
			secondTransport.JW_TransportMode = transportMode;
			return shipment;
		}

		#endregion
		#region EstimateDefaultedFromList
		protected override TimeSpan GetUtcOffsetForDateTimeSourceType(IWorkflowProvider workflowProvider, string dateTimeSourceType)
		{
			switch (dateTimeSourceType)
			{
				case AgencyShipmentDefaultedFromList.Codes.AnticipatedTimeOfDeparture:
					return TimeSpan.FromHours(11);
				default:
					return base.GetUtcOffsetForDateTimeSourceType(workflowProvider, dateTimeSourceType);
			}
		}

		protected override void SetDateTimeSourcePropertyValue(IWorkflowProvider workflowProvider, string dateTimeSourceType, ZDateTime localTime)
		{
			switch (dateTimeSourceType)
			{
				case AgencyShipmentDefaultedFromList.Codes.AnticipatedTimeOfDeparture:
					((AgencyShipment)workflowProvider).Transports[0].JW_ATD = localTime;
					break;
				default:
					base.SetDateTimeSourcePropertyValue(workflowProvider, dateTimeSourceType, localTime);
					break;
			}
		}
		#endregion
	}
}
