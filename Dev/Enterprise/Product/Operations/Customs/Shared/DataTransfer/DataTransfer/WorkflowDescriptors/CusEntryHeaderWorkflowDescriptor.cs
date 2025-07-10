using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using ResString = Enterprise.Customs.DataTransfer.ResString;

namespace Enterprise.Customs.Business
{
	public sealed class CusEntryHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CusEntryHeaderWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("CusEntryHeaderWorkflowDescriptor|Description", "Entry Header Line Trigger");

		public override Type WorkflowProviderType => typeof(CusEntryHeader);

		public override ControllerID ControllerID => null;

		public override BusinessContext[] DocumentBusinessContext => new[] { BusinessContext.CusEntryHeader };

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Email;
		}

		public override bool SupportsWorkflowTriggerActionXML => false;

		public override bool SupportsBufferManagement => false;

		public override bool SupportsWorkflowTemplates => false;

		public override bool SupportsEventTracking => true;

		public override bool AreTasksCompanySpecific => true;

		public override bool SupportsUniversalTemplates => false;

		protected override bool SupportsWorkflowTriggerActionUniversalEventXML => true;
	}
}
