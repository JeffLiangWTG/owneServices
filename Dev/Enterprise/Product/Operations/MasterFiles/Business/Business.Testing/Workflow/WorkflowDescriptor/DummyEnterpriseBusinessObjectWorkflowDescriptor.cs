using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class DummyEnterpriseBusinessObjectWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.DummyWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return (NoResString)"DummyEnterpriseBusinessObject Workflow Descriptor"; }
		}

		public override ControllerID ControllerID
		{
			get { return null; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(DummyWithWorkflow); }
		}

		protected override Dictionary<ZString, SecurityCheckpoint> GetWorkflowTriggerActionTypeSecurityCheckPoints()
		{
			var result = base.GetWorkflowTriggerActionTypeSecurityCheckPoints();
			result.Add(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, Env.Security.None);
			return result;
		}

		protected internal override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var factory = new BusinessObjectFactory();
			var recipientOrg1 = factory.NewWithValidTestData<OrgHeader>();
			recipientOrg1.OH_Code = "Recipient1";
			var communicationsMode = recipientOrg1.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = Code;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationsMode.EK_Destination = "contact@Recipient1.com";

			var recipientOrg2 = factory.NewWithValidTestData<OrgHeader>();
			recipientOrg2.OH_Code = "Recipient2";
			var communicationsMode2 = recipientOrg2.EDICommunicationsModes.AddNew();
			communicationsMode2.EK_Module = Code;
			communicationsMode2.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			communicationsMode2.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationsMode2.EK_Destination = "contact@Recipient2.com";

			var communicationsMode3 = recipientOrg2.EDICommunicationsModes.AddNew();
			communicationsMode3.EK_Module = Code;
			communicationsMode3.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			communicationsMode3.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationsMode3.EK_Destination = "contact2@Recipient2.com";

			messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(recipientOrg1, ZString.Empty));
			messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(recipientOrg2, ZString.Empty));

			if (partyType == MessageRecipientPartyTypeList.Codes.Forwarder)
			{
				var recipientOrg3 = GlbCompany.CurrentCompany.Branches.First().OrgProxy;
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(recipientOrg3, ZString.Empty));
			}
		}

		public override bool ParentSupportsWorkflowTriggerActionUniversalShipmentXML(IBusiness parent) => true;
	}
}
