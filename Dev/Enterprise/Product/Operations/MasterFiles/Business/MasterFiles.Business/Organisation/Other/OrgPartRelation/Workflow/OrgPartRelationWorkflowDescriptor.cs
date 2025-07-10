using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartRelationWorkflowDescriptor : WorkflowDescriptor
	{
		#region Identification

		public override string Code
		{
			get { return WorkflowDescriptors.OrgPartRelationWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("MasterFiles|OrgPartRelationWorkflowDescriptor|Description", "Organization Product Relationship"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.WhsConfigProduct; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(OrgPartRelation); }
		}

		#endregion

		#region Capabilities

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo) { return false; }
		public override bool SupportsPostOverseasAgentCharges { get { return false; } }
		public override bool IncludeWorkflowTriggerActionXMLDebtorBalance { get { return false; } }
		public override bool SupportsTasks => false;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.None;
		}
		public override bool SupportsBufferManagement { get { return false; } }

		public override bool SupportsUniversalTemplates => false;

		#endregion

		#region Trigger Actions

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			return new LogAction(FormattableString.Invariant($"Trigger Actions are not supported for Workflow Type {source.Trigger.WorkflowProcessType}"));
		}

		#endregion

	}
}
