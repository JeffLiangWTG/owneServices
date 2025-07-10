using System;
using System.Collections;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class TransportWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return JobInvoicingConsumerTypes.LocalCartage.Code; }
		}

		public override IMultilingualString Description
		{
			get { return JobInvoicingConsumerTypes.LocalCartage.MultilingualDescription; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Cartage; }
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var cartage = (CommonCartage)bizObj;
			if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				JobDocAddress[] consigneeAddr = cartage.DocAddresses.FindDocAddressesByType(DocAddressType.LocalCartageImporter);
				if (consigneeAddr.Length > 0 && consigneeAddr[0].Organisation != null)
				{
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(consigneeAddr[0]));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.BillToParty && cartage.LocalClient != null)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(cartage.LocalClient, ZString.Empty));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(cartage.BookingParty, ZString.Empty));
			}
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Consignee
				| MessageRecipientPartyType.BillToParty
				| MessageRecipientPartyType.OrgProxy
				| MessageRecipientPartyType.Email
				| MessageRecipientPartyType.BookingParty;
		}

		public override bool IsMessagingOrEmailNotificationTriggerAction(ZString triggerAction)
		{
			return !(triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendDocument) && base.IsMessagingOrEmailNotificationTriggerAction(triggerAction);
		}

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				ArrayList list = new ArrayList();
				list.AddRange(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType(Res.GetString("00a7d264-6e90-4fa9-b25d-641619cb820e", "Job Type"), TransportJobTypeList));

				return (ProcessTemplateSubType[])list.ToArray(typeof(ProcessTemplateSubType));
			}
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public CodeDescriptionPairList TransportJobTypeList
		{
			get { return BindToLists.GetCachedLists(Factory).NewCartageJobTypes; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.Cartage, BusinessContext.ContainerLeg }; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(CommonCartage); }
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new LocalTransportFormCustomisationSettingsProvider();
		}

		public new BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
