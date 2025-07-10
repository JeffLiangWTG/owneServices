using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class AccComplianceSequenceWorkflowDescriptor : WorkflowDescriptor
	{
		#region Identification

		public const string WorkflowTypeCode = "CSQ";

		public override string Code { get { return WorkflowTypeCode; } }

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("MasterFiles|AccComplianceSequenceWorkflowDescriptor|Description", "Compliance Sequence"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(AccComplianceSequence); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AccComplianceSequence; }
		}

		#endregion

		#region Capabilities

		public override bool RequiresClient { get { return false; } }

		public override bool RequiresBranch { get { return true; } }

		public override bool RequiresDepartment { get { return true; } }

		public override bool SupportsEventTracking { get { return true; } }

		public override bool IncludeWorkflowTriggerActionXMLDebtorBalance { get { return false; } }

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.ARInvoice }; }
		}

		#endregion
	}
}
