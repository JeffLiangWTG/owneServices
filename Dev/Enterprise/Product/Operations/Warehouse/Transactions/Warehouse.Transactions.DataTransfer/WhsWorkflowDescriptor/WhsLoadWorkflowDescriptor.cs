using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsLoadWorkflowDescriptor : WorkflowDescriptor
	{
		#region ControllerID

		public override ControllerID ControllerID => ControllerIDs.WhsLoad;

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType => typeof(WhsLoad);

		#endregion

		#region Workflow Triggers

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent));
			if (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value)
			{
				result.AddPair(ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning, ActionTypes.Descriptions.SetTaskPlanningStatusToReadyForPlanning);
			}
			return result;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var load = (WhsLoad)source.Job;
			var action = source.Action;

			switch (action.PQ_TriggerType)
			{
				case ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning:
					return new SetTaskPlanningStatusToReadyForPlanningProcessor(load);

				default:
					return base.GetWorkflowTriggerActionCore(source,queuedLog);
			}
		}

		#endregion

		#region Code

		public override string Code => WorkflowDescriptors.WhsLoadWorkflowDescriptorCode;

		#endregion

		#region Description

		public override IMultilingualString Description => DataTransfer.ResString.GetMultilingualString("Warehouse|WhsLoadWorkflowDescriptor|Description", "Warehouse Load");

		#endregion

		#region Events

		public override bool SupportsEventTracking => true;

		#endregion

		#region DocumentBusinessContext

		public override BusinessContext[] DocumentBusinessContext => new[] { BusinessContext.WhsLoad };

		#endregion
	}
}
