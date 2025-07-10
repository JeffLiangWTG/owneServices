using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdjustmentWorkflowDescriptor : WhsDocketWorkflowDescriptor
	{
		protected override ZString GetCode() => WorkflowDescriptors.WhsAdjustmentWorkflowDescriptorCode;

		protected override IMultilingualString GetDescription() => Enterprise.Warehouse.Transactions.DataTransfer.ResString.GetMultilingualString("Warehouse|WhsAdjustmentWorkflowDescriptor|Description", "Warehouse Adjustment");

		protected override ControllerID GetControllerID() => ControllerIDs.WhsAdjustment;

		public override Type WorkflowProviderType => typeof(WhsAdjustment);

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.WhsAdjustment };

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(ActionTypes.Codes.Confirmation, ActionTypes.Descriptions.Confirmation);
			return result;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queued)
		{
			var adjustment = (WhsAdjustment)source.Job;
			var action = source.Action;

			switch (action.PQ_TriggerType)
			{
				case ActionTypes.Codes.Confirmation:
					return GetConfirmationAction();

				default:
					return base.GetWorkflowTriggerActionCore(source, queued);
			}

			IProcessor GetConfirmationAction()
			{
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
				var valueAdapter = new WhsAdjustmentValueObjectDataAdapter(
					GetDocketTriggeredByEvents(source,
					EventsWithSourceType.SourceType.WhsAdjustment,
					queued,
					action));

				return new XmlMessageDeliver(xmlModes, adjustment, valueAdapter, action);
			}
		}
	}
}
