using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ContainerWorkflowDescriptor))]
	sealed class ContainerWorkflowDescriptorTest : WorkflowDescriptorTestCase<ContainerWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.ContainerWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Container", WorkflowDescriptor.Description);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestSubTypes()
		{
			AssertEquals("2 sub types", 2, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Sub Type 1 is Transport Mode", "Transport Mode", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Sub Type 2 is Container Mode", "Container Mode", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);

			Assert(WorkflowDescriptor.SubTypeInformation[0].List.ContainsCode("SEA"));
			Assert(WorkflowDescriptor.SubTypeInformation[0].List.ContainsCode("ROA"));

			WorkflowDescriptor.LastProcessTaskTemplate = Factory.New<ProcessTaskTemplate>();
			WorkflowDescriptor.LastProcessTaskTemplate.P0_SubType1 = "SEA";
			Assert(WorkflowDescriptor.SubTypeInformation[1].List.ContainsCode("FCL"));
			Assert(WorkflowDescriptor.SubTypeInformation[1].List.ContainsCode("LCL"));
			Assert(!WorkflowDescriptor.SubTypeInformation[1].List.ContainsCode("ULD"));

			WorkflowDescriptor.LastProcessTaskTemplate.P0_SubType1 = "AIR";
			Assert(WorkflowDescriptor.SubTypeInformation[1].List.ContainsCode("ULD"));
			Assert(!WorkflowDescriptor.SubTypeInformation[1].List.ContainsCode("FCL"));

			WorkflowDescriptor.LastProcessTaskTemplate.P0_SubType1 = "ROA";
			Assert(WorkflowDescriptor.SubTypeInformation[1].List.ContainsCode("FTL"));
			Assert(WorkflowDescriptor.SubTypeInformation[1].List.ContainsCode("LTL"));
			Assert(!WorkflowDescriptor.SubTypeInformation[1].List.ContainsCode("ULD"));

			WorkflowDescriptor.LastProcessTaskTemplate.P0_SubType1 = "RAI";
			Assert(WorkflowDescriptor.SubTypeInformation[1].List.ContainsCode("FCL"));
			Assert(!WorkflowDescriptor.SubTypeInformation[1].List.ContainsCode("FTL"));
		}

		public void TestContainerEventDataModel()
		{
			var container = Factory.New<CommonContainer>();
			var eventDataModel = WorkflowDescriptor.GetEventDataModel(container);
			Assert("Event data model should be ContainerEventDataModel", eventDataModel is ContainerEventDataModel);
		}

		public void TestExtraDataSubstitution()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var commonContainer = NewContainerWithConfiguredOrgParties();
			var customsContainer = ((BusinessObjectCollection)declaration["CusContainers"]).AddNew();

			commonContainer.JC_ContainerNum = "12121212";
			commonContainer.Consol.JK_UniqueConsignRef = "C000001001";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VesselC";

			var sailing = NewSailing(vessel, "VoyageC", "AUSYD", "USLAX");
			commonContainer.Consol.Transports[0].JW_JX = sailing.PK;

			customsContainer[CusContainerSchema.CO_JC] = commonContainer.PK;
			declaration[JobDeclarationSchema.JE_DeclarationReference] = "B00001001";
			declaration[JobDeclarationSchema.JE_VesselName] = "VesselD";
			declaration[JobDeclarationSchema.JE_VoyageFlightNo] = "VoyageD";

			var trigger = commonContainer.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "DUMMT TASK";
			trigger.ReferenceCode = "REF";

			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "dummy@test.com";

			var query = new DocumentZQuery("RepBookingsReports", "FCL Container Availability");
			var containerAvailabilityReportUrl = ObjectFactory.Get<IReportUrlHandler>().Create(Factory.LoadTop1<ReportCommand>(query));

			var consolURL = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.JobConsol, commonContainer.Consol.PK.ToGuid());
			var declarationURL = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Customs.JobDeclaration, declaration.PK.ToGuid());

			var expectedJobsAffected = string.Format(@"<a href=""{0}"">Consol C000001001</a>&nbsp;<br />
&nbsp;<a href=""{1}"">Declaration B00001001</a>", consolURL, declarationURL);

			notification.PQ_EmailText = "Email content: ContainerAvailabilityReportUrl:(*ContainerAvailabilityReportUrl*)+VesselName:(*VesselName*)+Voyage:(*Voyage*)+ContainerNumber:(*ContainerNumber*)+JobsAffected:(*JobsAffected*)";
			string expectedEmailContent = "Email content: ContainerAvailabilityReportUrl:{0}+VesselName:VesselC+Voyage:VoyageC+ContainerNumber:12121212+JobsAffected:{1}";

			var workflowTriggerNotification = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)) as WorkflowTriggerNotification;

			Env.OutgoingMailManager.EmailsCreated.Clear();
			((IProcessor)workflowTriggerNotification).Process(null);
			Factory.Save();
			AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains(string.Format(expectedEmailContent, containerAvailabilityReportUrl, expectedJobsAffected), Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		JobSailing NewSailing(RefVessel vessel, ZString voyageFlight, ZString load, ZString discharge)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = voyageFlight;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = load;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = discharge;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.Consignor |
					MessageRecipientPartyType.Carrier |
					MessageRecipientPartyType.PickupCartage |
					MessageRecipientPartyType.DeliveryCartage |
					MessageRecipientPartyType.DepartureContainerYard |
					MessageRecipientPartyType.ArrivalContainerYard |
					MessageRecipientPartyType.DepartureCTO |
					MessageRecipientPartyType.ArrivalCTO |
					MessageRecipientPartyType.DepartureCFS |
					MessageRecipientPartyType.ArrivalCFS |
					MessageRecipientPartyType.SendingAgent |
					MessageRecipientPartyType.ReceivingAgent |
					MessageRecipientPartyType.ExportBroker |
					MessageRecipientPartyType.ImportBroker |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email;
			}
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { NewContainerWithConfiguredOrgParties() };
		}

		CommonContainer NewContainerWithConfiguredOrgParties()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();
			var packline = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(packline);

			shipment.ConsigneePK = ConsigneeOrg.PK;
			shipment.ConsignorPK = ConsignorOrg.PK;
			shipment.JS_OH_ExportBroker = BrokerOrg.PK;
			shipment.JS_OH_ImportBroker = BrokerOrg.PK;

			consol.JK_OA_SendingForwarderAddress = SendingAgentOrg.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = ReceivingAgentOrg.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = DepartureCFSOrg.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = ArrivalCFSOrg.MainAddress.PK;
			consol.JK_OA_DeparturePackCFSTransportAddress = PickupCartageOrg.MainAddress.PK;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = DeliveryCartageOrg.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = CarrierOrg.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = DepartureCTOOrg.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = ArrivalCTOOrg.MainAddress.PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = DepartureContainerYardOrg.MainAddress.PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = ArrivalContainerYardOrg.MainAddress.PK;

			return container;
		}

		#region ConditionList1

		public void TestConditionList1()
		{
			var container = Factory.New<CommonContainer>();
			var milestone1 = container.WorkflowItems.Milestones.AddNew();

			var conditonList1 = WorkflowDescriptor.GetConditionList1(milestone1);
			AssertNotNull(conditonList1);

			foreach (CodeDescriptionPair codeDescPair in new JobContainerWorkflowCondition1CodeList())
			{
				AssertEquals("List should come from " + nameof(JobContainerWorkflowCondition1CodeList), true, conditonList1.ContainsCode(codeDescPair.Code));
			}
		}

		#endregion
	}
}
