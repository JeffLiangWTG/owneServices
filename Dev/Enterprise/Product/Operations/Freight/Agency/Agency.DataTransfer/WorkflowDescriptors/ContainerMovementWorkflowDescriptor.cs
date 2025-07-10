using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Agency.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.DataTransfer
{
	public class ContainerMovementWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.ContainerMovementWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("38e1ec96-f16c-4f2c-b206-9182595a5574", "Container Movement"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AgencyContainerMove; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(ContainerMovement); }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy;
		}

		public override bool SupportsBufferManagement
		{
			get { return false; }
		}

		public override bool SupportsUniversalTemplates => false;
	}
}
