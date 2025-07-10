using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Integration;

namespace Enterprise.MasterFiles.Business
{
	class AssignStaffAndEmail : IProcessor
	{
		readonly WorkflowTriggerActionSource source;
		readonly Func<ProcessTaskNotification, BusinessObject, WorkflowTriggerActionSource, IMessageProcessor> msgGetter;

		public AssignStaffAndEmail(WorkflowTriggerActionSource source, Func<ProcessTaskNotification, BusinessObject, WorkflowTriggerActionSource, IMessageProcessor> msgGetter)
		{
			this.source = source;
			this.msgGetter = msgGetter;
		}

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			var fieldContent = source.Action.PQ_EmailAddr;
			var emailContent = source.Action.PQ_EmailTextFallbackToTemplate;

			source.Action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			source.Action.PQ_FieldName = "<P9_GS_NKAssignedStaffMember>";
			source.Action.PQ_FieldValue = fieldContent;
			ObjectFactory.Get<IWorkflowTriggerActionProcessorCreator>().GetSetFieldProcessor(source).Process(notifications, token);

			source.Action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			source.Action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.AssignedStaff;
			source.Action.PQ_EmailAddr = string.Empty;
			source.Action.PQ_EmailText = emailContent;
			msgGetter(source.Action, source.Job, source).Process(notifications, token);
		}
	}
}
