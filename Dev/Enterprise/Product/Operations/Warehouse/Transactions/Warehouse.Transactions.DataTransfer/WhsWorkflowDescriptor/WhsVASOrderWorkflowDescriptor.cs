using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderWorkflowDescriptor : WorkflowDescriptor
	{
		#region ControllerID

		public override ControllerID ControllerID => ControllerIDs.WhsVASOrder;

		#endregion

		#region WorkflowProviderType

		public override Type WorkflowProviderType => typeof(WhsVASOrder);

		#endregion

		#region Code

		public override string Code => WorkflowDescriptors.WhsVASOrderWorkflowDescriptorCode;

		#endregion

		#region Description

		public override IMultilingualString Description => Enterprise.Warehouse.Transactions.DataTransfer.ResString.GetMultilingualString("WhsVASOrderWorkflowDescriptor|Description", "Warehouse VAS Order");

		#endregion

		#region RequiresWarehouse

		public override bool RequiresWarehouse => true;

		#endregion

		#region SupportedMessageRecipientParties

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
		}

		#endregion

		#region WorkflowTriggerActionTypes

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent));
			result.AddPair(ActionTypes.Codes.CreateInitialVASOrderTransfer, ActionTypes.Descriptions.CreateInitialVASOrderTransfer);
			result.AddPair(ActionTypes.Codes.CreateReturnVASOrderTransfer, ActionTypes.Descriptions.CreateReturnVASOrderTransfer);
			result.AddPair(ActionTypes.Codes.CompleteAndFinaliseVASOrder, ActionTypes.Descriptions.CompleteAndFinaliseVASOrder);

			return result;
		}

		#endregion

		#region WorkflowTriggerActionProcessor

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			switch (source.Action.PQ_TriggerType)
			{
				case ActionTypes.Codes.CreateInitialVASOrderTransfer:
					return new CreateInitialVASOrderTransferProcessor((WhsVASOrder)source.Job);
				case ActionTypes.Codes.CreateReturnVASOrderTransfer:
					return new CreateReturnVASOrderTransferProcessor((WhsVASOrder)source.Job);
				case ActionTypes.Codes.CompleteAndFinaliseVASOrder:
					return new CompleteAndFinaliseVASOrderProcessor((WhsVASOrder)source.Job);

				default:
					return base.GetWorkflowTriggerActionCore(source);
			}
		}

		#endregion

		#region DocumentBusinessContext

		public override BusinessContext[] DocumentBusinessContext => new[] { BusinessContext.INVALID };

		#endregion

		#region SupportsEventTracking

		public override bool SupportsEventTracking => true;

		#endregion

		#region AreTasksCompanySpecific

		public override bool AreTasksCompanySpecific => false;

		#endregion
	}
}
