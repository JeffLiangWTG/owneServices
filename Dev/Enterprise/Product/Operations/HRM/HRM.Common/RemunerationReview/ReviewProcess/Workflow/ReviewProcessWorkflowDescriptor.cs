using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.HRM.Common
{
	public class ReviewProcessWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.ReviewProcessWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("cf690240-a22e-4ebf-9e93-1d6f4ee17e48", "Review");

		public override ControllerID ControllerID => ControllerIDs.ReviewProcess;

		public override Type WorkflowProviderType => typeof(ReviewProcess);

		public override bool RequiresClient
			=> false;

		public override bool AreTasksCompanySpecific
			=> false;

		public override bool SupportsBufferManagement
			=> false;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.Email;
	}
}
