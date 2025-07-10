using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.Module
{
	public abstract class RatingHeaderWorkflowDescriptor<T> : WorkflowDescriptor
		where T : RatingHeader
	{
		public override Type WorkflowProviderType
		{
			get { return typeof(T); }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return true; }
		}

		#region Requires

		public override bool RequiresPort1
		{
			get { return false; }
		}

		public override bool RequiresPort2
		{
			get { return false; }
		}

		public override bool RequiresBranch
		{
			get { return false; }
		}

		public override bool RequiresDepartment
		{
			get { return false; }
		}

		public override bool RequiresClient
		{
			get { return true; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		#endregion

		#region Workflow Trigger

		public override bool SupportsWorkflowTriggerActionXML
		{
			get { return true; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Client |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			var action = source.Action;
			IProcessor result;

			if (WorkflowTriggerActionTypeConstants.IsStandardXml(action.PQ_TriggerType))
			{
				var ratingHeader = (T)source.Job;
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
				result = new XmlMessageDeliver(xmlModes, ratingHeader, GetValueObjectDataAdapterCore(), action);
			}
			else
			{
				result = base.GetWorkflowTriggerActionCore(source);
			}

			return result;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var ratingHeader = (T)bizObj;

			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ratingHeader.Header, ZString.Empty));
			}
		}

		protected override WorkflowTriggerNotification GetWorkflowTriggerForNotificationEmail(Lazy<MessageProcessorCommunicationModesResult> modes, ProcessTaskNotification action, BusinessObject parent, Lazy<IStmALog> logProvider)
		{
			var trigger = new WorkflowTriggerNotification(modes, action, parent, logProvider)
			{
				ExtraDataSubstitution = RatingHeaderWorkflowTriggerNotification.Substitute
			};
			return trigger;
		}

		#endregion

		#region Abstract methods

		protected abstract IValueObjectDataAdapter GetValueObjectDataAdapterCore();

		#endregion
	}
}

