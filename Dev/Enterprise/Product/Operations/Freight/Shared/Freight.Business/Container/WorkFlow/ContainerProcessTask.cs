using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	public class ContainerProcessTask : RoutingSupportProcessTask, IContainerProcessTask
	{
		public ContainerProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool P9_ActualDate_ReadOnly
		{
			get { return base.P9_ActualDate_ReadOnly || P9_SE_NKMilestoneEvent == Events.CargoAvailable.Code; }
		}

		protected override Type ParentType
		{
			get { return typeof(CommonContainer); }
		}

		public new CommonContainer Parent
		{
			get { return (CommonContainer)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Containers; }
		}

		#region RoutingSupportProcessTask

		protected override Type TransportType => typeof(Transport);

		protected override Type TransportParentType => Parent?.ContainerParent?.GetType() ?? typeof(CommonConsol);

		protected override Transport GetTransportLegToAttach()
		{
			Transport result = null;
			if (Parent?.ContainerParent != null && CanBeLinkedToTransport)
			{
				Transport[] transportLegsFromLoadToDischarge = TransportLegsFromLoadToDischarge;

				var isArrivalRelatedProcessTask = arrivalRelatedEvents.Contains(P9_SE_NKMilestoneEvent);
				var isDepartureRelatedProcessTask = departureRelatedEvents.Contains(P9_SE_NKMilestoneEvent);

				if (isArrivalRelatedProcessTask)
				{
					if (P9_Condition1 == JobContainerWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg
						&& transportLegsFromLoadToDischarge.Length >= 1)
					{
						result = transportLegsFromLoadToDischarge[0];
					}
					else if (P9_Condition1 == JobContainerWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg
						&& transportLegsFromLoadToDischarge.Length >= 2)
					{
						result = transportLegsFromLoadToDischarge[1];
					}
					else if (P9_Condition1 == JobContainerWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg
						&& transportLegsFromLoadToDischarge.Length >= 3)
					{
						result = transportLegsFromLoadToDischarge[2];
					}
					else if (P9_Condition1.IsEmpty)
					{
						result = transportLegsFromLoadToDischarge
							.FirstOrDefault(t => t.JW_RL_NKDiscPort == (Parent?.ContainerParent.DischargePort?.RL_Code ?? ZString.Empty));
					}
				}
				else if (isDepartureRelatedProcessTask)
				{
					if (P9_Condition1 == JobContainerWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg
						&& transportLegsFromLoadToDischarge.Length >= 2)
					{
						result = transportLegsFromLoadToDischarge[1];
					}
					else if (P9_Condition1 == JobContainerWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg
						&& transportLegsFromLoadToDischarge.Length >= 3)
					{
						result = transportLegsFromLoadToDischarge[2];
					}
					else if (P9_Condition1 == JobContainerWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg
						&& transportLegsFromLoadToDischarge.Length >= 4)
					{
						result = transportLegsFromLoadToDischarge[3];
					}
					else if (P9_Condition1.IsEmpty)
					{
						result = transportLegsFromLoadToDischarge
							.FirstOrDefault(t => t.JW_RL_NKLoadPort == (Parent?.ContainerParent.LoadPort?.RL_Code ?? ZString.Empty));
					}
				}
			}

			return result;
		}

		protected override bool IsTransportLinkedEvent(ZString eventCode)
		{
			return arrivalRelatedEvents.Contains(eventCode)
				|| departureRelatedEvents.Contains(eventCode);
		}

		Transport[] TransportLegsFromLoadToDischarge
		{
			get
			{
				var containerParent = Parent?.ContainerParent;
				if (containerParent != null)
				{
					return RoutineSupportHelper.GetTransportLegsFromLoadToDischarge(TransportLegsInMovementOrder,
						(containerParent.LoadPort?.RL_Code).GetValueOrDefault(),
						(containerParent.DischargePort?.RL_Code).GetValueOrDefault());
				}

				return Array.Empty<Transport>();
			}
		}

		#endregion

		#region GetTriggerConditionsComparer

		protected override IEqualityComparer<ProcessTask> GetTriggerConditionsComparer() => new ContainerTriggerConditionsComparer(base.GetTriggerConditionsComparer());

		class ContainerTriggerConditionsComparer : IEqualityComparer<ProcessTask>
		{
			public ContainerTriggerConditionsComparer(IEqualityComparer<ProcessTask> comparer)
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
					var containerTask = task as ContainerProcessTask;
					if (containerTask != null && containerTask.MatchesTemplateTask(itemTemplate))
					{
						return true;
					}

					containerTask = itemTemplate as ContainerProcessTask;
					if (containerTask != null && containerTask.MatchesTemplateTask(task))
					{
						return true;
					}

					return false;
				}
			}

			public int GetHashCode(ProcessTask obj) => comparer.GetHashCode(obj);
		}

		bool MatchesTemplateTask(ProcessTask itemTemplate)
		{
			var hasCorrectArrivalRelatedConditionPair = HasCorrectArrivalRelatedConditionPair(itemTemplate);
			var hasCorrectDepartureRelatedConditionPair = HasCorrectDepartureRelatedConditionPair(itemTemplate);

			return P9_TriggerCondition == EventReferenceConditionList.Codes.EventReferenceParameters
				&& itemTemplate.P9_TriggerCondition.IsEmpty
				&& ((IsEventCodeProcessTaskMatched(itemTemplate, Events.ArrivalCode) && hasCorrectArrivalRelatedConditionPair)
					|| (IsEventCodeProcessTaskMatched(itemTemplate, Events.FreightUnloadedCode) && hasCorrectArrivalRelatedConditionPair)
					|| (IsEventCodeProcessTaskMatched(itemTemplate, Events.GateOutCode) && hasCorrectArrivalRelatedConditionPair)
					|| (IsEventCodeProcessTaskMatched(itemTemplate, Events.DepartureCode) && hasCorrectDepartureRelatedConditionPair)
					|| (IsEventCodeProcessTaskMatched(itemTemplate, Events.FreightLoadedCode) && hasCorrectDepartureRelatedConditionPair)
					|| (IsEventCodeProcessTaskMatched(itemTemplate, Events.GateInCode) && hasCorrectDepartureRelatedConditionPair));
		}

		bool IsEventCodeProcessTaskMatched(ProcessTask itemTemplate, ZString eventCode)
		{
			return itemTemplate.P9_SE_NKMilestoneEvent == eventCode
				&& P9_SE_NKMilestoneEvent == eventCode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used as a constant")]
		bool HasCorrectArrivalRelatedConditionPair(ProcessTask itemTemplate)
		{
			return P9_TriggerConditionValue == "LOC=<FirstLeg.Destination>,FAC=CTO" && P9Condition1Is2ndLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<SecondLeg.Destination>,FAC=CTO" && P9Condition1Is3rdLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<ThirdLeg.Destination>,FAC=CTO" && P9Condition1Is4thLeg(itemTemplate);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used as a constant")]
		bool HasCorrectDepartureRelatedConditionPair(ProcessTask itemTemplate)
		{
			return P9_TriggerConditionValue == "LOC=<SecondLeg.Origin>,FAC=CTO" && P9Condition1Is2ndLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<ThirdLeg.Origin>,FAC=CTO" && P9Condition1Is3rdLeg(itemTemplate)
				|| P9_TriggerConditionValue == "LOC=<FourthLeg.Origin>,FAC=CTO" && P9Condition1Is4thLeg(itemTemplate);
		}

		bool P9Condition1Is2ndLeg(ProcessTask itemTemplate) => itemTemplate.P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg;
		bool P9Condition1Is3rdLeg(ProcessTask itemTemplate) => itemTemplate.P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg;
		bool P9Condition1Is4thLeg(ProcessTask itemTemplate) => itemTemplate.P9_Condition1 == JobConsolWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg;

		readonly List<string> arrivalRelatedEvents = new List<string>
		{
			Events.ArrivalCode,
			Events.FreightUnloadedCode,
			Events.GateOutCode
		};

		readonly List<string> departureRelatedEvents = new List<string>
		{
			Events.DepartureCode,
			Events.FreightLoadedCode,
			Events.GateInCode
		};

		#endregion
	}
}
