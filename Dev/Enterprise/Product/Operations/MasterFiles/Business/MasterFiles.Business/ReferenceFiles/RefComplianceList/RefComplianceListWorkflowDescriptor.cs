using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class RefComplianceListWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.RefComplianceListWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("RefComplianceListWorkflowDescriptor|Description", "Compliance List"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(RefComplianceList); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.RefComplianceList; }
		}

		public override bool SupportsEventTracking => true;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.Email;
	}
}
