using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.AccDraftInvoiceCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("AccDraftInvoiceWorkflowDescriptor|Description", "AP Draft");

		public override ControllerID ControllerID => null;

		public override Type WorkflowProviderType => typeof(AccDraftInvoiceHeader);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;

		public override bool SupportsDelayedEvents => false;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, CargoWise.EntityFramework.IBusiness business) => MessageRecipientPartyType.Email;
	}
}
