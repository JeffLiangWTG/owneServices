using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolProcessTask : RoutingSupportProcessTask, Enterprise.Integration.Forwarding.IForwardingConsolProcessTask
	{
		public ForwardingConsolProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(ForwardingConsol); }
		}

		protected override bool P9_ActualDate_ReadOnly
		{
			get { return base.P9_ActualDate_ReadOnly || P9_SE_NKMilestoneEvent == Events.CargoAvailable.Code; }
		}

		public new ForwardingConsol Parent
		{
			get { return (ForwardingConsol)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.JobConsol; }
		}

		protected override bool MatchesParentForMessageTriggerAction(ProcessTask relatedTrigger)
		{
			bool result = base.MatchesParentForMessageTriggerAction(relatedTrigger);
			if (!result &&
				ProcessTaskNotifications.ContainsTriggerType(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback) &&
				relatedTrigger.ProcessTaskNotifications.ContainsTriggerType(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback))
			{
				JobShipmentPreplanning relatedPreadvice = relatedTrigger.Parent as JobShipmentPreplanning;
				ForwardingShipment relatedShipment = relatedTrigger.Parent as ForwardingShipment;

				if (relatedPreadvice != null || relatedShipment != null)
				{
					foreach (ForwardingShipment shipment in Parent.Shipments)
					{
						if (shipment == relatedShipment ||
							shipment.PreAdvice == relatedPreadvice)
						{
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}

		protected override IEqualityComparer<ProcessTask> GetTriggerConditionsComparer() => new TriggerConditionsComparer(base.GetTriggerConditionsComparer());

		class TriggerConditionsComparer : IEqualityComparer<ProcessTask>
		{
			public TriggerConditionsComparer(IEqualityComparer<ProcessTask> comparer)
			{
				this.comparer = comparer;
			}
			readonly IEqualityComparer<ProcessTask> comparer;

			public bool Equals(ProcessTask task, ProcessTask itemTemplate)
			{
				if (comparer.Equals(task, itemTemplate))
				{
					return true;
				}
				else
				{
					var forwardingTask = task as ForwardingConsolProcessTask;
					if (forwardingTask != null && forwardingTask.MatchesTemplateTask(itemTemplate))
					{
						return true;
					}

					forwardingTask = itemTemplate as ForwardingConsolProcessTask;
					if (forwardingTask != null && forwardingTask.MatchesTemplateTask(task))
					{
						return true;
					}

					return false;
				}
			}

			public int GetHashCode(ProcessTask task) => comparer.GetHashCode(task);
		}

		bool MatchesTemplateTask(ProcessTask itemTemplate)
		{
			return TaskP9TriggerEqEventRefParam() && P9TriggerConditionIsEmpty(itemTemplate.P9_TriggerCondition)
					&& (IsInputArrivalTask(itemTemplate) && IsArrivalRelatedProcessTask && HasCorrectArrivalConditionPair(itemTemplate)
						|| IsInputDepartureTask(itemTemplate) && IsDepartureRelatedProcessTask && HasCorrectDepartureConditionPair(itemTemplate)
						|| IsInputCutOffDateTask(itemTemplate) && IsCutOffDateProcessTask && HasCorrectCutOffDateConditionPair(itemTemplate)
						|| IsInputStorageCommencedDateTask(itemTemplate) && IsStorageCommencedProcessTask && HasCorrectStorageCommencedConditionPair(itemTemplate)
						|| IsInputReceiptCommencedDateTask(itemTemplate) && IsReceiptCommencedProcessTask && HasCorrectReceiptCommencedConditionPair(itemTemplate));
		}

		bool P9TriggerConditionIsEmpty(ZString conditon) => conditon == "";

		bool TaskP9TriggerEqEventRefParam() => P9_TriggerCondition == EventReferenceConditionList.Codes.EventReferenceParameters;

		bool IsInputArrivalTask(ProcessTask itemTemplate) => itemTemplate.P9_SE_NKMilestoneEvent == Events.Arrival.Code;

		bool IsInputDepartureTask(ProcessTask itemTemplate) => itemTemplate.P9_SE_NKMilestoneEvent == Events.Departure.Code;

		bool IsInputCutOffDateTask(ProcessTask itemTemplate) => itemTemplate.P9_SE_NKMilestoneEvent == Events.CutOffDate.Code;

		bool IsInputStorageCommencedDateTask(ProcessTask itemTemplate) => itemTemplate.P9_SE_NKMilestoneEvent == Events.StorageCommenced.Code;

		bool IsInputReceiptCommencedDateTask(ProcessTask itemTemplate) => itemTemplate.P9_SE_NKMilestoneEvent == Events.ReceiptCommenced.Code;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used as a constant")]
		bool HasCorrectArrivalConditionPair(ProcessTask itemTemplate)
		{
			return P9_TriggerConditionValue == "LOC=<FirstLeg.Destination>,FAC=CTO" && P9Condition1Is2ndLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<SecondLeg.Destination>,FAC=CTO" && P9Condition1Is3rdLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<ThirdLeg.Destination>,FAC=CTO" && P9Condition1Is4thLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<LastLeg.Destination>,FAC=CTO";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used as a constant")]
		bool HasCorrectDepartureConditionPair(ProcessTask itemTemplate)
		{
			return P9_TriggerConditionValue == "LOC=<SecondLeg.Origin>,FAC=CTO" && P9Condition1Is2ndLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<ThirdLeg.Origin>,FAC=CTO" && P9Condition1Is3rdLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<FourthLeg.Origin>,FAC=CTO" && P9Condition1Is4thLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<FirstLeg.Origin>,FAC=CTO";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used as a constant")]
		bool HasCorrectCutOffDateConditionPair(ProcessTask itemTemplate)
		{
			return P9_TriggerConditionValue == "LOC=<SecondLeg.Origin>" && P9Condition1Is2ndLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<ThirdLeg.Origin>" && P9Condition1Is3rdLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<FourthLeg.Origin>" && P9Condition1Is4thLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<FirstLeg.Origin>";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used as a constant")]
		bool HasCorrectStorageCommencedConditionPair(ProcessTask itemTemplate)
		{
			return P9_TriggerConditionValue == "LOC=<FirstLeg.Destination>,FAC=CTO" && P9Condition1Is2ndLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<SecondLeg.Destination>,FAC=CTO" && P9Condition1Is3rdLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<ThirdLeg.Destination>,FAC=CTO" && P9Condition1Is4thLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<LastLeg.Destination>,FAC=CTO";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used as a constant")]
		bool HasCorrectReceiptCommencedConditionPair(ProcessTask itemTemplate)
		{
			return P9_TriggerConditionValue == "LOC=<SecondLeg.Origin>,FAC=CTO" && P9Condition1Is2ndLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<ThirdLeg.Origin>,FAC=CTO" && P9Condition1Is3rdLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<FourthLeg.Origin>,FAC=CTO" && P9Condition1Is4thLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<FirstLeg.Origin>,FAC=CTO";
		}

		bool P9Condition1Is2ndLeg(ProcessTask itemTemplate) => itemTemplate.P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;

		bool P9Condition1Is3rdLeg(ProcessTask itemTemplate) => itemTemplate.P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;

		bool P9Condition1Is4thLeg(ProcessTask itemTemplate) => itemTemplate.P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;

		#region Transport Legs

		protected override Type TransportType
		{
			get { return typeof(Transport); }
		}

		protected override Type TransportParentType
		{
			get { return typeof(ForwardingConsol); }
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override Transport GetTransportLegToAttach()
		{
			Transport result = null;
			if (P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.MainTransport)
			{
				result = GetMainTransport(Parent.Transports);
			}
			else if (CanBeLinkedToTransport)
			{
				Transport[] transportLegsFromLoadToDischarge = TransportLegsFromLoadToDischarge;
				if ((IsDepartureRelatedProcessTask || IsCutOffDateProcessTask || IsReceiptCommencedProcessTask || IsVesselVoyageProcessTask) && (P9_Condition1.IsEmpty || P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Import || P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Export))
				{
					result = Parent.Transports.FindTransportByLoadPort(Parent.JK_RL_NKLoadPort);
				}
				else if ((IsArrivalRelatedProcessTask || IsStorageCommencedProcessTask) && P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg && transportLegsFromLoadToDischarge.Length >= 1)
				{
					result = transportLegsFromLoadToDischarge[0];
				}
				else if ((IsDepartureRelatedProcessTask || IsCutOffDateProcessTask || IsReceiptCommencedProcessTask || IsVesselVoyageProcessTask) && P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg && transportLegsFromLoadToDischarge.Length >= 2)
				{
					result = transportLegsFromLoadToDischarge[1];
				}
				else if ((IsArrivalRelatedProcessTask || IsStorageCommencedProcessTask) && P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg && transportLegsFromLoadToDischarge.Length >= 2)
				{
					result = transportLegsFromLoadToDischarge[1];
				}
				else if ((IsDepartureRelatedProcessTask || IsCutOffDateProcessTask || IsReceiptCommencedProcessTask || IsVesselVoyageProcessTask) && P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg && transportLegsFromLoadToDischarge.Length >= 3)
				{
					result = transportLegsFromLoadToDischarge[2];
				}
				else if ((IsArrivalRelatedProcessTask || IsStorageCommencedProcessTask) && P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg && transportLegsFromLoadToDischarge.Length >= 3)
				{
					result = transportLegsFromLoadToDischarge[2];
				}
				else if ((IsDepartureRelatedProcessTask || IsCutOffDateProcessTask || IsReceiptCommencedProcessTask || IsVesselVoyageProcessTask) && P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg && transportLegsFromLoadToDischarge.Length >= 4)
				{
					result = transportLegsFromLoadToDischarge[3];
				}
				else if ((IsArrivalRelatedProcessTask || IsStorageCommencedProcessTask) && (P9_Condition1.IsEmpty || P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Import || P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Export))
				{
					result = Parent.Transports.FindTransportByDischargePort(Parent.JK_RL_NKDischargePort);
				}
			}
			return result;
		}

		bool IsDepartureRelatedProcessTask
		{
			get
			{
				return P9_SE_NKMilestoneEvent == Events.Departure.Code ||
					(P9_SE_NKMilestoneEvent.IsEmpty && (P9_TriggerField == Transport.Schema.JW_ETD ||
					P9_TriggerField == Transport.Schema.JW_ATD));
			}
		}

		bool IsArrivalRelatedProcessTask
		{
			get
			{
				return P9_SE_NKMilestoneEvent == Events.Arrival.Code ||
					(P9_SE_NKMilestoneEvent.IsEmpty && (P9_TriggerField == Transport.Schema.JW_ETA ||
					P9_TriggerField == Transport.Schema.JW_ATA));
			}
		}

		bool IsCutOffDateProcessTask
		{
			get { return P9_SE_NKMilestoneEvent == Events.CutOffDate.Code; }
		}

		bool IsStorageCommencedProcessTask => P9_SE_NKMilestoneEvent == Events.StorageCommenced.Code;

		bool IsReceiptCommencedProcessTask => P9_SE_NKMilestoneEvent == Events.ReceiptCommenced.Code;

		bool IsVesselVoyageProcessTask => P9_TriggerField == Transport.Schema.JW_Vessel || P9_TriggerField == Transport.Schema.JW_VoyageFlight;

		Transport GetMainTransport(TransportCollection transports)
		{
			foreach (Transport transport in transports)
			{
				if (transport.JW_TransportType == Core.Constants.TransportPlanningType.MainVessel)
				{
					return transport;
				}
			}
			return null;
		}

		Transport[] TransportLegsFromLoadToDischarge
		{
			get
			{
				return RoutineSupportHelper.GetTransportLegsFromLoadToDischarge(TransportLegsInMovementOrder,
					Parent.JK_RL_NKLoadPort, Parent.JK_RL_NKDischargePort);
			}
		}

		#endregion

		#region ProcessTask Property Overrides

		#region ETA

		public override ZDateTime ETA
		{
			get { return Parent?.MostInterestingTransportForBinding.Cast<Transport>().FirstOrDefault()?.JW_ETA ?? ZDateTime.Empty; }
		}

		#endregion

		#region ETD

		public override ZDateTime ETD
		{
			get { return Parent?.MostInterestingTransportForBinding.Cast<Transport>().FirstOrDefault()?.JW_ETD ?? ZDateTime.Empty; }
		}

		#endregion

		#region Load/Origin Port

		public override ZString LoadOrOriginPort
		{
			get { return Parent == null ? ZString.Empty : Parent.JK_RL_NKLoadPort; }
		}

		#endregion

		#region Discharge/Destination Port

		public override ZString DischargeOrDestinationPort
		{
			get { return Parent == null ? ZString.Empty : Parent.JK_RL_NKDischargePort; }
		}

		#endregion

		#endregion
	}
}
