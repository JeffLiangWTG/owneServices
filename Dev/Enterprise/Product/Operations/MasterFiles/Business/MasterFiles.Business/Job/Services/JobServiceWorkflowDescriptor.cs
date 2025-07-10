using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class JobServiceWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.ServiceWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("JobServiceWorkflowDescriptor|Description", "Service");

		public override Type WorkflowProviderType => typeof(JobService);

		public override ControllerID ControllerID => null;

		#region Flags

		public override bool SupportsWorkflowTemplates => false;

		public override bool SupportsEventTracking => true;

		public override bool SupportsBufferManagement => false;

		public override bool AreTasksCompanySpecific => false;

		public override bool SupportsUniversalTemplates => false;

		#endregion

		protected internal override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageRecipientParties, bizObj, partyType);

			var service = bizObj as JobService;
			if (service == null)
			{
				return;
			}

			if (service.RequestForServiceParent is IConsignment consignment && partyType == MessageRecipientPartyTypeList.Codes.Forwarder)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consignment.BookingPartyDocAddress as JobDocAddress));
			}
		}

		#region SupportedMessageRecipientParties

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy
				 | MessageRecipientPartyType.Forwarder;
		}

		#endregion
	}
}
