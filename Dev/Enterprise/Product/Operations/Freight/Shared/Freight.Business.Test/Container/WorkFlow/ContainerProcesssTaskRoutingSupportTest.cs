using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ContainerProcessTask))]
	sealed class ContainerProcesssTaskRoutingSupportTest : RoutingSupportProcessTaskTest<CommonContainer>
	{
		#region Arrival Related Events

		public void TestContainer_IsMatchForItemTemplateMerge_ArrivalEvent()
		{
			AssertArrivalRelatedEvent(Events.ArrivalCode);
		}

		public void TestContainer_IsMatchForItemTemplateMerge_FreightUnloadedlEvent()
		{
			AssertArrivalRelatedEvent(Events.FreightUnloadedCode);
		}

		public void TestContainer_IsMatchForItemTemplateMerge_GateOutEvent()
		{
			AssertArrivalRelatedEvent(Events.GateOutCode);
		}

		void AssertArrivalRelatedEvent(string eventCode)
		{
			var container1 = Factory.New<CommonContainer>();
			container1.JC_JK = consol.PK;
			var dummy = Factory.New<DummyWithWorkflow>();

			var milestone1 = container1.WorkflowItems.AddNew();
			milestone1.IsMilestone = true;
			milestone1.TriggerConditions.TriggerEventCode = eventCode;
			milestone1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;

			var milestone2 = dummy.WorkflowItems.AddNew();
			milestone2.IsMilestone = true;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			milestone2.TriggerConditions.TriggerEventCode = eventCode;

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<FirstLeg.Destination>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<SecondLeg.Destination>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<ThirdLeg.Destination>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));
		}

		#endregion

		#region Departure Related Events

		public void TestContainer_IsMatchForItemTemplateMerge_DepartureEvent()
		{
			AssertDepartureRelevantEvent(Events.DepartureCode);
		}

		public void TestContainer_IsMatchForItemTemplateMerge_FreightLoadedEvent()
		{
			AssertDepartureRelevantEvent(Events.FreightLoadedCode);
		}

		public void TestContainer_IsMatchForItemTemplateMerge_GateInEvent()
		{
			AssertDepartureRelevantEvent(Events.GateInCode);
		}

		void AssertDepartureRelevantEvent(string eventCode)
		{
			var container1 = Factory.New<CommonContainer>();
			container1.JC_JK = consol.PK;
			var dummy = Factory.New<DummyWithWorkflow>();

			var milestone1 = container1.WorkflowItems.AddNew();
			milestone1.IsMilestone = true;
			milestone1.TriggerConditions.TriggerEventCode = eventCode;
			milestone1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;

			var milestone2 = dummy.WorkflowItems.AddNew();
			milestone2.IsMilestone = true;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			milestone2.TriggerConditions.TriggerEventCode = eventCode;

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<SecondLeg.Origin>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<ThirdLeg.Origin>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone1.TriggerConditions.TriggerConditionValue = "LOC=<FourthLeg.Origin>,FAC=CTO";
			milestone1.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;
			milestone2.TriggerConditions.TriggerCondition = string.Empty;
			AssertEquals(true, milestone1.IsMatchForItemTemplateMerge(milestone2));

			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(false, milestone1.IsMatchForItemTemplateMerge(milestone2));
		}

		#endregion

		#region Transport Legs

		public void TestContainer_P9_ReferencedID_PopulatedAutomatically()
		{
			// Note the order of OriginTransport/IntermediateTransport/DestinationTransport cannot be changed.
			var originTransport = OriginTransport;
			var intermediateTransport = IntermediateTransport;
			var destinationTransport = DestinationTransport;

			AssertEquals("Departure", originTransport.PK, DepartureMilestone.P9_ReferencedID);
			AssertEquals("FreightLoaded", originTransport.PK, FreightLoadedMilestone.P9_ReferencedID);
			AssertEquals("GateIn", originTransport.PK, GateInMilestone.P9_ReferencedID);
			AssertEquals("Arrival for Leg 2", originTransport.PK, ArrivalMilestoneForIntermediateTransport.P9_ReferencedID);
			AssertEquals("Departure for Leg 2", intermediateTransport.PK, DepartureMilestoneForIntermediateTransport.P9_ReferencedID);
			AssertEquals("Arrival for Leg 3", intermediateTransport.PK, ArrivalMilestoneForDestinationTransport.P9_ReferencedID);
			AssertEquals("Departure for Leg 3", destinationTransport.PK, DepartureMilestoneForDestinationTransport.P9_ReferencedID);
			AssertEquals("Arrival", destinationTransport.PK, ArrivalMilestone.P9_ReferencedID);
			AssertEquals("FreightUnloaded", destinationTransport.PK, FreightUnloadedMilestone.P9_ReferencedID);
			AssertEquals("GateOut", destinationTransport.PK, GateOutMilestone.P9_ReferencedID);
		}

		new ContainerProcessTask DepartureMilestone
		{
			get
			{
				if (departureMilestone == null)
				{
					departureMilestone = (ContainerProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
				}
				return departureMilestone;
			}
		}
		ContainerProcessTask departureMilestone;

		ContainerProcessTask FreightLoadedMilestone
		{
			get
			{
				if (freightLoadedMilestone == null)
				{
					freightLoadedMilestone = (ContainerProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					freightLoadedMilestone.TriggerConditions.TriggerEventCode = Events.FreightLoaded.Code;
				}
				return freightLoadedMilestone;
			}
		}
		ContainerProcessTask freightLoadedMilestone;

		ContainerProcessTask GateInMilestone
		{
			get
			{
				if (gateInMilestone == null)
				{
					gateInMilestone = (ContainerProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					gateInMilestone.TriggerConditions.TriggerEventCode = Events.GateIn.Code;
				}
				return gateInMilestone;
			}
		}
		ContainerProcessTask gateInMilestone;

		new ContainerProcessTask ArrivalMilestone
		{
			get
			{
				if (arrivalMilestone == null)
				{
					arrivalMilestone = (ContainerProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalMilestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
				}
				return arrivalMilestone;
			}
		}
		ContainerProcessTask arrivalMilestone;

		ContainerProcessTask FreightUnloadedMilestone
		{
			get
			{
				if (freightUnloadedMilestone == null)
				{
					freightUnloadedMilestone = (ContainerProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					freightUnloadedMilestone.TriggerConditions.TriggerEventCode = Events.FreightUnloaded.Code;
				}
				return freightUnloadedMilestone;
			}
		}
		ContainerProcessTask freightUnloadedMilestone;

		ContainerProcessTask GateOutMilestone
		{
			get
			{
				if (gateOutMilestone == null)
				{
					gateOutMilestone = (ContainerProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					gateOutMilestone.TriggerConditions.TriggerEventCode = Events.GateOut.Code;
				}
				return gateOutMilestone;
			}
		}
		ContainerProcessTask gateOutMilestone;

		ContainerProcessTask DepartureMilestoneForIntermediateTransport
		{
			get
			{
				if (departureMilestoneForIntermediateTransport == null)
				{
					departureMilestoneForIntermediateTransport = (ContainerProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureMilestoneForIntermediateTransport.TriggerConditions.TriggerEventCode = Events.Departure.Code;
					departureMilestoneForIntermediateTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
				}
				return departureMilestoneForIntermediateTransport;
			}
		}
		ContainerProcessTask departureMilestoneForIntermediateTransport;

		ContainerProcessTask ArrivalMilestoneForIntermediateTransport
		{
			get
			{
				if (arrivalMilestoneForIntermediateTransport == null)
				{
					arrivalMilestoneForIntermediateTransport = (ContainerProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalMilestoneForIntermediateTransport.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
					arrivalMilestoneForIntermediateTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
				}
				return arrivalMilestoneForIntermediateTransport;
			}
		}
		ContainerProcessTask arrivalMilestoneForIntermediateTransport;

		ContainerProcessTask ArrivalMilestoneForDestinationTransport
		{
			get
			{
				if (arrivalMilestoneForDestinationTransport == null)
				{
					arrivalMilestoneForDestinationTransport = (ContainerProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalMilestoneForDestinationTransport.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
					arrivalMilestoneForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
				}
				return arrivalMilestoneForDestinationTransport;
			}
		}
		ContainerProcessTask arrivalMilestoneForDestinationTransport;

		ContainerProcessTask DepartureMilestoneForDestinationTransport
		{
			get
			{
				if (departureMilestoneForDestinationTransport == null)
				{
					departureMilestoneForDestinationTransport = (ContainerProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureMilestoneForDestinationTransport.TriggerConditions.TriggerEventCode = Events.Departure.Code;
					departureMilestoneForDestinationTransport.TemplateConditions.TemplateCondition1 = JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
				}
				return departureMilestoneForDestinationTransport;
			}
		}
		ContainerProcessTask departureMilestoneForDestinationTransport;

		#endregion

		#region Implementation

		protected override ZString ParentOrigin
		{
			get => Job.Consol?.JK_RL_NKLoadPort ?? ZString.Empty;
			set
			{
				if (Job.Consol != null)
				{
					Job.Consol.JK_RL_NKLoadPort = value;
				}
			}
		}

		protected override ZString ParentDestination
		{
			get => Job.Consol?.JK_RL_NKDischargePort ?? ZString.Empty;
			set
			{
				if (Job.Consol != null)
				{
					Job.Consol.JK_RL_NKDischargePort = value;
				}
			}
		}

		protected override CommonContainer CreateNewJob()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_JK = consol.PK;
			return container;
		}

		protected override void SetUp()
		{
			base.SetUp();

			consol = Factory.NewWithValidTestData<CommonConsol>();
		}

		CommonConsol consol;

		#endregion
	}
}
