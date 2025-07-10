using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSLoadListConsolWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description / ControllerID

		public override string Code
		{
			get { return JobInvoicingConsumerTypes.CFSLoadList.Code; }
		}

		public override IMultilingualString Description
		{
			get { return JobInvoicingConsumerTypes.CFSLoadList.MultilingualDescription; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.LoadListConsol; }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				var result = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				result.Add(new ProcessTemplateSubType(Res.GetString("d4f2aba9-6711-4b7a-87b9-48f60d47eded", "Transport Mode"), TransportModeList));
				return result.ToArray();
			}
		}

		public CodeDescriptionPairList TransportModeList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.TransportType); }
		}

		#endregion

		public override bool RequiresPort1
		{
			get { return true; }
		}

		public override bool RequiresPort2
		{
			get { return true; }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.CFSLoadList }; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(CFSLoadListConsol); }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy
				| MessageRecipientPartyType.Email
				| MessageRecipientPartyType.TransportCo
				| MessageRecipientPartyType.ContainerYard
				| MessageRecipientPartyType.CTO
				| MessageRecipientPartyType.Forwarder;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var consol = (CFSLoadListConsol)bizObj;

			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.TransportCo:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.CartageCoAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.CTO:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.CTOAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.ContainerYard:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.EmptyContainerYardAddress));
					break;
				case MessageRecipientPartyTypeList.Codes.Forwarder:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consol.Forwarder, ZString.Empty));
					break;
			}
		}
	}
}
