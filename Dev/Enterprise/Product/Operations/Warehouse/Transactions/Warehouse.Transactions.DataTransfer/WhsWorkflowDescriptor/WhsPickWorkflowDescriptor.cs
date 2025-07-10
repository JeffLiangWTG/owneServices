using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.WhsPickWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description => DataTransfer.ResString.GetMultilingualString("WhsPick|WhsPickWorkflowDescriptor|Description", "Warehouse Pick");

		public override Type WorkflowProviderType
		{
			get { return typeof(WhsPick); }
		}

		public override bool RequiresWarehouse => true;

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new[] { WhsPickSchema.WP_IsCartonised };
		}

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
		{
			return triggerAction != WorkflowTriggerActionTypeConstants.Codes.SendDocument
						 && base.IsMessagingOrEmailNotificationTriggerAction(triggerAction);
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.OrgProxy;
		}

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent));
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.AutoFinalisation, WorkflowTriggerActionTypeConstants.Descriptions.AutoFinalisation);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.PrintAllPackageLabels, WorkflowTriggerActionTypeConstants.Descriptions.PrintAllPackageLabels);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.PrintAllCarrierLabels, WorkflowTriggerActionTypeConstants.Descriptions.PrintAllCarrierLabels);
			if (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.Value)
			{
				result.AddPair(ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning, ActionTypes.Descriptions.SetTaskPlanningStatusToReadyForPlanning);
			}

			return result;
		}

		public override bool IsPrintingTriggerAction(ZString triggerAction)
		{
			return base.IsPrintingTriggerAction(triggerAction)
				|| triggerAction == WorkflowTriggerActionTypeConstants.Codes.PrintAllPackageLabels
				|| triggerAction == WorkflowTriggerActionTypeConstants.Codes.PrintAllCarrierLabels;
		}

		protected override bool IsPrintOnlyTriggerAction(ZString triggerAction)
		{
			return base.IsPrintOnlyTriggerAction(triggerAction)
				|| triggerAction == WorkflowTriggerActionTypeConstants.Codes.PrintAllCarrierLabels;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			var action = source.Action;
			switch (action.PQ_TriggerType)
			{
				case WorkflowTriggerActionTypeConstants.Codes.AutoFinalisation:
					return new WhsOrderAndPickAutoFinalisationProcessor((WhsPick)source.Job);

				case WorkflowTriggerActionTypeConstants.Codes.PrintAllPackageLabels:
					return new PrintPackageLabelsProcessor((WhsPick)source.Job, action.PQ_SQ);

				case WorkflowTriggerActionTypeConstants.Codes.PrintAllCarrierLabels:
					return new PrintCarrierLabelsProcessor((WhsPick)source.Job, action.PQ_SQ);

				case ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning:
					return new SetTaskPlanningStatusToReadyForPlanningProcessor((WhsPick)source.Job);

				default:
					return base.GetWorkflowTriggerActionCore(source);
			}
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.WhsDespatch }; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsPicking; }
		}
	}
}
