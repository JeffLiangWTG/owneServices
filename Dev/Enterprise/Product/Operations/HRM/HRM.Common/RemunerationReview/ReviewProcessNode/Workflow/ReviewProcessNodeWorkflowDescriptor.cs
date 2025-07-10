using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.HRM.Common
{
	public class ReviewProcessNodeWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.ReviewProcessNodeWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("6d81fcd7-077b-4668-9ce7-7bae0fcaf40d", "Manager Review");

		public override ControllerID ControllerID => ControllerIDs.ReviewProcessNode;

		public override Type WorkflowProviderType => typeof(ReviewProcessNode);

		public override bool RequiresClient => false;

		public override bool RequiresPort1 => false;

		public override bool RequiresPort2 => false;

		public override bool RequiresBranch => false;

		public override bool AreTasksCompanySpecific => false;

		public override bool SupportsEventTracking => true;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.Email;
	}
}
